package pl.proszczok.stroik;

import android.Manifest;
import android.app.*;
import android.content.*;
import android.content.pm.PackageManager;
import android.graphics.*;
import android.media.AudioManager;
import android.net.Uri;
import android.os.*;
import android.speech.tts.TextToSpeech;
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
  private final Button[] tabs = new Button[4];
  private final Map<Spinner, int[]> selections = new IdentityHashMap<>();
  private View pairControls, micControls, forkControls, monitorControls, advanceControls;
  private View ukuleleControls, orderControls;
  private CheckBox monitorCheck;
  private int renderedPage = -1, renderGeneration, restoredFocus = View.NO_ID;
  private int focusBeforeRecreate = View.NO_ID;
  private AudioEngine audio;
  private Updater updater;
  private TextToSpeech speech;
  private boolean speechReady, autoPaused;
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
    if (!getSharedPreferences("stroik", 0).contains("settings"))
      model.english = !Locale.getDefault().getLanguage().equals("pl");
    if (state != null) {
      page = Math.max(0, Math.min(3, state.getInt("stroik.page", 0)));
      lastResult = state.getString("stroik.result", "");
      autoPaused = state.getBoolean("stroik.autoPaused", false);
      restoredFocus = state.getInt("stroik.accessibilityFocus", View.NO_ID);
    }
    setTheme(
        model.dark
            ? android.R.style.Theme_Material_NoActionBar
            : android.R.style.Theme_Material_Light_NoActionBar);
    getWindow().setSoftInputMode(WindowManager.LayoutParams.SOFT_INPUT_ADJUST_RESIZE);
    audio = new AudioEngine(model, this, (AudioManager) getSystemService(AUDIO_SERVICE));
    updater = new Updater(this);
    speech =
        new TextToSpeech(
            this,
            code -> {
              speechReady = code == TextToSpeech.SUCCESS;
            });
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
  }

  @Override
  protected void onSaveInstanceState(Bundle state) {
    state.putInt("stroik.page", page);
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
    audio.configure(model);
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
    Spinner s = new Spinner(this);
    s.setId(nextId++);
    s.setMinimumHeight(dp(64));
    l.setLabelFor(s.getId());
    s.setContentDescription(label);
    ArrayAdapter<String> a = new ArrayAdapter<>(this, android.R.layout.simple_spinner_item, items);
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
    visible(pairControls, model.interval != 0 && model.method != 0);
    visible(micControls, model.method != 0 || model.forkTap);
    visible(forkControls, model.method != 1);
    visible(monitorControls, model.method == 3);
    visible(advanceControls, model.method == 1);
    View focused = accessibilityFocus(root);
    if (focused != null)
      focused.post(() -> {
        if (focused.isAccessibilityFocused() && focused.isShown())
          focused.requestRectangleOnScreen(
              new Rect(0, 0, focused.getWidth(), focused.getHeight()), true);
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
    String[] pages = {
      t("Stroik", "Tuner"),
      t("Instrument", "Instrument"),
      t("Ustawienia", "Settings"),
      t("Pomoc", "Help")
    };
    heading.setText("Stroik " + version() + " — " + pages[page]);
    result = guide = status = null;
    meter = null;
    targetList = octaveList = null;
    pairControls = micControls = forkControls = monitorControls = advanceControls = null;
    ukuleleControls = orderControls = null;
    monitorCheck = null;
    if (page == 0) tuner();
    else if (page == 1) instrument();
    else if (page == 2) settings();
    else help();
    for (int i = 0; i < tabs.length; i++) {
      tabs[i].setText(pages[i]);
      tabs[i].setSelected(i == page);
      tabs[i].setContentDescription(pages[i] + t(", zakładka", ", tab"));
    }
    renderedPage = page;
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
    nav.setOrientation(1);
    LinearLayout navigationRow = null;
    for (int i = 0; i < 4; i++) {
      if (i % 2 == 0) {
        navigationRow = new LinearLayout(this);
        navigationRow.setOrientation(0);
        nav.addView(navigationRow, new LinearLayout.LayoutParams(-1, -2));
      }
      final int at = i;
      Button b = new Button(this);
      tabs[i] = b;
      b.setId(10000 + i);
      b.setAllCaps(false);
      b.setMinHeight(dp(64));
      b.setTextSize(16);
      b.setOnClickListener(v -> {
        if (page == at) return;
        getCurrentFocusClear();
        page = at;
        render();
      });
      navigationRow.addView(b, new LinearLayout.LayoutParams(0, -2, 1));
    }
    root.addView(nav);
  }

  private void getCurrentFocusClear() {
    View focus = getCurrentFocus();
    if (focus != null) focus.clearFocus();
  }

  @Override
  public boolean onKeyDown(int key, KeyEvent event) {
    if (event.isCtrlPressed()) {
      if (key == KeyEvent.KEYCODE_Q) {
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
      changed();
      syncTarget();
      return true;
    }
    return super.onKeyDown(key, event);
  }

  private void tuner() {
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
          audio.stop();
          model.method = v;
          if (status != null) status.setText(t("Mikrofon wyłączony", "Microphone off"));
          if (model.target < 0 && v != 1) model.target = model.base();
          updateTunerSections();
        });
    choice(
        t("Sprawdź współbrzmienie", "Check consonance"),
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
    targets();
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
          button(t("Włącz mikrofon", "Start microphone"), this::startMic);
          button(
              t("Wyłącz mikrofon", "Stop microphone"),
              () -> {
                audio.stop();
                status(t("Mikrofon wyłączony", "Microphone off"));
              });
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
    if (!model.strings()) {
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
                  ? t("Kamerton gra oba dźwięki razem.", "The fork plays both notes together.")
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
    number(
        t("Częstotliwość odniesienia A, Hz (390–466)", "A reference frequency, Hz (390–466)"),
        model.reference,
        390,
        466,
        v -> model.reference = v);
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
          View focused = accessibilityFocus(root);
          focusBeforeRecreate = focused == null ? View.NO_ID : focused.getId();
          persist();
          recreate();
        });
    check(
        t("Widok dla początkujących z podpowiedziami", "Beginner view with guidance"),
        model.beginner,
        v -> model.beginner = v);
    check(
        t("Pokazuj wskaźnik na żywo i klawiaturę", "Show live meter and keyboard"),
        model.graphics,
        v -> model.graphics = v);
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
    if (model.method == 0 && !model.forkTap) {
      message(
          t(
              "Włącz nasłuch dwóch stuknięć albo wybierz stroik elektroniczny.",
              "Enable tap listening or choose electronic tuning."));
      return;
    }
    try {
      audio.configure(model);
      audio.start();
      status(
          t(
              "Mikrofon włączony. Graj pojedynczą nutę i wycisz ją.",
              "Microphone on. Play one note and let it stop."));
    } catch (Exception e) {
      audio.stop();
      message(e.getMessage());
    }
  }

  @Override
  public void onRequestPermissionsResult(int request, String[] permissions, int[] grants) {
    super.onRequestPermissionsResult(request, permissions, grants);
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
    if (text == null || text.isEmpty() || !active) return;
    AccessibilityManager a = (AccessibilityManager) getSystemService(ACCESSIBILITY_SERVICE);
    if (a.isEnabled() && a.isTouchExplorationEnabled()) root.announceForAccessibility(text);
    else if (speechReady) {
      speech.setLanguage(model.english ? Locale.US : new Locale("pl"));
      speech.speak(text, TextToSpeech.QUEUE_FLUSH, null, "stroik-result");
    }
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
    say(t("Następny dźwięk: ", "Next note: ") + model.note(next));
    if (model.method != 1) audio.strike();
  }

  public void failed(String text) {
    audio.stop();
    status(t("Mikrofon zatrzymany: ", "Microphone stopped: ") + text);
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
        audio.stop();
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
    audio.stop();
    if (speech != null) speech.stop();
    persist();
    super.onPause();
  }

  @Override
  protected void onDestroy() {
    updater.close();
    audio.close();
    if (speech != null) speech.shutdown();
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
      c.drawText(String.format(Locale.US, "%+.1f cents", cents), dp(8), dp(30), paint);
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
