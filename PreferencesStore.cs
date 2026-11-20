using System.Text.Json;

namespace Stroik;

internal sealed class PreferencesStore(string programDirectory, string legacyFile, bool migrateOnLoad = true)
{
    private readonly string _portableFile = Path.Combine(programDirectory, "Stroik.settings.json");
    private readonly string _legacyRoot = Path.GetDirectoryName(legacyFile)!;
    internal bool CanSave { get; private set; }
    internal bool MigrationPending { get; private set; }

    internal static PreferencesStore ForApplication(bool migrateOnLoad = true) => new(PortableData.ProgramDirectory,
        Path.Combine(PortableData.LegacyDirectory, "settings.json"), migrateOnLoad);

    internal Preferences Load()
    {
        if (!Path.IsPathFullyQualified(_legacyRoot) || PortableData.Contains(_legacyRoot, programDirectory))
            return Read(_portableFile).Settings ?? new Preferences();

        var portable = Read(_portableFile);
        if (portable.Exists)
        {
            // A damaged or unreadable file stays available for manual recovery.
            CanSave = portable.Settings is not null;
            if (migrateOnLoad) CompleteMigration();
            return portable.Settings ?? ReadLegacy().Settings ?? new Preferences();
        }

        var legacy = ReadLegacy();
        if (legacy.Blocked)
        {
            // A valid profile may become readable on the next launch. Do not create
            // defaults that would then take precedence over that profile.
            CanSave = false;
            DeferMigration();
            return new Preferences();
        }
        Preferences settings = legacy.Settings ?? new Preferences();
        if (TryWrite(_portableFile, legacy.Json ?? settings.Serialize(), overwrite: false))
        {
            CanSave = true;
            if (migrateOnLoad) CompleteMigration();
            return settings;
        }

        // Another instance may have created the portable file while this one was starting.
        portable = Read(_portableFile);
        CanSave = portable.Settings is not null;
        if (migrateOnLoad) CompleteMigration();
        return portable.Settings ?? settings;
    }

    internal bool Save(Preferences settings) => CanSave && TryWrite(_portableFile, settings.Serialize(), overwrite: true);

    internal void CompleteMigration()
    {
        MigrationPending = CanSave ? !LegacyDataMigration.TryMove(_legacyRoot, programDirectory) : Directory.Exists(_legacyRoot);
    }

    internal void DeferMigration() => MigrationPending = Directory.Exists(_legacyRoot);

    private (bool Exists, bool Blocked, Preferences? Settings, string? Json) ReadLegacy() => Read(legacyFile, _legacyRoot);

    private static (bool Exists, bool Blocked, Preferences? Settings, string? Json) Read(string path, string? root = null)
    {
        try
        {
            if (root is null) PortableData.RejectLink(path);
            else PortableData.RejectLinksUnder(root, path);
            string json = File.ReadAllText(path);
            return (true, false, Preferences.Parse(json), json);
        }
        catch (Exception error) when (error is FileNotFoundException or DirectoryNotFoundException) { return (false, false, null, null); }
        catch (Exception error) when (error is JsonException or InvalidDataException) { return (true, false, null, null); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException) { return (true, true, null, null); }
    }

    private static bool TryWrite(string path, string json, bool overwrite)
    {
        string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream, leaveOpen: true))
            {
                writer.Write(json);
                writer.Flush();
                stream.Flush(flushToDisk: true);
            }
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
