using System.Runtime.InteropServices;

namespace Stroik;

internal sealed class AudioMonitor : IDisposable
{
    private readonly object _sync = new();
    private WindowsAudioStream? _stream;
    private bool _disposed;
    private readonly string _label;

    internal AudioMonitor(string label = "Stroik — microphone monitoring") => _label = label;

    internal void Write(short[] samples, int rate, int volume = 100)
    {
        lock (_sync)
        {
            if (_disposed) return;
            try
            {
                _stream ??= new WindowsAudioStream(rate, WindowsAudioStream.Monitor, _label);
                int count = Math.Min(samples.Length, _stream.Available);
                if (count <= 0) return;
                if (volume != 100)
                {
                    var scaled = new short[count];
                    for (int i = 0; i < count; i++) scaled[i] = (short)(samples[i] * volume / 100);
                    samples = scaled;
                }
                _stream.Write(samples, 0, count);
            }
            catch (COMException error) when ((uint)error.HResult is 0x88890004 or 0x88890026)
            {
                // An endpoint changed while monitoring. Open the current default on the next frame.
                _stream?.Dispose();
                _stream = null;
            }
        }
    }

    public void Dispose()
    {
        lock (_sync)
        {
            _disposed = true;
            _stream?.Dispose();
            _stream = null;
        }
    }
}
