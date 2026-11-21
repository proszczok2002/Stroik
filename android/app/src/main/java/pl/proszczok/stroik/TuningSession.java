package pl.proszczok.stroik;

import android.content.*;
import android.media.AudioManager;
import android.os.*;
import android.speech.tts.TextToSpeech;
import android.speech.tts.UtteranceProgressListener;
import java.lang.ref.WeakReference;

/** Owns capture independently of an Activity while the user-started service is active. */
final class TuningSession implements AudioEngine.Listener {
  private static TuningSession instance;
  final Context context;
  final AudioEngine audio;
  private WeakReference<MainActivity> screen = new WeakReference<>(null);
  Model model = new Model();
  String lastResult = "";
  boolean serviceActive, ready;
  private boolean speechInitialized;
  private final TextToSpeech speech;
  private String pendingSpeech = "";
  private boolean pendingResultSpeech;
  private final Handler main = new Handler(Looper.getMainLooper());
  private int speechEpoch;
  private String utterance = "";
  final MediaControls media;

  static synchronized TuningSession get(Context context) {
    if (instance == null) instance = new TuningSession(context.getApplicationContext());
    return instance;
  }

  private TuningSession(Context context) {
    this.context = context;
    try (java.io.BufferedReader input = new java.io.BufferedReader(new java.io.InputStreamReader(
        context.getAssets().open("fork-calibration.json"), java.nio.charset.StandardCharsets.UTF_8))) {
      StringBuilder json = new StringBuilder();
      for (String line; (line = input.readLine()) != null;) json.append(line);
      Model.loadFactoryCalibration(json.toString());
    } catch (java.io.IOException | org.json.JSONException error) {
      throw new IllegalStateException("Invalid factory calibration", error);
    }
    audio =
        new AudioEngine(
            model, this, (AudioManager) context.getSystemService(Context.AUDIO_SERVICE));
    media = new MediaControls(context, this);
    speech =
        new TextToSpeech(
            context,
            status -> {
              speechInitialized = true;
              ready = status == TextToSpeech.SUCCESS;
              if (ready && !pendingSpeech.isEmpty()) speak(pendingSpeech, pendingResultSpeech);
              else if (!ready) { pendingSpeech = ""; audio.speechFinished(); }
            });
    speech.setOnUtteranceProgressListener(
        new UtteranceProgressListener() {
          public void onStart(String id) {}

          public void onDone(String id) {
            speechDone(id);
          }

          public void onError(String id) {
            speechDone(id);
          }

          public void onStop(String id, boolean interrupted) {
            speechDone(id);
          }
        });
  }

  void attach(MainActivity activity) {
    screen = new WeakReference<>(activity);
    media.setAvailable(true);
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
    speak(value, false);
  }

  private void speak(String value, boolean isResult) {
    pendingSpeech = value;
    pendingResultSpeech = isResult;
    audio.holdForSpeech(isResult);
    if (!ready) {
      if (speechInitialized) { pendingSpeech = ""; audio.speechFinished(); }
      return;
    }
    speech.setLanguage(model.locale());
    speech.setSpeechRate((float) model.speechRate);
    utterance = "stroik-" + (++speechEpoch);
    String id = utterance;
    if (speech.speak(value, TextToSpeech.QUEUE_FLUSH, null, id) == TextToSpeech.ERROR)
      speechDone(id);
    // A dead TTS engine must not block the microphone indefinitely.
    main.postDelayed(() -> speechDone(id), 120000);
    pendingSpeech = "";
  }

  private void speechDone(String id) {
    main.post(() -> {
      if (utterance.equals(id)) {
        utterance = "";
        audio.speechFinished();
      }
    });
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

  void captureStarted() {
    if (!audio.mappingActive() && model.method >= 2 && model.beginner) {
      int epoch = speechEpoch;
      main.postDelayed(() -> {
        if (serviceActive && audio.running() && epoch == speechEpoch)
          speak(model.methodGuidance() + "\n\n" + model.beginnerGuidance());
      }, 300);
    }
  }

  void stop() {
    pendingSpeech = "";
    utterance = "";
    speechEpoch++;
    speech.stop();
    audio.stop();
    serviceActive = false;
    context.stopService(new Intent(context, TuningService.class));
    state();
  }

  void state() {
    MainActivity activity = screen.get();
    if (activity != null) activity.microphoneState();
    media.refresh();
  }

  public void playbackChanged() {
    media.refresh();
  }

  boolean playing() {
    MainActivity activity = screen.get();
    return activity != null && activity.active
        ? activity.playbackActive()
        : audio.running() || audio.forkPlaying();
  }

  void toggleFromMedia() {
    MainActivity activity = screen.get();
    if (activity != null && activity.active) activity.magicTap();
    else if (audio.running() || audio.forkPlaying()) stop();
    else if (model.method == 0 && !model.forkTap) {
      audio.strike();
      media.refresh();
    } else
      speak(
          model.t("Otwórz Stroik, aby włączyć mikrofon.", "Open Stroik to start the microphone."));
  }

  void endSequence() {
    audio.setPaused(true);
    stop();
    MainActivity activity = screen.get();
    if (activity != null) activity.sequenceEnded();
  }

  public void result(String text) {
    lastResult = text;
    MainActivity activity = screen.get();
    if (activity != null && activity.active && !screenOff()) activity.displayResult(text);
    // TalkBack announcements have no completion callback. Use the app's TTS
    // for measurement speech so playback and navigation wait for the real end.
    if (screenOff() ? !model.keepScreenOn : !model.graphics || model.voiceInVisual) speak(text, true);
    else audio.speechFinished();
  }

  public void reading(double cents) {
    MainActivity activity = screen.get();
    if (activity != null && activity.active) activity.reading(cents);
  }

  public void mapped(int midi, int count, int needed, double average) {
    MainActivity activity = screen.get();
    if (activity != null && activity.active) activity.mapped(midi, count, needed, average);
    else audio.speechFinished();
  }

  public void nextMapped() {
    MainActivity activity = screen.get();
    if (activity != null && activity.active) activity.nextMapped();
  }

  public void advance() {
    MainActivity activity = screen.get();
    if (activity != null && activity.active) {
      activity.advance();
      return;
    }
    int next = model.next();
    if (next == model.base()) {
      endSequence();
      speak(
          model.t(
              "Koniec wybranej kolejności. Strojenie zatrzymane.",
              "End of selected order. Tuning stopped."));
      return;
    }
    model.target = next;
    audio.configure(model);
    context
        .getSharedPreferences("stroik", 0)
        .edit()
        .putString("settings", model.json().toString())
        .apply();
    speak(model.nextAnnouncement(next));
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
