using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;

namespace Stroik;

internal sealed record ReleaseInfo(Version Version, Uri Executable, Uri Checksum);

internal static class Updater
{
    private const string Owner = "proszczok2002";
    private const string Repository = "Stroik";
    private const string ExecutableName = "Stroik.exe";
    private const string ChecksumName = "Stroik.exe.sha256";
    private static readonly Uri Latest = new($"https://api.github.com/repos/{Owner}/{Repository}/releases/latest");
    private static readonly HttpClient Client = CreateClient();

    private static HttpClient CreateClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Stroik-Updater/1.0.1");
        client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        return client;
    }

    internal static async Task<ReleaseInfo?> FindNewReleaseAsync()
    {
        using var response = await Client.GetAsync(Latest);
        response.EnsureSuccessStatusCode();
        string json = await response.Content.ReadAsStringAsync();
        Version current = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0);
        return ParseRelease(json, current);
    }

    internal static ReleaseInfo? ParseRelease(string json, Version current)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        if (root.GetProperty("draft").GetBoolean() || root.GetProperty("prerelease").GetBoolean()) return null;
        string tag = root.GetProperty("tag_name").GetString() ?? "";
        if (!Version.TryParse(tag.TrimStart('v', 'V'), out Version? version))
            throw new InvalidDataException("GitHub release has an invalid version tag.");
        if (version <= current) return null;
        Uri? executable = null, checksum = null;
        foreach (JsonElement asset in root.GetProperty("assets").EnumerateArray())
        {
            string? name = asset.GetProperty("name").GetString();
            string? url = asset.GetProperty("browser_download_url").GetString();
            if (url is null || !Uri.TryCreate(url, UriKind.Absolute, out Uri? uri)) continue;
            if (uri.Scheme != Uri.UriSchemeHttps || uri.Host != "github.com" ||
                !uri.AbsolutePath.StartsWith($"/{Owner}/{Repository}/releases/download/", StringComparison.OrdinalIgnoreCase)) continue;
            if (name == ExecutableName) executable = uri;
            if (name == ChecksumName) checksum = uri;
        }
        if (executable is null || checksum is null)
            throw new InvalidDataException("The GitHub release is missing Stroik.exe or Stroik.exe.sha256.");
        return new ReleaseInfo(version, executable, checksum);
    }

    internal static async Task<string> DownloadVerifiedAsync(ReleaseInfo release)
    {
        string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Stroik", "updates", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string destination = Path.Combine(directory, ExecutableName);
        try
        {
            string expected = ParseChecksum(await Client.GetStringAsync(release.Checksum));
            using (var response = await Client.GetAsync(release.Executable, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();
                await using var source = await response.Content.ReadAsStreamAsync();
                await using var target = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                await source.CopyToAsync(target);
            }
            await using var file = File.OpenRead(destination);
            string actual = Convert.ToHexString(await SHA256.HashDataAsync(file));
            if (!actual.Equals(expected, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Downloaded EXE does not match the release SHA-256 checksum.");
            if (new FileInfo(destination).Length < 1_000_000)
                throw new InvalidDataException("Downloaded EXE is unexpectedly small.");
            return destination;
        }
        catch
        {
            try { Directory.Delete(directory, true); } catch (IOException) { }
            throw;
        }
    }

    internal static string ParseChecksum(string text)
    {
        string[] fields = text.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (fields.Length != 2 || fields[1].TrimStart('*') != ExecutableName ||
            fields[0].Length != 64 || !fields[0].All(Uri.IsHexDigit))
            throw new InvalidDataException("Invalid SHA-256 checksum file.");
        return fields[0];
    }

    internal static void LaunchReplacement(string downloaded)
    {
        string current = Environment.ProcessPath ?? throw new InvalidOperationException("Cannot locate the running EXE.");
        if (!Path.GetFileName(current).Equals(ExecutableName, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Automatic update requires Stroik.exe.");
        _ = Process.Start(new ProcessStartInfo(downloaded)
        {
            UseShellExecute = false,
            Arguments = $"--apply-update {Quote(current)} {Environment.ProcessId}"
        }) ?? throw new InvalidOperationException("Cannot start the update helper.");
    }

    internal static void ApplyReplacement(string current, int oldProcessId)
    {
        string helper = Environment.ProcessPath ?? throw new InvalidOperationException("Cannot locate the update helper.");
        if (!Path.GetFileName(current).Equals(ExecutableName, StringComparison.OrdinalIgnoreCase) ||
            Path.GetFullPath(current).Equals(Path.GetFullPath(helper), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Invalid update destination.");
        try { Process.GetProcessById(oldProcessId).WaitForExit(30000); }
        catch (ArgumentException) { /* Old process already closed. */ }
        string backup = current + ".old-" + Guid.NewGuid().ToString("N");
        bool moved = false;
        try
        {
            for (int attempt = 0; ; attempt++)
            {
                try { File.Move(current, backup); moved = true; break; }
                catch (IOException) when (attempt < 20) { Thread.Sleep(500); }
            }
            File.Copy(helper, current);
            _ = Process.Start(new ProcessStartInfo(current)
            {
                UseShellExecute = false,
                Arguments = $"--cleanup-update {Quote(Path.GetDirectoryName(helper)!)} {Environment.ProcessId}"
            }) ?? throw new InvalidOperationException("Cannot start the updated app.");
            File.Delete(backup);
        }
        catch
        {
            if (moved)
            {
                try { if (File.Exists(current)) File.Delete(current); File.Move(backup, current); }
                catch (IOException) { /* Keep backup for manual recovery. */ }
            }
            throw;
        }
    }

    internal static void CleanupAfterUpdate(string directory, int helperProcessId)
    {
        _ = Task.Run(() =>
        {
            try { Process.GetProcessById(helperProcessId).WaitForExit(30000); }
            catch (ArgumentException) { }
            string updates = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Stroik", "updates");
            if (Path.GetDirectoryName(Path.GetFullPath(directory))?.Equals(Path.GetFullPath(updates), StringComparison.OrdinalIgnoreCase) == true)
            {
                for (int attempt = 0; attempt < 10; attempt++)
                {
                    try { Directory.Delete(directory, true); break; }
                    catch (IOException) { Thread.Sleep(500); }
                }
            }
        });
    }

    private static string Quote(string value) => "\"" + value.Replace("\"", "") + "\"";
}
