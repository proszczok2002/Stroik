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
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Stroik-Updater/" + Assembly.GetExecutingAssembly().GetName().Version);
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
            UpdateInstaller.Log($"Downloading release {release.Version}");
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
                        long copied = await CopyWithDeadlineAsync(source, target);
                        if (response.Content.Headers.ContentLength is long length && copied != length)
                            throw new IOException("Incomplete EXE download.");
                    }
                    await using var file = File.OpenRead(destination);
                    string actual = Convert.ToHexString(await SHA256.HashDataAsync(file));
                    if (!actual.Equals(expected, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidDataException("Downloaded EXE does not match the release SHA-256 checksum.");
                    if (new FileInfo(destination).Length < 1_000_000)
                        throw new InvalidDataException("Downloaded EXE is unexpectedly small.");
                    UpdateInstaller.Log($"Download and SHA-256 verified: {new FileInfo(destination).Length} bytes");
                    return destination;
                }
                catch (Exception error) when (attempt < 2 && IsRetryableDownloadFailure(error))
                {
                    UpdateInstaller.Log($"Download retry {attempt + 1}", error);
                    if (File.Exists(destination)) File.Delete(destination);
                    await Task.Delay(RetryDelay(attempt));
                }
            }
            throw new InvalidOperationException("Update download retries exhausted.");
        }
        catch (Exception error)
        {
            UpdateInstaller.Log("Download failed", error);
            try { Directory.Delete(directory, true); } catch (Exception cleanup) when (cleanup is IOException or UnauthorizedAccessException) { }
            throw;
        }
    }

    // ResponseHeadersRead does not apply HttpClient.Timeout to the response body.
    internal static async Task<long> CopyWithDeadlineAsync(Stream source, Stream target, TimeSpan? idleTimeout = null)
    {
        using var overall = new CancellationTokenSource(TimeSpan.FromMinutes(10));
        var buffer = new byte[81920];
        long total = 0;
        while (true)
        {
            using var readTimeout = CancellationTokenSource.CreateLinkedTokenSource(overall.Token);
            readTimeout.CancelAfter(idleTimeout ?? TimeSpan.FromSeconds(45));
            int read = await source.ReadAsync(buffer.AsMemory(), readTimeout.Token);
            if (read == 0) return total;
            await target.WriteAsync(buffer.AsMemory(0, read), overall.Token);
            total += read;
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

    internal static Task LaunchReplacementAsync(string downloaded) => UpdateInstaller.LaunchAsync(downloaded);

    internal static void ApplyReplacement(string current, int oldProcessId, string? ready = null) => UpdateInstaller.Apply(current, oldProcessId, ready);

    internal static void CleanupAfterUpdate(string directory, int helperProcessId, string? backup = null) => UpdateInstaller.InstalledWindowReady(directory, helperProcessId, backup);
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
}
