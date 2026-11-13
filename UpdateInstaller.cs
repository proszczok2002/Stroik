using System.Diagnostics;
using System.Security.Cryptography;

namespace Stroik;

internal static class UpdateInstaller
{
    internal static string LogPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Stroik", "update.log");
    private static readonly object LogLock = new();

    internal static void Log(string stage, Exception? error = null)
    {
        try
        {
            lock (LogLock)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
                if (File.Exists(LogPath) && new FileInfo(LogPath).Length > 1_000_000) File.Move(LogPath, LogPath + ".previous", true);
                using var file = new FileStream(LogPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                using var writer = new StreamWriter(file);
                writer.WriteLine($"{DateTimeOffset.Now:O} [{Environment.ProcessId}] {stage}{(error is null ? "" : $": {error.GetType().Name} 0x{error.HResult:X8}: {error.Message}")}");
            }
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException) { }
    }

    private static ProcessStartInfo StartInfo(string executable, params string[] args)
    {
        var info = new ProcessStartInfo(executable) { UseShellExecute = false, WorkingDirectory = Path.GetDirectoryName(executable)!, WindowStyle = ProcessWindowStyle.Normal };
        foreach (string arg in args) info.ArgumentList.Add(arg);
        return info;
    }

    internal static async Task LaunchAsync(string downloaded)
    {
        string current = Environment.ProcessPath ?? throw new InvalidOperationException("Cannot locate Stroik.exe.");
        ValidatePaths(current, downloaded);
        ProbeDirectory(current);
        string ready = Path.Combine(Path.GetDirectoryName(downloaded)!, "helper.ready");
        if (File.Exists(ready)) File.Delete(ready);
        Log($"Starting helper for {current}");
        using var helper = Process.Start(StartInfo(downloaded, "--apply-update", current, Environment.ProcessId.ToString(), ready))
            ?? throw new InvalidOperationException("Cannot start the update helper.");
        var timer = Stopwatch.StartNew();
        while (timer.Elapsed < TimeSpan.FromSeconds(45))
        {
            if (File.Exists(ready)) { Log("Helper ready; current application may close"); return; }
            if (helper.HasExited) throw new IOException("Update helper stopped before preparing the new EXE. " + LogPath);
            await Task.Delay(150);
        }
        throw new TimeoutException("Update helper is not ready; the current application remains open. " + LogPath);
    }

    private static void ValidatePaths(string current, string helper)
    {
        if (!Path.GetFileName(current).Equals("Stroik.exe", StringComparison.OrdinalIgnoreCase) ||
            Path.GetFullPath(current).Equals(Path.GetFullPath(helper), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Automatic updates require a separate, writable Stroik.exe.");
    }

    private static void ProbeDirectory(string current)
    {
        if ((File.GetAttributes(current) & FileAttributes.ReadOnly) != 0)
            throw new UnauthorizedAccessException("Stroik.exe is read-only. Move it to a writable folder before updating.");
        string probe = Path.Combine(Path.GetDirectoryName(current)!, ".stroik-update-check-" + Guid.NewGuid().ToString("N"));
        using var file = new FileStream(probe, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1, FileOptions.DeleteOnClose);
    }

    internal static void RetryFile(Action action, int attempts = 61)
    {
        for (int attempt = 0; ; attempt++)
        {
            try { action(); return; }
            catch (Exception error) when ((error is IOException or UnauthorizedAccessException) && attempt + 1 < attempts)
            {
                if (attempt == 0 || attempt % 10 == 0) Log($"File operation retry {attempt + 1}", error);
                Thread.Sleep(500);
            }
        }
    }

    internal static void Apply(string current, int oldProcessId, string? ready)
    {
        string helper = Environment.ProcessPath ?? throw new InvalidOperationException("Cannot locate update helper.");
        ValidatePaths(current, helper);
        ProbeDirectory(current);
        string helperDirectory = Path.GetDirectoryName(helper)!;
        string staged = current + ".new-" + Guid.NewGuid().ToString("N");
        string backup = current + ".old-" + Guid.NewGuid().ToString("N");
        string marker = Path.Combine(helperDirectory, "app.ready");
        using var operation = new FileStream(Path.Combine(Path.GetDirectoryName(current)!, ".stroik-update.lock"),
            FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 1, FileOptions.DeleteOnClose);
        bool restartAttempted = false;
        try
        {
            Log("Staging verified EXE beside destination");
            RetryFile(() => File.Copy(helper, staged, true));
            if (Hash(helper) != Hash(staged)) throw new InvalidDataException("Staged EXE checksum mismatch.");
            if (File.Exists(marker)) File.Delete(marker);
            if (ready is not null)
            {
                if (!Path.GetFullPath(ready).Equals(Path.GetFullPath(Path.Combine(helperDirectory, "helper.ready")), StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Invalid helper readiness path.");
                File.WriteAllText(ready, Environment.ProcessId.ToString());
            }
            Log($"Waiting for previous process {oldProcessId}");
            try
            {
                using var previous = Process.GetProcessById(oldProcessId);
                if (!previous.WaitForExit(60000)) throw new TimeoutException("Previous Stroik is still open; close its dialogs and retry.");
            }
            catch (ArgumentException) { }
            ReplaceFiles(staged, current, backup, () =>
            {
                restartAttempted = true;
                Log("Starting installed application");
                using var next = Process.Start(StartInfo(current, "--cleanup-update", helperDirectory, Environment.ProcessId.ToString(), backup))
                    ?? throw new IOException("Cannot restart updated Stroik.");
                var elapsed = Stopwatch.StartNew();
                while (elapsed.Elapsed < TimeSpan.FromSeconds(45))
                {
                    next.Refresh();
                    if (File.Exists(marker) || (!next.HasExited && next.MainWindowHandle != IntPtr.Zero))
                    { Log("Installed application window ready"); return; }
                    if (next.HasExited) throw new IOException("Updated Stroik exited before opening its window.");
                    Thread.Sleep(150);
                }
                try { if (!next.HasExited) { next.Kill(); next.WaitForExit(5000); } } catch (InvalidOperationException) { }
                throw new TimeoutException("Updated Stroik did not open its window; restoring the previous EXE.");
            });
            Log("Replacement and restart completed");
        }
        catch (Exception error)
        {
            Log("Replacement failed; previous EXE retained or restored", error);
            if (restartAttempted)
            {
                try { using var restored = Process.Start(StartInfo(current)); Log("Restarted previous application after rollback"); }
                catch (Exception restartError) { Log("Could not restart restored EXE", restartError); }
            }
            throw;
        }
        finally
        {
            try { if (File.Exists(staged)) File.Delete(staged); }
            catch (Exception cleanup) when (cleanup is IOException or UnauthorizedAccessException) { Log("Staged recovery file retained", cleanup); }
        }
    }

    internal static void ReplaceFiles(string staged, string current, string backup, Action start)
    {
        if (!Updater.IsOwnedBackup(current, backup) || !Path.GetDirectoryName(Path.GetFullPath(staged))!.Equals(Path.GetDirectoryName(Path.GetFullPath(current)), StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Invalid replacement paths.");
        bool replaced = false;
        try
        {
            RetryFile(() => File.Replace(staged, current, backup));
            replaced = true;
            start();
        }
        catch
        {
            if (replaced && File.Exists(backup))
            {
                RetryFile(() => File.Replace(backup, current, null));
                Log("Restored previous EXE after failed restart");
            }
            throw;
        }
    }

    internal static void InstalledWindowReady(string directory, int helperPid, string? backup)
    {
        if (!IsUpdateDirectory(directory)) return;
        try { File.WriteAllText(Path.Combine(directory, "app.ready"), Environment.ProcessId.ToString()); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException) { Log("Could not write window-ready marker", error); }
        Cleanup(directory, helperPid, backup);
    }

    private static bool IsUpdateDirectory(string directory)
    {
        string root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Stroik", "updates");
        return Path.GetDirectoryName(Path.GetFullPath(directory))?.Equals(Path.GetFullPath(root), StringComparison.OrdinalIgnoreCase) == true;
    }

    internal static void Cleanup(string directory, int helperPid, string? backup)
    {
        _ = Task.Run(() =>
        {
            try
            {
                try { using var helper = Process.GetProcessById(helperPid); if (!helper.WaitForExit(60000)) { Log("Cleanup deferred: helper still running"); return; } }
                catch (ArgumentException) { }
                if (backup is not null && Updater.IsOwnedBackup(Environment.ProcessPath ?? "", backup)) RetryFile(() => File.Delete(backup));
                if (IsUpdateDirectory(directory)) RetryFile(() => { if (Directory.Exists(directory)) Directory.Delete(directory, true); });
                Log("Old EXE and download cleanup completed");
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException) { Log("Cleanup incomplete; recovery files kept", error); }
        });
    }

    private static string Hash(string path) { using var file = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(file)); }
}
