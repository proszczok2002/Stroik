package pl.proszczok.stroik;

import android.app.*;
import android.content.*;
import android.graphics.Rect;
import android.os.*;
import android.view.*;
import android.view.accessibility.*;
import android.widget.*;
import java.util.*;
import java.util.concurrent.atomic.AtomicInteger;

// Runs on the actual device without suppressing the user's accessibility services.
public final class DeviceTests extends Instrumentation {
  private final StringBuilder report = new StringBuilder();
  private MainActivity activity;
  private UiAutomation automation;
  private int assertions;
  private String scenario = "";
  private final AtomicInteger focused = new AtomicInteger(), clicked = new AtomicInteger();

  @Override
  public void onCreate(Bundle args) {
    super.onCreate(args);
    scenario = args == null ? "" : args.getString("scenario", "");
    start();
  }

  private void require(boolean value, String reason) {
    assertions++;
    if (!value) throw new AssertionError(reason);
  }

  private void log(String text) {
    report.append(text).append('\n');
    android.util.Log.i("StroikDeviceTests", text);
  }

  private void main(Runnable action) {
    runOnMainSync(action);
    waitForIdleSync();
  }

  private List<View> views() {
    List<View> all = new ArrayList<>();
    collect(activity.getWindow().getDecorView(), all);
    return all;
  }

  private void collect(View view, List<View> all) {
    all.add(view);
    if (view instanceof ViewGroup)
      for (int i = 0; i < ((ViewGroup) view).getChildCount(); i++)
        collect(((ViewGroup) view).getChildAt(i), all);
  }

  private Button findButton(String text) {
    for (View v : views())
      if (v instanceof Button && ((Button) v).getText().toString().equals(text)) return (Button) v;
    throw new AssertionError("Missing button: " + text);
  }

  private Spinner findSpinner(String label) {
    for (View v : views())
      if (v instanceof Spinner && label.contentEquals(v.getContentDescription()))
        return (Spinner) v;
    throw new AssertionError("Missing spinner: " + label);
  }

  private CheckBox findCheck(String label) {
    for (View v : views())
      if (v instanceof CheckBox && ((CheckBox) v).getText().toString().equals(label))
        return (CheckBox) v;
    throw new AssertionError("Missing checkbox: " + label);
  }

  private void page(String name) {
    main(() -> findButton(name).performClick());
  }

  private void inspect(String label) {
    float density = activity.getResources().getDisplayMetrics().density;
    int controls = 0;
    for (View v : views()) {
      if (v.getVisibility() != View.VISIBLE) continue;
      if (v instanceof Button || v instanceof Spinner || v instanceof EditText) {
        controls++;
        require(
            v.getHeight() >= 48 * density - 1,
            "Small touch target: "
                + label
                + " / "
                + v.getClass().getSimpleName()
                + " height="
                + v.getHeight());
        if (v instanceof TextView)
          require(((TextView) v).getText().length() > 0, "Unnamed text control");
        if (v instanceof Spinner || v instanceof EditText)
          require(
              v.getContentDescription() != null && v.getContentDescription().length() > 0,
              "Unlabelled input");
        require(v.isFocusable(), "Input not focusable");
      }
    }
    log(label + ": checked " + controls + " controls");
  }

  private void algorithms() throws Exception {
    Model m = new Model();
    m.english = false;
    require(m.note(67).contains("razkreślnej"), "Polish octave label");
    m.updates = false;
    m.minutes = 37;
    m.method = 3;
    m.monitor = true;
    m.interval = 2;
    m.forkTap = true;
    Model copy = Model.from(m.json());
    require(
        !copy.updates
            && copy.minutes == 37
            && copy.method == 3
            && copy.monitor
            && copy.interval == 2
            && copy.forkTap,
        "Profile round trip");
    try {
      Model bad = new Model();
      bad.minutes = 0;
      bad.validate();
      throw new AssertionError("Invalid interval accepted");
    } catch (IllegalArgumentException expected) {
    }
    for (int rate : new int[] {44100, 48000})
      for (double f : new double[] {27.5, 110, 261.625565, 440, 880, 4186}) {
        short[] x = new short[8192];
        for (int i = 0; i < x.length; i++)
          x[i] = (short) (6000 * Math.sin(2 * Math.PI * f * i / rate));
        double p = Dsp.pitch(x, rate);
        require(
            p > 0 && Math.abs(p - f) < Math.max(.8, f * .006),
            "Pitch " + f + " at " + rate + ": " + p);
      }
    Dsp.Session session = new Dsp.Session();
    for (int i = 0; i < 3; i++) session.observe(440, i * 85);
    require(Math.abs(session.observe(0, 700) - 440) < .01, "Noise must not keep note open");
    for (int interval : new int[] {1, 2}) {
      double lower = 261.625565, upper = Model.hz(interval == 1 ? 67 : 72, 440) + 2;
      short[] x = new short[65536];
      for (int i = 0; i < x.length; i++)
        x[i] =
            (short)
                (6000 * Math.sin(2 * Math.PI * lower * i / 48000)
                    + 4000 * Math.sin(2 * Math.PI * upper * i / 48000));
      double[] result = Dsp.together(x, 48000, lower, interval);
      require(result != null && Math.abs(result[1] - upper) < .2, "Interval FFT");
    }
    require(
        !Updater.assetUrl("https://evil.example/Stroik.apk")
            && Updater.assetUrl(
                "https://github.com/proszczok2002/Stroik/releases/download/android-v1.01/Stroik.apk"),
        "Updater URL validation");
    Dsp.Taps taps = new Dsp.Taps();
    require(!taps.observe(.03, .008, 1000, 1.2), "Single tap");
    taps.observe(0, .008, 1200, 1.2);
    require(taps.observe(.025, .008, 1800, 1.2), "Double tap");
    log("DSP, profiles, interval and updater validation passed");
  }

  @Override
  public void onStart() {
    if (scenario.equals("prepare-update") || scenario.equals("verify-update")) {
      persistenceScenario();
      return;
    }
    if (scenario.equals("gestures")) {
      gestureScenario();
      return;
    }
    if (scenario.equals("audio")) {
      audioScenario();
      return;
    }
    if (scenario.equals("update")) {
      updateScenario();
      return;
    }
    Bundle result = new Bundle();
    String saved = null;
    try {
      automation = getUiAutomation(UiAutomation.FLAG_DONT_SUPPRESS_ACCESSIBILITY_SERVICES);
      automation.setOnAccessibilityEventListener(e -> {
        if (e.getPackageName() != null
            && e.getPackageName().toString().equals("pl.proszczok.stroik")) {
          if (e.getEventType() == AccessibilityEvent.TYPE_VIEW_ACCESSIBILITY_FOCUSED)
            focused.incrementAndGet();
          if (e.getEventType() == AccessibilityEvent.TYPE_VIEW_CLICKED) clicked.incrementAndGet();
        }
      });
      algorithms();
      Context target = getTargetContext();
      saved = target.getSharedPreferences("stroik", 0).getString("settings", null);
      Model base = new Model();
      base.updates = false;
      target
          .getSharedPreferences("stroik", 0)
          .edit()
          .putString("settings", base.json().toString())
          .commit();
      activity =
          (MainActivity)
              startActivitySync(
                  new Intent(target, MainActivity.class).addFlags(Intent.FLAG_ACTIVITY_NEW_TASK));
      waitForIdleSync();
      for (boolean english : new boolean[] {false, true}) {
        main(() -> {
          activity.model.english = english;
          invokeRender();
        });
        String settings = english ? "Settings" : "Ustawienia",
            tuner = english ? "Tuner" : "Stroik",
            instrument = english ? "Instrument" : "Instrument",
            help = english ? "Help" : "Pomoc";
        page(settings);
        inspect((english ? "EN" : "PL") + " settings");
        main(() ->
            findCheck(
                    english
                        ? "Beginner view with guidance"
                        : "Widok dla początkujących z podpowiedziami")
                .performClick());
        main(() ->
            findCheck(
                    english
                        ? "Automatically check for updates"
                        : "Automatycznie sprawdzaj aktualizacje")
                .performClick());
        main(() ->
            findCheck(
                    english
                        ? "Automatically check for updates"
                        : "Automatycznie sprawdzaj aktualizacje")
                .performClick());
        page(instrument);
        inspect((english ? "EN" : "PL") + " instrument");
        page(help);
        inspect((english ? "EN" : "PL") + " help");
        page(tuner);
        for (int method = 0; method < 4; method++) {
          final int at = method;
          main(() -> findSpinner(english ? "Tuning method" : "Metoda strojenia").setSelection(at));
          inspect((english ? "EN" : "PL") + " method " + method);
          require(
              findSpinner(english ? "Choose octave" : "Wybierz oktawę").getCount() == 9,
              "Octave selector missing");
          for (int interval = 1; interval <= 2; interval++) {
            final int chosen = interval;
            main(() ->
                findSpinner(english ? "Check consonance" : "Sprawdź współbrzmienie")
                    .setSelection(chosen));
            inspect("interval " + method + "/" + interval);
          }
        }
      }
      main(() -> {
        activity.model.dark = true;
        invokeRender();
      });
      inspect("Dark theme");
      main(() -> {
        activity.model.dark = false;
        activity.model.english = false;
        activity.model.method = 0;
        activity.model.interval = 0;
        invokeRender();
      });
      // Move actual accessibility focus to labelled controls. No service suppression.
      for (int i = 0; i < 5; i++) {
        final int index = i;
        main(() -> {
          List<View> controls = new ArrayList<>();
          for (View v : views()) if (v instanceof Spinner || v instanceof Button) controls.add(v);
          View v = controls.get(Math.min(index, controls.size() - 1));
          v.requestRectangleOnScreen(new Rect(0, 0, v.getWidth(), v.getHeight()), true);
          v.performAccessibilityAction(AccessibilityNodeInfo.ACTION_ACCESSIBILITY_FOCUS, null);
        });
        SystemClock.sleep(180);
      }
      log("Accessibility focus events=" + focused.get() + ", click events=" + clicked.get());
      require(focused.get() > 0, "No actual accessibility focus events received");
      require(clicked.get() > 0, "No checkbox/button click events received");
      log("Assertions=" + assertions);
      log(
          "TalkBack speech/gesture listening still requires a human listening check; this test"
              + " covers view labels, geometry, state and focus events.");
      result.putString("stream", report.toString());
      result.putString("result", "PASS");
      restoreSettings(saved);
      finish(Activity.RESULT_OK, result);
    } catch (Throwable e) {
      log("FAIL: " + e);
      result.putString("stream", report.toString());
      result.putString("result", "FAIL");
      restoreSettings(saved);
      finish(Activity.RESULT_CANCELED, result);
    }
  }

  private void persistenceScenario() {
    Bundle result = new Bundle();
    try {
      Context target = getTargetContext();
      android.content.SharedPreferences prefs = target.getSharedPreferences("stroik", 0);
      android.content.pm.PackageInfo info =
          target
              .getPackageManager()
              .getPackageInfo(
                  "pl.proszczok.stroik",
                  android.content.pm.PackageManager.GET_SIGNING_CERTIFICATES);
      String certificate =
          Updater.hex(
              java.security.MessageDigest.getInstance("SHA-256")
                  .digest(info.signingInfo.getApkContentsSigners()[0].toByteArray()));
      String canonical =
          Model.from(new org.json.JSONObject(prefs.getString("settings", "{}"))).json().toString();
      if (scenario.equals("prepare-update")) {
        require(info.getLongVersionCode() == 100, "Baseline must be installed 1.0");
        prefs
            .edit()
            .putString("updateBaseline", canonical)
            .putString("updateCertificate", certificate)
            .putString("updateTestMarker", "from-100-to-101")
            .commit();
        log("Prepared 1.0 settings and signing baseline for real system update");
      } else {
        require(
            info.getLongVersionCode() == 101 && "1.01".equals(info.versionName),
            "Expected Android 1.01 / 101");
        require(
            "from-100-to-101".equals(prefs.getString("updateTestMarker", "")),
            "App data marker was lost");
        require(
            canonical.equals(prefs.getString("updateBaseline", "")),
            "User settings changed during update");
        require(
            certificate.equals(prefs.getString("updateCertificate", "")),
            "Signing certificate changed");
        log(
            "Real update verified: version=1.01 code=101, same certificate, all model settings and"
                + " app data marker retained");
      }
      result.putString("stream", report.toString());
      result.putString("result", "PASS");
      finish(Activity.RESULT_OK, result);
    } catch (Throwable e) {
      log("FAIL: " + e);
      result.putString("stream", report.toString());
      result.putString("result", "FAIL");
      finish(Activity.RESULT_CANCELED, result);
    }
  }

  private void updateScenario() {
    Bundle result = new Bundle();
    try {
      activity =
          (MainActivity)
              startActivitySync(
                  new Intent(getTargetContext(), MainActivity.class)
                      .addFlags(Intent.FLAG_ACTIVITY_NEW_TASK));
      waitForIdleSync();
      java.lang.reflect.Field page = MainActivity.class.getDeclaredField("page");
      page.setAccessible(true);
      main(() -> {
        try {
          page.setInt(activity, 2);
        } catch (Exception e) {
          throw new IllegalStateException(e);
        }
        invokeRender();
        findButton(activity.model.english ? "Check for updates" : "Sprawdź aktualizacje")
            .performClick();
      });
      log("Opened real in-app updater. Android confirmation must be completed on the phone.");
      SystemClock.sleep(180000);
      result.putString("stream", report.toString());
      result.putString("result", "PROMPT_OPENED");
      finish(Activity.RESULT_OK, result);
    } catch (Throwable e) {
      log("FAIL: " + e);
      result.putString("stream", report.toString());
      result.putString("result", "FAIL");
      finish(Activity.RESULT_CANCELED, result);
    }
  }

  private void gestureScenario() {
    Bundle result = new Bundle();
    try {
      automation = getUiAutomation(UiAutomation.FLAG_DONT_SUPPRESS_ACCESSIBILITY_SERVICES);
      automation.setOnAccessibilityEventListener(event -> {
        if (event.getEventType() == AccessibilityEvent.TYPE_VIEW_ACCESSIBILITY_FOCUSED
            && event.getPackageName() != null
            && event.getPackageName().toString().equals("pl.proszczok.stroik")) {
          focused.incrementAndGet();
          log("TalkBack swipe focus: " + event.getText() + " / " + event.getContentDescription());
        }
      });
      activity =
          (MainActivity)
              startActivitySync(
                  new Intent(getTargetContext(), MainActivity.class)
                      .addFlags(Intent.FLAG_ACTIVITY_NEW_TASK));
      waitForIdleSync();
      SystemClock.sleep(15000);
      require(focused.get() >= 3, "TalkBack swipes did not generate enough focus changes");
      log(
          "TalkBack actual swipe navigation PASS; focus events="
              + focused.get()
              + ". Human pronunciation/listening remains separate.");
      result.putString("stream", report.toString());
      result.putString("result", "PASS");
      finish(Activity.RESULT_OK, result);
    } catch (Throwable e) {
      log("FAIL: " + e);
      result.putString("stream", report.toString());
      result.putString("result", "FAIL");
      finish(Activity.RESULT_CANCELED, result);
    }
  }

  private void audioScenario() {
    Bundle result = new Bundle();
    String saved = null;
    try {
      automation = getUiAutomation(UiAutomation.FLAG_DONT_SUPPRESS_ACCESSIBILITY_SERVICES);
      Context target = getTargetContext();
      saved = target.getSharedPreferences("stroik", 0).getString("settings", null);
      activity =
          (MainActivity)
              startActivitySync(
                  new Intent(target, MainActivity.class).addFlags(Intent.FLAG_ACTIVITY_NEW_TASK));
      waitForIdleSync();
      java.lang.reflect.Field audioField = MainActivity.class.getDeclaredField("audio");
      audioField.setAccessible(true);
      AudioEngine engine = (AudioEngine) audioField.get(activity);
      main(() -> {
        activity.model.method = 1;
        activity.model.autoFork = false;
        activity.model.updates = false;
        activity.model.target = 69;
        activity.model.interval = 0;
        activity.model.monitor = false;
        invokeRender();
        findButton(activity.model.english ? "Start microphone" : "Włącz mikrofon").performClick();
      });
      long deadline = SystemClock.elapsedRealtime() + 45000;
      while (!engine.running() && SystemClock.elapsedRealtime() < deadline) SystemClock.sleep(300);
      require(engine.running(), "Microphone permission/start not completed within 45 seconds");
      SystemClock.sleep(700);
      java.lang.reflect.Field recordField = AudioEngine.class.getDeclaredField("recorder");
      recordField.setAccessible(true);
      android.media.AudioRecord record = (android.media.AudioRecord) recordField.get(engine);
      require(
          record.getRecordingState() == android.media.AudioRecord.RECORDSTATE_RECORDING,
          "AudioRecord is not recording");
      log("Actual AudioRecord capture started with Android microphone permission");
      engine.strike();
      SystemClock.sleep(600);
      java.lang.reflect.Field forkField = AudioEngine.class.getDeclaredField("fork");
      forkField.setAccessible(true);
      android.media.AudioTrack fork = (android.media.AudioTrack) forkField.get(engine);
      require(
          fork != null && fork.getPlayState() == android.media.AudioTrack.PLAYSTATE_PLAYING,
          "Fork output did not start");
      log("Actual AudioTrack fork playback started");
      main(engine::stop);
      require(!engine.running() && recordField.get(engine) == null, "Stop left recording active");
      main(() -> {
        activity.model.method = 2;
        engine.configure(activity.model);
        findButton(activity.model.english ? "Start microphone" : "Włącz mikrofon").performClick();
      });
      SystemClock.sleep(500);
      require(engine.running(), "Speaker hybrid capture did not start");
      java.lang.reflect.Field mute = AudioEngine.class.getDeclaredField("muteUntil");
      mute.setAccessible(true);
      require(
          mute.getLong(engine) > SystemClock.elapsedRealtime(),
          "Speaker hybrid did not suppress its own reference");
      SystemClock.sleep(3500);
      require(
          engine.running() && mute.getLong(engine) <= SystemClock.elapsedRealtime(),
          "Speaker hybrid did not reopen listening");
      log("Speaker hybrid ignored reference and reopened listening");
      main(engine::stop);
      boolean headphones =
          AudioEngine.headphones(
              (android.media.AudioManager) activity.getSystemService(Context.AUDIO_SERVICE));
      log(
          "Headphones connected="
              + headphones
              + "; headphone monitoring physical test "
              + (headphones ? "available but not exercised" : "pending (no headphones)"));
      log(
          "Audio assertions="
              + assertions
              + "; instrument accuracy and human speech listening not established");
      restoreSettings(saved);
      result.putString("stream", report.toString());
      result.putString("result", "PASS");
      finish(Activity.RESULT_OK, result);
    } catch (Throwable e) {
      log("FAIL: " + e);
      restoreSettings(saved);
      result.putString("stream", report.toString());
      result.putString("result", "FAIL");
      finish(Activity.RESULT_CANCELED, result);
    }
  }

  private void restoreSettings(String saved) {
    if (activity == null) return;
    main(() -> {
      try {
        activity.model = Model.from(new org.json.JSONObject(saved == null ? "{}" : saved));
      } catch (Exception ignored) {
        activity.model = new Model();
      }
      android.content.SharedPreferences.Editor edit =
          activity.getSharedPreferences("stroik", 0).edit();
      if (saved == null) edit.remove("settings");
      else edit.putString("settings", saved);
      edit.commit();
      invokeRender();
    });
  }

  private void invokeRender() {
    try {
      java.lang.reflect.Method method = MainActivity.class.getDeclaredMethod("render");
      method.setAccessible(true);
      method.invoke(activity);
    } catch (Exception e) {
      throw new IllegalStateException(e);
    }
  }
}
