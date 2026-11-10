using System.Numerics;

namespace Stroik;

internal readonly record struct IntervalReading(double Lower, double Upper, double Cents, double Beats);
internal static class IntervalAnalysis
{
    internal static int Semitones(Consonance kind) => kind == Consonance.Octave ? 12 : 7;
    internal static IntervalReading Compare(double lower, double upper, Consonance kind)
    {
        double cents = 1200 * Math.Log2(upper / lower) - Semitones(kind) * 100;
        double beats = kind == Consonance.Octave ? Math.Abs(upper - 2 * lower) : Math.Abs(2 * upper - 3 * lower);
        return new(lower, upper, cents, beats);
    }

    // Targeted spectral estimate, deliberately not a polyphonic note recognizer.
    // A harmonic of the lower note can mimic an upper note, especially an octave.
    internal static IntervalReading? Together(short[] samples, int rate, double lower, Consonance kind)
    {
        int n = samples.Length;
        if (n < 32768 || (n & (n - 1)) != 0) return null;
        var bins = new Complex[n];
        double mean = samples.Average(x => (double)x);
        for (int i = 0; i < n; i++) bins[i] = (samples[i] - mean) * (0.5 - 0.5 * Math.Cos(2 * Math.PI * i / (n - 1)));
        for (int i = 1, j = 0; i < n; i++)
        {
            int bit = n >> 1;
            for (; (j & bit) != 0; bit >>= 1) j ^= bit;
            j ^= bit;
            if (i < j) (bins[i], bins[j]) = (bins[j], bins[i]);
        }
        for (int length = 2; length <= n; length <<= 1)
        {
            Complex step = Complex.FromPolarCoordinates(1, -2 * Math.PI / length);
            for (int i = 0; i < n; i += length)
            {
                Complex w = Complex.One;
                for (int j = 0; j < length / 2; j++, w *= step)
                {
                    Complex a = bins[i + j], b = w * bins[i + j + length / 2];
                    bins[i + j] = a + b; bins[i + j + length / 2] = a - b;
                }
            }
        }
        double? Peak(double expected)
        {
            int lo = Math.Max(2, (int)(expected * Math.Pow(2, -60.0 / 1200) * n / rate));
            int hi = Math.Min(n / 2 - 2, (int)Math.Ceiling(expected * Math.Pow(2, 60.0 / 1200) * n / rate));
            if (hi <= lo) return null;
            int best = lo;
            for (int i = lo + 1; i <= hi; i++) if (bins[i].Magnitude > bins[best].Magnitude) best = i;
            if (best == lo || best == hi || bins[best].Magnitude < n * 2) return null;
            double a = Math.Log(Math.Max(1e-12, bins[best - 1].Magnitude));
            double b = Math.Log(Math.Max(1e-12, bins[best].Magnitude));
            double c = Math.Log(Math.Max(1e-12, bins[best + 1].Magnitude));
            double delta = Math.Clamp(0.5 * (a - c) / (a - 2 * b + c), -0.5, 0.5);
            return (best + delta) * rate / n;
        }
        double? first = Peak(lower), second = Peak(lower * Math.Pow(2, Semitones(kind) / 12.0));
        return first is > 0 && second is > 0 ? Compare(first.Value, second.Value, kind) : null;
    }
}
