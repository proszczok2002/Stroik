package pl.proszczok.stroik;

import java.util.*;

public final class CoreChecks {
  private static void require(boolean ok, String text) {
    if (!ok) throw new AssertionError(text);
  }

  public static void main(String[] args) {
    for (String language : Translations.CODES) {
      Model translated = new Model();
      translated.language = language;
      translated.validate();
      require(translated.t("Język", "Language").length() > 0, "Language label");
      require(
          translated.note(67).length() > 3 && translated.position(67).length() > 10,
          "Note and position");
      require(translated.locale().getLanguage().equals(language), "TTS locale");
      if (!language.equals("pl") && !language.equals("en")) {
        require(
            !translated.t("Mikrofon wyłączony", "Microphone off").equals("Microphone off"),
            "Microphone translation missing");
        require(!translated.position(67).contains("white key"), "Position translation missing");
      }
    }
    Model invalidLanguage = new Model();
    invalidLanguage.language = "xx";
    try {
      invalidLanguage.validate();
      throw new AssertionError("Unknown language accepted");
    } catch (IllegalArgumentException expected) {
    }
    Model navigation = new Model();
    navigation.english = false;
    require(navigation.voiceInVisual && !navigation.keepScreenOn, "Speech and screen defaults");
    require(navigation.spokenNote(67).equals("g razkreślne"), "Concise next-note name");
    require(
        navigation.targetLabel(67).contains("pierwszym i drugim")
            && navigation.nextAnnouncement(67).contains("pierwszym i drugim"),
        "Position is missing from note or beginner announcement");
    navigation.beginner = false;
    require(
        navigation.nextAnnouncement(67).equals("g razkreślne"),
        "Classic announcement must stay concise");
    navigation.target = navigation.anchor = 67;
    for (int next = 68; next <= 108; next++) {
      require(navigation.next() == next, "Ascending fork navigation skipped a note");
      navigation.target = next;
    }
    require(navigation.next() == 108, "Navigation exceeded piano range");
    int natural = 60;
    for (int next : new int[] {62, 64, 65, 67, 69, 71, 72}) {
      natural = Model.nextNatural(natural, 1);
      require(natural == next, "White keys must include E-F and B-C");
    }
    for (int next : new int[] {71, 69, 67, 65, 64, 62, 60}) {
      natural = Model.nextNatural(natural, -1);
      require(natural == next, "Descending white keys");
    }
    require(
        Model.nextNatural(21, -1) == 21 && Model.nextNatural(108, 1) == 108, "White-key bounds");
    navigation.interval = 1;
    navigation.target = 101;
    require(navigation.next() == 101, "Fifth exceeds C8");
    navigation.anchor = 100;
    navigation.order = 1;
    require(navigation.next() == 99, "Alternate plan must skip invalid upper pairs");
    require(
        Dsp.level(110) > Dsp.level(440) && Dsp.level(440) > Dsp.level(1760),
        "Bounded pitch-dependent gain");
    require(Dsp.level(27.5) <= .6 && Dsp.level(4186) >= .045, "Fork gain limits");
    for (int midi : new int[] {21, 40, 60, 67, 69, 95, 108}) {
      short[] tone = Dsp.fork(Model.hz(midi, 440), 0, 44100);
      for (short sample : tone) require(Math.abs((int) sample) < 32767, "Fork clipped");
    }
    int count = 0;
    Dsp.ConfirmedStrikes confirmed = new Dsp.ConfirmedStrikes();
    confirmed.arm(440, 5, 0);
    require(
        !confirmed.observe(.03, .008, 440, 400, 1.2), "Attack cannot use stale pitch immediately");
    confirmed.observe(.025, .008, 440, 600, 1.2);
    confirmed.observe(0, .008, 0, 700, 1.2);
    confirmed.observe(.03, .008, 440, 1000, 1.2);
    require(!confirmed.observe(.025, .008, 448, 1200, 1.2), "Detuned second strike advanced");
    confirmed.arm(440, 5, 2000);
    confirmed.observe(.03, .008, 440, 2400, 1.2);
    confirmed.observe(.025, .008, 440, 2600, 1.2);
    confirmed.observe(0, .008, 0, 2700, 1.2);
    confirmed.observe(.03, .008, 440, 3000, 1.2);
    require(confirmed.observe(.025, .008, 440, 3200, 1.2), "Two in-tune strikes did not advance");
    navigation.target = 61;
    navigation.anchor = 60;
    require(navigation.move(-1) == 60, "Previous target shortcut plan");
    for (int rate : new int[] {44100, 48000})
      for (double frequency :
          new double[] {27.5, 41.203, 65.406, 110, 261.625565, 440, 880, 2093, 4186}) {
        short[] samples = new short[8192];
        for (int i = 0; i < samples.length; i++)
          samples[i] = (short) (4000 * Math.sin(2 * Math.PI * frequency * i / rate) + 500);
        double measured = Dsp.pitch(samples, rate);
        require(
            measured > 0 && Math.abs(measured - frequency) < Math.max(.8, frequency * .006),
            "Pitch " + frequency + " at " + rate + ": " + measured);
        count++;
      }
    require(Dsp.pitch(new short[8192], 48000) == 0, "Silence has pitch");
    short[] dc = new short[4096];
    Arrays.fill(dc, (short) 2000);
    require(Dsp.rms(dc, dc.length) == 0, "DC gate");
    Dsp.Session session = new Dsp.Session();
    for (int i = 0; i < 3; i++) session.observe(440, i * 85);
    require(session.observe(0, 400) == 0, "Early result");
    require(Math.abs(session.observe(0, 700) - 440) < .001, "Noise held note");
    require(session.observe(0, 900) == 0, "Duplicate result");
    session.reset();
    for (int i = 0; i < 3; i++) session.observe(330, i * 85);
    require(Math.abs(session.observe(0, 700) - 330) < .001, "Recovery");
    for (int interval : new int[] {1, 2})
      for (int rate : new int[] {44100, 48000}) {
        double lower = 261.625565, upper = lower * Math.pow(2, (interval == 2 ? 12 : 7) / 12.0) + 2;
        short[] pair = new short[65536];
        for (int i = 0; i < pair.length; i++)
          pair[i] =
              (short)
                  (6000 * Math.sin(2 * Math.PI * lower * i / rate)
                      + 4000 * Math.sin(2 * Math.PI * upper * i / rate));
        double[] v = Dsp.together(pair, rate, lower, interval);
        require(
            v != null && Math.abs(v[0] - lower) < .2 && Math.abs(v[1] - upper) < .2,
            "FFT " + rate + "/" + interval);
        count++;
      }
    double fifth = 261.625565 * Math.pow(2, 7 / 12.0);
    require(Dsp.compare(261.625565, fifth, 1)[3] > .5, "Temperament beats");
    short[] fork = Dsp.fork(261.625565, fifth, 44100);
    double[] v = Dsp.together(Arrays.copyOf(fork, 65536), 44100, 261.625565, 1);
    require(v != null && Math.abs(v[1] - fifth) < .2, "Fork pair");
    for (int midi : new int[] {36, 48, 60, 72, 84}) {
      double a = Model.hz(midi, 440), b = Model.hz(midi + 7, 440);
      short[] signal = Arrays.copyOf(Dsp.fork(a, b, 44100), 65536);
      double aa = projection(signal, a), bb = projection(signal, b);
      require(aa > 100 && bb > 100, "Missing fifth fundamentals MIDI " + midi);
      require(
          projection(signal, a / 2) < Math.min(aa, bb) * .05
              && projection(signal, b / 2) < Math.min(aa, bb) * .05,
          "Fifth contains suboctave MIDI " + midi);
    }
    Dsp.Taps taps = new Dsp.Taps();
    require(!taps.observe(.03, .008, 1000, 1.2), "Single strike");
    taps.observe(0, .008, 1200, 1.2);
    require(taps.observe(.02, .008, 1800, 1.2), "Two strikes");
    taps.reset();
    taps.observe(.03, .008, 1000, .5);
    taps.observe(0, .008, 1200, .5);
    require(!taps.observe(.02, .008, 1800, .5), "Overdue strikes");
    System.out.println(
        "Core checks PASS: "
            + count
            + " pitch/interval cases plus silence, noise, DC, recovery, fork and taps.");
  }

  private static double projection(short[] signal, double frequency) {
    double re = 0, im = 0;
    for (int i = 0; i < signal.length; i++) {
      double angle = 2 * Math.PI * frequency * i / 44100;
      re += signal[i] * Math.cos(angle);
      im += signal[i] * Math.sin(angle);
    }
    return Math.hypot(re, im) / signal.length;
  }
}
