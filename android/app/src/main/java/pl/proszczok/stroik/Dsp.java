package pl.proszczok.stroik;

import java.util.*;

public final class Dsp {
  public static double rms(short[] x, int count) {
    if (count == 0) return 0;
    double mean = 0, sum = 0;
    for (int i = 0; i < count; i++) mean += x[i];
    mean /= count;
    for (int i = 0; i < count; i++) {
      double v = (x[i] - mean) / 32768;
      sum += v * v;
    }
    return Math.sqrt(sum / count);
  }

  public static double pitch(short[] x, int rate) {
    int n = x.length;
    if (n < 4096) return 0;
    int lo = Math.max(2, (int) (rate / 4200.0)),
        hi = Math.min((int) Math.ceil(rate / 27.5), n - 3),
        len = Math.min(2048, n - hi - 2);
    if (len < 512) return 0;
    double[] v = new double[hi + 2];
    double sum = 0;
    for (int t = 1; t <= hi + 1; t++) {
      double diff = 0;
      for (int i = 0; i < len; i++) {
        double d = x[i] - x[i + t];
        diff += d * d;
      }
      sum += diff;
      v[t] = sum > 0 ? diff * t / sum : 1;
    }
    int best = 0;
    for (int t = lo; t < hi; t++)
      if (v[t] < .15) {
        while (t + 1 <= hi && v[t + 1] < v[t]) t++;
        best = t;
        break;
      }
    if (best == 0) {
      double score = .25;
      for (int t = lo; t < hi; t++)
        if (v[t] < score) {
          score = v[t];
          best = t;
        }
    }
    if (best == 0) return 0;
    double divisor = 2 * (2 * v[best] - v[best - 1] - v[best + 1]);
    double adjust =
        divisor == 0 ? 0 : Math.max(-1, Math.min(1, (v[best + 1] - v[best - 1]) / divisor));
    double f = rate / (best + adjust);
    return f >= 27.0 && f <= 4250 ? f : 0;
  }

  public static double median(List<Double> v) {
    double[] s = v.stream().mapToDouble(Double::doubleValue).sorted().toArray();
    return s.length % 2 == 1 ? s[s.length / 2] : (s[s.length / 2 - 1] + s[s.length / 2]) / 2;
  }

  public static final class Session {
    final ArrayList<Double> f = new ArrayList<>();
    long last = -1;
    boolean reported;

    public void reset() {
      f.clear();
      last = -1;
      reported = false;
    }

    public double observe(double frequency, long now) {
      if (frequency > 0) {
        if (reported) reset();
        if (!f.isEmpty() && Math.abs(1200 * Math.log(frequency / median(f)) / Math.log(2)) > 75)
          f.clear();
        f.add(frequency);
        if (f.size() > 12) f.remove(0);
        last = now;
        return 0;
      }
      if (reported || last < 0 || now - last < 450) return 0;
      reported = true;
      return f.size() >= 3 ? median(f) : 0;
    }
  }

  public static final class Taps {
    double envelope;
    long first = -1, last = -1000;
    boolean sound;

    public void reset() {
      first = -1;
      last = -1000;
      sound = false;
      envelope = 0;
    }

    public boolean observe(double rms, double threshold, long now, double gap) {
      boolean active = rms >= threshold * 1.1;
      boolean attack =
          active && (!sound || (envelope > 0 && rms > envelope * 1.7)) && now - last >= 180;
      envelope = envelope == 0 ? rms : envelope * .8 + rms * .2;
      sound = active;
      if (!attack) return false;
      last = now;
      if (first >= 0 && now - first <= gap * 1000) {
        reset();
        return true;
      }
      first = now;
      return false;
    }
  }

  public static double[] compare(double lower, double upper, int interval) {
    return new double[] {
      lower,
      upper,
      1200 * Math.log(upper / lower) / Math.log(2) - (interval == 2 ? 1200 : 700),
      interval == 2 ? Math.abs(upper - 2 * lower) : Math.abs(2 * upper - 3 * lower)
    };
  }

  public static short[] fork(double lower, double upper, int rate) {
    short[] samples = new short[rate * 3];
    for (int i = 0; i < samples.length; i++) {
      double t = i / (double) rate,
          phase = 2 * Math.PI * lower * t,
          env = Math.min(1, t / .012) * Math.exp(-1.6 * t),
          v = level(lower) * env * (Math.sin(phase) + .015 * Math.sin(2 * phase));
      if (upper > 0) v = (v + level(upper) * env * Math.sin(2 * Math.PI * upper * t)) / 2;
      samples[i] = (short) Math.round(v * 32767);
    }
    return samples;
  }

  public static double level(double frequency) {
    return Math.max(.045, Math.min(.34, .14 * Math.sqrt(440 / frequency)));
  }

  // Targeted FFT for two expected spectral components. Not proof of two independent strings.
  public static double[] together(short[] samples, int rate, double lower, int interval) {
    int n = samples.length;
    if (n < 32768 || (n & (n - 1)) != 0) return null;
    double[] r = new double[n], im = new double[n];
    double mean = 0;
    for (short s : samples) mean += s;
    mean /= n;
    for (int i = 0; i < n; i++)
      r[i] = (samples[i] - mean) * (.5 - .5 * Math.cos(2 * Math.PI * i / (n - 1)));
    for (int i = 1, j = 0; i < n; i++) {
      int bit = n >> 1;
      for (; (j & bit) != 0; bit >>= 1) j ^= bit;
      j ^= bit;
      if (i < j) {
        double a = r[i];
        r[i] = r[j];
        r[j] = a;
      }
    }
    for (int len = 2; len <= n; len <<= 1) {
      double ar = Math.cos(-2 * Math.PI / len), ai = Math.sin(-2 * Math.PI / len);
      for (int i = 0; i < n; i += len) {
        double wr = 1, wi = 0;
        for (int j = 0; j < len / 2; j++) {
          int a = i + j, b = a + len / 2;
          double br = wr * r[b] - wi * im[b], bi = wr * im[b] + wi * r[b];
          r[b] = r[a] - br;
          im[b] = im[a] - bi;
          r[a] += br;
          im[a] += bi;
          double nw = wr * ar - wi * ai;
          wi = wr * ai + wi * ar;
          wr = nw;
        }
      }
    }
    double a = peak(r, im, rate, lower),
        b = peak(r, im, rate, lower * Math.pow(2, (interval == 2 ? 12 : 7) / 12.0));
    return a > 0 && b > 0 ? compare(a, b, interval) : null;
  }

  private static double peak(double[] r, double[] im, int rate, double expected) {
    int n = r.length,
        lo = Math.max(2, (int) (expected * Math.pow(2, -60.0 / 1200) * n / rate)),
        hi = Math.min(n / 2 - 2, (int) Math.ceil(expected * Math.pow(2, 60.0 / 1200) * n / rate));
    if (hi <= lo) return 0;
    int best = lo;
    for (int i = lo + 1; i <= hi; i++)
      if (Math.hypot(r[i], im[i]) > Math.hypot(r[best], im[best])) best = i;
    if (best == lo || best == hi || Math.hypot(r[best], im[best]) < n * 2) return 0;
    double a = Math.log(Math.max(1e-12, Math.hypot(r[best - 1], im[best - 1]))),
        b = Math.log(Math.max(1e-12, Math.hypot(r[best], im[best]))),
        c = Math.log(Math.max(1e-12, Math.hypot(r[best + 1], im[best + 1])));
    double d = Math.max(-.5, Math.min(.5, .5 * (a - c) / (a - 2 * b + c)));
    return (best + d) * rate / n;
  }
}
