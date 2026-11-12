using System.Media;
using System.Text;

namespace Stroik;

// Short, decaying tone with a gentle attack and weak harmonics, inspired by a struck tuning fork.
internal sealed class TuningFork : IDisposable
{
    private SoundPlayer? _player;
    private MemoryStream? _wave;

    internal void Strike(double frequency, double? upper = null)
    {
        Stop();
        _wave = BuildWave(frequency, upper);
        _player = new SoundPlayer(_wave);
        _player.Play();
    }

    internal void Stop()
    {
        _player?.Stop();
        _player?.Dispose();
        _player = null;
        _wave?.Dispose();
        _wave = null;
    }

    internal static MemoryStream BuildWave(double frequency, double? upper = null)
    {
        if (!double.IsFinite(frequency) || frequency is < 20 or > 10000)
            throw new ArgumentOutOfRangeException(nameof(frequency));
        if (upper is double high && (!double.IsFinite(high) || high is < 20 or > 10000))
            throw new ArgumentOutOfRangeException(nameof(upper));
        const int rate = 44100, seconds = 3, count = rate * seconds;
        var stream = new MemoryStream(44 + count * 2);
        using (var writer = new BinaryWriter(stream, Encoding.ASCII, true))
        {
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + count * 2);
            writer.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)1);
            writer.Write(rate);
            writer.Write(rate * 2);
            writer.Write((short)2);
            writer.Write((short)16);
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(count * 2);
            for (int i = 0; i < count; i++)
            {
                double time = (double)i / rate;
                double attack = Math.Min(1, time / 0.012);
                double envelope = attack * Math.Exp(-1.6 * time);
                double phase = 2 * Math.PI * frequency * time;
                double sample = Level(frequency) * envelope * (Math.Sin(phase) + 0.015 * Math.Sin(2 * phase));
                if (upper is double second) sample = (sample + Level(second) * envelope * Math.Sin(2 * Math.PI * second * time)) / 2;
                writer.Write((short)Math.Round(Math.Clamp(sample, -1, 1) * short.MaxValue));
            }
        }
        stream.Position = 0;
        return stream;
    }

    internal static double Level(double frequency) => Math.Clamp(0.14 * Math.Sqrt(440 / frequency), 0.045, 0.34);

    public void Dispose() => Stop();
}
