package pl.proszczok.stroik;

import java.util.*;
import org.json.JSONObject;

public final class Model {
  public String language = "";
  public String instrumentMap = "", forkCalibration = "";
  private JSONObject mappedNotes, calibratedNotes;
  public boolean english,
      dark,
      beginner = true,
      graphics = false,
      autoFork = false,
      advance = true,
      forkTap = false,
      monitor = false,
      updates = true,
      swapped = false,
      voiceInVisual = true,
      keepScreenOn = false;
  public int method = 0,
      instrument = 0,
      target = 67,
      anchor = 67,
      order = 0,
      interval = 0,
      pairMode = 0,
      sensitivity = 1,
      hybridPause = 0,
      forkVolume = 100,
      monitorVolume = 100,
      minutes = 5;
  public double reference = 440,
      tolerance = 5,
      correction = 0,
      gap = 1.2,
      forkSeconds = 3,
      speechRate = 1;
  static final String[] PL = {"c", "cis", "d", "dis", "e", "f", "fis", "g", "gis", "a", "b", "h"};
  static final String[] EN = {
    "C", "C sharp", "D", "D sharp", "E", "F", "F sharp", "G", "G sharp", "A", "B flat", "B"
  };
  static final String[] OCT_PL = {
    "subkontra",
    "kontra",
    "wielkiej",
    "małej",
    "razkreślnej",
    "dwukreślnej",
    "trzykreślnej",
    "czterokreślnej",
    "pięciokreślnej"
  };
  static final String[] OCT_EN = {
    "sub-contra",
    "contra",
    "great",
    "small",
    "one-line",
    "two-line",
    "three-line",
    "four-line",
    "five-line"
  };

  public String code() {
    return language.isEmpty() ? (english ? "en" : "pl") : language;
  }

  public Locale locale() {
    return Locale.forLanguageTag(code());
  }

  public String t(String pl, String en) {
    return Translations.get(code(), pl, en);
  }

  public String note(int midi) {
    if (midi < 21 || midi > 108) return t("Poza zakresem", "Out of range");
    int o = midi / 12 - 1;
    if (!code().equals("pl") && !code().equals("en")) return pitch(midi % 12) + ", " + octave(o);
    return code().equals("en")
        ? EN[midi % 12] + ", " + OCT_EN[o] + " octave"
        : PL[midi % 12] + " w oktawie " + OCT_PL[o];
  }

  public String octave(int o) {
    String[] nominative = {
      "subkontra",
      "kontra",
      "wielka",
      "mała",
      "razkreślna",
      "dwukreślna",
      "trzykreślna",
      "czterokreślna",
      "pięciokreślna"
    };
    switch (code()) {
      case "de":
        return new String[] {
              "Subkontraoktave",
              "Kontraoktave",
              "große Oktave",
              "kleine Oktave",
              "eingestrichene Oktave",
              "zweigestrichene Oktave",
              "dreigestrichene Oktave",
              "viergestrichene Oktave",
              "fünfgestrichene Oktave"
            }
            [o];
      case "ru":
        return new String[] {
              "субконтроктава",
              "контроктава",
              "большая октава",
              "малая октава",
              "первая октава",
              "вторая октава",
              "третья октава",
              "четвёртая октава",
              "пятая октава"
            }
            [o];
      case "fr":
        return "octave " + o;
      case "es":
        return "octava " + o;
      default:
        return code().equals("en") ? OCT_EN[o] + " octave" : "Oktawa " + nominative[o];
    }
  }

  public String spokenNote(int midi) {
    if (!code().equals("pl")) return note(midi);
    return PL[midi % 12]
        + " "
        + new String[] {
              "subkontra",
              "kontra",
              "wielkie",
              "małe",
              "razkreślne",
              "dwukreślne",
              "trzykreślne",
              "czterokreślne",
              "pięciokreślne"
            }
            [midi / 12 - 1];
  }

  public static double hz(int midi, double a) {
    return a * Math.pow(2, (midi - 69) / 12.0);
  }

  public double targetHz(int midi) {
    return mappedNotes == null ? hz(midi, reference) : mappedNotes.optDouble(Integer.toString(midi), hz(midi, reference));
  }

  public int tone(int midi, String setting) {
    JSONObject note = calibratedNotes == null ? null : calibratedNotes.optJSONObject(Integer.toString(midi));
    return note == null ? 100 : note.optInt(setting, 100);
  }

  public void refreshTuningFiles() {
    try {
      mappedNotes = instrumentMap.isEmpty() ? null : TuningFiles.parse(instrumentMap, TuningFiles.MAP);
      calibratedNotes = forkCalibration.isEmpty() ? null : TuningFiles.parse(forkCalibration, TuningFiles.CALIBRATION);
    } catch (Exception invalid) { throw new IllegalArgumentException("Invalid Stroik tuning file", invalid); }
  }

  public int semitones() {
    return interval == 2 ? 12 : 7;
  }

  public boolean strings() {
    return instrument != 0 && instrument != 5;
  }

  public int[] stringsMidi() {
    switch (instrument) {
      case 1:
        return new int[] {40, 45, 50, 55, 59, 64};
      case 2:
        return swapped ? new int[] {69, 60, 64, 67} : new int[] {67, 60, 64, 69};
      case 3:
        return new int[] {55, 62, 69, 76};
      case 4:
        return new int[] {28, 33, 38, 43};
      default:
        return new int[0];
    }
  }

  public int resolved(double frequency) {
    if (target >= 21) return target;
    int n = (int) Math.round(69 + 12 * Math.log(frequency / reference) / Math.log(2));
    if (strings() || mappedNotes != null) {
      double best = Double.MAX_VALUE;
      int[] candidates = strings() ? stringsMidi() : java.util.stream.IntStream.rangeClosed(21, 108).toArray();
      for (int v : candidates) {
        double d = Math.abs(Math.log(frequency / targetHz(v)));
        if (d < best) {
          best = d;
          n = v;
        }
      }
    }
    return Math.max(21, Math.min(108, n));
  }

  public int base() {
    return target < 21 ? (strings() ? stringsMidi()[0] : 67) : target;
  }

  public int next() {
    return move(1);
  }

  public int move(int direction) {
    if (direction != -1 && direction != 1) throw new IllegalArgumentException("Direction");
    int highest = method == 1 || interval == 0 ? 108 : 108 - semitones();
    if (strings()) {
      int[] n = stringsMidi();
      for (int i = 0; i < n.length; i++)
        if (n[i] == base())
          for (int j = i + direction; j >= 0 && j < n.length; j += direction)
            if (n[j] <= highest) return n[j];
      return base();
    }
    if (order == 0)
      return (direction > 0 || base() + direction >= anchor)
              && base() + direction >= 21
              && base() + direction <= highest
          ? base() + direction
          : base();
    List<Integer> p = new ArrayList<>();
    p.add(anchor);
    int step = order == 2 ? 2 : 1;
    for (int first = 1; first <= 88; first += 5)
      for (int dir : new int[] {1, -1})
        for (int i = first; i < first + 5; i++) {
          int v = anchor + dir * i * step;
          if (v >= 21 && v <= 108) p.add(v);
        }
    int at = p.indexOf(base());
    if (at >= 0)
      for (int i = at + direction; i >= 0 && i < p.size(); i += direction)
        if (p.get(i) <= highest) return p.get(i);
    return base();
  }

  public static int nextNatural(int current, int direction) {
    if (direction != -1 && direction != 1) throw new IllegalArgumentException("Direction");
    for (int midi = current + direction; midi >= 21 && midi <= 108; midi += direction) {
      int pitch = midi % 12;
      if (pitch == 0
          || pitch == 2
          || pitch == 4
          || pitch == 5
          || pitch == 7
          || pitch == 9
          || pitch == 11) return midi;
    }
    return current;
  }

  public String position(int midi) {
    String[] pl = {
      "biały przed grupą dwóch czarnych",
      "pierwszy czarny w grupie dwóch",
      "biały między dwoma czarnymi",
      "drugi czarny w grupie dwóch",
      "biały za grupą dwóch czarnych",
      "biały przed grupą trzech czarnych",
      "pierwszy czarny w grupie trzech",
      "biały między pierwszym i drugim czarnym w grupie trzech",
      "drugi czarny w grupie trzech",
      "biały między drugim i trzecim czarnym w grupie trzech",
      "trzeci czarny w grupie trzech",
      "biały za grupą trzech czarnych"
    };
    String[] en = {
      "white before two black keys",
      "first black of two",
      "white between two black keys",
      "second black of two",
      "white after two black keys",
      "white before three black keys",
      "first black of three",
      "white between first and second black of three",
      "second black of three",
      "white between second and third black of three",
      "third black of three",
      "white after three black keys"
    };
    String[] full = {
      "white key immediately before a group of two black keys",
      "first black key in a group of two",
      "white key between the two black keys in a group of two",
      "second black key in a group of two",
      "white key immediately after a group of two black keys",
      "white key immediately before a group of three black keys",
      "first black key in a group of three",
      "white key between the first and second black keys in a group of three",
      "second black key in a group of three",
      "white key between the second and third black keys in a group of three",
      "third black key in a group of three",
      "white key immediately after a group of three black keys"
    };
    return t(pl[midi % 12], full[midi % 12]);
  }

  public String pitch(int n) {
    switch (code()) {
      case "de":
        return new String[] {"C", "Cis", "D", "Dis", "E", "F", "Fis", "G", "Gis", "A", "B", "H"}[n];
      case "ru":
        return new String[] {
              "до",
              "до-диез",
              "ре",
              "ре-диез",
              "ми",
              "фа",
              "фа-диез",
              "соль",
              "соль-диез",
              "ля",
              "си-бемоль",
              "си"
            }
            [n];
      case "fr":
        return new String[] {
              "do",
              "do dièse",
              "ré",
              "ré dièse",
              "mi",
              "fa",
              "fa dièse",
              "sol",
              "sol dièse",
              "la",
              "si bémol",
              "si"
            }
            [n];
      case "es":
        return new String[] {
              "do",
              "do sostenido",
              "re",
              "re sostenido",
              "mi",
              "fa",
              "fa sostenido",
              "sol",
              "sol sostenido",
              "la",
              "si bemol",
              "si"
            }
            [n];
      default:
        return (code().equals("pl") ? PL : EN)[n];
    }
  }

  public static int nextBlack(int current, int direction) {
    for (int next = current + direction; next >= 21 && next <= 108; next += direction) {
      int pitch = next % 12;
      if (pitch == 1 || pitch == 3 || pitch == 6 || pitch == 8 || pitch == 10) return next;
    }
    return current;
  }

  public String guide() {
    return note(base()) + ": " + position(base());
  }

  public String beginnerGuidance() {
    String common =
        t(
            "Zbyt słabe lub zbyt mocne uderzenia mogą powodować przekłamane odczyty. Graj"
                + " równomiernie, bez przesterowania. ",
            "Strikes that are too weak or too strong can cause inaccurate readings. Play evenly"
                + " without clipping. ");
    if (instrument == 0)
      return common
          + t(
              "Stare pianino powinien ocenić technik przed podnoszeniem naciągu. Zacznij od środka,"
                  + " zmieniaj strony równomiernie i wytłumiaj pozostałe struny.",
              "Have an old piano assessed before raising tension. Start near the middle, work"
                  + " evenly on both sides and mute unused strings.") + " " + t(
              "Dla pianin powojennych A4 = 440 Hz to zwykły punkt odniesienia. Pianina przedwojenne mogły być przewidziane do niższego stroju. Przed podniesieniem stroju starego pianina poproś technika o ocenę instrumentu.",
              "A4 = 440 Hz is the usual reference for postwar pianos. Prewar pianos may have been designed for lower pitch. Ask a technician to assess an old piano before raising its pitch.");
    return common
        + (strings()
            ? t(
                "Graj jedną pustą strunę naraz. Kolejność strun i ich nazwy są na liście dźwięków.",
                "Play one open string at a time. String order and names are in the note list.")
            : t("Graj jeden dźwięk naraz.", "Play one note at a time."));
  }

  public String methodGuidance() {
    switch (method) {
      case 0: return t("Kamerton: wzorzec do strojenia na słuch. W trybie ręcznym Alt+U powtarza dźwięk, a automat odtwarza go cyklicznie. Pole dwóch stuknięć samodzielnie włącza i wyłącza nasłuch mikrofonu.", "Tuning fork: a reference for tuning by ear. In manual mode Alt+U repeats the note; automatic mode repeats it at intervals. The double-strike checkbox starts and stops microphone listening.");
      case 1: return t("Stroik elektroniczny: pomiar mikrofonem bez wzorca. Graj jedną nutę i pozwól jej ucichnąć; wynik podaje kierunek i odchyłkę. Przydatny do samodzielnego strojenia i kontroli wysokości.", "Electronic tuner: microphone measurement without a reference tone. Play one note and let it stop; the result gives direction and deviation. Useful for independent tuning and pitch checks.");
      case 2: return t("Hybryda z głośnikami: nauka strojenia na słuch bez słuchawek. Kamerton i mikrofon działają na przemian. Po wyniku program czeka na zakończenie mowy i wybraną przerwę. Najpierw wysłuchaj wskazówek.", "Speaker hybrid: learn tuning by ear without headphones. The reference and microphone alternate. After a result, the app waits for speech and the chosen pause. Listen to guidance first.");
      default: return t("Hybryda ze słuchawkami: słyszysz wzorzec, a mikrofon jednocześnie mierzy instrument. Opcjonalny odsłuch pomaga słyszeć instrument w słuchawkach. Załóż słuchawki przed jego włączeniem, aby uniknąć sprzężenia.", "Headphone hybrid: hear the reference while the microphone measures the instrument. Optional monitoring helps you hear the instrument in headphones. Put headphones on before enabling it to prevent feedback.");
    }
  }

  public String targetLabel(int midi) {
    return note(midi) + (beginner && instrument == 0 ? ": " + position(midi) : "");
  }

  public String nextAnnouncement(int midi) {
    return spokenNote(midi) + (beginner && instrument == 0 ? ": " + position(midi) : "");
  }

  public JSONObject json() {
    JSONObject j = new JSONObject();
    try {
      for (java.lang.reflect.Field f : Model.class.getFields())
        if (!java.lang.reflect.Modifier.isStatic(f.getModifiers())) j.put(f.getName(), f.get(this));
      j.put("profileVersion", 1);
    } catch (Exception e) {
      throw new IllegalStateException(e);
    }
    return j;
  }

  public static Model from(JSONObject j) throws Exception {
    if (j.optInt("profileVersion", 1) != 1) throw new IllegalArgumentException("Profile version");
    Model m = new Model();
    for (java.lang.reflect.Field f : Model.class.getFields()) {
      if (java.lang.reflect.Modifier.isStatic(f.getModifiers()) || !j.has(f.getName())) continue;
      if (f.getType() == String.class) f.set(m, j.getString(f.getName()));
      else if (f.getType() == boolean.class) f.setBoolean(m, j.getBoolean(f.getName()));
      else if (f.getType() == int.class) f.setInt(m, j.getInt(f.getName()));
      else if (f.getType() == double.class) f.setDouble(m, j.getDouble(f.getName()));
    }
    m.validate();
    return m;
  }

  public void validate() {
    refreshTuningFiles();
    if (language == null || (!language.isEmpty() && !Translations.supported(language)))
      throw new IllegalArgumentException("Language");
    if (method < 0
        || method > 3
        || instrument < 0
        || instrument > 5
        || target < -1
        || target > 108
        || (target >= 0 && target < 21)
        || anchor < 21
        || anchor > 108
        || order < 0
        || order > 2
        || interval < 0
        || interval > 2
        || pairMode < 0
        || pairMode > 1
        || sensitivity < 0
        || sensitivity > 2
        || hybridPause < 0
        || hybridPause > 60
        || forkVolume < 0 || forkVolume > 100 || monitorVolume < 0 || monitorVolume > 100
        || minutes < 1
        || minutes > 1440
        || !Double.isFinite(reference)
        || reference < 390
        || reference > 466
        || !Double.isFinite(tolerance)
        || tolerance < 1
        || tolerance > 20
        || !Double.isFinite(correction)
        || Math.abs(correction) > 100
        || !Double.isFinite(gap)
        || gap < .5
        || gap > 2.5
        || !Double.isFinite(forkSeconds)
        || forkSeconds < 1
        || forkSeconds > 10
        || !Double.isFinite(speechRate)
        || speechRate < .5
        || speechRate > 3)
      throw new IllegalArgumentException("Invalid settings / Błędne ustawienia");
  }

  public Model copy() {
    try {
      return from(json());
    } catch (Exception e) {
      throw new IllegalStateException(e);
    }
  }
}
