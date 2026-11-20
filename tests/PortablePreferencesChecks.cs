using Stroik;

internal static class PortablePreferencesChecks
{
    internal sealed class Fixture : IDisposable
    {
        private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("stroik-settings-");
        internal string ProgramDirectory => Path.Combine(_root.FullName, "program with spaces");
        internal string PortableFile => Path.Combine(ProgramDirectory, "Stroik.settings.json");
        internal string LegacyDirectory => Path.Combine(_root.FullName, "local", "Stroik");
        internal string LegacyFile => Path.Combine(LegacyDirectory, "settings.json");
        internal PreferencesStore Store(bool migrateOnLoad = true) => new(ProgramDirectory, LegacyFile, migrateOnLoad);
        internal string Snapshot() => Directory.GetDirectories(PortableData.DataDirectory(ProgramDirectory), "migracja-*").Single();
        public void Dispose() => _root.Delete(recursive: true);
    }

    internal static void Run()
    {
        using var migration = new Fixture();
        Directory.CreateDirectory(migration.LegacyDirectory);
        var original = new Preferences
        {
            Language = "pl", ConcertA = 435, ForkVolume = 72, MonitorVolume = 43,
            InstrumentMap = new InstrumentMap { Notes = new() { [69] = 427 } },
            ForkCalibration = new ForkCalibration { Notes = new() { [69] = new ForkTone(21, 67) } }
        };
        Preferences.SaveToFile(migration.LegacyFile, original);
        string originalJson = File.ReadAllText(migration.LegacyFile);
        File.WriteAllText(Path.Combine(migration.LegacyDirectory, "update.log"), "previous log");
        string downloadDirectory = Path.Combine(migration.LegacyDirectory, "updates", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(downloadDirectory);
        Directory.CreateDirectory(Path.Combine(migration.LegacyDirectory, "empty"));
        File.WriteAllBytes(Path.Combine(downloadDirectory, "Stroik.exe"), [0, 255, 15, 30]);
        var previousFiles = Directory.GetFiles(migration.LegacyDirectory, "*", SearchOption.AllDirectories).ToDictionary(
            path => Path.GetRelativePath(migration.LegacyDirectory, path), path => File.ReadAllBytes(path));
        var store = migration.Store();
        var loaded = store.Load();
        if (loaded.Serialize() != original.Serialize() || File.ReadAllText(migration.PortableFile) != originalJson || Directory.Exists(migration.LegacyDirectory))
            throw new Exception("Portable migration must preserve the full profile, map and calibration.");
        string snapshot = migration.Snapshot();
        if (!Directory.Exists(Path.Combine(snapshot, "empty")) || previousFiles.Any(file => !File.ReadAllBytes(Path.Combine(snapshot, file.Key)).SequenceEqual(file.Value)))
            throw new Exception("The complete legacy backup must retain nested files and directories.");
        loaded.ForkVolume = 53;
        store.Save(loaded);
        if (migration.Store().Load().ForkVolume != 53 || Directory.Exists(migration.LegacyDirectory) || migration.Snapshot() != snapshot)
            throw new Exception("Portable changes were lost or the migration ran again.");

        using var existing = new Fixture();
        Directory.CreateDirectory(existing.ProgramDirectory);
        Directory.CreateDirectory(existing.LegacyDirectory);
        Preferences.SaveToFile(existing.PortableFile, new Preferences { ForkVolume = 22 });
        File.WriteAllText(existing.LegacyFile, originalJson);
        var existingStore = existing.Store(migrateOnLoad: false);
        if (existingStore.Load().ForkVolume != 22 || !Directory.Exists(existing.LegacyDirectory))
            throw new Exception("Startup during an update must retain the portable settings and defer legacy cleanup.");
        existingStore.CompleteMigration();
        if (Directory.Exists(existing.LegacyDirectory) || Preferences.ReadFile(existing.PortableFile).ForkVolume != 22 ||
            File.ReadAllText(Path.Combine(existing.Snapshot(), "settings.json")) != originalJson)
            throw new Exception("Migration did not keep both portable and historical settings.");

        using var newInstallation = new Fixture();
        Directory.CreateDirectory(newInstallation.ProgramDirectory);
        string otherApplication = Path.Combine(newInstallation.ProgramDirectory, "settings.json");
        File.WriteAllText(otherApplication, "{\"otherApplication\":true}");
        if (newInstallation.Store().Load().ForkVolume != 100 || !File.Exists(newInstallation.PortableFile) || File.Exists(newInstallation.LegacyFile))
            throw new Exception("A new installation did not create its default portable file.");
        if (File.ReadAllText(otherApplication) != "{\"otherApplication\":true}")
            throw new Exception("Portable settings touched another application's settings.json.");

        using var blocked = new Fixture();
        File.WriteAllText(blocked.ProgramDirectory, "cannot create a file below this path");
        var blockedStore = blocked.Store();
        if (blockedStore.Save(blockedStore.Load()) || Directory.Exists(blocked.LegacyDirectory))
            throw new Exception("A blocked installation wrote data to AppData.");
        Directory.CreateDirectory(blocked.LegacyDirectory);
        File.WriteAllText(blocked.LegacyFile, originalJson);
        var blockedSettings = blockedStore.Load();
        if (blockedSettings.ForkVolume != 72 || blockedStore.Save(blockedSettings) || !blockedStore.MigrationPending || File.ReadAllText(blocked.LegacyFile) != originalJson)
            throw new Exception("A failed migration must protect existing AppData and read it without writing there.");

        using var locked = new Fixture();
        Directory.CreateDirectory(locked.LegacyDirectory);
        File.WriteAllText(locked.LegacyFile, originalJson);
        string log = Path.Combine(locked.LegacyDirectory, "update.log");
        using (var held = new FileStream(log, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            held.WriteByte(5);
            var lockedStore = locked.Store();
            lockedStore.Load();
            if (!lockedStore.MigrationPending || !File.Exists(locked.LegacyFile))
                throw new Exception("A locked file allowed deletion of other source files before the complete copy.");
        }
        locked.Store().Load();
        if (Directory.Exists(locked.LegacyDirectory) || File.ReadAllBytes(Path.Combine(locked.Snapshot(), "update.log"))[0] != 5)
            throw new Exception("A previously locked migration did not resume safely.");

        using var insideSource = new Fixture();
        Directory.CreateDirectory(insideSource.LegacyDirectory);
        File.WriteAllText(insideSource.LegacyFile, originalJson);
        string nestedProgram = Path.Combine(insideSource.LegacyDirectory, "program");
        var refusedStore = new PreferencesStore(nestedProgram, insideSource.LegacyFile);
        if (refusedStore.Save(refusedStore.Load()) || LegacyDataMigration.TryMove(insideSource.LegacyDirectory, nestedProgram) ||
            File.ReadAllText(insideSource.LegacyFile) != originalJson || Directory.Exists(nestedProgram))
            throw new Exception("A program located inside the source allowed migration or recursive writes.");

        string updateRoot = PortableData.UpdatesDirectory(locked.ProgramDirectory);
        string allowedDownload = Path.Combine(updateRoot, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(allowedDownload);
        string otherFolder = Path.Combine(updateRoot, "unrelated");
        Directory.CreateDirectory(otherFolder);
        if (!UpdateInstaller.IsDownloadDirectory(allowedDownload, updateRoot) || UpdateInstaller.IsDownloadDirectory(otherFolder, updateRoot) ||
            UpdateInstaller.IsDownloadDirectory(Path.Combine(allowedDownload, Guid.NewGuid().ToString("N")), updateRoot))
            throw new Exception("Updater cleanup accepted a folder outside its transaction.");

        using var damaged = new Fixture();
        Directory.CreateDirectory(damaged.ProgramDirectory);
        File.WriteAllText(damaged.PortableFile, "{ incomplete");
        Directory.CreateDirectory(damaged.LegacyDirectory);
        File.WriteAllText(damaged.LegacyFile, originalJson);
        var damagedStore = damaged.Store();
        if (damagedStore.Save(damagedStore.Load()) || File.ReadAllText(damaged.PortableFile) != "{ incomplete" || File.ReadAllText(damaged.LegacyFile) != originalJson)
            throw new Exception("A damaged portable settings file or its legacy recovery copy was overwritten.");

        using var damagedLegacy = new Fixture();
        Directory.CreateDirectory(Path.GetDirectoryName(damagedLegacy.LegacyFile)!);
        File.WriteAllText(damagedLegacy.LegacyFile, "{ incomplete");
        damagedLegacy.Store().Load();
        if (Directory.Exists(damagedLegacy.LegacyDirectory) || File.ReadAllText(Path.Combine(damagedLegacy.Snapshot(), "settings.json")) != "{ incomplete" || !File.Exists(damagedLegacy.PortableFile))
            throw new Exception("Damaged legacy preferences prevented a safe portable startup.");

        Console.WriteLine("Portable data: full verified archive and AppData removal, portable priority, atomic saves, locked and damaged files, no AppData writes, updater path scope: OK");
    }
}
