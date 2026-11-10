using System.Runtime.InteropServices;

namespace Stroik;

// Optional headphone monitoring. Bounded queue; never changes Windows settings.
internal sealed class AudioMonitor : IDisposable
{
    private readonly object _sync = new();
    private readonly List<(IntPtr Header, IntPtr Data)> _pending = [];
    private IntPtr _device;
    private bool _disposed;
    internal void Write(short[] samples, int rate)
    {
        lock (_sync)
        {
            if (_disposed) return;
            if (_device == IntPtr.Zero)
            {
                var format = new Format { Tag = 1, Channels = 1, Rate = (uint)rate, Bytes = (uint)rate * 2, Align = 2, Bits = 16 };
                Check(waveOutOpen(out _device, uint.MaxValue, ref format, IntPtr.Zero, IntPtr.Zero, 0));
            }
            for (int i = _pending.Count - 1; i >= 0; i--)
                if ((Marshal.PtrToStructure<Header>(_pending[i].Header).Flags & 1) != 0) Free(i);
            if (_pending.Count >= 4) return;
            IntPtr data = Marshal.AllocHGlobal(samples.Length * 2);
            IntPtr header = Marshal.AllocHGlobal(Marshal.SizeOf<Header>());
            Marshal.Copy(samples, 0, data, samples.Length);
            Marshal.StructureToPtr(new Header { Data = data, Length = (uint)samples.Length * 2 }, header, false);
            _pending.Add((header, data));
            Check(waveOutPrepareHeader(_device, header, (uint)Marshal.SizeOf<Header>()));
            Check(waveOutWrite(_device, header, (uint)Marshal.SizeOf<Header>()));
        }
    }
    private void Free(int i)
    {
        var item = _pending[i];
        Check(waveOutUnprepareHeader(_device, item.Header, (uint)Marshal.SizeOf<Header>()));
        Marshal.FreeHGlobal(item.Header); Marshal.FreeHGlobal(item.Data); _pending.RemoveAt(i);
    }
    public void Dispose()
    {
        lock (_sync)
        {
            _disposed = true;
            if (_device == IntPtr.Zero) return;
            Check(waveOutReset(_device));
            for (int i = _pending.Count - 1; i >= 0; i--) Free(i);
            Check(waveOutClose(_device)); _device = IntPtr.Zero;
        }
    }
    private static void Check(int result)
    {
        if (result != 0) throw new InvalidOperationException($"Odsłuch mikrofonu / Microphone monitor: Windows {result}");
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct Format { public ushort Tag, Channels; public uint Rate, Bytes; public ushort Align, Bits, Extra; }
    [StructLayout(LayoutKind.Sequential)]
    private struct Header { public IntPtr Data; public uint Length, Recorded; public UIntPtr User; public uint Flags, Loops; public IntPtr Next; public UIntPtr Reserved; }
    [DllImport("winmm.dll")] private static extern int waveOutOpen(out IntPtr device, uint id, ref Format format, IntPtr callback, IntPtr instance, uint flags);
    [DllImport("winmm.dll")] private static extern int waveOutPrepareHeader(IntPtr device, IntPtr header, uint size);
    [DllImport("winmm.dll")] private static extern int waveOutWrite(IntPtr device, IntPtr header, uint size);
    [DllImport("winmm.dll")] private static extern int waveOutUnprepareHeader(IntPtr device, IntPtr header, uint size);
    [DllImport("winmm.dll")] private static extern int waveOutReset(IntPtr device);
    [DllImport("winmm.dll")] private static extern int waveOutClose(IntPtr device);
}
