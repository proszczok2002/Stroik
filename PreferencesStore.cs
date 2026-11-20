using System.Text.Json;

namespace Stroik;

internal sealed class PreferencesStore(string programDirectory, string legacyFile)
{
    private readonly string _portableFile = Path.Combine(programDirectory, "Stroik.settings.json");
    private string? _savePath;

    internal static PreferencesStore ForApplication() => new(AppContext.BaseDirectory,
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Stroik", "settings.json"));

    internal Preferences Load()
    {
        var portable = Read(_portableFile);
        if (portable.Exists)
        {
            // A damaged or unreadable file stays available for manual recovery.
            _savePath = portable.Settings is not null ? _portableFile : null;
            return portable.Settings ?? Read(legacyFile).Settings ?? new Preferences();
        }

        var legacy = Read(legacyFile);
        Preferences settings = legacy.Settings ?? new Preferences();
        if (TryWrite(_portableFile, legacy.Json ?? settings.Serialize(), overwrite: false))
        {
            _savePath = _portableFile;
            return settings;
        }

        // Another instance may have created the portable file while this one was starting.
        portable = Read(_portableFile);
        _savePath = portable.Exists
            ? (portable.Settings is not null ? _portableFile : null)
            : (legacy.Settings is not null || !legacy.Exists ? legacyFile : null);
        return portable.Settings ?? settings;
    }

    internal void Save(Preferences settings)
    {
        if (_savePath is not null) TryWrite(_savePath, settings.Serialize(), overwrite: true);
    }

    private static (bool Exists, Preferences? Settings, string? Json) Read(string path)
    {
        try
        {
            string json = File.ReadAllText(path);
            return (true, Preferences.Parse(json), json);
        }
        catch (Exception error) when (error is FileNotFoundException or DirectoryNotFoundException) { return (false, null, null); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException or InvalidDataException)
        { return (true, null, null); }
    }

    private static bool TryWrite(string path, string json, bool overwrite)
    {
        string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream))
                writer.Write(json);
            File.Move(temporary, path, overwrite);
            return true;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException) { return false; }
        finally
        {
            try { File.Delete(temporary); }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException) { }
        }
    }
}
