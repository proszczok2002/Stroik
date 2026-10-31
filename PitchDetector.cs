namespace Stroik;

internal static class PitchDetector
{
    internal static readonly string[] NoteNames = ["C", "Cis", "D", "Dis", "E", "F", "Fis", "G", "Gis", "A", "B", "H"];

    internal static double Rms(ReadOnlySpan<short> samples)
    {
        double sum = 0;
        foreach (short sample in samples)
        {
            double value = sample / 32768.0;
            sum += value * value;
        }
        return Math.Sqrt(sum / samples.Length);
    }

    // YIN: cumulative mean normalized difference and parabolic interpolation.
    internal static (double Frequency, double Confidence)? Estimate(ReadOnlySpan<short> samples, int sampleRate)
    {
        if (samples.Length < 4096 || sampleRate < 8000) return null;
        int length = samples.Length;
        var signal = new double[length];
        for (int i = 0; i < length; i++) signal[i] = samples[i];

        double rate = sampleRate;
        int minTau = Math.Max(2, (int)Math.Floor(rate / 4200));
        int maxTau = Math.Min((int)Math.Ceiling(rate / 27.5), length - 3);
        int comparisonLength = Math.Min(2048, length - maxTau - 2);
        if (comparisonLength < 512) return null;

        var normalized = new double[maxTau + 1];
        double runningSum = 0;
        for (int tau = 1; tau <= maxTau; tau++)
        {
            double difference = 0;
            for (int i = 0; i < comparisonLength; i++)
            {
                double delta = signal[i] - signal[i + tau];
                difference += delta * delta;
            }
            runningSum += difference;
            normalized[tau] = runningSum > 0 ? difference * tau / runningSum : 1;
        }

        int bestTau = 0;
        for (int tau = minTau; tau < maxTau; tau++)
        {
            if (normalized[tau] < 0.15)
            {
                while (tau + 1 <= maxTau && normalized[tau + 1] < normalized[tau]) tau++;
                bestTau = tau;
                break;
            }
        }
        if (bestTau == 0)
        {
            double best = 0.25;
            for (int tau = minTau; tau < maxTau; tau++)
            {
                if (normalized[tau] < best)
                {
                    best = normalized[tau];
                    bestTau = tau;
                }
            }
        }
        if (bestTau == 0) return null;

        double left = normalized[bestTau - 1];
        double center = normalized[bestTau];
        double right = normalized[bestTau + 1];
        double divisor = 2 * (2 * center - left - right);
        double correction = divisor == 0 ? 0 : Math.Clamp((right - left) / divisor, -1, 1);
        double frequency = rate / (bestTau + correction);
        if (!double.IsFinite(frequency) || frequency < 27.5 || frequency > 4200) return null;
        return (frequency, 1 - center);
    }

    internal static Note NoteFromMidi(int midi, double concertA)
    {
        int octave = midi / 12 - 1;
        string name = $"{NoteNames[midi % 12]}{octave}";
        double frequency = concertA * Math.Pow(2, (midi - 69) / 12.0);
        return new Note(midi, name, frequency);
    }

    internal static Note Nearest(double frequency, double concertA)
    {
        int midi = (int)Math.Round(69 + 12 * Math.Log2(frequency / concertA));
        return NoteFromMidi(midi, concertA);
    }

    internal static double Median(IReadOnlyList<double> values)
    {
        double[] sorted = values.OrderBy(x => x).ToArray();
        int mid = sorted.Length / 2;
        return sorted.Length % 2 == 1 ? sorted[mid] : (sorted[mid - 1] + sorted[mid]) / 2;
    }
}

internal readonly record struct Note(int Midi, string Name, double Frequency);

internal sealed class NoteSession
{
    private readonly List<double> _frequencies = [];
    private DateTime? _lastSoundAt;
    private bool _reported;

    internal void Reset()
    {
        _frequencies.Clear();
        _lastSoundAt = null;
        _reported = false;
    }

    internal double? Observe(bool soundPresent, double? frequency, DateTime now)
    {
        if (soundPresent)
        {
            if (_reported) Reset();
            _lastSoundAt = now;
            if (frequency is > 0)
            {
                if (_frequencies.Count > 0)
                {
                    double reference = PitchDetector.Median(_frequencies);
                    if (Math.Abs(1200 * Math.Log2(frequency.Value / reference)) > 75) _frequencies.Clear();
                }
                _frequencies.Add(frequency.Value);
                if (_frequencies.Count > 12) _frequencies.RemoveAt(0);
            }
            return null;
        }

        if (_reported || _lastSoundAt is null || (now - _lastSoundAt.Value).TotalMilliseconds < 450) return null;
        _reported = true;
        return _frequencies.Count >= 3 ? PitchDetector.Median(_frequencies) : null;
    }
}
