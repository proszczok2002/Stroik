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
  private TextView heading, result, guide, status;
  private ScrollView scroll;
  private Spinner targetList, octaveList;
  private final Button[] tabs = new Button[3];
  private Button microphoneButton;
  private TuningSession session;
  private LinearLayout targetsBox;
  private int targetIds;
  private Boolean announcedMicState;
  private String consonanceLabel;
  private Spinner consonanceList;
  private final Map<Spinner, int[]> selections = new IdentityHashMap<>();
  private View pairControls, micControls, forkControls, monitorControls, advanceControls;
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
              && (model.method == 0 || model.method == 3)
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
    if (firstLaunch) model.english = !Locale.getDefault().getLanguage().equals("pl");
    if (state != null) {
      int oldPage = state.getInt("stroik.page", 0);
      page =
          state.getInt("stroik.layout", 1) < 2
              ? (oldPage < 2 ? 0 : oldPage - 1)
              : Math.max(0, Math.min(2, oldPage));
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
    ArrayAdapter<String> a = new ArrayAdapter<>(this, android.R.layout.simple_spinner_item, items) {
      @Override public View getView(int position, View reusable, ViewGroup parent) {
        View selected = super.getView(position, reusable, parent);
        // The collapsed list is one labelled accessibility stop. Popup rows
        // still use getDropDownView and retain their ordinary accessibility.
        selected.setImportantForAccessibility(View.IMPORTANT_FOR_ACCESSIBILITY_NO_HIDE_DESCENDANTS);
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
    TextView l = text(label, false);
    l.setImportantForAccessibility(View.IMPORTANT_FOR_ACCESSIBILITY_NO);
    EditText e = new EditText(this);
    e.setId(nextId++);
    l.setLabelFor(e.getId());
    e.setHint(label);
    e.setSingleLine(true);
    e.setTextSize(18);
    e.setMinHeight(dp(64));
    e.setInputType(
        android.text.InputType.TYPE_CLASS_NUMBER
            | android.text.InputType.TYPE_NUMBER_FLAG_DECIMAL
            | (min < 0 ? android.text.InputType.TYPE_NUMBER_FLAG_SIGNED : 0));
    e.setSelectAllOnFocus(true);
    e.setText(String.valueOf(value));
    Runnable commit =
        () -> {
          try {
            double n = Double.parseDouble(e.getText().toString().trim().replace(',', '.'));
            if (!Double.isFinite(n) || n < min || n > max) throw new IllegalArgumentException();
            change.accept(n);
            changed();
          } catch (Exception ex) {
            e.setText(String.valueOf(value));
            message(t("Wpisz wartość od ", "Enter a value from ") + min + t(" do ", " to ") + max);
          }
        };
    e.setOnFocusChangeListener((v, focus) -> {
      if (!focus) commit.run();
    });
    e.setOnEditorActionListener((v, action, event) -> {
      commit.run();
      return false;
    });
    body.addView(e, new LinearLayout.LayoutParams(-1, -2));
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
    visible(micControls, model.method != 0 || model.forkTap);
    visible(forkControls, model.method != 1);
    visible(monitorControls, model.method == 3);
    visible(advanceControls, model.method == 1);
    if (focused != null)
      focused.post(() -> {
        if (focused.isAttachedToWindow() && focused.isShown()) {
          focused.requestRectangleOnScreen(
              new Rect(0, 0, focused.getWidth(), focused.getHeight()), true);
          if (!focused.isAccessibilityFocused()) focused.performAccessibilityAction(
              android.view.accessibility.AccessibilityNodeInfo.ACTION_ACCESSIBILITY_FOCUS, null);
        }
      });
    updateGuide();
  }

  private void updateInstrumentSections() {
    visible(ukuleleControls, model.instrument == 2);
    visible(orderControls, !model.strings());
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
      int light = WindowInsetsController.APPEARANCE_LIGHT_STATUS_BARS
          | WindowInsetsController.APPEARANCE_LIGHT_NAVIGATION_BARS;
      bars.setSystemBarsAppearance(model.dark ? 0 : light, light);
    }
  }

  private void referenceControl() {
    String name = t("Częstotliwość odniesienia A, Hz", "A reference frequency, Hz");
    TextView label = text(name, false);
    label.setText(name + ": " + String.format(model.english ? Locale.US : new Locale("pl"), "%.1f Hz", model.reference));
    label.setImportantForAccessibility(View.IMPORTANT_FOR_ACCESSIBILITY_NO);
    SeekBar slider =
        new SeekBar(this) {
          @Override
          public void onInitializeAccessibilityNodeInfo(
              android.view.accessibility.AccessibilityNodeInfo info) {
            super.onInitializeAccessibilityNodeInfo(info);
            info.setRangeInfo(
                android.view.accessibility.AccessibilityNodeInfo.RangeInfo.obtain(
                    android.view.accessibility.AccessibilityNodeInfo.RangeInfo.RANGE_TYPE_FLOAT,
                    390,
                    466,
                    (float) model.reference));
            info.addAction(
                new android.view.accessibility.AccessibilityNodeInfo.AccessibilityAction(
                    android.view.accessibility.AccessibilityNodeInfo.ACTION_CLICK,
                    t("Wpisz częstotliwość", "Enter frequency")));
          }

          @Override
          public boolean performAccessibilityAction(int action, Bundle args) {
            if (action == android.view.accessibility.AccessibilityNodeInfo.ACTION_CLICK) {
              editReference();
              return true;
            }
            if (action
                    == android.view.accessibility.AccessibilityNodeInfo.AccessibilityAction
                        .ACTION_SET_PROGRESS
                        .getId()
                && args != null) {
              float value =
                  args.getFloat(
                      android.view.accessibility.AccessibilityNodeInfo
                          .ACTION_ARGUMENT_PROGRESS_VALUE);
              if (!Float.isFinite(value) || value < 390 || value > 466) return false;
              model.reference = Math.round(value * 10) / 10.0;
              setProgress((int) Math.round((model.reference - 390) * 10));
              setStateDescription(String.format(Locale.US, "%.1f Hz", model.reference));
              label.setText(name + ": " + String.format(model.english ? Locale.US : new Locale("pl"), "%.1f Hz", model.reference));
              changed();
              return true;
            }
            if (action == android.view.accessibility.AccessibilityNodeInfo.ACTION_SCROLL_FORWARD
                || action
                    == android.view.accessibility.AccessibilityNodeInfo.ACTION_SCROLL_BACKWARD) {
              model.reference =
                  Math.max(
                      390,
                      Math.min(
                          466,
                          model.reference
                              + (action
                                      == android.view.accessibility.AccessibilityNodeInfo
                                          .ACTION_SCROLL_FORWARD
                                  ? .1
                                  : -.1)));
              setProgress((int) Math.round((model.reference - 390) * 10));
              setStateDescription(String.format(Locale.US, "%.1f Hz", model.reference));
              label.setText(name + ": " + String.format(model.english ? Locale.US : new Locale("pl"), "%.1f Hz", model.reference));
              changed();
              return true;
            }
            return super.performAccessibilityAction(action, args);
          }
        };
    slider.setId(nextId++);
    slider.setTag(label);
    slider.setContentDescription(name);
    label.setLabelFor(slider.getId());
    slider.setMinHeight(dp(64));
    slider.setMax(760);
    slider.setKeyProgressIncrement(1);
    slider.setProgress((int) Math.round((model.reference - 390) * 10));
    slider.setStateDescription(
        String.format(model.english ? Locale.US : new Locale("pl"), "%.1f Hz", model.reference));
    slider.setOnSeekBarChangeListener(
        new SeekBar.OnSeekBarChangeListener() {
          public void onStartTrackingTouch(SeekBar bar) {}

          public void onStopTrackingTouch(SeekBar bar) {}

          public void onProgressChanged(SeekBar bar, int progress, boolean user) {
            if (!user) return;
            model.reference = 390 + progress / 10.0;
            label.setText(name + ": " + String.format(model.english ? Locale.US : new Locale("pl"), "%.1f Hz", model.reference));
            bar.setStateDescription(
                String.format(
                    model.english ? Locale.US : new Locale("pl"), "%.1f Hz", model.reference));
            changed();
          }
        });
    GestureDetector doubleTap =
        new GestureDetector(
            this,
            new GestureDetector.SimpleOnGestureListener() {
              @Override
              public boolean onDown(android.view.MotionEvent event) {
                return true;
              }

              @Override
              public boolean onDoubleTap(android.view.MotionEvent event) {
                editReference();
                return true;
              }
            });
    slider.setOnTouchListener((view, event) -> {
      doubleTap.onTouchEvent(event);
      return false;
    });
    slider.setOnKeyListener((view, key, event) -> {
      if (event.getAction() == KeyEvent.ACTION_UP
          && (key == KeyEvent.KEYCODE_ENTER || key == KeyEvent.KEYCODE_DPAD_CENTER)) {
        editReference();
        return true;
      }
      return false;
    });
    body.addView(slider, new LinearLayout.LayoutParams(-1, -2));
    text(
        t(
            "Przesuń suwak; dwukrotne stuknięcie otwiera wpis liczbowy. TalkBack: gest zmiany"
                + " wartości; dwukrotne stuknięcie — klawiatura.",
            "Drag the slider; double tap for numeric entry. TalkBack: use the adjust-value gesture;"
                + " double tap opens the keyboard."),
        false);
  }

  private void editReference() {
    EditText field = new EditText(this);
    field.setInputType(
        android.text.InputType.TYPE_CLASS_NUMBER | android.text.InputType.TYPE_NUMBER_FLAG_DECIMAL);
    field.setHint("390–466 Hz");
    field.setText(String.valueOf(model.reference));
    field.selectAll();
    AlertDialog dialog =
        new AlertDialog.Builder(this)
            .setTitle(t("Częstotliwość odniesienia, Hz", "Reference frequency, Hz"))
            .setView(field)
            .setPositiveButton(t("Zapisz", "Save"), null)
            .setNegativeButton(t("Anuluj", "Cancel"), null)
            .create();
    dialog.setOnShowListener(unused -> {
      field.requestFocus();
      dialog
          .getWindow()
          .setSoftInputMode(WindowManager.LayoutParams.SOFT_INPUT_STATE_ALWAYS_VISIBLE);
      dialog
          .getButton(AlertDialog.BUTTON_POSITIVE)
          .setOnClickListener(view -> {
            try {
              double value =
                  Double.parseDouble(field.getText().toString().trim().replace(',', '.'));
              if (!Double.isFinite(value) || value < 390 || value > 466)
                throw new IllegalArgumentException();
              model.reference = value;
              changed();
              for (int i = 0; i < body.getChildCount(); i++) {
                View child = body.getChildAt(i);
                if (child instanceof SeekBar) {
                  ((SeekBar) child).setProgress((int) Math.round((value - 390) * 10));
                  child.setStateDescription(String.format(Locale.US, "%.1f Hz", value));
                  if (child.getTag() instanceof TextView) ((TextView) child.getTag()).setText(
                      t("Częstotliwość odniesienia A, Hz", "A reference frequency, Hz") + ": "
                      + String.format(model.english ? Locale.US : new Locale("pl"), "%.1f Hz", value));
                }
              }
              dialog.dismiss();
            } catch (Exception error) {
              field.setError(t("Wpisz 390–466 Hz", "Enter 390–466 Hz"));
            }
          });
    });
    dialog.show();
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
    String[] pages = {t("Stroik", "Tuner"), t("Ustawienia", "Settings"), t("Pomoc", "Help")};
    heading.setText("Stroik " + version() + " — " + pages[page]);
    result = guide = status = null;
    consonanceList = null;
    meter = null;
    targetList = octaveList = null;
    targetsBox = null;
    microphoneButton = null;
    pairControls = micControls = forkControls = monitorControls = advanceControls = null;
    ukuleleControls = orderControls = null;
    monitorCheck = null;
    if (page == 0) tuner();
    else if (page == 1) settings();
    else help();
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
    root.post(() -> {
      if (generation != renderGeneration || isFinishing() || isDestroyed()) return;
      scroll.scrollTo(0, scrollY);
      View target = restore && focusId != View.NO_ID ? root.findViewById(focusId) : null;
      if (target != null && target.isShown() && !target.isAccessibilityFocused())
        target.performAccessibilityAction(
            android.view.accessibility.AccessibilityNodeInfo.ACTION_ACCESSIBILITY_FOCUS, null);
    });
    root.requestApplyInsets();
  }

  private void styleTabs() {
    for (int i = 0; i < tabs.length; i++) {
      tabs[i].setTypeface(null, i == page ? Typeface.BOLD : Typeface.NORMAL);
      tabs[i].setTextColor(i == page ? Color.WHITE : model.dark ? Color.WHITE : Color.BLACK);
      android.graphics.drawable.GradientDrawable tabBackground = new android.graphics.drawable.GradientDrawable();
      tabBackground.setColor(i == page ? Color.rgb(0, 83, 145) : model.dark ? Color.rgb(48, 48, 48) : Color.rgb(235, 240, 245));
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
      view.setBackgroundTintList(android.content.res.ColorStateList.valueOf(
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
    heading.setTextSize(24);
    heading.setTextColor(model.dark ? Color.WHITE : Color.BLACK);
    heading.setPadding(dp(16), dp(12), dp(16), dp(12));
    heading.setAccessibilityHeading(true);
    root.addView(heading);
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
      b.setSingleLine(true);
      b.setEllipsize(android.text.TextUtils.TruncateAt.END);
      b.setPadding(dp(2), dp(4), dp(2), dp(4));
      b.setOnClickListener(v -> {
        if (page == at) return;
        getCurrentFocusClear();
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
    if (event.getAction() == KeyEvent.ACTION_DOWN && handleKey(event.getKeyCode(), event)) return true;
    return super.dispatchKeyEvent(event);
  }

  @Override
  public boolean onKeyDown(int key, KeyEvent event) {
    return handleKey(key, event) || super.onKeyDown(key, event);
  }

  private boolean handleKey(int key, KeyEvent event) {
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
        advance();
        return true;
      }
    }
    if (event.isAltPressed() && key == KeyEvent.KEYCODE_N) {
      advance();
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
      model.target = key == KeyEvent.KEYCODE_DPAD_UP || key == KeyEvent.KEYCODE_DPAD_DOWN
          ? Model.nextNatural(model.base(), key == KeyEvent.KEYCODE_DPAD_UP ? -1 : 1)
          : Math.max(21, Math.min(108, model.base() + (key == KeyEvent.KEYCODE_DPAD_LEFT ? -1 : 1)));
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
    consonanceList = choice(
        consonanceLabel,
        new String[] {t("Wyłączone", "Off"), t("Kwinty", "Fifths"), t("Oktawy", "Octaves")},
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
    targetIds = nextId;
    targetsBox = (LinearLayout) section(this::targets);
    guide = text("", false);
    updateGuide();
    if (model.beginner)
      text(
          t(
              "Zbyt słabe lub zbyt mocne uderzenia mogą powodować przekłamane odczyty. Graj"
                  + " równomiernie, bez przesterowania. Stare pianino powinien ocenić technik przed"
                  + " podnoszeniem naciągu. Zacznij od środka, zmieniaj strony równomiernie i"
                  + " wytłumiaj pozostałe struny.",
              "Strikes that are too weak or too strong can cause inaccurate readings. Play evenly"
                  + " without clipping. Have an old piano assessed before raising tension. Start"
                  + " near the middle, work evenly on both sides and mute unused strings."),
          false);
    status =
        text(
            audio.running()
                ? t("Mikrofon włączony", "Microphone on")
                : t("Mikrofon wyłączony", "Microphone off"),
            false);
    micControls =
        section(() -> {
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
          check(
              t(
                  "Dwukrotne stuknięcie przechodzi do kolejnego dźwięku zgodnego z wybraną"
                      + " kolejnością",
                  "Two strikes advance to the next note in the selected order"),
              model.forkTap,
              v -> {
                model.forkTap = v;
                updateTunerSections();
              });
        });
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
    advanceControls =
        section(() ->
            check(
                t(
                    "Po dobrym wyniku dwa uderzenia wybierają następną nutę",
                    "After an in-tune result two strikes advance"),
                model.advance,
                v -> model.advance = v));
    number(
        t(
            "Maksymalny odstęp dwóch uderzeń, sekundy (0,5–2,5)",
            "Maximum gap between two strikes, seconds (0.5–2.5)"),
        model.gap,
        .5,
        2.5,
        v -> model.gap = v);
    button(t("Następny dźwięk", "Next note"), this::advance);
    result =
        text(
            lastResult.isEmpty()
                ? t(
                    "Wynik pojawi się po wyciszeniu nuty.",
                    "The result appears after the note stops.")
                : lastResult,
            false);
    result.setTextIsSelectable(true);
    result.setContentDescription(t("Ostatni wynik: ", "Last result: ") + result.getText());
    button(
        t("Powtórz wynik", "Repeat result"),
        () -> say(lastResult.isEmpty() ? t("Brak wyniku.", "No result.") : lastResult));
    button(
        t("Sprawdź komunikat czytnika ekranu", "Test screen reader announcement"),
        () -> say(t("Stroik. Komunikaty są dostępne.", "Stroik. Announcements are available.")));
    if (model.graphics) {
      meter = new Meter(this);
      meter.setImportantForAccessibility(View.IMPORTANT_FOR_ACCESSIBILITY_NO);
      body.addView(meter, new LinearLayout.LayoutParams(-1, dp(160)));
    }
    button(t("Zapisz plik ustawień", "Save settings file"), this::saveProfile);
    button(t("Wczytaj plik ustawień", "Open settings file"), this::loadProfile);
    updateTunerSections();
  }

  private void targets() {
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
              + model.note(options[i]);
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
              if (octaveList != null) selectSilently(octaveList, model.base() / 12 - 1);
              updateGuide();
            });
    targetList.setTag(options);
  }

  private void syncTarget() {
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
    if (guide == null) return;
    String value = model.instrument == 0 ? model.guide() : model.note(model.base());
    if (model.interval != 0) {
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
                  ? t("Kamerton gra oba dźwięki razem. Wybrana nuta jest dolnym dźwiękiem pary.", "The fork plays both notes together. The selected note is the lower note of the pair.")
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
    if (consonanceList != null) consonanceList.setStateDescription(model.interval == 0 ? null
        : model.spokenNote(model.base()) + " + " + model.note(model.base() + model.semitones()));
    if (!value.contentEquals(guide.getText())) guide.setText(value);
  }

  private void instrument() {
    choice(
        t("Instrument", "Instrument"),
        new String[] {
          t("Pianino / fortepian", "Piano"),
          t("Gitara", "Guitar"),
          "Ukulele",
          t("Skrzypce", "Violin"),
          t("Gitara basowa", "Bass guitar"),
          t("Chromatyczny", "Chromatic")
        },
        model.instrument,
        v -> {
          model.instrument = v;
          model.target = model.strings() ? model.stringsMidi()[0] : 67;
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
                  model.anchor = model.base();
                }));
    referenceControl();
    text(
        t(
            "Standard to 440 Hz. Wiek pianina nie określa bezpiecznej wysokości stroju; przed"
                + " podniesieniem naciągu starego instrumentu poproś technika o ocenę. Ukulele G/A"
                + " można zamienić. Do innych strojów użyj trybu chromatycznego.",
            "440 Hz is standard. A piano's age alone does not determine a safe pitch; have a"
                + " technician assess old instruments before raising tension. Ukulele G/A can be"
                + " swapped. Use chromatic mode for other tunings."),
        false);
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
    updateInstrumentSections();
  }

  private void settings() {
    choice(
        t("Język", "Language"),
        new String[] {"Polski", "English"},
        model.english ? 1 : 0,
        v -> {
          model.english = v == 1;
          render();
        });
    check(
        t("Ciemny motyw", "Dark theme"),
        model.dark,
        v -> {
          model.dark = v;
          setTheme(model.dark ? android.R.style.Theme_Material_NoActionBar
              : android.R.style.Theme_Material_Light_NoActionBar);
          applyThemeInPlace(root);
          root.setBackgroundColor(model.dark ? Color.rgb(30, 30, 30) : Color.WHITE);
          styleTabs();
        });
    check(
        t("Widok dla początkujących z podpowiedziami", "Beginner view with guidance"),
        model.beginner,
        v -> model.beginner = v);
    check(
        t("Pokazuj wskaźnik na żywo i klawiaturę", "Show live meter and keyboard"),
        model.graphics,
        v -> {
          model.graphics = v;
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
    check(
        t("Odczyt głosowy w trybie wizualnym", "Spoken feedback in visual mode"),
        model.voiceInVisual,
        v -> model.voiceInVisual = v);
    check(
        t("Nie pozwalaj na autoblokadę ekranu", "Keep screen awake"),
        model.keepScreenOn,
        v -> model.keepScreenOn = v);
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
                asset(model.english ? "changes.en.txt" : "changes.pl.txt"),
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
    text(asset(model.english ? "guide.en.txt" : "guide.pl.txt"), false);
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

  private void startMic() {
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
    if (model.method == 0 && !model.forkTap) {
      message(
          t(
              "Włącz nasłuch dwóch stuknięć albo wybierz stroik elektroniczny.",
              "Enable tap listening or choose electronic tuning."));
      return;
    }
    try {
      session.configure(model);
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
    lastResult = text;
    if (result != null) {
      result.setText(text);
      result.setContentDescription(t("Ostatni wynik: ", "Last result: ") + text);
    }
    say(text);
  }

  public void reading(double cents) {
    if (meter != null) {
      meter.cents = cents;
      meter.invalidate();
    }
  }

  public void advance() {
    int next = model.next();
    if (next == model.base()) {
      say(t("Koniec wybranej kolejności.", "End of selected order."));
      return;
    }
    model.target = next;
    changed();
    syncTarget();
    sayAction(model.spokenNote(next), targetList);
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
    if (announcedMicState != null && announcedMicState != audio.running()) sayAction(value, microphoneButton);
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
    if (!session.serviceActive) audio.stop();
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
    double cents;
    final Paint paint = new Paint(3);

    Meter(Context c) {
      super(c);
    }

    protected void onDraw(Canvas c) {
      super.onDraw(c);
      paint.setColor(model.dark ? Color.WHITE : Color.BLACK);
      paint.setTextSize(dp(22));
      c.drawText(String.format(model.english ? Locale.US : new Locale("pl"),
          model.t("%+.1f centów", "%+.1f cents"), cents), dp(8), dp(30), paint);
      float middle = getWidth() / 2f;
      paint.setStrokeWidth(dp(3));
      c.drawLine(middle, dp(40), middle, dp(95), paint);
      paint.setColor(
          Math.abs(cents) <= model.tolerance ? Color.rgb(0, 150, 90) : Color.rgb(180, 70, 0));
      float x = (float) (middle + Math.max(-50, Math.min(50, cents)) / 50 * (middle - dp(16)));
      c.drawCircle(x, dp(70), dp(10), paint);
      int selected = model.base() % 12;
      int[] whites = {0, 2, 4, 5, 7, 9, 11};
      float width = getWidth() / 7f;
      for (int i = 0; i < whites.length; i++) {
        paint.setColor(whites[i] == selected ? Color.rgb(0, 130, 220) : Color.WHITE);
        c.drawRect(i * width, dp(105), (i + 1) * width - 2, dp(158), paint);
      }
      int[] blacks = {1, 3, 6, 8, 10}, positions = {1, 2, 4, 5, 6};
      for (int i = 0; i < blacks.length; i++) {
        paint.setColor(blacks[i] == selected ? Color.rgb(0, 130, 220) : Color.BLACK);
        float center = positions[i] * width;
        c.drawRect(center - width * .28f, dp(105), center + width * .28f, dp(138), paint);
      }
    }
  }
}
