package pl.proszczok.stroik;

import java.util.*;
import org.json.JSONObject;

public final class Model {
  public boolean english,
      dark,
      beginner = true,
      graphics = false,
      autoFork = false,
      advance = true,
      forkTap = false,
      monitor = false,
      updates = true,
      swapped = false;
  public int method = 0,
      instrument = 0,
      target = 67,
      anchor = 67,
      order = 0,
      interval = 0,
      pairMode = 0,
      sensitivity = 1,
      minutes = 5;
  public double reference = 440, tolerance = 5, correction = 0, gap = 1.2, forkSeconds = 3;
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

  public String t(String pl, String en) {
    return english ? en : pl;
  }

  public String note(int midi) {
    if (midi < 21 || midi > 108) return t("Poza zakresem", "Out of range");
    int o = midi / 12 - 1;
    return english
        ? EN[midi % 12] + ", " + OCT_EN[o] + " octave"
        : PL[midi % 12] + " w oktawie " + OCT_PL[o];
  }

  public String octave(int o) {
    return english ? OCT_EN[o] + " octave" : "Oktawa " + OCT_PL[o];
  }

  public static double hz(int midi, double a) {
    return a * Math.pow(2, (midi - 69) / 12.0);
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
    if (strings()) {
      double best = Double.MAX_VALUE;
      for (int v : stringsMidi()) {
        double d = Math.abs(Math.log(frequency / hz(v, reference)));
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
    if (strings()) {
      int[] n = stringsMidi();
      for (int i = 0; i < n.length - 1; i++) if (n[i] == base()) return n[i + 1];
      return base();
    }
    if (order == 0) return Math.min(108, base() + 1);
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
    return at >= 0 && at + 1 < p.size() ? p.get(at + 1) : base();
  }

  public String guide() {
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
    return note(base()) + ": " + (english ? en : pl)[base() % 12];
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
      if (f.getType() == boolean.class) f.setBoolean(m, j.getBoolean(f.getName()));
      else if (f.getType() == int.class) f.setInt(m, j.getInt(f.getName()));
      else if (f.getType() == double.class) f.setDouble(m, j.getDouble(f.getName()));
    }
    m.validate();
    return m;
  }

  public void validate() {
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
        || forkSeconds > 10)
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
