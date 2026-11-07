using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;

namespace Stroik;

internal sealed record ReleaseInfo(Version Version, Uri Executable, Uri Checksum, string? Notes);

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
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Stroik-Updater/1.0.11");
        client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
        client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        return client;
    }

    // Retry only temporary network failures. The EXE is still accepted solely after SHA-256 verification.
    internal static async Task<HttpResponseMessage> GetWithRetryAsync(HttpClient client, Uri uri,
        HttpCompletionOption completion = HttpCompletionOption.ResponseContentRead)
    {
        const int attempts = 4;
        for (int attempt = 0; attempt < attempts; attempt++)
        {
            HttpResponseMessage response;
            try
            {
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                response = await client.GetAsync(uri, completion, timeout.Token);
            }
            catch (HttpRequestException error) when (attempt < attempts - 1 &&
                (error.StatusCode is null || IsTemporary(error.StatusCode.Value)))
            {
                await Task.Delay(RetryDelay(attempt));
                continue;
            }
            catch (OperationCanceledException) when (attempt < attempts - 1)
            {
                await Task.Delay(RetryDelay(attempt));
                continue;
            }

            if (IsTemporary(response.StatusCode) && attempt < attempts - 1)
            {
                TimeSpan delay = response.Headers.RetryAfter?.Delta ?? RetryDelay(attempt);
                response.Dispose();
                await Task.Delay(TimeSpan.FromMilliseconds(Math.Clamp(delay.TotalMilliseconds, 250, 10000)));
                continue;
            }
            try { response.EnsureSuccessStatusCode(); return response; }
            catch { response.Dispose(); throw; }
        }
        throw new InvalidOperationException("Update request retries exhausted.");
    }

    private static bool IsTemporary(HttpStatusCode status) =>
        status is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests ||
        (int)status >= 500;

    private static TimeSpan RetryDelay(int attempt) => TimeSpan.FromMilliseconds(400 * (1 << attempt));
    internal static async Task<ReleaseInfo?> FindNewReleaseAsync()
    {
        using var response = await GetWithRetryAsync(Client, Latest);
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
        string? notes = root.TryGetProperty("body", out JsonElement body) && body.ValueKind == JsonValueKind.String
            ? body.GetString() : null;
        return new ReleaseInfo(version, executable, checksum, notes);
    }

    internal static async Task<string> DownloadVerifiedAsync(ReleaseInfo release)
    {
        string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Stroik", "updates", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string destination = Path.Combine(directory, ExecutableName);
        try
        {
            using var checksumResponse = await GetWithRetryAsync(Client, release.Checksum);
            string expected = ParseChecksum(await checksumResponse.Content.ReadAsStringAsync());
            for (int attempt = 0; attempt < 3; attempt++)
            {
                try
                {
                    using (var response = await GetWithRetryAsync(Client, release.Executable, HttpCompletionOption.ResponseHeadersRead))
                    {
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
                catch (Exception error) when (attempt < 2 && IsRetryableDownloadFailure(error))
                {
                    if (File.Exists(destination)) File.Delete(destination);
                    await Task.Delay(RetryDelay(attempt));
                }
            }
            throw new InvalidOperationException("Update download retries exhausted.");
        }
        catch
        {
            try { Directory.Delete(directory, true); } catch (IOException) { }
            throw;
        }
    }

    private static bool IsRetryableDownloadFailure(Exception error) =>
        error is IOException or InvalidDataException or OperationCanceledException ||
        error is HttpRequestException request && (request.StatusCode is null || IsTemporary(request.StatusCode.Value));
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
        string probe = Path.Combine(Path.GetDirectoryName(current)!, ".stroik-update-check-" + Guid.NewGuid().ToString("N"));
        using (new FileStream(probe, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1, FileOptions.DeleteOnClose)) { }
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
        try
        {
            using var oldProcess = Process.GetProcessById(oldProcessId);
            if (!oldProcess.WaitForExit(30000)) throw new TimeoutException("The old Stroik process did not close.");
        }
        catch (ArgumentException) { /* Old process already closed. */ }
        string backup = current + ".old-" + Guid.NewGuid().ToString("N");
        bool moved = false, started = false;
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
                Arguments = $"--cleanup-update {Quote(Path.GetDirectoryName(helper)!)} {Environment.ProcessId} {Quote(backup)}"
            }) ?? throw new InvalidOperationException("Cannot start the updated app.");
            started = true;
            try { File.Delete(backup); }
            catch (IOException) { /* New process retries cleanup. */ }
            catch (UnauthorizedAccessException) { /* New process retries cleanup. */ }
        }
        catch
        {
            if (moved && !started)
            {
                try { if (File.Exists(current)) File.Delete(current); File.Move(backup, current); }
                catch (IOException) { /* Keep backup for manual recovery. */ }
            }
            throw;
        }
    }

    internal static void CleanupAfterUpdate(string directory, int helperProcessId, string? backup = null)
    {
        _ = Task.Run(() =>
        {
            try { Process.GetProcessById(helperProcessId).WaitForExit(30000); }
            catch (ArgumentException) { }
            string updates = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Stroik", "updates");
            string current = Environment.ProcessPath ?? "";
            if (backup is not null && IsOwnedBackup(current, backup))
            {
                for (int attempt = 0; attempt < 20 && File.Exists(backup); attempt++)
                {
                    try { File.Delete(backup); }
                    catch (IOException) { Thread.Sleep(500); }
                    catch (UnauthorizedAccessException) { Thread.Sleep(500); }
                }
            }
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

    internal static bool IsOwnedBackup(string current, string backup)
    {
        if (!Path.GetFileName(current).Equals(ExecutableName, StringComparison.OrdinalIgnoreCase)) return false;
        string currentDirectory = Path.GetDirectoryName(Path.GetFullPath(current))!;
        string backupDirectory = Path.GetDirectoryName(Path.GetFullPath(backup))!;
        string prefix = ExecutableName + ".old-";
        string name = Path.GetFileName(backup);
        return currentDirectory.Equals(backupDirectory, StringComparison.OrdinalIgnoreCase) &&
            name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
            Guid.TryParseExact(name[prefix.Length..], "N", out _);
    }
    private static string Quote(string value) => "\"" + value.Replace("\"", "") + "\"";
}
