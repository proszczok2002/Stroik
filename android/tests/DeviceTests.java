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
  private int expectedCode = 105;
  private String expectedName = "1.05";
  private final AtomicInteger focused = new AtomicInteger(), clicked = new AtomicInteger();

  @Override
  public void onCreate(Bundle args) {
    super.onCreate(args);
    scenario = args == null ? "" : args.getString("scenario", "");
    if (args != null) {
      expectedCode = args.getInt("expectedCode", 105);
      expectedName = args.getString("expectedName", "1.05");
    }
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
      if (!v.isShown()) continue;
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
              (v.getContentDescription() != null && v.getContentDescription().length() > 0)
                  || (v instanceof EditText
                      && ((EditText) v).getHint() != null
                      && ((EditText) v).getHint().length() > 0),
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
    if (scenario.equals("focus")) {
      focusScenario();
      return;
    }
    if (scenario.equals("interaction")) {
      interactionScenario();
      return;
    }
    if (scenario.equals("fork")) {
      forkScenario();
      return;
    }
    if (scenario.equals("install-local")) {
      localInstallScenario();
      return;
    }
    if (scenario.equals("screen-off")) {
      screenOffScenario();
      return;
    }
    if (scenario.equals("state-events")) {
      stateEventScenario();
      return;
    }
    if (scenario.equals("lifecycle")) {
      lifecycleScenario();
      return;
    }
    if (scenario.equals("manifest") || scenario.equals("download")) {
      Bundle result = new Bundle();
      try {
        String manifest =
            new String(
                Updater.fetch(Updater.MANIFEST, 100000), java.nio.charset.StandardCharsets.UTF_8);
        org.json.JSONObject parsed = new org.json.JSONObject(manifest);
        require("pl.proszczok.stroik".equals(parsed.getString("packageName")), "Manifest package");
        log("Device updater manifest versionCode=" + parsed.getInt("versionCode"));
        log("Device updater manifest sha256=" + parsed.getString("sha256"));
        if (scenario.equals("download")) {
          byte[] apk = Updater.fetch(parsed.getString("apkUrl"), 40000000);
          String hash = Updater.hex(java.security.MessageDigest.getInstance("SHA-256").digest(apk));
          require(hash.equalsIgnoreCase(parsed.getString("sha256")), "Public download SHA-256 mismatch");
          java.io.File file = new java.io.File(getTargetContext().getCacheDir(), "verified-download.apk");
          try {
            try (java.io.OutputStream out = new java.io.FileOutputStream(file)) { out.write(apk); }
            android.content.pm.PackageManager pm = getTargetContext().getPackageManager();
            android.content.pm.PackageInfo archive = pm.getPackageArchiveInfo(file.getAbsolutePath(), android.content.pm.PackageManager.GET_SIGNING_CERTIFICATES);
            android.content.pm.PackageInfo installed = pm.getPackageInfo(getTargetContext().getPackageName(), android.content.pm.PackageManager.GET_SIGNING_CERTIFICATES);
            require(archive != null && archive.packageName.equals(parsed.getString("packageName"))
                && archive.getLongVersionCode() == parsed.getInt("versionCode")
                && Updater.sameSignatures(installed, archive), "Downloaded package/version/certificate mismatch");
          } finally { file.delete(); }
          log("Production fetch on phone verified public APK bytes=" + apk.length + ", SHA-256=" + hash);
          log("Download verification only; no higher-version update or installation was simulated.");
        }
        result.putString("stream", report.toString());
        result.putString("result", "PASS");
        finish(Activity.RESULT_OK, result);
      } catch (Throwable error) {
        result.putString("stream", "Manifest probe failed: " + error);
        finish(Activity.RESULT_CANCELED, result);
      }
      return;
    }
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
          activity.model.instrument = 0;
          activity.model.method = 0;
          activity.model.interval = 0;
          activity.model.target = 67;
          activity.model.forkTap = false;
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
        page(tuner);
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

  private void interactionScenario() {
    Bundle result = new Bundle();
    String saved = null;
    try {
      automation = getUiAutomation(UiAutomation.FLAG_DONT_SUPPRESS_ACCESSIBILITY_SERVICES);
      Context context = getTargetContext();
      saved = context.getSharedPreferences("stroik", 0).getString("settings", null);
      activity =
          (MainActivity)
              startActivitySync(
                  new Intent(context, MainActivity.class).addFlags(Intent.FLAG_ACTIVITY_NEW_TASK));
      waitForIdleSync();
      for (boolean english : new boolean[] {false, true}) {
        main(() -> {
          activity.model.english = english;
          activity.model.updates = false;
          activity.model.method = 1;
          activity.model.instrument = 0;
          invokeRender();
        });
        page(english ? "Tuner" : "Stroik");
        List<Button> micButtons = new ArrayList<>();
        SeekBar reference = null;
        for (View view : views()) {
          if (view instanceof Button
              && (((Button) view)
                      .getText()
                      .toString()
                      .equals(english ? "Start microphone" : "Włącz mikrofon")
                  || ((Button) view)
                      .getText()
                      .toString()
                      .equals(english ? "Stop microphone" : "Wyłącz mikrofon")))
            micButtons.add((Button) view);
          if (view instanceof SeekBar) reference = (SeekBar) view;
          if (view instanceof Spinner && view.isShown()) {
            Spinner list = (Spinner) view;
            require(
                list.createAccessibilityNodeInfo()
                    .getContentDescription()
                    .toString()
                    .contains(list.getSelectedItem().toString()),
                "Collapsed list omits current value");
          }
        }
        require(micButtons.size() == 1, "Microphone is not one toggle button");
        require(micButtons.get(0).getStateDescription() != null, "Microphone state is unnamed");
        require(reference != null, "Missing frequency slider");
        SeekBar slider = reference;
        Bundle value = new Bundle();
        value.putFloat(AccessibilityNodeInfo.ACTION_ARGUMENT_PROGRESS_VALUE, 435.2f);
        main(() ->
            require(
                slider.performAccessibilityAction(
                    AccessibilityNodeInfo.AccessibilityAction.ACTION_SET_PROGRESS.getId(), value),
                "Slider rejects Hz action"));
        require(Math.abs(activity.model.reference - 435.2) < .01, "Slider progress is not in Hz");
        Bundle invalid = new Bundle();
        invalid.putFloat(AccessibilityNodeInfo.ACTION_ARGUMENT_PROGRESS_VALUE, 900);
        main(() ->
            require(
                !slider.performAccessibilityAction(
                    AccessibilityNodeInfo.AccessibilityAction.ACTION_SET_PROGRESS.getId(), invalid),
                "Slider accepted invalid Hz"));
        main(() ->
            slider.performAccessibilityAction(AccessibilityNodeInfo.ACTION_SCROLL_FORWARD, null));
        require(
            Math.abs(activity.model.reference - 435.3) < .01, "Slider adjustment is not 0.1 Hz");
        page(english ? "Settings" : "Ustawienia");
        CheckBox awake =
            findCheck(english ? "Keep screen awake" : "Nie pozwalaj na autoblokadę ekranu");
        main(() -> {
          if (!awake.isChecked()) awake.performClick();
        });
        require(
            (activity.getWindow().getAttributes().flags
                    & WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON)
                != 0,
            "Keep-screen flag missing");
        main(awake::performClick);
        require(
            (activity.getWindow().getAttributes().flags
                    & WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON)
                == 0,
            "Keep-screen flag remains");
        page(english ? "Tuner" : "Stroik");
        main(() -> {
          activity.model.method = 0;
          activity.model.target = 67;
          activity.model.order = 0;
          invokeRender();
          activity.onKeyDown(
              KeyEvent.KEYCODE_N,
              new KeyEvent(
                  0, 0, KeyEvent.ACTION_DOWN, KeyEvent.KEYCODE_N, 0, KeyEvent.META_ALT_ON));
        });
        require(activity.model.target == 68, "Alt+N fork navigation failed");
        List<Button> tabs = new ArrayList<>();
        for (View view : views())
          if (view instanceof Button && view.getId() >= 10000 && view.getId() <= 10002)
            tabs.add((Button) view);
        require(tabs.size() == 3, "Expected three bottom tabs");
        require(
            tabs.get(0).getParent() == tabs.get(1).getParent()
                && tabs.get(1).getParent() == tabs.get(2).getParent(),
            "Tabs use different rows");
        for (int method = 0; method < 4; method++) {
          int choice = method;
          main(() ->
              findSpinner(english ? "Tuning method" : "Metoda strojenia").setSelection(choice));
          require(
              findSpinner(english ? "Check consonance" : "Sprawdź współbrzmienie").isShown(),
              "Consonance disappeared");
        }
      }
      log(
          "Interaction assertions="
              + assertions
              + "; slider/list/toggle/tab/shortcut checks PASS. Screen-off capture and speech need"
              + " a separate physical test.");
      restoreSettings(saved);
      result.putString("result", "PASS");
    } catch (Throwable error) {
      log("FAIL: " + error);
      restoreSettings(saved);
      result.putString("result", "FAIL");
    }
    result.putString("stream", report.toString());
    finish(Activity.RESULT_OK, result);
  }

  private void focus(View view) {
    main(() -> {
      view.requestRectangleOnScreen(new Rect(0, 0, view.getWidth(), view.getHeight()), true);
      if (!hasAccessibilityFocus(view))
        view.performAccessibilityAction(AccessibilityNodeInfo.ACTION_ACCESSIBILITY_FOCUS, null);
    });
    SystemClock.sleep(150);
    if (!hasAccessibilityFocus(view)) logActualFocus();
    require(hasAccessibilityFocus(view), "Cannot focus " + view.getClass().getSimpleName());
  }

  private boolean hasAccessibilityFocus(View view) {
    if (view.isAccessibilityFocused()) return true;
    if (view instanceof ViewGroup)
      for (int i = 0; i < ((ViewGroup) view).getChildCount(); i++)
        if (hasAccessibilityFocus(((ViewGroup) view).getChildAt(i))) return true;
    return false;
  }

  private void logActualFocus() {
    AccessibilityNodeInfo root = automation.getRootInActiveWindow();
    AccessibilityNodeInfo focused =
        root == null ? null : root.findFocus(AccessibilityNodeInfo.FOCUS_ACCESSIBILITY);
    log("Actual focus=" + focused);
    for (View view : views())
      if (view.isAccessibilityFocused())
        log(
            "Focused view="
                + view.getClass().getSimpleName()
                + " id="
                + view.getId()
                + " desc="
                + view.getContentDescription());
  }

  private void keepsFocus(View view, Runnable action, String label) {
    focus(view);
    main(action);
    SystemClock.sleep(250);
    require(view.isAttachedToWindow(), label + ": focused control was replaced");
    if (!hasAccessibilityFocus(view)) logActualFocus();
    require(hasAccessibilityFocus(view), label + ": accessibility focus moved");
    log(label + ": same control and accessibility focus retained");
  }

  private void focusScenario() {
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
      main(() -> {
        activity.model.updates = false;
        activity.model.instrument = 0;
        activity.model.method = 0;
        activity.model.autoFork = false;
        invokeRender();
      });
      // TalkBack completes its initial window announcement asynchronously after Activity idle.
      SystemClock.sleep(1400);
      for (boolean english : new boolean[] {false, true}) {
        main(() -> {
          activity.model.english = english;
          activity.model.instrument = 0;
          activity.model.method = 0;
          activity.model.interval = 0;
          activity.model.target = 67;
          activity.model.forkTap = false;
          invokeRender();
        });
        page(english ? "Tuner" : "Stroik");
        Spinner method = findSpinner(english ? "Tuning method" : "Metoda strojenia");
        for (int at : new int[] {1, 2, 3, 0}) {
          keepsFocus(method, () -> method.setSelection(at), "Method " + at);
          require(
              findSpinner(english ? "Tuning method" : "Metoda strojenia") == method,
              "Method spinner identity changed");
        }
        Spinner interval = findSpinner(english ? "Check consonance" : "Sprawdź współbrzmienie");
        for (int at : new int[] {1, 2, 0})
          keepsFocus(interval, () -> interval.setSelection(at), "Interval " + at);
        CheckBox taps =
            findCheck(
                english
                    ? "Two strikes advance to the next note in the selected order"
                    : "Dwukrotne stuknięcie przechodzi do kolejnego dźwięku zgodnego z wybraną"
                        + " kolejnością");
        keepsFocus(taps, taps::performClick, "Tap listening enabled");
        keepsFocus(taps, taps::performClick, "Tap listening disabled");
        Spinner note = findSpinner(english ? "Target note" : "Dźwięk docelowy");
        keepsFocus(
            note,
            () ->
                note.setSelection(
                    Math.min(note.getCount() - 1, note.getSelectedItemPosition() + 13)),
            "Target across octave");
        Spinner octave = findSpinner(english ? "Choose octave" : "Wybierz oktawę");
        keepsFocus(
            octave,
            () -> octave.setSelection((octave.getSelectedItemPosition() + 1) % octave.getCount()),
            "Octave and target sync");
        keepsFocus(
            octave,
            () -> activity.result(english ? "Focus test result" : "Wynik testu fokusu"),
            "Result announcement");
        main(() -> method.setSelection(1));
        int[] options = (int[]) note.getTag();
        int lastNote = -1;
        for (int i = 0; i < options.length; i++) if (options[i] == 71) lastNote = i;
        require(lastNote >= 0, "Missing boundary note for advance test");
        final int boundaryNote = lastNote;
        main(() -> {
          activity.model.order = 0;
          note.setSelection(boundaryNote);
        });
        int anchor = activity.model.anchor;
        Button next = findButton(english ? "Next note" : "Następny dźwięk");
        keepsFocus(next, next::performClick, "Next note across octave");
        require(activity.model.target == 72, "Next note did not cross octave");
        require(
            activity.model.anchor == anchor,
            "Programmatic selection reset the tuning order anchor");
        require(octave.getSelectedItemPosition() == 5, "Octave list did not follow next note");
        String[] pages =
            english
                ? new String[] {"Settings", "Help", "Tuner"}
                : new String[] {"Ustawienia", "Pomoc", "Stroik"};
        for (String name : pages) {
          Button tab = findButton(name);
          keepsFocus(tab, tab::performClick, "Tab " + name);
          require(tab.isSelected(), "Tab selected state missing");
          keepsFocus(tab, tab::performClick, "Selected tab " + name);
        }
        page(english ? "Tuner" : "Stroik");
        Spinner instrument = findSpinner("Instrument");
        for (int at : new int[] {2, 1, 0, 5, 2})
          keepsFocus(instrument, () -> instrument.setSelection(at), "Instrument " + at);
        Spinner ukulele = findSpinner(english ? "Ukulele tuning" : "Strój ukulele");
        keepsFocus(
            ukulele,
            () -> ukulele.setSelection(1 - ukulele.getSelectedItemPosition()),
            "Ukulele tuning");
      }
      page("Settings");
      Spinner language = findSpinner("Language");
      int languageId = language.getId();
      focus(language);
      main(() -> language.setSelection(0));
      SystemClock.sleep(300);
      Spinner polish = findSpinner("Język");
      require(
          polish.getId() == languageId && polish.isAccessibilityFocused(),
          "Language replacement lost semantic focus");
      CheckBox dark = findCheck("Ciemny motyw");
      focus(dark);
      main(dark::performClick);
      SystemClock.sleep(400);
      require(
          findCheck("Ciemny motyw") == dark && dark.isAccessibilityFocused(),
          "Theme change replaced checkbox or lost focus");
      main(dark::performClick);
      require(dark.isAccessibilityFocused(), "Returning theme lost checkbox focus");
      for (View view : views())
        if (view instanceof EditText && view.isShown()) {
          EditText edit = (EditText) view;
          require(
              edit.getContentDescription() == null,
              "Editable value is masked by contentDescription");
          require(
              edit.getHint() != null && edit.getHint().length() > 0, "Numeric field has no hint");
          require(
              edit.createAccessibilityNodeInfo().isEditable(),
              "Numeric field lost editable semantics");
        }
      log("Focus assertions=" + assertions + "; PL/EN focus retention and editable labels PASS");
      restoreSettings(saved);
      result.putString("stream", report.toString());
      result.putString("result", "PASS");
      finish(Activity.RESULT_OK, result);
    } catch (Throwable error) {
      log("FAIL: " + error);
      restoreSettings(saved);
      result.putString("stream", report.toString());
      result.putString("result", "FAIL");
      finish(Activity.RESULT_CANCELED, result);
    }
  }

  private void recreateAndWait() {
    replaceAndWait(() -> activity.recreate());
  }

  private void replaceAndWait(Runnable action) {
    ActivityMonitor monitor = addMonitor(MainActivity.class.getName(), null, false);
    try {
      main(action);
      Activity replacement = waitForMonitorWithTimeout(monitor, 10000);
      require(replacement instanceof MainActivity, "Recreated activity did not start");
      activity = (MainActivity) replacement;
      waitForIdleSync();
      require(!activity.isFinishing() && !activity.isDestroyed(), "Recreated activity closed");
    } finally {
      removeMonitor(monitor);
    }
  }

  private void lifecycleScenario() {
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
      main(() -> activity.model.updates = false);
      page(activity.model.english ? "Settings" : "Ustawienia");
      log("Recreating Settings: original releases crash here with incompatible widget SavedState");
      recreateAndWait();
      require(
          findSpinner(activity.model.english ? "Language" : "Język") != null,
          "Settings page was lost");
      android.util.SparseArray<Parcelable> legacy = new android.util.SparseArray<>();
      main(() -> {
        ViewGroup content = activity.findViewById(android.R.id.content);
        View dynamicRoot = content.getChildAt(0);
        dynamicRoot.setSaveFromParentEnabled(true);
        content.saveHierarchyState(legacy);
        dynamicRoot.setSaveFromParentEnabled(false);
      });
      require(legacy.size() > 0, "Legacy hierarchy test did not save widget state");
      page(activity.model.english ? "Tuner" : "Stroik");
      main(() -> {
        ViewGroup content = activity.findViewById(android.R.id.content);
        content.restoreHierarchyState(legacy);
      });
      require(
          findSpinner(activity.model.english ? "Tuning method" : "Metoda strojenia") != null,
          "Legacy hierarchy damaged the tuner");
      for (boolean english : new boolean[] {false, true}) {
        main(() -> {
          activity.model.english = english;
          activity.model.updates = false;
          activity.model.dark = false;
          invokeRender();
        });
        String[] pages =
            english
                ? new String[] {"Tuner", "Settings", "Help"}
                : new String[] {"Stroik", "Ustawienia", "Pomoc"};
        for (int cycle = 0; cycle < 3; cycle++)
          for (int at = 0; at < pages.length; at++) {
            page(pages[at]);
            final int chosenPage = at;
            String before = activity.model.json().toString();
            recreateAndWait();
            java.lang.reflect.Field pageField = MainActivity.class.getDeclaredField("page");
            pageField.setAccessible(true);
            require(pageField.getInt(activity) == chosenPage, "Page changed after recreation");
            require(
                before.equals(activity.model.json().toString()),
                "Settings changed after recreation");
            inspect((english ? "EN" : "PL") + " recreation " + cycle + "/" + at);
          }
        page(english ? "Settings" : "Ustawienia");
        main(() -> findCheck(english ? "Dark theme" : "Ciemny motyw").performClick());
        recreateAndWait();
        require(activity.model.dark, "Dark theme did not survive recreation");
        main(() -> findCheck(english ? "Dark theme" : "Ciemny motyw").performClick());
        recreateAndWait();
        require(!activity.model.dark, "Light theme did not survive recreation");
      }
      log(
          "Lifecycle assertions="
              + assertions
              + "; repeated page recreation and both theme changes PASS");
      result.putString("result", "PASS");
      restoreSettings(saved);
      result.putString("stream", report.toString());
      finish(Activity.RESULT_OK, result);
    } catch (Throwable error) {
      log("FAIL: " + error);
      restoreSettings(saved);
      result.putString("stream", report.toString());
      result.putString("result", "FAIL");
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
        require(
            info.getLongVersionCode() <= expectedCode, "Baseline cannot be newer than the candidate");
        prefs
            .edit()
            .putString("updateBaseline", canonical)
            .putString("updateCertificate", certificate)
            .putString(
                "updateTestMarker", "from-" + info.getLongVersionCode() + "-to-" + expectedCode)
            .commit();
        log(
            "Prepared version "
                + info.versionName
                + " settings/signing baseline for "
                + expectedName);
      } else {
        require(
            info.getLongVersionCode() == expectedCode && expectedName.equals(info.versionName),
            "Expected Android " + expectedName + " / " + expectedCode);
        require(
            prefs.getString("updateTestMarker", "").endsWith("-to-" + expectedCode),
            "App data marker was lost");
        require(
            canonical.equals(prefs.getString("updateBaseline", "")),
            "User settings changed during update");
        require(
            certificate.equals(prefs.getString("updateCertificate", "")),
            "Signing certificate changed");
        log(
            "System install verified: version="
                + expectedName
                + " code="
                + expectedCode
                + ", same certificate, all model settings and app data marker retained");
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
          page.setInt(activity, 1);
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

  private void localInstallScenario() {
    Bundle output = new Bundle();
    try {
      activity = (MainActivity) startActivitySync(new Intent(getTargetContext(), MainActivity.class).addFlags(Intent.FLAG_ACTIVITY_NEW_TASK));
      java.io.File candidate = new java.io.File(activity.getCacheDir(), "stroik-update.apk");
      try (java.io.InputStream in = getContext().getAssets().open("candidate.apk");
          java.io.OutputStream out = new java.io.FileOutputStream(candidate)) { in.transferTo(out); }
      android.content.pm.PackageManager pm = activity.getPackageManager();
      android.content.pm.PackageInfo archive = pm.getPackageArchiveInfo(candidate.getAbsolutePath(), android.content.pm.PackageManager.GET_SIGNING_CERTIFICATES);
      android.content.pm.PackageInfo installed = pm.getPackageInfo(activity.getPackageName(), android.content.pm.PackageManager.GET_SIGNING_CERTIFICATES);
      require(archive != null && activity.getPackageName().equals(archive.packageName)
          && Updater.sameSignatures(installed, archive), "Local candidate package/signature mismatch");
      log("Preparing local candidate for SYSTEM confirmation only; not a GitHub updater round trip.");
      if (!pm.canRequestPackageInstalls()) {
        main(() -> activity.startActivity(new Intent(android.provider.Settings.ACTION_MANAGE_UNKNOWN_APP_SOURCES,
            android.net.Uri.parse("package:" + activity.getPackageName()))));
        output.putString("stream", report + "Allow Stroik as an installation source in the visible system screen, then rerun install-local.\n");
        finish(Activity.RESULT_OK, output);
        return;
      }
      main(() -> {
        try {
          java.lang.reflect.Method install = Updater.class.getDeclaredMethod("install", java.io.File.class);
          install.setAccessible(true);
          install.invoke(new Updater(activity), candidate);
        } catch (Exception error) { throw new IllegalStateException(error); }
      });
      log("System installer opened; waiting for its confirmation and package replacement.");
      SystemClock.sleep(180000);
      output.putString("stream", report.toString());
      finish(Activity.RESULT_OK, output);
    } catch (Throwable error) {
      output.putString("stream", report + "FAIL: " + error + "\n");
      finish(Activity.RESULT_CANCELED, output);
    }
  }

  private void gestureScenario() {
    Bundle result = new Bundle();
    Set<String> stops = java.util.concurrent.ConcurrentHashMap.newKeySet();
    try {
      automation = getUiAutomation(UiAutomation.FLAG_DONT_SUPPRESS_ACCESSIBILITY_SERVICES);
      automation.setOnAccessibilityEventListener(event -> {
        if (event.getEventType() == AccessibilityEvent.TYPE_VIEW_ACCESSIBILITY_FOCUSED
            && event.getPackageName() != null
            && event.getPackageName().toString().equals("pl.proszczok.stroik")) {
          focused.incrementAndGet();
          stops.add(event.getText() + " / " + event.getContentDescription());
          log("TalkBack swipe focus: " + event.getText() + " / " + event.getContentDescription());
        }
      });
      activity =
          (MainActivity)
              startActivitySync(
                  new Intent(getTargetContext(), MainActivity.class)
                      .addFlags(Intent.FLAG_ACTIVITY_NEW_TASK));
      waitForIdleSync();
      SystemClock.sleep(60000);
      require(focused.get() >= 3, "TalkBack swipes did not generate enough focus changes");
      require(stops.size() >= 3, "TalkBack navigation cycles between fewer than three distinct stops: " + stops);
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

  private void forkScenario() {
    Bundle output = new Bundle();
    String saved = null;
    try {
      automation = getUiAutomation(UiAutomation.FLAG_DONT_SUPPRESS_ACCESSIBILITY_SERVICES);
      Context context = getTargetContext();
      saved = context.getSharedPreferences("stroik", 0).getString("settings", null);
      activity = (MainActivity) startActivitySync(new Intent(context, MainActivity.class).addFlags(Intent.FLAG_ACTIVITY_NEW_TASK));
      java.lang.reflect.Field trackField = AudioEngine.class.getDeclaredField("fork");
      trackField.setAccessible(true);
      java.lang.reflect.Field configField = AudioEngine.class.getDeclaredField("config");
      configField.setAccessible(true);
      AudioEngine engine = TuningSession.get(activity).audio;
      for (boolean english : new boolean[] {false, true}) {
        main(() -> {
          TuningSession.get(activity).stop();
          activity.model = new Model();
          activity.model.english = english;
          activity.model.updates = false;
          activity.model.target = activity.model.anchor = 60;
          invokeRender();
          TuningSession.get(activity).configure(activity.model);
        });
        page(english ? "Tuner" : "Stroik");
        SystemClock.sleep(1400);
        log("Fork initial strike, language=" + english);
        main(() -> findButton(english ? "Strike tuning fork" : "Uderz kamerton").performClick());
        android.media.AudioTrack previous = awaitTrack(engine, trackField, null);
        Spinner consonance = findSpinner(english ? "Check consonance" : "Sprawdź współbrzmienie");
        for (int mode : new int[] {1, 2, 0}) {
          log("Fork consonance change=" + mode);
          main(() -> consonance.setSelection(mode));
          previous = awaitTrack(engine, trackField, previous);
          Model actual = (Model) configField.get(engine);
          require(actual.base() == 60 && actual.interval == mode, "Wrong actual playback configuration");
          if (mode != 0) {
            double lower = Model.hz(actual.base(), actual.reference);
            double upper = Model.hz(actual.base() + actual.semitones(), actual.reference);
            double[] pair = Dsp.together(Arrays.copyOf(Dsp.fork(lower, upper, 44100), 65536), 44100, lower, mode);
            require(pair != null && Math.abs(pair[0] - lower) < .15 && Math.abs(pair[1] - upper) < .15,
                "Generator lacks both components after UI configuration");
          }
          require(consonance.isAttachedToWindow(), "Consonance selector replaced");
        }
        // Dispatch through the Activity before Spinner consumes its native input keys.
        Spinner notes = findSpinner(english ? "Target note" : "Dźwięk docelowy");
        main(() -> {
          // Synthetic dispatch does not switch Android out of touch mode as a
          // physical keyboard does. Establish real input focus explicitly.
          notes.setFocusableInTouchMode(true);
          require(notes.requestFocus(), "Cannot focus note list for synthetic keyboard test");
        });
        for (int expected : new int[] {62, 64, 65, 67, 69, 71, 72}) {
          log("Fork key expected=" + expected);
          main(() -> activity.dispatchKeyEvent(new KeyEvent(KeyEvent.ACTION_DOWN, KeyEvent.KEYCODE_DPAD_DOWN)));
          previous = awaitTrack(engine, trackField, previous);
          require(activity.model.target == expected, "Native key dispatch skipped a white key");
        }
        CheckBox automatic = findCheck(english ? "Strike tuning fork automatically" : "Uderzaj kamerton automatycznie");
        main(() -> { if (!automatic.isChecked()) automatic.performClick(); });
        main(() -> {
          notes.setSelection(108 - 21 + 1);
          consonance.setSelection(1);
        });
        SystemClock.sleep(700);
        java.lang.reflect.Field paused = AudioEngine.class.getDeclaredField("paused");
        paused.setAccessible(true);
        require(paused.getBoolean(engine), "Out-of-range interval did not pause automatic fork");
        require(trackField.get(engine) == null, "Out-of-range interval retained old playback");
        Model previousConfig = ((Model) configField.get(engine)).copy();
        java.lang.reflect.Field generation = AudioEngine.class.getDeclaredField("generation");
        generation.setAccessible(true);
        int before = generation.getInt(engine);
        previousConfig.graphics = !previousConfig.graphics;
        main(() -> engine.configure(previousConfig));
        require(generation.getInt(engine) == before, "Appearance change reset acoustic measurement");
      }
      log("Fork UI refresh, real AudioTrack replacement, expected pair synthesis, dispatch keys and range-pause assertions=" + assertions);
      log("AudioTrack/generator checks do not establish speaker frequency response or human listening.");
      output.putString("result", "PASS");
    } catch (Throwable error) {
      log("FAIL: " + error);
      output.putString("result", "FAIL");
    } finally { restoreSettings(saved); }
    output.putString("stream", report.toString());
    finish("PASS".equals(output.getString("result")) ? Activity.RESULT_OK : Activity.RESULT_CANCELED, output);
  }

  private android.media.AudioTrack awaitTrack(AudioEngine engine, java.lang.reflect.Field field, android.media.AudioTrack previous) throws Exception {
    long deadline = SystemClock.elapsedRealtime() + 5000;
    do {
      android.media.AudioTrack current = (android.media.AudioTrack) field.get(engine);
      if (current != null && current != previous && current.getPlayState() == android.media.AudioTrack.PLAYSTATE_PLAYING) {
        require(current.getSampleRate() == 44100, "Unexpected fork playback sample rate");
        return current;
      }
      SystemClock.sleep(80);
    } while (SystemClock.elapsedRealtime() < deadline);
    Model state = TuningSession.get(activity).model;
    throw new AssertionError("Changed setting did not replace actual AudioTrack; engine=" + field.get(engine)
        + ", activity active=" + activity.active + ", target=" + state.target + ", interval=" + state.interval);
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
      main(() -> TuningSession.get(activity).stop());
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
      main(() -> TuningSession.get(activity).stop());
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

  private void screenOffScenario() {
    Bundle output = new Bundle();
    String saved = null;
    try {
      automation = getUiAutomation(UiAutomation.FLAG_DONT_SUPPRESS_ACCESSIBILITY_SERVICES);
      Context context = getTargetContext();
      saved = context.getSharedPreferences("stroik", 0).getString("settings", null);
      activity = (MainActivity) startActivitySync(new Intent(context, MainActivity.class).addFlags(Intent.FLAG_ACTIVITY_NEW_TASK));
      TuningSession session = TuningSession.get(activity);
      main(() -> {
        session.stop();
        activity.model.method = 1;
        activity.model.updates = false;
        activity.model.graphics = true;
        activity.model.voiceInVisual = false;
        activity.model.keepScreenOn = false;
        activity.model.autoFork = false;
        session.configure(activity.model);
        session.start();
      });
      long deadline = SystemClock.elapsedRealtime() + 10000;
      while (!session.audio.running() && SystemClock.elapsedRealtime() < deadline) SystemClock.sleep(100);
      require(session.audio.running() && session.serviceActive, "Foreground capture did not start");
      try (ParcelFileDescriptor command = automation.executeShellCommand("input keyevent 223")) {
        try (java.io.InputStream in = new ParcelFileDescriptor.AutoCloseInputStream(command)) { in.readAllBytes(); }
      }
      SystemClock.sleep(2500);
      require(session.screenOff(), "Physical screen did not switch off");
      require(session.audio.running() && session.serviceActive, "Screen off stopped capture");
      java.lang.reflect.Field record = AudioEngine.class.getDeclaredField("recorder");
      record.setAccessible(true);
      android.media.AudioRecord microphone = (android.media.AudioRecord) record.get(session.audio);
      require(microphone != null && microphone.getRecordingState() == android.media.AudioRecord.RECORDSTATE_RECORDING,
          "Screen-off AudioRecord is not recording");
      java.lang.reflect.Field speech = TuningSession.class.getDeclaredField("speech");
      speech.setAccessible(true);
      android.speech.tts.TextToSpeech tts = (android.speech.tts.TextToSpeech) speech.get(session);
      main(() -> session.result(activity.t("Stroik. Próba odczytu przy wygaszonym ekranie. g razkreślne. Mikrofon nadal działa.",
          "Stroik. Screen-off spoken feedback test. G in the one-line octave. Microphone is still running.")));
      boolean speaking = false;
      deadline = SystemClock.elapsedRealtime() + 5000;
      while (!speaking && SystemClock.elapsedRealtime() < deadline) { speaking = tts.isSpeaking(); SystemClock.sleep(50); }
      require(speaking, "System TTS did not start with screen off and visual voice disabled");
      main(session::stop);
      SystemClock.sleep(500);
      require(!session.audio.running() && !session.serviceActive && record.get(session.audio) == null,
          "Stop did not release screen-off capture");
      log("Screen-off assertions=" + assertions + "; actual power off, foreground AudioRecord, TTS dispatch despite visual-voice mute and recording release PASS.");
      log("TTS isSpeaking is not human pronunciation/listening or instrument accuracy.");
      output.putString("result", "PASS");
    } catch (Throwable error) { log("FAIL: " + error); output.putString("result", "FAIL"); }
    finally {
      try (ParcelFileDescriptor command = automation.executeShellCommand("input keyevent 224")) {
        try (java.io.InputStream in = new ParcelFileDescriptor.AutoCloseInputStream(command)) { in.readAllBytes(); }
      } catch (Exception ignored) { }
      restoreSettings(saved);
    }
    output.putString("stream", report.toString());
    finish(Activity.RESULT_OK, output);
  }

  private void stateEventScenario() {
    Bundle output = new Bundle();
    String saved = null;
    java.util.concurrent.CopyOnWriteArrayList<String> announcements = new java.util.concurrent.CopyOnWriteArrayList<>();
    try {
      automation = getUiAutomation(UiAutomation.FLAG_DONT_SUPPRESS_ACCESSIBILITY_SERVICES);
      automation.setOnAccessibilityEventListener(event -> {
        if (event.getEventType() == AccessibilityEvent.TYPE_ANNOUNCEMENT
            && "pl.proszczok.stroik".contentEquals(event.getPackageName()))
          announcements.add(event.getText().toString());
      });
      Context context = getTargetContext();
      saved = context.getSharedPreferences("stroik", 0).getString("settings", null);
      activity = (MainActivity) startActivitySync(new Intent(context, MainActivity.class).addFlags(Intent.FLAG_ACTIVITY_NEW_TASK));
      TuningSession session = TuningSession.get(activity);
      for (boolean english : new boolean[] {false, true}) {
        main(() -> {
          session.stop();
          activity.model = new Model();
          activity.model.english = english;
          activity.model.updates = false;
          activity.model.method = 1;
          activity.model.graphics = true;
          activity.model.voiceInVisual = false;
          session.configure(activity.model);
          invokeRender();
        });
        page(english ? "Tuner" : "Stroik");
        SystemClock.sleep(1400);
        Button mic = findButton(english ? "Start microphone" : "Włącz mikrofon");
        announcements.clear();
        main(mic::performClick);
        long deadline = SystemClock.elapsedRealtime() + 7000;
        while (!session.audio.running() && SystemClock.elapsedRealtime() < deadline) SystemClock.sleep(100);
        require(session.audio.running(), "State-event microphone did not start");
        require((english ? "Microphone on" : "Mikrofon włączony").contentEquals(mic.getStateDescription()), "Microphone on state missing");
        main(mic::performClick);
        SystemClock.sleep(500);
        String off = english ? "Microphone off" : "Mikrofon wyłączony";
        require(!session.audio.running() && off.contentEquals(mic.getStateDescription()), "Microphone off state missing");
        require(announcements.toString().contains(off), "No immediate microphone off accessibility announcement");
        main(() -> activity.result("suppressed automatic reading"));
        SystemClock.sleep(200);
        require(!announcements.toString().contains("suppressed automatic reading"), "Visual voice option did not suppress automatic reading");
        announcements.clear();
        main(() -> findButton(english ? "Next note" : "Następny dźwięk").performClick());
        SystemClock.sleep(300);
        String note = activity.model.spokenNote(activity.model.base());
        require(announcements.toString().contains(note), "Next note omitted its concise announcement");
        require(!announcements.toString().contains(english ? "Next note" : "Następny dźwięk"), "App announcement includes button label");
      }
      log("State/voice event assertions=" + assertions + "; explicit microphone off and concise Next announcements with muted visual results PASS.");
      output.putString("result", "PASS");
    } catch (Throwable error) { log("FAIL: " + error); output.putString("result", "FAIL"); }
    finally { restoreSettings(saved); }
    output.putString("stream", report.toString());
    finish(Activity.RESULT_OK, output);
  }

  private void restoreSettings(String saved) {
    if (activity == null) return;
    main(() -> {
      TuningSession.get(activity).stop();
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
