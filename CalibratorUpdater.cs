using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;

namespace Stroik;

internal sealed record CalibratorRelease(Version Version, Uri Executable, string Sha256, long Size);

internal static class CalibratorUpdater
{
    internal const string ExecutableName = "Stroik-Kalibrator.exe";
    private const string Repository = "proszczok2002/Stroik";
    private static readonly Uri Manifest = new($"https://raw.githubusercontent.com/{Repository}/main/calibrator-update.json");
    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(15) };

    static CalibratorUpdater() => Client.DefaultRequestHeaders.UserAgent.ParseAdd("Stroik-Calibrator-Updater/1.0");

    internal static async Task<CalibratorRelease> FetchAsync()
    {
        using var response = await Client.GetAsync(Manifest, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        using var data = new MemoryStream();
        await using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        await Updater.CopyWithDeadlineAsync(stream, data, TimeSpan.FromSeconds(8), maxBytes: 4096).ConfigureAwait(false);
        return ParseManifest(data.ToArray());
    }

    internal static CalibratorRelease ParseManifest(byte[] data)
    {
        using var json = JsonDocument.Parse(data);
        JsonElement root = json.RootElement;
        string? tag = root.GetProperty("releaseTag").GetString();
        string? hash = root.GetProperty("sha256").GetString();
        long size = root.GetProperty("size").GetInt64();
        if (root.GetProperty("schema").GetInt32() != 1 || !Version.TryParse(root.GetProperty("version").GetString(), out Version? version) ||
            version.Build < 0 || version.Revision >= 0 || !ValidTag(tag, version) ||
            size is < 1_000_000 or > 100_000_000 || hash is not { Length: 64 } || !hash.All(Uri.IsHexDigit))
            throw new InvalidDataException("Invalid calibrator release manifest.");
        return new(version, new Uri($"https://github.com/{Repository}/releases/download/{tag}/{ExecutableName}"), hash, size);
    }

    private static bool ValidTag(string? tag, Version version)
    {
        if (tag == $"calibrator-v{version}") return true;
        return tag is not null && tag.StartsWith('v') && Version.TryParse(tag.AsSpan(1), out Version? windowsVersion) &&
            tag == $"v{windowsVersion}" && windowsVersion.Build >= 0 && windowsVersion.Revision < 0;
    }

    internal static Version? InstalledVersion(string directory) => ReadVersion(Path.Combine(directory, ExecutableName));

    private static Version? ReadVersion(string path)
    {
        FileVersionInfo info;
        try { info = FileVersionInfo.GetVersionInfo(path); }
        catch (FileNotFoundException) { return null; }
        if (!string.Equals(info.OriginalFilename, "Stroik-Kalibrator.dll", StringComparison.OrdinalIgnoreCase) ||
            info.FileMajorPart < 1 || info.FilePrivatePart != 0)
            throw new InvalidDataException("The existing executable is not a recognized Stroik calibrator.");
        return new Version(info.FileMajorPart, info.FileMinorPart, info.FileBuildPart);
    }

    internal static bool ShouldOffer(Version? installed, Version available, bool startup, bool installOffered, Version? updateOffered)
    {
        if (installed is null) return !startup || !installOffered;
        return installed < available && (!startup || updateOffered is null || updateOffered < available);
    }

    internal static Task InstallAsync(string directory, CalibratorRelease release) => InstallAsync(directory, release, Client);

    internal static async Task InstallAsync(string directory, CalibratorRelease release, HttpClient client)
    {
        string destination = Path.Combine(directory, ExecutableName);
        string temporary = destination + "." + Guid.NewGuid().ToString("N") + ".tmp";
        if (InstalledVersion(directory) is Version installed && installed >= release.Version) return;
        try
        {
            await using (var target = new FileStream(temporary, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 81920, useAsync: true))
            {
                using var response = await Updater.GetWithRetryAsync(client, release.Executable, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                await using var source = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                if (await Updater.CopyWithDeadlineAsync(source, target, maxBytes: release.Size).ConfigureAwait(false) != release.Size)
                    throw new InvalidDataException("Incomplete calibrator download.");
                target.Position = 0;
                string hash = Convert.ToHexString(await SHA256.HashDataAsync(target).ConfigureAwait(false));
                if (!hash.Equals(release.Sha256, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Calibrator SHA-256 verification failed.");
            }
            if (ReadVersion(temporary) != release.Version) throw new InvalidDataException("Unexpected calibrator version.");
            Version? current = InstalledVersion(directory);
            if (current is null || current < release.Version) File.Move(temporary, destination, overwrite: current is not null);
        }
        finally
        {
            try { File.Delete(temporary); }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException) { }
        }
    }
}
