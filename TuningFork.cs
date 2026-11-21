using System.Media;
using System.Runtime.InteropServices;
using System.Text;

namespace Stroik;

// Short, decaying tone with a gentle attack and weak harmonics, inspired by a struck tuning fork.
internal sealed class TuningFork : IDisposable
{
    private MemoryStream? _wave;
    private CancellationTokenSource? _playback;
    private Task? _player;
    private SoundPlayer? _fallback;
    private readonly Guid _session;

    internal TuningFork(bool calibrator = false) => _session = calibrator ? WindowsAudioStream.Calibrator : WindowsAudioStream.Fork;

    internal void Strike(double frequency, double? upper = null, ForkTone? lowerTone = null, ForkTone? upperTone = null, int volume = 100,
        string label = "Stroik — tuning fork")
    {
        Stop();
        _wave = BuildWave(frequency, upper, lowerTone, upperTone, volume);
        var pcm = new short[(_wave.Length - 44) / 2];
        Buffer.BlockCopy(_wave.GetBuffer(), 44, pcm, 0, pcm.Length * 2);
        _playback = new CancellationTokenSource();
        CancellationToken cancellation = _playback.Token;
        var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _player = Task.Run(() => Play(pcm, label, cancellation, ready));
        try { ready.Task.WaitAsync(TimeSpan.FromSeconds(2)).GetAwaiter().GetResult(); }
        catch (COMException)
        {
            _playback.Cancel();
            _fallback = new SoundPlayer(_wave);
            _fallback.Play();
        }
        catch { Stop(); throw; }
    }

    private void Play(short[] pcm, string label, CancellationToken cancellation, TaskCompletionSource ready)
    {
        try
        {
            using var output = new WindowsAudioStream(44100, _session, label);
            int offset = 0;
            while (!cancellation.IsCancellationRequested && (offset < pcm.Length || !output.Empty))
            {
                int count = Math.Min(output.Available, pcm.Length - offset);
                output.Write(pcm, offset, count);
                offset += count;
                if (offset > 0) ready.TrySetResult();
                if (cancellation.WaitHandle.WaitOne(8)) break;
            }
            ready.TrySetCanceled(cancellation);
        }
        catch (Exception error) { ready.TrySetException(error); }
    }

    internal void Stop()
    {
        _fallback?.Stop();
        _fallback?.Dispose();
        _fallback = null;
        _playback?.Cancel();
        if (_player is not null && !_player.Wait(TimeSpan.FromMilliseconds(150)))
        {
            CancellationTokenSource pending = _playback!;
            _ = _player.ContinueWith(_ => pending.Dispose(), TaskScheduler.Default);
        }
        else _playback?.Dispose();
        _playback = null;
        _player = null;
        _wave?.Dispose();
        _wave = null;
    }

    internal static MemoryStream BuildWave(double frequency, double? upper = null, ForkTone? lowerTone = null, ForkTone? upperTone = null, int volume = 100)
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
            ForkTone lower = lowerTone ?? ForkTone.Default, higher = upperTone ?? ForkTone.Default;
            double lowerLevel = Level(frequency) * lower.Volume / 100, upperLevel = upper is double hz ? Level(hz) * higher.Volume / 100 : 0;
            for (int i = 0; i < count; i++)
            {
                double time = (double)i / rate;
                double attack = Math.Min(1, time / 0.012);
                double envelope = attack * Math.Min(1, (seconds - time) / 0.08) * Math.Exp(-1.2 * time);
                double sample = lowerLevel * Oscillator(frequency, time, lower.Harmonics, lower.UpperPartials ?? 100);
                if (upper is double second) sample = (sample + upperLevel * Oscillator(second, time, higher.Harmonics, higher.UpperPartials ?? 100)) / 1.55;
                sample *= envelope * volume / 100;
                writer.Write((short)Math.Round(Math.Clamp(sample, -1, 1) * short.MaxValue));
            }
        }
        stream.Position = 0;
        return stream;
    }

    internal static double Level(double frequency) => Math.Clamp(0.18 * Math.Pow(440 / frequency, 0.65), 0.045, 0.6);
    internal static double Oscillator(double frequency, double time, int harmonics = 100, int upperPartials = 100)
    {
        double phase = 2 * Math.PI * frequency * time;
        // Quiet upper partials help small speakers reproduce bass identity without transposition.
        double support = Math.Clamp(Math.Log2(110 / frequency), 0, 1);
        double bass = Math.Clamp(Math.Log2(160 / frequency), 0, 1);
        bass = bass * bass * (3 - 2 * bass);
        double second = .015 + .105 * bass + .18 * support;
        double third = .06 * bass + .16 * support;
        double strength = harmonics / 100.0;
        double extra = Math.Max(0, strength - 1) * (1 - support);
        return (Math.Sin(phase) + (strength * second + .18 * extra) * Math.Sin(2 * phase)
            + (strength * third + .10 * extra) * Math.Sin(3 * phase)
            + strength * support * (.26 * Math.Sin(4 * phase) + upperPartials / 100.0 * (.14 * Math.Sin(5 * phase) + .20 * Math.Sin(6 * phase) + .12 * Math.Sin(8 * phase))))
            / (1 + .62 * support * strength);

    }

    public void Dispose() => Stop();
}
