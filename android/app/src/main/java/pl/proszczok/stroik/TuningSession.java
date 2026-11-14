package pl.proszczok.stroik;

import android.content.*;
import android.media.AudioManager;
import android.os.*;
import android.speech.tts.TextToSpeech;
import java.lang.ref.WeakReference;
import java.util.Locale;

/** Owns capture independently of an Activity while the user-started service is active. */
final class TuningSession implements AudioEngine.Listener {
  private static TuningSession instance;
  final Context context;
  final AudioEngine audio;
  private WeakReference<MainActivity> screen = new WeakReference<>(null);
  Model model = new Model();
  String lastResult = "";
  boolean serviceActive, ready;
  private final TextToSpeech speech;
  private String pendingSpeech = "";

  static synchronized TuningSession get(Context context) {
    if (instance == null) instance = new TuningSession(context.getApplicationContext());
    return instance;
  }

  private TuningSession(Context context) {
    this.context = context;
    audio =
        new AudioEngine(
            model, this, (AudioManager) context.getSystemService(Context.AUDIO_SERVICE));
    speech =
        new TextToSpeech(
            context,
            status -> {
              ready = status == TextToSpeech.SUCCESS;
              if (ready && !pendingSpeech.isEmpty()) speak(pendingSpeech);
            });
  }

  void attach(MainActivity activity) {
    screen = new WeakReference<>(activity);
  }

  void detach(MainActivity activity) {
    if (screen.get() == activity) screen.clear();
  }

  void configure(Model value) {
    model = value.copy();
    audio.configure(model);
  }

  boolean screenOff() {
    return !((PowerManager) context.getSystemService(Context.POWER_SERVICE)).isInteractive();
  }

  void speak(String value) {
    pendingSpeech = value;
    if (!ready) return;
    speech.setLanguage(model.english ? Locale.US : new Locale("pl"));
    speech.speak(value, TextToSpeech.QUEUE_FLUSH, null, "stroik");
    pendingSpeech = "";
  }

  void start() {
    if (serviceActive) return;
    serviceActive = true;
    try {
      context.startForegroundService(new Intent(context, TuningService.class));
    } catch (RuntimeException error) {
      serviceActive = false;
      throw error;
    }
  }

  void stop() {
    pendingSpeech = "";
    speech.stop();
    audio.stop();
    serviceActive = false;
    context.stopService(new Intent(context, TuningService.class));
    state();
  }

  void state() {
    MainActivity activity = screen.get();
    if (activity != null) activity.microphoneState();
  }

  public void result(String text) {
    lastResult = text;
    MainActivity activity = screen.get();
    if (activity != null && activity.active && !screenOff()) activity.result(text);
    else if (screenOff() || !model.graphics || model.voiceInVisual) speak(text);
  }

  public void reading(double cents) {
    MainActivity activity = screen.get();
    if (activity != null && activity.active) activity.reading(cents);
  }

  public void advance() {
    MainActivity activity = screen.get();
    if (activity != null && activity.active) {
      activity.advance();
      return;
    }
    int next = model.next();
    if (next == model.base()) return;
    model.target = next;
    audio.configure(model);
    context
        .getSharedPreferences("stroik", 0)
        .edit()
        .putString("settings", model.json().toString())
        .apply();
    speak(model.spokenNote(next));
  }

  public void forkPaused(String text) {
    audio.setPaused(true);
    MainActivity activity = screen.get();
    if (activity != null && activity.active) activity.forkPaused(text);
    else speak(text);
  }

  public void failed(String text) {
    stop();
    MainActivity activity = screen.get();
    if (activity != null && activity.active) activity.failed(text);
    else speak(model.t("Mikrofon zatrzymany: ", "Microphone stopped: ") + text);
  }
}
