using System.Runtime.InteropServices;

namespace Stroik;

// Windows multimedia input without external packages. The event callback only
// wakes the capture thread; all buffer work stays outside the audio driver.
internal sealed class Microphone : IDisposable
{
    private const int SamplesPerBuffer = 4096;
    private const int BufferCount = 4;
    private const uint CallbackEvent = 0x00050000;
    private const uint HeaderDone = 0x00000001;

    private readonly AutoResetEvent _ready = new(false);
    private readonly List<(IntPtr Header, IntPtr Data)> _buffers = [];
    private IntPtr _handle;
    private Thread? _worker;
    private volatile bool _running;
    private bool _disposed;

    internal int SampleRate { get; private set; }
    internal event Action<short[]>? FrameReady;

    internal void Start()
    {
        if (_running) return;
        if (_disposed) throw new ObjectDisposedException(nameof(Microphone));
        try
        {
            int openResult = 0;
            foreach (int rate in new[] { 48000, 44100 })
            {
                var format = new WaveFormat
                {
                    FormatTag = 1,
                    Channels = 1,
                    SamplesPerSec = (uint)rate,
                    AvgBytesPerSec = (uint)(rate * sizeof(short)),
                    BlockAlign = sizeof(short),
                    BitsPerSample = 16
                };
                openResult = waveInOpen(out _handle, uint.MaxValue, ref format,
                    _ready.SafeWaitHandle.DangerousGetHandle(), IntPtr.Zero, CallbackEvent);
                if (openResult == 0)
                {
                    SampleRate = rate;
                    break;
                }
            }
            if (_handle == IntPtr.Zero) throw new InvalidOperationException($"Nie można otworzyć mikrofonu / Cannot open microphone (Windows code {openResult}). Sprawdź uprawnienia mikrofonu / Check microphone permissions.");

            int bytes = SamplesPerBuffer * sizeof(short);
            for (int i = 0; i < BufferCount; i++)
            {
                IntPtr data = Marshal.AllocHGlobal(bytes);
                IntPtr header = Marshal.AllocHGlobal(Marshal.SizeOf<WaveHeader>());
                var value = new WaveHeader { Data = data, BufferLength = (uint)bytes };
                Marshal.StructureToPtr(value, header, false);
                _buffers.Add((header, data));
                Check(waveInPrepareHeader(_handle, header, (uint)Marshal.SizeOf<WaveHeader>()), "Przygotowanie bufora mikrofonu");
                Check(waveInAddBuffer(_handle, header, (uint)Marshal.SizeOf<WaveHeader>()), "Uruchomienie bufora mikrofonu");
            }

            _running = true;
            _worker = new Thread(Receive) { Name = "Stroik microphone", IsBackground = true };
            _worker.Start();
            Check(waveInStart(_handle), "Uruchomienie mikrofonu");
        }
        catch
        {
            Stop();
            throw;
        }
    }

    private void Receive()
    {
        while (_running)
        {
            _ready.WaitOne(200);
            if (!_running) break;
            foreach (var buffer in _buffers)
            {
                var header = Marshal.PtrToStructure<WaveHeader>(buffer.Header);
                if ((header.Flags & HeaderDone) == 0) continue;
                int count = Math.Min((int)header.BytesRecorded / sizeof(short), SamplesPerBuffer);
                if (count > 0)
                {
                    short[] samples = new short[count];
                    Marshal.Copy(buffer.Data, samples, 0, count);
                    try { FrameReady?.Invoke(samples); }
                    catch { /* Keep the capture thread alive; UI handles its own errors. */ }
                }
                if (!_running) break;
                header.BytesRecorded = 0;
                header.Flags &= ~HeaderDone;
                Marshal.StructureToPtr(header, buffer.Header, false);
                if (waveInAddBuffer(_handle, buffer.Header, (uint)Marshal.SizeOf<WaveHeader>()) != 0) break;
            }
        }
    }

    internal void Stop()
    {
        _running = false;
        if (_handle != IntPtr.Zero)
        {
            waveInStop(_handle);
            waveInReset(_handle);
        }
        _ready.Set();
        _worker?.Join(1500);
        _worker = null;
        if (_handle != IntPtr.Zero)
        {
            foreach (var buffer in _buffers)
                waveInUnprepareHeader(_handle, buffer.Header, (uint)Marshal.SizeOf<WaveHeader>());
            waveInClose(_handle);
            _handle = IntPtr.Zero;
        }
        foreach (var buffer in _buffers)
        {
            Marshal.FreeHGlobal(buffer.Header);
            Marshal.FreeHGlobal(buffer.Data);
        }
        _buffers.Clear();
    }

    public void Dispose()
    {
        if (_disposed) return;
        Stop();
        _ready.Dispose();
        _disposed = true;
    }

    private static void Check(int result, string operation)
    {
        if (result != 0) throw new InvalidOperationException($"{operation}: błąd Windows / Windows error {result}.");
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WaveFormat
    {
        public ushort FormatTag;
        public ushort Channels;
        public uint SamplesPerSec;
        public uint AvgBytesPerSec;
        public ushort BlockAlign;
        public ushort BitsPerSample;
        public ushort ExtraSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WaveHeader
    {
        public IntPtr Data;
        public uint BufferLength;
        public uint BytesRecorded;
        public UIntPtr User;
        public uint Flags;
        public uint Loops;
        public IntPtr Next;
        public UIntPtr Reserved;
    }

    [DllImport("winmm.dll")]
    private static extern int waveInOpen(out IntPtr handle, uint deviceId, ref WaveFormat format,
        IntPtr callback, IntPtr instance, uint flags);
    [DllImport("winmm.dll")]
    private static extern int waveInPrepareHeader(IntPtr handle, IntPtr header, uint headerSize);
    [DllImport("winmm.dll")]
    private static extern int waveInUnprepareHeader(IntPtr handle, IntPtr header, uint headerSize);
    [DllImport("winmm.dll")]
    private static extern int waveInAddBuffer(IntPtr handle, IntPtr header, uint headerSize);
    [DllImport("winmm.dll")]
    private static extern int waveInStart(IntPtr handle);
    [DllImport("winmm.dll")]
    private static extern int waveInStop(IntPtr handle);
    [DllImport("winmm.dll")]
    private static extern int waveInReset(IntPtr handle);
    [DllImport("winmm.dll")]
    private static extern int waveInClose(IntPtr handle);
}
