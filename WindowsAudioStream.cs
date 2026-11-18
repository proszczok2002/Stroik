using System.Runtime.InteropServices;

namespace Stroik;

// Shared WASAPI rendering with a dedicated, named Windows mixer session.
internal sealed class WindowsAudioStream : IDisposable
{
    internal static readonly Guid Fork = new("f57e35dc-e17f-477e-850b-a06b59db2032");
    internal static readonly Guid Monitor = new("1a5fb0e8-52dc-4eb5-90f5-437a4e287cd7");
    internal static readonly Guid Calibrator = new("988dda84-63a9-4f67-a9b5-b9feefe05baa");
    private IAudioClient? _client;
    private IAudioRenderClient? _renderer;
    private readonly uint _capacity;
    private bool _started;

    internal WindowsAudioStream(int rate, Guid id, string label)
    {
        IMMDeviceEnumerator? enumerator = null;
        IMMDevice? device = null;
        IAudioSessionControl? session = null;
        try
        {
            enumerator = (IMMDeviceEnumerator)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("bcde0395-e52f-467c-8e3d-c4579291692e"), true)!)!;
            enumerator.GetDefaultAudioEndpoint(0, 1, out device);
            Guid clientId = typeof(IAudioClient).GUID;
            device.Activate(ref clientId, 23, IntPtr.Zero, out object client);
            _client = (IAudioClient)client;
            var format = new WaveFormat { Tag = 1, Channels = 1, SamplesPerSecond = (uint)rate, BytesPerSecond = (uint)rate * 2, BlockAlign = 2, Bits = 16 };
            const uint convertPcm = 0x80000000, highQualityConversion = 0x08000000;
            _client.Initialize(0, convertPcm | highQualityConversion, 1_000_000, 0, ref format, ref id);
            _client.GetBufferSize(out _capacity);
            Guid sessionId = typeof(IAudioSessionControl).GUID;
            _client.GetService(ref sessionId, out object sessionService);
            session = (IAudioSessionControl)sessionService;
            Guid context = Guid.Empty;
            session.SetDisplayName(label, ref context);
            session.SetGroupingParam(ref id, ref context);
            Guid renderId = typeof(IAudioRenderClient).GUID;
            _client.GetService(ref renderId, out object renderer);
            _renderer = (IAudioRenderClient)renderer;
        }
        catch { Dispose(); throw; }
        finally { Release(session); Release(device); Release(enumerator); }
    }

    internal int Available
    {
        get { _client!.GetCurrentPadding(out uint padding); return (int)(_capacity - padding); }
    }

    internal bool Empty => Available == _capacity;

    internal void Write(short[] pcm, int offset, int count)
    {
        if (count == 0) return;
        _renderer!.GetBuffer((uint)count, out IntPtr data);
        try { Marshal.Copy(pcm, offset, data, count); }
        finally { _renderer.ReleaseBuffer((uint)count, 0); }
        if (!_started) { _client!.Start(); _started = true; }
    }

    public void Dispose()
    {
        if (_started) { try { _client?.Stop(); } catch (COMException) { } }
        Release(_renderer); _renderer = null;
        Release(_client); _client = null;
        _started = false;
    }

    private static void Release(object? com) { if (com is not null) Marshal.ReleaseComObject(com); }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    private struct WaveFormat
    {
        internal ushort Tag, Channels;
        internal uint SamplesPerSecond, BytesPerSecond;
        internal ushort BlockAlign, Bits, ExtraSize;
    }

    [ComImport, Guid("a95664d2-9614-4f35-a746-de8db63617e6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMMDeviceEnumerator
    {
        void EnumAudioEndpoints(int flow, uint states, out IntPtr devices);
        void GetDefaultAudioEndpoint(int flow, int role, out IMMDevice device);
    }

    [ComImport, Guid("d666063f-1587-4e43-81f1-b948e807363f"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMMDevice
    {
        void Activate(ref Guid iid, uint context, IntPtr parameters, [MarshalAs(UnmanagedType.IUnknown)] out object instance);
    }

    [ComImport, Guid("1cb9ad4c-dbfa-4c32-b178-c2f568a703b2"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IAudioClient
    {
        void Initialize(int shareMode, uint flags, long duration, long periodicity, ref WaveFormat format, ref Guid session);
        void GetBufferSize(out uint frames);
        void GetStreamLatency(out long latency);
        void GetCurrentPadding(out uint frames);
        void IsFormatSupported(int shareMode, ref WaveFormat format, out IntPtr closest);
        void GetMixFormat(out IntPtr format);
        void GetDevicePeriod(out long defaultPeriod, out long minPeriod);
        void Start();
        void Stop();
        void Reset();
        void SetEventHandle(IntPtr handle);
        void GetService(ref Guid iid, [MarshalAs(UnmanagedType.IUnknown)] out object service);
    }

    [ComImport, Guid("f294acfc-3146-4483-a7bf-addca7c260e2"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IAudioRenderClient
    {
        void GetBuffer(uint frames, out IntPtr address);
        void ReleaseBuffer(uint frames, uint flags);
    }

    [ComImport, Guid("f4b1a599-7266-4319-a8ca-e70acb11e8cd"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IAudioSessionControl
    {
        void GetState(out int state);
        void GetDisplayName(out IntPtr name);
        void SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string name, ref Guid context);
        void GetIconPath(out IntPtr path);
        void SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string path, ref Guid context);
        void GetGroupingParam(out Guid grouping);
        void SetGroupingParam(ref Guid grouping, ref Guid context);
    }
}
