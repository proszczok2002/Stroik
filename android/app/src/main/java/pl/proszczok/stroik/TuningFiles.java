package pl.proszczok.stroik;

import java.util.Iterator;
import org.json.JSONObject;
import org.json.JSONException;

final class TuningFiles {
  static final String MAP = "stroik-instrument-map", CALIBRATION = "stroik-fork-calibration";

  static JSONObject create(String format) {
    try {
      return new JSONObject().put("format", format).put("version", 1).put("notes", new JSONObject());
    } catch (JSONException impossible) {
      throw new IllegalStateException(impossible);
    }
  }

  static JSONObject parse(String json, String format) throws JSONException {
    if (json.length() > 100000) throw new JSONException("JSON exceeds 100 KB");
    JSONObject file = new JSONObject(json);
    int version = file.getInt("version");
    if (!format.equals(file.getString("format")) || (version != 1 && !(CALIBRATION.equals(format) && version == 2)))
      throw new JSONException("Invalid Stroik format");
    JSONObject notes = file.getJSONObject("notes");
    if (notes.length() > 88) throw new JSONException("Too many notes");
    for (Iterator<String> keys = notes.keys(); keys.hasNext();) {
      String key = keys.next();
      int midi = Integer.parseInt(key);
      if (midi < 21 || midi > 108 || !key.equals(Integer.toString(midi))) throw new JSONException("Invalid MIDI note");
      if (MAP.equals(format)) {
        double hz = notes.getDouble(key);
        if (!Double.isFinite(hz) || hz < 20 || hz > 10000) throw new JSONException("Invalid frequency");
      } else {
        JSONObject tone = notes.getJSONObject(key);
        double harmonics = tone.getDouble("harmonics"), volume = tone.getDouble("volume");
        double upper = tone.has("upperPartials") ? tone.getDouble("upperPartials") : 100;
        if (upper != (int) upper || upper < 0 || upper > 100 || !Double.isFinite(harmonics) || !Double.isFinite(volume) || harmonics != (int)harmonics || volume != (int)volume ||
            harmonics < 0 || harmonics > 200 || volume < 0 || volume > 150) throw new JSONException("Invalid fork tone");
      }
    }
    return notes;
  }
}
