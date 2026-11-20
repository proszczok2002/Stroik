using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Stroik;

// Unmodified NV Access controller client. No add-on or changes to NVDA are required.
internal static class NvdaReader
{
    private const string Digest = "8C8D9E1952C8B52820988081EA58C26175E69C0FBB5A09E8F2034AAE70D55F0A";
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate uint Running();
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate uint Speaking(out byte value);
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)] private delegate uint Speak([MarshalAs(UnmanagedType.LPWStr)] string text, int symbols, int priority, byte asynchronous);
    private sealed record Client(Running Running, Speaking Speaking, Speak Speak);
    private static readonly Lazy<Client?> Api = new(Load);
    private static Client? Load()
    {
        try
        {
            string? replacement = Environment.GetEnvironmentVariable("STROIK_NVDA_CLIENT");
            string file;
            if (!string.IsNullOrWhiteSpace(replacement)) file = Path.GetFullPath(replacement);
            else
            {
                string dataDirectory = PortableData.DataDirectory(PortableData.ProgramDirectory);
                string clientDirectory = Path.Combine(dataDirectory, "nvda-client");
                string versionDirectory = Path.Combine(clientDirectory, "2026.3.1");
                file = Path.Combine(versionDirectory, "nvdaControllerClient.dll");
                using var embedded = typeof(NvdaReader).Assembly.GetManifestResourceStream("Stroik.nvdaControllerClient.dll")!;
                using var memory = new MemoryStream(); embedded.CopyTo(memory); byte[] data = memory.ToArray();
                if (Convert.ToHexString(SHA256.HashData(data)) != Digest) throw new InvalidDataException("NVDA client checksum");
                PortableData.EnsureDataDirectory(dataDirectory);
                PortableData.EnsureDataDirectory(clientDirectory);
                PortableData.EnsureDataDirectory(versionDirectory);
                if (File.Exists(file)) PortableData.RejectLink(file);
                if (!File.Exists(file) || Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file))) != Digest)
                {
                    string temporary = file + "." + Guid.NewGuid().ToString("N"); File.WriteAllBytes(temporary, data); File.Move(temporary, file, true);
                }
            }
            nint library = NativeLibrary.Load(file);
            return new(Marshal.GetDelegateForFunctionPointer<Running>(NativeLibrary.GetExport(library, "nvdaController_testIfRunning")),
                Marshal.GetDelegateForFunctionPointer<Speaking>(NativeLibrary.GetExport(library, "nvdaController_isSpeaking")),
                Marshal.GetDelegateForFunctionPointer<Speak>(NativeLibrary.GetExport(library, "nvdaController_speakSsml")));
        }
        catch { return null; }
    }
    internal static bool Available { get { try { return DefaultDesktop() && Api.Value?.Running() == 0; } catch { return false; } } }
    internal static bool IsSpeaking { get { try { return Available && Api.Value!.Speaking(out byte value) == 0 && value != 0; } catch { return false; } } }
    internal static async Task<bool> ReadAsync(string text, string language = "pl")
    {
        if (!Available) return false;
        string xml = "<speak><voice xml:lang=\"" + UiLanguages.Culture(language).Name + "\">" + System.Security.SecurityElement.Escape(text) + "</voice></speak>";
        try { return await Task.Run(() => DefaultDesktop() && Api.Value!.Speak(xml, 0, 0, 0) == 0).WaitAsync(TimeSpan.FromSeconds(120)); }
        catch { return false; }
    }
    // Do not send application text to NVDA on a lock/secure desktop.
    private static bool DefaultDesktop()
    {
        nint desktop = OpenInputDesktop(0, false, 1);
        if (desktop == 0) return false;
        try { var name = new StringBuilder(128); return GetUserObjectInformation(desktop, 2, name, name.Capacity * 2, out _) && name.ToString() == "Default"; }
        finally { CloseDesktop(desktop); }
    }
    [DllImport("user32.dll", SetLastError = true)] private static extern nint OpenInputDesktop(uint flags, bool inherit, uint access);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetUserObjectInformationW")] private static extern bool GetUserObjectInformation(nint handle, int index, StringBuilder value, int length, out int needed);
    [DllImport("user32.dll")] private static extern bool CloseDesktop(nint handle);
}
