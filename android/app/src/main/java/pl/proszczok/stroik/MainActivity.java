package pl.proszczok.stroik;

import android.Manifest;
import android.app.*;
import android.content.*;
import android.content.pm.PackageManager;
import android.graphics.*;
import android.media.AudioManager;
import android.net.Uri;
import android.os.*;
import android.view.*;
import android.view.accessibility.AccessibilityManager;
import android.widget.*;
import java.io.*;
import java.nio.charset.StandardCharsets;
import java.util.*;
import java.util.function.*;
import org.json.JSONObject;

public final class MainActivity extends Activity implements AudioEngine.Listener {
  Model model;
  boolean active;
  private LinearLayout root, body, nav;
  private TextView heading, result, guide, status, referenceHelp, beginnerHelp, methodHelp;
  private ScrollView scroll;
  private Spinner targetList, octaveList;
  private TargetSlider targetSlider;
  private TextView magicTapHelp;
  private View visualVoiceControls;
  private CheckBox mapUse;
  private int noteKey;
  private long noteKeyUntil;
  private final Button[] tabs = new Button[4];
  private JSONObject mapDraft;
  private int mappingMidi = 69, mappingCount = 1;
  private boolean mappingTap = true;
  private Spinner mappingOctave, mappingNote;
  private TextView mappingStatus;
  private Button mappingStart, mappingFinish;
  private Button microphoneButton;
  private TuningSession session;
  private LinearLayout targetsBox;
  private int targetIds;
  private Boolean announcedMicState;
  private String consonanceLabel;
  private Spinner consonanceList;
  private final Map<Spinner, int[]> selections = new IdentityHashMap<>();
  private View pairControls,
      micControls,
      forkControls,
      monitorControls,
      advanceControls,
      forkTapControls,
      hybridPauseControls;
  private View ukuleleControls, orderControls;
  private CheckBox monitorCheck;
  private int renderedPage = -1, renderGeneration, restoredFocus = View.NO_ID;
  private int focusBeforeRecreate = View.NO_ID;
  private AudioEngine audio;
  private Updater updater;
  private boolean autoPaused;
  private final Handler handler = new Handler(Looper.getMainLooper());
  private int page = 0, nextId = 1000;
  private long lastFork;
  private String lastResult = "";
  private Meter meter;
  private LinearLayout visualPanel;
  private final Runnable updateTick =
      new Runnable() {
        public void run() {
          if (!active) return;
          if (model.updates) updater.check(true);
          handler.postDelayed(this, model.minutes * 60000L);
        }
      };
  private final Runnable forkTick =
      new Runnable() {
        public void run() {
          if (!active) return;
          if (model.autoFork
              && !autoPaused
              && model.method == 0
              && !audio.running()
              && SystemClock.elapsedRealtime() - lastFork >= (long) (model.forkSeconds * 1000)) {
            lastFork = SystemClock.elapsedRealtime();
            audio.strike();
          }
          handler.postDelayed(this, 200);
        }
      };

  String t(String pl, String en) {
    return model.t(pl, en);
  }

  private int dp(int n) {
    return (int) (n * getResources().getDisplayMetrics().density + .5);
  }

  @Override
  public void onCreate(Bundle state) {
    super.onCreate(state);
    try {
      model =
          Model.from(new JSONObject(getSharedPreferences("stroik", 0).getString("settings", "{}")));
    } catch (Exception e) {
      model = new Model();
    }
    boolean firstLaunch = !getSharedPreferences("stroik", 0).contains("settings");
    if (firstLaunch) {
      String code = Locale.getDefault().getLanguage();
      model.language = Translations.supported(code) ? code : "en";
      model.english = !model.language.equals("pl");
    }
    if (state != null) {
      int oldPage = state.getInt("stroik.page", 0);
      page =
          state.getInt("stroik.layout", 1) < 2
              ? (oldPage < 2 ? 0 : oldPage - 1)
              : Math.max(0, Math.min(3, oldPage));
      mappingMidi = Math.max(21, Math.min(108, state.getInt("stroik.mappingMidi", 69)));
      mappingCount = Math.max(1, Math.min(10, state.getInt("stroik.mappingCount", 1)));
      mappingTap = state.getBoolean("stroik.mappingTap", true);
      lastResult = state.getString("stroik.result", "");
      autoPaused = state.getBoolean("stroik.autoPaused", false);
      restoredFocus = state.getInt("stroik.accessibilityFocus", View.NO_ID);
    }
    setTheme(
        model.dark
            ? android.R.style.Theme_Material_NoActionBar
            : android.R.style.Theme_Material_Light_NoActionBar);
    getWindow().setSoftInputMode(WindowManager.LayoutParams.SOFT_INPUT_ADJUST_RESIZE);
    session = TuningSession.get(this);
    if (session.serviceActive) model = session.model.copy();
    session.attach(this);
    audio = session.audio;
    try { mapDraft = new JSONObject(getSharedPreferences("stroik", 0).getString("mappingDraft", "")); }
    catch (Exception absent) { mapDraft = TuningFiles.create(TuningFiles.MAP); }
    if (!session.serviceActive) session.configure(model);
    updater = new Updater(this);
    render();
    if (state != null) {
      final int scrollY = Math.max(0, state.getInt("stroik.scroll", 0));
      ScrollView restoredScroll = scroll;
      restoredScroll.post(() -> restoredScroll.scrollTo(0, scrollY));
    }
    getWindow()
        .getDecorView()
        .setOnApplyWindowInsetsListener((v, insets) -> {
          android.graphics.Insets bars =
              insets.getInsets(WindowInsets.Type.systemBars() | WindowInsets.Type.displayCutout());
          root.setPadding(bars.left, bars.top, bars.right, bars.bottom);
          return insets;
        });
    getWindow().getDecorView().requestApplyInsets();
    if (firstLaunch)
      handler.postDelayed(
          () -> {
            if (active && !isFinishing())
              message(
                  t(
                      "Widok klasyczny lub dla początkujących wybierzesz na początku strony Stroik,"
                          + " przed metodą strojenia. Ustawienia głosu i autoblokady są w"
                          + " Ustawieniach.",
                      "Choose Classic or Beginner view at the start of the Tuner page, before the"
                          + " tuning method. Voice and screen-awake options are in Settings."));
          },
          800);
  }

  @Override
  protected void onSaveInstanceState(Bundle state) {
    state.putInt("stroik.page", page);
    state.putInt("stroik.layout", 2);
    state.putInt("stroik.mappingMidi", mappingMidi);
    state.putInt("stroik.mappingCount", mappingCount);
    state.putBoolean("stroik.mappingTap", mappingTap);
    state.putInt("stroik.scroll", scroll == null ? 0 : scroll.getScrollY());
    state.putString("stroik.result", lastResult);
    state.putBoolean("stroik.autoPaused", autoPaused);
    View focused = accessibilityFocus(root);
    int focusId = focused == null ? focusBeforeRecreate : focused.getId();
    if (focusId != View.NO_ID) state.putInt("stroik.accessibilityFocus", focusId);
    persist();
    super.onSaveInstanceState(state);
  }

  private void persist() {
    getSharedPreferences("stroik", 0).edit().putString("settings", model.json().toString()).apply();
  }

  private void changed() {
    persist();
    session.configure(model);
    applyScreenPolicy();
    updateGuide();
    rebuildTraversal();
    if (visualPanel != null)
      visualPanel.setVisibility(page == 0 && model.graphics ? View.VISIBLE : View.GONE);
    handler.removeCallbacks(updateTick);
    if (active && model.updates) handler.postDelayed(updateTick, model.minutes * 60000L);
  }

  private TextView text(String value, boolean title) {
    TextView v = new TextView(this);
    v.setId(nextId++);
    v.setText(value);
    v.setTextSize(title ? 22 : 18);
    v.setPadding(dp(8), dp(8), dp(8), dp(8));
    v.setTextColor(model.dark ? Color.WHITE : Color.BLACK);
    if (title) {
      v.setTypeface(null, Typeface.BOLD);
      v.setAccessibilityHeading(true);
    }
    body.addView(v, new LinearLayout.LayoutParams(-1, -2));
    return v;
  }

  private Button button(String label, Runnable action) {
    Button b = new Button(this);
    b.setId(nextId++);
    b.setText(label);
    b.setTextSize(18);
    b.setMinHeight(dp(64));
    b.setAllCaps(false);
    b.setOnClickListener(v -> action.run());
    body.addView(b, new LinearLayout.LayoutParams(-1, -2));
    return b;
  }

  private CheckBox check(String label, boolean value, Consumer<Boolean> change) {
    CheckBox box = new CheckBox(this);
    box.setId(nextId++);
    box.setText(label);
    box.setTextSize(18);
    box.setMinHeight(dp(64));
    box.setPadding(dp(8), dp(4), dp(8), dp(4));
    box.setChecked(value);
    box.setOnCheckedChangeListener((b, v) -> {
      change.accept(v);
      changed();
    });
    body.addView(box, new LinearLayout.LayoutParams(-1, -2));
    return box;
  }

  private Spinner choice(String label, String[] items, int value, IntConsumer change) {
    TextView l = text(label, false);
    l.setImportantForAccessibility(View.IMPORTANT_FOR_ACCESSIBILITY_NO);
    Spinner s = new Spinner(this);
    s.setId(nextId++);
    s.setMinimumHeight(dp(64));
    s.setImportantForAccessibility(View.IMPORTANT_FOR_ACCESSIBILITY_YES);
    s.setScreenReaderFocusable(true);
    l.setLabelFor(s.getId());
    s.setContentDescription(label);
    s.setAccessibilityDelegate(
        new View.AccessibilityDelegate() {
          @Override
          public void onInitializeAccessibilityNodeInfo(
              View host, android.view.accessibility.AccessibilityNodeInfo info) {
            super.onInitializeAccessibilityNodeInfo(host, info);
            Object selected = s.getSelectedItem();
            info.setContentDescription(
                label + ": " + (selected == null ? "" : selected.toString()));
          }
        });
    ArrayAdapter<String> a =
        new ArrayAdapter<>(this, android.R.layout.simple_spinner_item, items) {
          @Override
          public View getView(int position, View reusable, ViewGroup parent) {
            View selected = super.getView(position, reusable, parent);
            // The collapsed list is one labelled accessibility stop. Popup rows
            // still use getDropDownView and retain their ordinary accessibility.
            selected.setImportantForAccessibility(
                View.IMPORTANT_FOR_ACCESSIBILITY_NO_HIDE_DESCENDANTS);
            return selected;
          }
        };
    a.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
    s.setAdapter(a);
    s.setSelection(Math.max(0, Math.min(items.length - 1, value)));
    final int[] last = {s.getSelectedItemPosition()};
    selections.put(s, last);
    s.setOnItemSelectedListener(
        new android.widget.AdapterView.OnItemSelectedListener() {
          public void onNothingSelected(android.widget.AdapterView<?> p) {}

          public void onItemSelected(android.widget.AdapterView<?> p, View v, int at, long id) {
            if (at == last[0]) return;
            last[0] = at;
            change.accept(at);
            changed();
          }
        });
    body.addView(s, new LinearLayout.LayoutParams(-1, -2));
    return s;
  }

  private void number(String label, double value, double min, double max, DoubleConsumer change) {
    number(label, value, min, max, max == 1440 || max == 20 ? 1 : max == 2.5 ? .05 : .1, change);
  }

  private void number(
      String label, double value, double min, double max, double step, DoubleConsumer change) {
    TextView caption = text(label, false);
    caption.setImportantForAccessibility(View.IMPORTANT_FOR_ACCESSIBILITY_NO);
    NumberSlider slider =
        new NumberSlider(
            this,
            label,
            value,
            min,
            max,
            step,
            caption,
            next -> {
              change.accept(next);
              changed();
            });
    slider.setId(nextId++);
    body.addView(slider, new LinearLayout.LayoutParams(-1, -2));
  }

  private View accessibilityFocus(View view) {
    if (view == null) return null;
    if (view.isAccessibilityFocused()) return view;
    if (view instanceof ViewGroup)
      for (int i = 0; i < ((ViewGroup) view).getChildCount(); i++) {
        View focused = accessibilityFocus(((ViewGroup) view).getChildAt(i));
        if (focused != null) return focused;
      }
    return null;
  }

  private View section(Runnable contents) {
    LinearLayout parent = body;
    LinearLayout group = new LinearLayout(this);
    group.setOrientation(1);
    parent.addView(group, new LinearLayout.LayoutParams(-1, -2));
    body = group;
    try {
      contents.run();
    } finally {
      body = parent;
    }
    return group;
  }

  private void visible(View view, boolean show) {
    if (view != null) view.setVisibility(show ? View.VISIBLE : View.GONE);
  }

  private void updateTunerSections() {
    // Capture focus before showing/hiding rows: a reflow can temporarily move
    // the focused control outside the viewport and Android can clear it.
    View focused = accessibilityFocus(root);
    visible(pairControls, model.interval != 0 && model.method != 0);
    visible(micControls, model.method != 0);
    visible(magicTapHelp, model.beginner && !model.graphics);
    if (targetSlider != null) targetSlider.sync();
    visible(forkControls, model.method == 0);
    visible(forkTapControls, model.method != 1);
    visible(monitorControls, model.method == 3);
    visible(advanceControls, model.method == 1);
    visible(hybridPauseControls, model.method == 2);
    if (focused != null)
      focused.post(() -> {
        View current = accessibilityFocus(root);
        if (focused.isAttachedToWindow()
            && focused.isShown()
            && (current == null || current == focused || current == heading)) {
          focused.requestRectangleOnScreen(
              new Rect(0, 0, focused.getWidth(), focused.getHeight()), true);
          if (!focused.isAccessibilityFocused())
            focused.performAccessibilityAction(
                android.view.accessibility.AccessibilityNodeInfo.ACTION_ACCESSIBILITY_FOCUS, null);
        }
      });
    updateGuide();
    rebuildTraversal();
  }

  private void updateInstrumentSections() {
    visible(ukuleleControls, model.instrument == 2);
    visible(orderControls, model.instrument == 0);
  }

  private void selectSilently(Spinner spinner, int position) {
    int[] last = selections.get(spinner);
    if (last != null) last[0] = position;
    spinner.setSelection(position);
  }

  private void refreshTargets() {
    if (targetsBox == null) return;
    LinearLayout parent = body;
    selections.remove(targetList);
    selections.remove(octaveList);
    targetsBox.removeAllViews();
    body = targetsBox;
    targetList = octaveList = null;
    int savedId = nextId;
    nextId = targetIds;
    try {
      targets();
    } finally {
      body = parent;
      nextId = savedId;
    }
    updateGuide();
  }

  private void applyScreenPolicy() {
    if (model.keepScreenOn) getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
    else getWindow().clearFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
    WindowInsetsController bars = getWindow().getInsetsController();
    if (bars != null) {
      int light =
          WindowInsetsController.APPEARANCE_LIGHT_STATUS_BARS
              | WindowInsetsController.APPEARANCE_LIGHT_NAVIGATION_BARS;
      bars.setSystemBarsAppearance(model.dark ? 0 : light, light);
    }
  }

  private void referenceControl() {
    number(
        t("Częstotliwość, Hz", "Frequency, Hz"),
        model.reference,
        390,
        466,
        .1,
        value -> model.reference = value);
  }

  private void render() {
    boolean samePage = renderedPage == page;
    View focused = accessibilityFocus(root);
    int focusId = focused == null ? restoredFocus : focused.getId();
    int scrollY = samePage && scroll != null ? scroll.getScrollY() : 0;
    if (root == null) createShell();
    nextId = 1000 + page * 1000;
    selections.clear();
    body.removeAllViews();
    root.setBackgroundColor(model.dark ? Color.rgb(30, 30, 30) : Color.WHITE);
    heading.setTextColor(model.dark ? Color.WHITE : Color.BLACK);
    String[] pages = {t("Stroik", "Tuner"), t("Ustawienia", "Settings"), t("Pomoc", "Help"), t("Mapowanie instrumentu", "Instrument mapping")};
    heading.setText("Stroik " + version() + " — " + pages[page]);
    result = guide = status = beginnerHelp = methodHelp = referenceHelp = null;
    consonanceList = null;
    targetList = octaveList = null;
    targetSlider = null;
    targetsBox = null;
    microphoneButton = null;
    mapUse = null;
    visualVoiceControls = null;
    mappingStart = mappingFinish = null;
    pairControls = micControls = forkControls = monitorControls = advanceControls = null;
    ukuleleControls = orderControls = null;
    monitorCheck = null;
    if (page == 0) tuner();
    else if (page == 1) settings();
    else if (page == 2) help();
    else mappingPage();
    visualPanel.setVisibility(page == 0 && model.graphics ? View.VISIBLE : View.GONE);
    for (int i = 0; i < tabs.length; i++) {
      tabs[i].setText(pages[i]);
      tabs[i].setSelected(i == page);
      tabs[i].setContentDescription(pages[i] + t(", zakładka", ", tab"));
      tabs[i].setStateDescription(i == page ? t("Wybrana", "Selected") : null);
    }
    styleTabs();
    renderedPage = page;
    applyScreenPolicy();
    int generation = ++renderGeneration;
    boolean restore = samePage || restoredFocus != View.NO_ID;
    restoredFocus = View.NO_ID;
    root.postDelayed(
        () -> {
          if (generation != renderGeneration || isFinishing() || isDestroyed()) return;
          scroll.scrollTo(0, scrollY);
          View target = restore && focusId != View.NO_ID ? root.findViewById(focusId) : null;
          View current = accessibilityFocus(root);
          if (target != null
              && target.isShown()
              && !target.isAccessibilityFocused()
              && (current == null || current == heading)) {
            target.requestRectangleOnScreen(
                new Rect(0, 0, target.getWidth(), target.getHeight()), true);
            target.performAccessibilityAction(
                android.view.accessibility.AccessibilityNodeInfo.ACTION_ACCESSIBILITY_FOCUS, null);
          }
        },
        150);
    root.requestApplyInsets();
    rebuildTraversal();
  }

  private void rebuildTraversal() {
    if (root == null) return;
    ArrayList<View> stops = new ArrayList<>();
    collectStops(root, stops);
    for (int i = 0; i < stops.size(); i++) {
      View stop = stops.get(i);
      stop.setAccessibilityTraversalBefore(View.NO_ID);
      stop.setAccessibilityTraversalAfter(i == 0 ? View.NO_ID : stops.get(i - 1).getId());
    }
  }

  private void collectStops(View view, List<View> stops) {
    if (view.getVisibility() != View.VISIBLE
        || view.getImportantForAccessibility()
            == View.IMPORTANT_FOR_ACCESSIBILITY_NO_HIDE_DESCENDANTS) return;
    if (view instanceof Spinner
        || view instanceof SeekBar
        || view instanceof Button
        || view instanceof TextView
            && view.getImportantForAccessibility() != View.IMPORTANT_FOR_ACCESSIBILITY_NO
            && ((TextView) view).length() > 0) {
      if (view.getId() == View.NO_ID) view.setId(View.generateViewId());
      stops.add(view);
      return;
    }
    if (view instanceof ViewGroup)
      for (int i = 0; i < ((ViewGroup) view).getChildCount(); i++)
        collectStops(((ViewGroup) view).getChildAt(i), stops);
  }

  private void styleTabs() {
    for (int i = 0; i < tabs.length; i++) {
      tabs[i].setTypeface(null, i == page ? Typeface.BOLD : Typeface.NORMAL);
      tabs[i].setTextColor(i == page ? Color.WHITE : model.dark ? Color.WHITE : Color.BLACK);
      android.graphics.drawable.GradientDrawable tabBackground =
          new android.graphics.drawable.GradientDrawable();
      tabBackground.setColor(
          i == page
              ? Color.rgb(0, 83, 145)
              : model.dark ? Color.rgb(48, 48, 48) : Color.rgb(235, 240, 245));
      tabBackground.setCornerRadius(dp(10));
      tabs[i].setBackground(tabBackground);
    }
  }

  private void applyThemeInPlace(View view) {
    int foreground = model.dark ? Color.WHITE : Color.BLACK;
    int accent = model.dark ? Color.rgb(130, 200, 255) : Color.rgb(0, 83, 145);
    if (view instanceof TextView) ((TextView) view).setTextColor(foreground);
    if (view instanceof EditText) {
      ((EditText) view).setHintTextColor(model.dark ? Color.LTGRAY : Color.DKGRAY);
      view.setBackgroundTintList(android.content.res.ColorStateList.valueOf(accent));
    } else if (view instanceof CompoundButton) {
      // Replace the old theme's animated icon, not the checkbox View. Keeping
      // that animation can leave the just-checked theme option looking empty.
      ((CompoundButton) view).setButtonDrawable(new CheckBox(this).getButtonDrawable());
      ((CompoundButton) view).setButtonTintList(android.content.res.ColorStateList.valueOf(accent));
      view.jumpDrawablesToCurrentState();
    } else if (view instanceof Spinner) {
      view.setBackgroundTintList(android.content.res.ColorStateList.valueOf(accent));
    } else if (view instanceof Button && view.getId() < 10000) {
      view.setBackgroundTintList(
          android.content.res.ColorStateList.valueOf(
              model.dark ? Color.rgb(64, 64, 64) : Color.rgb(225, 231, 237)));
    }
    if (view instanceof ViewGroup)
      for (int i = 0; i < ((ViewGroup) view).getChildCount(); i++)
        applyThemeInPlace(((ViewGroup) view).getChildAt(i));
    view.invalidate();
  }

  private void createShell() {
    root = new LinearLayout(this);
    // Dynamic pages reuse view IDs for different widget types. Restore the model and
    // explicit page state instead of replaying Android's widget state into another layout.
    // This also ignores incompatible hierarchy state saved by releases 1.0/1.01.
    root.setSaveFromParentEnabled(false);
    root.setOrientation(1);
    setContentView(root);
    heading = new TextView(this);
    heading.setId(9000);
    heading.setTextSize(24);
    heading.setTextColor(model.dark ? Color.WHITE : Color.BLACK);
    heading.setPadding(dp(16), dp(12), dp(16), dp(12));
    heading.setAccessibilityHeading(true);
    root.addView(heading);
    visualPanel = new LinearLayout(this);
    visualPanel.setOrientation(1);
    visualPanel.setImportantForAccessibility(View.IMPORTANT_FOR_ACCESSIBILITY_NO_HIDE_DESCENDANTS);
    meter = new Meter(this);
    visualPanel.addView(meter, new LinearLayout.LayoutParams(-1, dp(210)));
    root.addView(visualPanel);
    scroll = new ScrollView(this);
    scroll.setFillViewport(true);
    body = new LinearLayout(this);
    body.setOrientation(1);
    body.setPadding(dp(12), dp(4), dp(12), dp(16));
    scroll.addView(body);
    root.addView(scroll, new LinearLayout.LayoutParams(-1, 0, 1));
    nav = new LinearLayout(this);
    nav.setOrientation(0);
    for (int i = 0; i < tabs.length; i++) {
      final int at = i;
      Button b = new Button(this);
      tabs[i] = b;
      b.setId(10000 + i);
      b.setAllCaps(false);
      b.setMinHeight(dp(64));
      b.setTextSize(14);
      b.setMaxLines(2);
      b.setEllipsize(android.text.TextUtils.TruncateAt.END);
      b.setPadding(dp(2), dp(4), dp(2), dp(4));
      b.setOnClickListener(v -> {
        if (page == at) return;
        getCurrentFocusClear();
        if (page == 3 || at == 3) session.stop();
        page = at;
        render();
      });
      LinearLayout.LayoutParams tabLayout = new LinearLayout.LayoutParams(0, -2, 1);
      tabLayout.setMargins(dp(3), dp(4), dp(3), dp(4));
      nav.addView(b, tabLayout);
    }
    root.addView(nav);
  }

  private void getCurrentFocusClear() {
    View focus = getCurrentFocus();
    if (focus != null) focus.clearFocus();
  }

  @Override
  public boolean dispatchKeyEvent(KeyEvent event) {
    if (event.getAction() == KeyEvent.ACTION_DOWN && handleKey(event.getKeyCode(), event))
      return true;
    return super.dispatchKeyEvent(event);
  }

  @Override
  public boolean onKeyDown(int key, KeyEvent event) {
    return handleKey(key, event) || super.onKeyDown(key, event);
  }

  private boolean handleKey(int key, KeyEvent event) {
    if (page == 3) {
      if (event.isCtrlPressed() && key == KeyEvent.KEYCODE_S) { saveMapping(); return true; }
      if (event.isCtrlPressed() && key == KeyEvent.KEYCODE_O) { openTuningFile(false); return true; }
      if ((event.isCtrlPressed() || event.isAltPressed()) && key == KeyEvent.KEYCODE_N) { nextMapped(); return true; }
      if (event.isAltPressed() && key == KeyEvent.KEYCODE_U) return true;
    }
    if (page == 0 && event.isAltPressed() && key == KeyEvent.KEYCODE_M) {
      if (event.getRepeatCount() == 0) magicTap();
      return true;
    }
    if (page == 0 && targetList != null && (targetList.hasFocus() || targetSlider.hasFocus()) && !model.strings()) {
      if (event.isCtrlPressed() && (key == KeyEvent.KEYCODE_DPAD_UP || key == KeyEvent.KEYCODE_DPAD_DOWN)) {
        changeOctave(key == KeyEvent.KEYCODE_DPAD_UP ? 1 : -1);
        return true;
      }
      if (!event.isCtrlPressed() && !event.isAltPressed()) {
        int letter = event.getUnicodeChar();
        if ((letter >= 'a' && letter <= 'h') || (letter >= 'A' && letter <= 'H')) {
          noteKey = letter;
          noteKeyUntil = SystemClock.elapsedRealtime() + 1800;
          return true;
        }
        if (letter >= '0' && letter <= '8' && SystemClock.elapsedRealtime() <= noteKeyUntil) {
          noteKeyUntil = 0;
          setTarget(Model.parseNote("" + (char)noteKey + (char)letter), true);
          return true;
        }
      }
    }
    if (event.isCtrlPressed()) {
      if (key == KeyEvent.KEYCODE_Q) {
        session.stop();
        finish();
        return true;
      }
      if (key == KeyEvent.KEYCODE_S) {
        saveProfile();
        return true;
      }
      if (key == KeyEvent.KEYCODE_O) {
        loadProfile();
        return true;
      }
      if (key == KeyEvent.KEYCODE_N) {
        moveTarget(event.isShiftPressed() ? -1 : 1);
        return true;
      }
    }
    if (event.isAltPressed() && key == KeyEvent.KEYCODE_N) {
      moveTarget(event.isShiftPressed() ? -1 : 1);
      return true;
    }
    if (targetList != null
        && targetList.hasFocus()
        && !model.strings()
        && !event.isAltPressed()
        && !event.isCtrlPressed()
        && (key == KeyEvent.KEYCODE_DPAD_UP
            || key == KeyEvent.KEYCODE_DPAD_DOWN
            || key == KeyEvent.KEYCODE_DPAD_LEFT
            || key == KeyEvent.KEYCODE_DPAD_RIGHT)) {
      model.target =
          key == KeyEvent.KEYCODE_DPAD_UP || key == KeyEvent.KEYCODE_DPAD_DOWN
              ? Model.nextNatural(model.base(), key == KeyEvent.KEYCODE_DPAD_UP ? -1 : 1)
              : Model.nextBlack(model.base(), key == KeyEvent.KEYCODE_DPAD_LEFT ? -1 : 1);
      model.anchor = model.target;
      changed();
      syncTarget();
      sayAction(model.spokenNote(model.target), targetList);
      return true;
    }
    if (event.isAltPressed() && key == KeyEvent.KEYCODE_U && model.method != 1) {
      if (model.autoFork) {
        autoPaused = !autoPaused;
        audio.setPaused(autoPaused);
      } else audio.strike();
      return true;
    }
    if ((key == KeyEvent.KEYCODE_I || key == KeyEvent.KEYCODE_S)
        && targetList != null
        && targetList.hasFocus()
        && !model.strings()) {
      int next = model.base();
      for (int i = 0; i < 88; i++) {
        next++;
        if (next > 108) next = 21;
        int pitch = next % 12;
        if (pitch == 1 || pitch == 3 || pitch == 6 || pitch == 8) break;
      }
      model.target = next;
      model.anchor = next;
      changed();
      syncTarget();
      sayAction(model.spokenNote(next), targetList);
      return true;
    }
    return false;
  }

  private void tuner() {
    choice(
        t("Widok", "View"),
        new String[] {t("Klasyczny", "Classic"), t("Dla początkujących", "Beginner")},
        model.beginner ? 1 : 0,
        v -> {
          model.beginner = v == 1;
          render();
        });
    instrument();
    forkTapControls =
        section(() -> {
          check(
              t(
                  "Dwukrotne stuknięcie przechodzi do kolejnego dźwięku zgodnego z wybraną"
                      + " kolejnością",
                  "Two strikes advance to the next note in the selected order"),
              model.forkTap,
              v -> {
                model.forkTap = v;
                updateTunerSections();
                if (model.method == 0) {
                  if (v && active) startMic();
                  else if (!v) session.stop();
                }
              });
        });
    advanceControls =
        section(() ->
            check(
                t(
                    "Po dobrym wyniku dwa uderzenia wybierają następną nutę",
                    "After an in-tune result two strikes advance"),
                model.advance,
                v -> model.advance = v));
    targetIds = nextId;
    targetsBox = (LinearLayout) section(this::targets);
    guide = text("", false);
    updateGuide();
    choice(
        t("Metoda strojenia", "Tuning method"),
        new String[] {
          t("Kamerton", "Tuning fork"),
          t("Stroik elektroniczny", "Electronic tuner"),
          t("Hybrydowy — głośniki naprzemiennie", "Hybrid — alternating speakers"),
          t("Hybrydowy — słuchawki", "Hybrid — headphones")
        },
        model.method,
        v -> {
          session.stop();
          model.method = v;
          if (status != null) status.setText(t("Mikrofon wyłączony", "Microphone off"));
          if (model.target < 0 && v != 1) model.target = model.base();
          updateTunerSections();
        });
    consonanceLabel = t("Sprawdź współbrzmienie", "Check consonance");
    consonanceList =
        choice(
            consonanceLabel,
            new String[] {t("Wyłączone", "Off"), t("Kwinty", "Fifths"), t("Oktawy", "Octaves"),
                model.beginner ? t("Sprawdzaj współbrzmienie co 5 dźwięków wybranej kolejności strojenia",
                    "Check consonance every five notes in the selected tuning order")
                    : t("Sprawdzaj współbrzmienie co kwintę wybranej kolejności strojenia",
                        "Check a fifth every five notes in the selected tuning order")},
            model.interval,
            v -> {
              model.interval = v;
              updateTunerSections();
            });
    pairControls =
        section(() ->
            choice(
                t("Sposób pomiaru współbrzmienia", "Consonance measurement method"),
                new String[] {
                  t("Osobno: dolny, potem górny", "Separate: lower, then upper"),
                  t("Razem — eksperymentalnie", "Together — experimental")
                },
                model.pairMode,
                v -> {
                  model.pairMode = v;
                  updateGuide();
                }));
    methodHelp = text(model.methodGuidance(), false);
    beginnerHelp = text(model.beginnerGuidance(), false);
    status =
        text(
            audio.running()
                ? t("Mikrofon włączony", "Microphone on")
                : t("Mikrofon wyłączony", "Microphone off"),
            false);
    // The microphone state belongs to the toggle, not a duplicate dead stop.
    status.setImportantForAccessibility(View.IMPORTANT_FOR_ACCESSIBILITY_NO);
    monitorControls =
        section(() -> {
          monitorCheck =
              check(
                  t(
                      "Odsłuch mikrofonu — tylko w słuchawkach",
                      "Microphone monitoring — headphones only"),
                  model.monitor,
                  v -> {
                    if (v
                        && !AudioEngine.headphones(
                            (AudioManager) getSystemService(AUDIO_SERVICE))) {
                      model.monitor = false;
                      monitorCheck.setChecked(false);
                      message(t("Najpierw podłącz słuchawki.", "Connect headphones first."));
                    } else model.monitor = v;
                  });
        });
    choice(
        t("Czułość mikrofonu", "Microphone sensitivity"),
        new String[] {
          t("Wysoka — ciche instrumenty", "High — quiet instruments"),
          t("Normalna", "Normal"),
          t("Niska — hałas w tle", "Low — background noise")
        },
        model.sensitivity,
        v -> model.sensitivity = v);
    number(
        t("Tolerancja oceny dobrze, centy (1–20)", "In-tune tolerance, cents (1–20)"),
        model.tolerance,
        1,
        20,
        v -> model.tolerance = v);
    hybridPauseControls =
        section(() -> number(
            t("Przerwa po komunikacie w hybrydzie, sekundy (0–60):",
              "Hybrid pause after spoken result, seconds (0–60):"),
            model.hybridPause, 0, 60, 1, v -> model.hybridPause = (int) v));
    number(
        t(
            "Maksymalny odstęp dwóch uderzeń, sekundy (0,5–2,5)",
            "Maximum gap between two strikes, seconds (0.5–2.5)"),
        model.gap,
        .5,
        2.5,
        v -> model.gap = v);
    result =
        text(
            lastResult.isEmpty()
                ? t(
                    "Wynik pojawi się po wyciszeniu nuty.",
                    "The result appears after the note stops.")
                : lastResult,
            false);
    // Read-only feedback must not acquire input focus or start text-selection
    // navigation while TalkBack is traversing the controls below it.
    result.setTextIsSelectable(false);
    result.setFocusable(false);
    result.setContentDescription(t("Ostatni wynik: ", "Last result: ") + result.getText());
    button(
        t("Powtórz wynik", "Repeat result"),
        () -> say(lastResult.isEmpty() ? t("Brak wyniku.", "No result.") : lastResult));
    button(
        t("Sprawdź komunikat czytnika ekranu", "Test screen reader announcement"),
        () -> say(t("Stroik. Komunikaty są dostępne.", "Stroik. Announcements are available.")));
    button(t("Zapisz plik ustawień", "Save settings file"), this::saveProfile);
    button(t("Wczytaj plik ustawień", "Open settings file"), this::loadProfile);
    micControls =
        section(() -> {
          magicTapHelp = text(t("Dwukrotne stuknięcie dwoma palcami włącza mikrofon.",
              "A two-finger double tap starts the microphone."), false);
          microphoneButton =
              button(
                  audio.running()
                      ? t("Wyłącz mikrofon", "Stop microphone")
                      : t("Włącz mikrofon", "Start microphone"),
                  () -> {
                    if (audio.running() || session.serviceActive) session.stop();
                    else startMic();
                  });
          microphoneButton.setStateDescription(
              audio.running()
                  ? t("Mikrofon włączony", "Microphone on")
                  : t("Mikrofon wyłączony", "Microphone off"));
        });
    forkControls =
        section(() -> {
          button(t("Uderz kamerton", "Strike tuning fork"), () -> audio.strike());
          check(
              t("Uderzaj kamerton automatycznie", "Strike tuning fork automatically"),
              model.autoFork,
              v -> {
                model.autoFork = v;
                autoPaused = false;
                audio.setPaused(false);
                if (!v) audio.stopFork();
              });
          button(
              t("Wstrzymaj lub wznów automat kamertonu", "Pause or resume automatic fork"),
              () -> {
                autoPaused = !autoPaused;
                audio.setPaused(autoPaused);
                message(
                    autoPaused
                        ? t("Automat wstrzymany", "Automatic strikes paused")
                        : t("Automat wznowiony", "Automatic strikes resumed"));
              });
          number(
              t("Odstęp kamertonu w sekundach (1–10)", "Fork interval in seconds (1–10)"),
              model.forkSeconds,
              1,
              10,
              v -> model.forkSeconds = v);
        });
    updateTunerSections();
  }

  private void targets() {
    targetSlider = new TargetSlider(this);
    targetSlider.setId(nextId++);
    body.addView(targetSlider);
    targetSlider.sync();
    if (model.beginner)
      text(t("Na suwaku przesuń palcem w górę lub w dół, aby zmienić dźwięk. Dwoma palcami zmienisz oktawę. W TalkBacku użyj akcji suwaka. Dotknij suwaka, aby wpisać dźwięk, np. C4.",
          "Swipe up or down on the slider to change note. Use two fingers to change octave. With TalkBack, use the slider actions. Tap the slider to enter a note such as C4."), false);
    {
      String[] oct = new String[9];
      for (int i = 0; i < 9; i++) oct[i] = model.octave(i);
      octaveList =
          choice(
              t("Wybierz oktawę", "Choose octave"),
              oct,
              model.base() / 12 - 1,
              v -> {
                int n = (v + 1) * 12 + model.base() % 12;
                model.target = Math.max(21, Math.min(108, n));
                model.anchor = model.target;
                syncTarget();
              });
      octaveList.setVisibility(model.strings() ? View.GONE : View.VISIBLE);
      View octaveLabel = body.getChildAt(body.getChildCount() - 2);
      octaveLabel.setVisibility(model.strings() ? View.GONE : View.VISIBLE);
    }
    int[] midis;
    if (model.strings()) midis = model.stringsMidi();
    else {
      midis = new int[88];
      for (int i = 0; i < 88; i++) midis[i] = 21 + i;
    }
    final int[] options = new int[midis.length + 1];
    options[0] = -1;
    System.arraycopy(midis, 0, options, 1, midis.length);
    String[] names = new String[options.length];
    names[0] = t("Automatycznie — stroik elektroniczny", "Automatic — electronic tuner");
    int selected = 0;
    for (int i = 1; i < names.length; i++) {
      names[i] =
          (model.strings() ? t("Struna ", "String ") + (names.length - i) + ": " : "")
              + model.targetLabel(options[i]);
      if (options[i] == model.target) selected = i;
    }
    targetList =
        choice(
            t("Dźwięk docelowy", "Target note"),
            names,
            selected,
            v -> {
              model.target = options[v];
              model.anchor = model.base();
              targetSlider.sync();
              if (octaveList != null) selectSilently(octaveList, model.base() / 12 - 1);
              updateGuide();
            });
    targetList.setTag(options);
  }

  boolean setTarget(int midi, boolean newAnchor) {
    if (midi < 21 || midi > 108 || (model.strings() && Arrays.stream(model.stringsMidi()).noneMatch(note -> note == midi))) return false;
    model.target = midi;
    if (newAnchor) model.anchor = midi;
    changed();
    syncTarget();
    return true;
  }

  void changeOctave(int direction) {
    if (!model.strings()) setTarget(Math.max(21, Math.min(108, model.base() + 12 * direction)), true);
  }

  void stepTarget(int direction) {
    int next = model.move(direction);
    if (next != model.base()) setTarget(next, false);
  }

  private void syncTarget() {
    if (targetSlider != null) targetSlider.sync();
    if (octaveList != null) selectSilently(octaveList, model.base() / 12 - 1);
    if (targetList != null) {
      int[] options = (int[]) targetList.getTag();
      for (int i = 0; i < options.length; i++)
        if (options[i] == model.target) {
          selectSilently(targetList, i);
          break;
        }
    }
    updateGuide();
  }

  private void updateGuide() {
    visible(referenceHelp, model.beginner && model.instrument == 0);
    if (beginnerHelp != null && model.beginner) {
      String instructions = model.beginnerGuidance();
      if (!instructions.contentEquals(beginnerHelp.getText())) beginnerHelp.setText(instructions);
    }
    visible(beginnerHelp, model.beginner);
    visible(methodHelp, model.beginner);
    if (methodHelp != null) methodHelp.setText(model.methodGuidance());
    if (beginnerHelp != null) beginnerHelp.setText(beginnerGuidance());
    if (guide == null) return;
    String value = "";
    if (meter != null) {
      if (meter.target != model.base()) {
        meter.cents = Double.NaN;
        meter.target = model.base();
      }
      meter.invalidate();
    }
    if (model.beginner && model.effectiveInterval() != 0) {
      int upper = model.base() + model.semitones();
      value +=
          "\n"
              + (upper > 108
                  ? t(
                      "Górny dźwięk poza zakresem. Wybierz niższy cel.",
                      "Upper note out of range. Choose a lower target.")
                  : model.note(model.base()) + " + " + model.note(upper));
      value +=
          "\n"
              + (model.method == 0
                  ? t(
                      "Kamerton gra oba dźwięki razem. Wybrana nuta jest dolnym dźwiękiem pary.",
                      "The fork plays both notes together. The selected note is the lower note of"
                          + " the pair.")
                  : model.pairMode == 0
                      ? t(
                          "Zagraj dolny, wycisz, potem górny i wycisz.",
                          "Play lower, mute, then upper and mute.")
                      : t(
                          "Eksperymentalnie: graj razem około 2 sekundy. Harmoniczna może udawać"
                              + " drugą nutę — sprawdź także osobno.",
                          "Experimental: play together about 2 seconds. A harmonic may mimic a"
                              + " second note — also check separately."));
    }
    if (consonanceList != null)
      consonanceList.setStateDescription(
          !model.beginner || model.effectiveInterval() == 0
              ? null
              : model.spokenNote(model.base())
                  + " + "
                  + model.note(model.base() + model.semitones()));
    if (!value.contentEquals(guide.getText())) guide.setText(value);
    guide.setVisibility(value.isEmpty() ? View.GONE : View.VISIBLE);
  }

  private String beginnerGuidance() {
    return model.beginnerGuidance();
  }


  private void instrument() {
    choice(
        t("Instrument", "Instrument"),
        new String[] {
          t("Pianino / fortepian", "Piano"),
          t("Gitara", "Guitar"),
          t("Ukulele", "Ukulele"),
          t("Skrzypce", "Violin"),
          t("Gitara basowa", "Bass guitar"),
          t("Chromatyczny", "Chromatic")
        },
        model.instrument,
        v -> {
          model.instrument = v;
          model.target = model.strings() ? model.stringsMidi()[0] : v == 0 && model.order == 0 ? 21 : 67;
          model.anchor = model.base();
          updateInstrumentSections();
          refreshTargets();
        });
    ukuleleControls =
        section(() ->
            choice(
                t("Strój ukulele", "Ukulele tuning"),
                new String[] {
                  t("Standard od podłogi: A E C G", "Standard, floor upward: A E C G"),
                  t("Alternatywny od podłogi: G E C A", "Alternative, floor upward: G E C A")
                },
                model.swapped ? 1 : 0,
                v -> {
                  model.swapped = v == 1;
                  model.target = model.stringsMidi()[0];
                  refreshTargets();
                }));
    referenceControl();
    orderControls =
        section(() ->
            choice(
                t("Kolejność strojenia", "Tuning order"),
                new String[] {
                  t("Rosnąco", "Ascending"),
                  t("Po 5 półtonów na prawo i lewo", "5 semitones right, then left"),
                  t("Po 5 całych tonów na prawo i lewo", "5 whole tones right, then left")
                },
                model.order,
                v -> {
                  model.order = v;
                  if (v == 0) model.target = 21;
                  model.anchor = model.base();
                  syncTarget();
                }));
    referenceHelp =
        text(
            t(
                "Standard to 440 Hz. Wiek pianina nie określa bezpiecznej wysokości stroju; przed"
                    + " podniesieniem naciągu starego instrumentu poproś technika o ocenę. Ukulele"
                    + " G/A można zamienić. Do innych strojów użyj trybu chromatycznego.",
                "440 Hz is standard. A piano's age alone does not determine a safe pitch; have a"
                    + " technician assess old instruments before raising tension. Ukulele G/A can"
                    + " be swapped. Use chromatic mode for other tunings."),
            false);
    updateInstrumentSections();
  }

  private void mappingPage() {
    String[] counts = new String[10], octaves = new String[9], notes = new String[88];
    for (int i = 0; i < counts.length; i++) counts[i] = Integer.toString(i + 1);
    for (int i = 0; i < octaves.length; i++) octaves[i] = model.octave(i);
    for (int i = 0; i < notes.length; i++) notes[i] = model.note(i + 21);
    choice(t("Liczba uderzeń na dźwięk (1–10)", "Strikes per note (1–10)"), counts, mappingCount - 1, v -> {
      mappingCount = v + 1;
      configureMapping();
    });
    mappingOctave = choice(t("Wybierz oktawę", "Choose octave"), octaves, mappingMidi / 12 - 1,
        v -> selectMapped(Math.max(21, Math.min(108, (v + 1) * 12 + mappingMidi % 12))));
    mappingNote = choice(t("Dźwięk docelowy", "Target note"), notes, mappingMidi - 21, v -> selectMapped(v + 21));
    check(t("Po zapisaniu pomiaru dwa stuknięcia wybierają następny dźwięk", "After a measurement is stored, two taps select the next note"), mappingTap, enabled -> {
      mappingTap = enabled;
      configureMapping();
    });
    mappingStart = button(t("Rozpocznij mapowanie", "Start mapping"), this::startMic);
    mappingFinish = button(t("Zakończ mapowanie", "Finish mapping"), () -> {
      session.stop();
      sayAction(t("Mapowanie zatrzymane.", "Mapping stopped."), mappingFinish);
    });
    mappingStart.setEnabled(!audio.running());
    mappingFinish.setEnabled(audio.running());
    button(t("Następny dźwięk", "Next note"), this::nextMapped);
    mappingStatus = text("", false);
    updateMappingStatus();
    button(t("Zapisz plik mapingu", "Save mapping file"), this::saveMapping);
    button(t("Wczytaj plik mapingu", "Open mapping file"), () -> openTuningFile(false));
    mapUse = check(t("Użyj mapy do strojenia", "Use map for tuning"),
        model.useInstrumentMap && !model.instrumentMap.isEmpty(), enabled -> {
          model.useInstrumentMap = enabled;
          if (enabled) {
            model.instrumentMap = mapDraft.toString();
            model.refreshTuningFiles();
          }
          mapUse.setStateDescription(mappingMode());
        });
    mapUse.setStateDescription(mappingMode());
    updateMappingStatus();
  }

  private String mappingMode() {
    return model.useInstrumentMap && !model.instrumentMap.isEmpty()
        ? t("Mapa aktywna.", "Instrument map active.")
        : t("Strój standardowy", "Standard tuning");
  }

  private void configureMapping() {
    if (audio.mappingActive()) audio.configureMapping(mappingMidi, mappingCount, mappingTap);
    updateMappingStatus();
  }

  private String storedMapping(double hz) {
    return String.format(model.locale(), t("Zapisano %.2f Hz.", "Stored %.2f Hz."), hz);
  }

  private void updateMappingStatus() {
    if (mappingStatus == null) return;
    if (mapUse != null) mapUse.setEnabled(mapDraft.optJSONObject("notes").length() != 0);
    double hz = mapDraft.optJSONObject("notes").optDouble(Integer.toString(mappingMidi), 0);
    mappingStatus.setText(hz > 0 ? storedMapping(hz) : t("Zagraj wybrany dźwięk i pozwól mu ucichnąć po każdym uderzeniu.", "Play the selected note and let it decay after each strike."));
  }

  private void selectMapped(int midi) {
    if (midi == mappingMidi) return;
    mappingMidi = midi;
    selectSilently(mappingOctave, midi / 12 - 1);
    selectSilently(mappingNote, midi - 21);
    configureMapping();
  }

  public void nextMapped() {
    if (page != 3) return;
    if (mappingMidi < 108) {
      selectMapped(mappingMidi + 1);
      session.speak(model.note(mappingMidi));
    } else session.stop();
  }

  private void persistMapDraft() {
    getSharedPreferences("stroik", 0).edit().putString("mappingDraft", mapDraft.toString()).apply();
  }

  public void mapped(int midi, int count, int needed, double average) {
    if (page != 3) { audio.speechFinished(); return; }
    String reading;
    if (count >= needed) {
      try { mapDraft.optJSONObject("notes").put(Integer.toString(midi), average); }
      catch (org.json.JSONException error) { audio.speechFinished(); message(error.getMessage()); return; }
      persistMapDraft();
      if (mapUse != null) mapUse.setEnabled(true);
      reading = storedMapping(average);
    } else reading = String.format(model.locale(), t("Pomiar %1$d z %2$d.", "Measurement %1$d of %2$d."), count, needed);
    mappingStatus.setText(reading);
    session.speak(reading);
  }

  private void saveMapping() {
    Intent intent = new Intent(Intent.ACTION_CREATE_DOCUMENT).setType("application/json")
        .addCategory(Intent.CATEGORY_OPENABLE).putExtra(Intent.EXTRA_TITLE, "stroik-instrument-map.json");
    startActivityForResult(intent, 61);
  }

  private void openTuningFile(boolean calibration) {
    Intent intent = new Intent(Intent.ACTION_OPEN_DOCUMENT).setType("*/*").addCategory(Intent.CATEGORY_OPENABLE);
    startActivityForResult(intent, calibration ? 63 : 62);
  }

  private void settings() {
    String announcements = t("Personalizacja komunikatów strojenia", "Customize tuning announcements");
    text(announcements, true);
    choice(announcements,
        new String[] {t("Tylko centy", "Cents only"), t("Tylko herce", "Hertz only"),
            t("Centy i herce", "Cents then hertz"), t("Herce i centy", "Hertz then cents")},
        model.resultUnits, v -> model.resultUnits = v);
    number(t("Głośność kamertonu, procent", "Fork volume, percent"), model.forkVolume, 0, 100, 1, v -> model.forkVolume = (int)v);
    number(t("Głośność odsłuchu mikrofonu, procent", "Microphone monitoring volume, percent"), model.monitorVolume, 0, 100, 1, v -> model.monitorVolume = (int)v);
    button(t("Wczytaj kalibrację kamertonu", "Open fork calibration"), () -> openTuningFile(true));
    button(t("Przywróć fabryczną barwę kamertonu", "Restore default fork calibration"), () -> {
      model.forkCalibration = "";
      model.refreshTuningFiles();
      changed();
    });
    choice(
        t("Język", "Language"),
        Translations.NAMES,
        java.util.Arrays.asList(Translations.CODES).indexOf(model.code()),
        v -> {
          model.language = Translations.CODES[v];
          model.english = v != 0;
          render();
        });
    check(
        t("Ciemny motyw", "Dark theme"),
        model.dark,
        v -> {
          model.dark = v;
          setTheme(
              model.dark
                  ? android.R.style.Theme_Material_NoActionBar
                  : android.R.style.Theme_Material_Light_NoActionBar);
          applyThemeInPlace(root);
          root.setBackgroundColor(model.dark ? Color.rgb(30, 30, 30) : Color.WHITE);
          styleTabs();
        });
    check(
        t("Pokazuj wskaźnik na żywo i klawiaturę", "Show live meter and keyboard"),
        model.graphics,
        v -> {
          model.graphics = v;
          visible(visualVoiceControls, v);
          rebuildTraversal();
          if (v && !getSharedPreferences("stroik", 0).getBoolean("visualVoiceHint", false)) {
            getSharedPreferences("stroik", 0).edit().putBoolean("visualVoiceHint", true).apply();
            showText(
                t("Odczyt głosowy", "Spoken feedback"),
                t(
                    "Jeśli chcesz wyłączyć komunikaty głosowe, kliknij opcję „Odczyt głosowy w"
                        + " trybie wizualnym” w Ustawieniach. ↓ Opcja jest bezpośrednio pod polem"
                        + " widoku graficznego.",
                    "To disable spoken feedback, clear ‘Spoken feedback in visual mode’ in"
                        + " Settings. ↓ The option is directly below the graphics checkbox."),
                null);
          }
        });
    visualVoiceControls = section(() -> check(t("Odczyt głosowy w trybie wizualnym", "Spoken feedback in visual mode"),
        model.voiceInVisual, v -> model.voiceInVisual = v));
    visible(visualVoiceControls, model.graphics);
    check(
        t("Nie pozwalaj na autoblokadę ekranu", "Keep screen awake"),
        model.keepScreenOn,
        v -> model.keepScreenOn = v);
    number(
        t("Prędkość mowy przy wygaszonym ekranie (0,5–3)", "Screen-off speech rate (0.5–3)"),
        model.speechRate,
        .5,
        3,
        .1,
        v -> model.speechRate = v);
    text(
        t(
            "Ta prędkość dotyczy mowy Stroika, nie zmienia prędkości TalkBacka. Kamerton w trybie"
                + " hybrydowym głośnikowym czeka na zakończenie wypowiedzi.",
            "This adjusts Stroik speech, without changing TalkBack. The alternating speaker fork"
                + " waits for speech to finish."),
        false);
    number(
        t("Korekta mikrofonu, centy (−100 do +100)", "Microphone correction, cents (−100 to +100)"),
        model.correction,
        -100,
        100,
        v -> model.correction = v);
    text(
        t(
            "Korekta jest ręczna. Ustaw ją tylko po porównaniu ze znanym wzorcem.",
            "Correction is manual. Use it only after comparison with a known reference."),
        false);
    check(
        t("Automatycznie sprawdzaj aktualizacje", "Automatically check for updates"),
        model.updates,
        v -> model.updates = v);
    number(
        t("Sprawdzaj co ile minut (1–1440)", "Check every how many minutes (1–1440)"),
        model.minutes,
        1,
        1440,
        v -> model.minutes = (int) v);
    button(t("Sprawdź aktualizacje", "Check for updates"), () -> updater.check(false));
    button(
        t("Ostatnie zmiany", "Latest changes"),
        () ->
            showText(
                t("Ostatnie zmiany", "Latest changes"),
                asset("changes." + model.code() + ".txt"),
                null));
    button(
        t("O programie", "About Stroik"),
        () ->
            showText(
                t("O programie", "About Stroik"),
                t("Stroik Android ", "Stroik Android ")
                    + version()
                    + "\n\n"
                    + t(
                        "Dostępny stroiciel dla osób niewidomych, słabowidzących i widzących:"
                            + " mikrofon, kamerton, hybryda, kwinty i oktawy.\n"
                            + "Wykonawca: Proszczok z Codex.\n"
                            + "Pomysłodawca: Witold.",
                        "An accessible tuner for blind, partially sighted and sighted musicians:"
                            + " microphone, tuning fork, hybrid, fifths and octaves.\n"
                            + "Created by: Proszczok with Codex.\n"
                            + "Original idea: Witold."),
                null));
    button(t("Strona Proszczoka", "Proszczok website"), () -> open("https://proszczok.pl/"));
    button(
        t("Wesprzyj przez PayPal", "Support via PayPal"),
        () -> open("https://paypal.me/proszczok"));
    button(
        t("Wesprzyj przez Revolut", "Support via Revolut"),
        () -> open("https://revolut.me/r_rosiejka"));
    button(t("Zapisz plik ustawień", "Save settings file"), this::saveProfile);
    button(t("Wczytaj plik ustawień", "Open settings file"), this::loadProfile);
  }

  private void help() {
    text(t("Instrukcja i dostępność", "Guide and accessibility"), true);
    text(asset("guide." + model.code() + ".txt"), false);
    button(
        t("Sprawdź komunikat czytnika ekranu", "Test screen reader announcement"),
        () -> say(t("Stroik. Komunikaty działają.", "Stroik. Announcements work.")));
    button(
        t("Otwórz projekt na GitHubie", "Open GitHub project"),
        () -> open("https://github.com/proszczok2002/Stroik"));
  }

  private static byte[] readBytes(InputStream in, int max) throws IOException {
    ByteArrayOutputStream out = new ByteArrayOutputStream();
    byte[] buffer = new byte[8192];
    int n;
    while ((n = in.read(buffer)) != -1) {
      if (out.size() + n > max) throw new IOException("File too large");
      out.write(buffer, 0, n);
    }
    return out.toByteArray();
  }

  private String asset(String name) {
    try (InputStream in = getAssets().open(name)) {
      return new String(readBytes(in, 1000000), StandardCharsets.UTF_8);
    } catch (Exception e) {
      return t("Nie można odczytać instrukcji: ", "Cannot read guide: ") + e.getMessage();
    }
  }

  private String version() {
    try {
      return getPackageManager().getPackageInfo(getPackageName(), 0).versionName;
    } catch (Exception e) {
      return "1.0";
    }
  }

  private void open(String url) {
    try {
      startActivity(new Intent(Intent.ACTION_VIEW, Uri.parse(url)));
    } catch (Exception e) {
      message(t("Nie można otworzyć strony.", "Cannot open page."));
    }
  }

  void startMic() {
    getCurrentFocusClear();
    if (checkSelfPermission(Manifest.permission.RECORD_AUDIO)
        != PackageManager.PERMISSION_GRANTED) {
      requestPermissions(new String[] {Manifest.permission.RECORD_AUDIO}, 41);
      return;
    }
    if (Build.VERSION.SDK_INT >= 33
        && checkSelfPermission(Manifest.permission.POST_NOTIFICATIONS)
            != PackageManager.PERMISSION_GRANTED
        && !getSharedPreferences("stroik", 0).getBoolean("askedNotifications", false)) {
      getSharedPreferences("stroik", 0).edit().putBoolean("askedNotifications", true).apply();
      requestPermissions(new String[] {Manifest.permission.POST_NOTIFICATIONS}, 42);
      return;
    }
    if (page != 3 && model.method == 0 && !model.forkTap) {
      message(
          t(
              "Włącz nasłuch dwóch stuknięć albo wybierz stroik elektroniczny.",
              "Enable tap listening or choose electronic tuning."));
      return;
    }
    try {
      session.configure(model);
      if (page == 3) audio.configureMapping(mappingMidi, mappingCount, mappingTap);
      session.start();
    } catch (Exception e) {
      session.stop();
      message(e.getMessage());
    }
  }

  @Override
  public void onRequestPermissionsResult(int request, String[] permissions, int[] grants) {
    super.onRequestPermissionsResult(request, permissions, grants);
    if (request == 42) {
      startMic();
      return;
    }
    if (request == 41) {
      if (grants.length > 0 && grants[0] == PackageManager.PERMISSION_GRANTED) startMic();
      else
        message(
            t(
                "Bez zgody na mikrofon działa tylko odtwarzanie kamertonu.",
                "Without microphone permission only fork playback is available."));
    }
  }

  void status(String text) {
    if (status != null) status.setText(text);
    say(text);
  }

  void message(String text) {
    showText(t("Stroik", "Stroik"), text, null);
  }

  void showText(String title, String text, Runnable yes) {
    TextView content = new TextView(this);
    content.setText(text);
    content.setTextSize(18);
    content.setTextIsSelectable(true);
    content.setPadding(dp(20), dp(12), dp(20), dp(12));
    ScrollView sc = new ScrollView(this);
    sc.addView(content);
    AlertDialog.Builder b = new AlertDialog.Builder(this).setTitle(title).setView(sc);
    if (yes != null)
      b.setPositiveButton(t("Tak, kontynuuj", "Yes, continue"), (d, w) -> yes.run())
          .setNegativeButton(t("Nie", "No"), null);
    else b.setPositiveButton(t("Zamknij", "Close"), null);
    AlertDialog dialog = b.create();
    dialog.setOnShowListener(d -> {
      dialog.getButton(AlertDialog.BUTTON_POSITIVE).setMinHeight(dp(64));
      if (dialog.getButton(AlertDialog.BUTTON_NEGATIVE) != null)
        dialog.getButton(AlertDialog.BUTTON_NEGATIVE).setMinHeight(dp(64));
    });
    dialog.show();
  }

  private void say(String text) {
    speak(text, root, true);
  }

  private void sayAction(String text, View source) {
    speak(text, source == null ? root : source, false);
  }

  private void speak(String text, View source, boolean respectVisualVoice) {
    if (text == null || text.isEmpty()) return;
    if (session.screenOff()) {
      session.speak(text);
      return;
    }
    if (!active || (respectVisualVoice && model.graphics && !model.voiceInVisual)) return;
    AccessibilityManager a = (AccessibilityManager) getSystemService(ACCESSIBILITY_SERVICE);
    if (a.isEnabled() && a.isTouchExplorationEnabled()) source.announceForAccessibility(text);
    else session.speak(text);
  }

  public void result(String text) {
    session.result(text);
  }

  void displayResult(String text) {
    lastResult = text;
    if (result != null) {
      result.setText(text);
      result.setContentDescription(t("Ostatni wynik: ", "Last result: ") + text);
    }
  }

  public void reading(double cents) {
    if (meter != null) {
      meter.cents = cents;
      meter.invalidate();
    }
  }

  public void advance() {
    moveTarget(1);
  }

  private void moveTarget(int direction) {
    int next = model.move(direction);
    if (next == model.base()) {
      if (direction > 0) session.endSequence();
      sayAction(
          direction < 0
              ? t("To początek wybranej kolejności.", "Start of the selected order.")
              : t(
                  "Koniec wybranej kolejności. Strojenie zatrzymane.",
                  "End of selected order. Tuning stopped."),
          targetList);
      return;
    }
    model.target = next;
    changed();
    syncTarget();
    sayAction(model.nextAnnouncement(next), targetList);
  }

  void sequenceEnded() {
    autoPaused = true;
    audio.setPaused(true);
    session.media.refresh();
  }

  void magicTap() {
    if (page == 3) {
      if (session.serviceActive) session.stop();
      else startMic();
      return;
    }
    if (playbackActive()) {
      session.endSequence();
    } else {
      autoPaused = false;
      audio.setPaused(false);
      if (model.method == 0 && !model.forkTap) audio.strike();
      else startMic();
    }
    session.media.refresh();
  }

  boolean playbackActive() {
    return audio.running()
        || session.serviceActive
        || audio.forkPlaying()
        || (model.method == 0 && model.autoFork && !autoPaused);
  }

  public void forkPaused(String text) {
    autoPaused = true;
    audio.setPaused(true);
    if (status != null) status.setText(text);
    sayAction(text, status);
  }

  public void failed(String text) {
    microphoneState();
    status(t("Mikrofon zatrzymany: ", "Microphone stopped: ") + text);
  }

  void microphoneState() {
    if (mappingStart != null) {
      mappingStart.setEnabled(!audio.running());
      mappingFinish.setEnabled(audio.running());
    }
    String value =
        audio.running()
            ? t("Mikrofon włączony", "Microphone on")
            : t("Mikrofon wyłączony", "Microphone off");
    if (microphoneButton != null) {
      microphoneButton.setText(
          audio.running()
              ? t("Wyłącz mikrofon", "Stop microphone")
              : t("Włącz mikrofon", "Start microphone"));
      microphoneButton.setStateDescription(value);
    }
    if (status != null) status.setText(value);
    if (announcedMicState != null && announcedMicState != audio.running()
        && !(audio.running() && model.method >= 2))
      sayAction(value, microphoneButton);
    announcedMicState = audio.running();
  }

  private void saveProfile() {
    getCurrentFocusClear();
    Intent i =
        new Intent(Intent.ACTION_CREATE_DOCUMENT)
            .setType("application/json")
            .addCategory(Intent.CATEGORY_OPENABLE)
            .putExtra(Intent.EXTRA_TITLE, "stroik-profile.json");
    startActivityForResult(i, 51);
  }

  private void loadProfile() {
    startActivityForResult(
        new Intent(Intent.ACTION_OPEN_DOCUMENT)
            .setType("application/json")
            .addCategory(Intent.CATEGORY_OPENABLE),
        52);
  }

  @Override
  protected void onActivityResult(int request, int code, Intent data) {
    super.onActivityResult(request, code, data);
    if (code != RESULT_OK || data == null || data.getData() == null) return;
    try {
      if (request == 61) {
        try (OutputStream out = getContentResolver().openOutputStream(data.getData(), "wt")) {
          out.write(mapDraft.toString(2).getBytes(StandardCharsets.UTF_8));
        }
        return;
      }
      if (request == 62 || request == 63) {
        boolean calibration = request == 63;
        try (InputStream in = getContentResolver().openInputStream(data.getData())) {
          byte[] bytes = readBytes(in, 100000);
          if (bytes.length > 100000) throw new IOException("JSON exceeds 100 KB");
          String json = new String(bytes, StandardCharsets.UTF_8);
          TuningFiles.parse(json, calibration ? TuningFiles.CALIBRATION : TuningFiles.MAP);
          if (calibration) {
            model.forkCalibration = json;
            model.refreshTuningFiles();
            changed();
            sayAction(t("Wczytano kalibrację kamertonu.", "Fork calibration loaded."), root);
          } else {
            session.stop();
            mapDraft = new JSONObject(json);
            if (mapUse != null && mapUse.isChecked()) {
              model.instrumentMap = json;
              model.refreshTuningFiles();
              changed();
            }
            persistMapDraft();
            updateMappingStatus();
            sayAction(t("Wczytano mapę instrumentu.", "Instrument map loaded."), mappingStatus);
          }
        }
        return;
      }
      if (request == 51) {
        try (OutputStream out = getContentResolver().openOutputStream(data.getData(), "wt")) {
          out.write(model.json().toString(2).getBytes(StandardCharsets.UTF_8));
        }
        message(t("Profil zapisany.", "Profile saved."));
      } else if (request == 52) {
        Model loaded;
        try (InputStream in = getContentResolver().openInputStream(data.getData())) {
          byte[] b = readBytes(in, 100000);
          if (b.length > 100000) throw new IOException("Profile too large");
          loaded = Model.from(new JSONObject(new String(b, StandardCharsets.UTF_8)));
        }
        session.stop();
        model = loaded;
        changed();
        recreate();
      }
    } catch (Exception e) {
      message(
          t("Nie można zapisać lub wczytać profilu: ", "Cannot save or load profile: ")
              + e.getMessage());
    }
  }

  @Override
  protected void onResume() {
    super.onResume();
    active = true;
    if (session.serviceActive) {
      model = session.model.copy();
      syncTarget();
      if (!session.lastResult.isEmpty()) {
        lastResult = session.lastResult;
        if (result != null) {
          result.setText(lastResult);
          result.setContentDescription(t("Ostatni wynik: ", "Last result: ") + lastResult);
        }
      }
    }
    session.attach(this);
    microphoneState();
    handler.post(forkTick);
    handler.removeCallbacks(updateTick);
    if (model.updates) {
      updater.check(true);
      handler.postDelayed(updateTick, model.minutes * 60000L);
    }
    String install = getSharedPreferences("stroik", 0).getString("installStatus", "");
    if (!install.isEmpty()) {
      getSharedPreferences("stroik", 0).edit().remove("installStatus").apply();
      handler.postDelayed(() -> message(install), 500);
    }
  }

  @Override
  protected void onPause() {
    getCurrentFocusClear();
    active = false;
    handler.removeCallbacksAndMessages(null);
    if (audio.mappingActive()) session.stop();
    else if (!session.serviceActive) audio.stop();
    session.media.setAvailable(false);
    persist();
    super.onPause();
  }

  @Override
  protected void onDestroy() {
    updater.close();
    session.detach(this);
    if (!session.serviceActive) audio.stop();
    super.onDestroy();
  }

  private final class Meter extends View {
    double cents = Double.NaN;
    int target = -1;
    final Paint paint = new Paint(Paint.ANTI_ALIAS_FLAG);

    Meter(Context context) {
      super(context);
    }

    protected void onDraw(Canvas canvas) {
      super.onDraw(canvas);
      int foreground = model.dark ? Color.WHITE : Color.BLACK;
      float cx = getWidth() / 2f, cy = dp(112), radius = dp(64);
      paint.setStyle(Paint.Style.FILL);
      paint.setColor(foreground);
      paint.setTextAlign(Paint.Align.CENTER);
      paint.setTextSize(dp(17));
      canvas.drawText(model.spokenNote(model.base()), cx, dp(23), paint);
      paint.setStyle(Paint.Style.STROKE);
      paint.setStrokeWidth(dp(3));
      paint.setColor(model.dark ? Color.LTGRAY : Color.DKGRAY);
      canvas.drawArc(cx - radius, cy - radius, cx + radius, cy + radius, 180, 180, false, paint);
      for (int tick = -50; tick <= 50; tick += 10) {
        double angle = Math.PI + (tick + 50) / 100.0 * Math.PI;
        float dx = (float) Math.cos(angle), dy = (float) Math.sin(angle);
        canvas.drawLine(
            cx + dx * (radius - dp(7)),
            cy + dy * (radius - dp(7)),
            cx + dx * (radius + dp(3)),
            cy + dy * (radius + dp(3)),
            paint);
      }
      boolean valid = Double.isFinite(cents) && audio.running();
      if (valid) {
        double angle = Math.PI + (Math.max(-50, Math.min(50, cents)) + 50) / 100 * Math.PI;
        paint.setColor(
            Math.abs(cents) <= model.tolerance ? Color.rgb(0, 160, 90) : Color.rgb(210, 95, 0));
        canvas.drawLine(
            cx,
            cy,
            cx + (float) Math.cos(angle) * (radius - dp(10)),
            cy + (float) Math.sin(angle) * (radius - dp(10)),
            paint);
        paint.setStyle(Paint.Style.FILL);
        canvas.drawCircle(cx, cy, dp(5), paint);
      }
      paint.setStyle(Paint.Style.FILL);
      paint.setTextSize(dp(14));
      paint.setColor(foreground);
      canvas.drawText("−50", cx - radius - dp(22), cy + dp(4), paint);
      canvas.drawText("+50", cx + radius + dp(22), cy + dp(4), paint);
      canvas.drawText("0", cx, cy - radius - dp(8), paint);
      canvas.drawText(
          valid
              ? String.format(model.locale(), model.t("%+.1f centów", "%+.1f cents"), cents)
              : model.t("Zagraj dźwięk", "Play a note"),
          cx,
          dp(137),
          paint);
      int[] whites = {0, 2, 4, 5, 7, 9, 11};
      int selected = model.base() % 12;
      int highlight =
          valid
              ? Math.abs(cents) <= model.tolerance ? Color.rgb(0, 130, 50) : Color.rgb(185, 35, 35)
              : Color.rgb(0, 115, 190);
      float left = dp(12), width = (getWidth() - dp(24)) / 7f;
      for (int i = 0; i < whites.length; i++) {
        paint.setStyle(Paint.Style.FILL);
        paint.setColor(whites[i] == selected ? highlight : Color.WHITE);
        canvas.drawRect(left + i * width, dp(147), left + (i + 1) * width, dp(205), paint);
        paint.setStyle(Paint.Style.STROKE);
        paint.setStrokeWidth(dp(1));
        paint.setColor(Color.DKGRAY);
        canvas.drawRect(left + i * width, dp(147), left + (i + 1) * width, dp(205), paint);
        paint.setStyle(Paint.Style.FILL);
        paint.setTextSize(dp(11));
        paint.setColor(whites[i] == selected ? Color.WHITE : Color.BLACK);
        canvas.drawText(model.pitch(whites[i]), left + (i + .5f) * width, dp(200), paint);
      }
      int[] blacks = {1, 3, 6, 8, 10}, positions = {1, 2, 4, 5, 6};
      for (int i = 0; i < blacks.length; i++) {
        float center = left + positions[i] * width;
        paint.setColor(blacks[i] == selected ? highlight : Color.BLACK);
        canvas.drawRect(center - width * .28f, dp(147), center + width * .28f, dp(181), paint);
        if (blacks[i] == selected) {
          paint.setColor(Color.WHITE);
          canvas.drawCircle(center, dp(169), dp(4), paint);
        }
      }
      paint.setTextAlign(Paint.Align.LEFT);
    }
  }
}
