using Stroik;

internal static class PortablePreferencesChecks
{
    internal sealed class Fixture : IDisposable
    {
        private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("stroik-settings-");
        internal string ProgramDirectory => Path.Combine(_root.FullName, "program with spaces");
        internal string PortableFile => Path.Combine(ProgramDirectory, "Stroik.settings.json");
        internal string LegacyFile => Path.Combine(_root.FullName, "local", "settings.json");
        internal PreferencesStore Store() => new(ProgramDirectory, LegacyFile);
        public void Dispose() => _root.Delete(recursive: true);
    }

    internal static void Run()
    {
        using var migration = new Fixture();
        Directory.CreateDirectory(Path.GetDirectoryName(migration.LegacyFile)!);
        var original = new Preferences
        {
            Language = "pl", ConcertA = 435, ForkVolume = 72, MonitorVolume = 43,
            InstrumentMap = new InstrumentMap { Notes = new() { [69] = 427 } },
            ForkCalibration = new ForkCalibration { Notes = new() { [69] = new ForkTone(21, 67) } }
        };
        Preferences.SaveToFile(migration.LegacyFile, original);
        string originalJson = File.ReadAllText(migration.LegacyFile);
        var store = migration.Store();
        var loaded = store.Load();
        if (loaded.Serialize() != original.Serialize() || File.ReadAllText(migration.PortableFile) != originalJson)
            throw new Exception("Portable migration must preserve the full profile, map and calibration.");
        loaded.ForkVolume = 53;
        store.Save(loaded);
        if (migration.Store().Load().ForkVolume != 53 || File.ReadAllText(migration.LegacyFile) != originalJson)
            throw new Exception("Portable changes were lost or the legacy file was modified.");

        using var newInstallation = new Fixture();
        Directory.CreateDirectory(newInstallation.ProgramDirectory);
        string otherApplication = Path.Combine(newInstallation.ProgramDirectory, "settings.json");
        File.WriteAllText(otherApplication, "{\"otherApplication\":true}");
        if (newInstallation.Store().Load().ForkVolume != 100 || !File.Exists(newInstallation.PortableFile) || File.Exists(newInstallation.LegacyFile))
            throw new Exception("A new installation did not create its default portable file.");
        if (File.ReadAllText(otherApplication) != "{\"otherApplication\":true}")
            throw new Exception("Portable settings touched another application's settings.json.");

        using var fallback = new Fixture();
        // A file in place of a directory makes the portable path unwritable even under CI administrator accounts.
        File.WriteAllText(fallback.ProgramDirectory, "cannot create a file below this path");
        var fallbackStore = fallback.Store();
        var fallbackSettings = fallbackStore.Load();
        fallbackSettings.ForkVolume = 47;
        fallbackStore.Save(fallbackSettings);
        if (fallback.Store().Load().ForkVolume != 47 || !File.Exists(fallback.LegacyFile))
            throw new Exception("An unwritable program location did not fall back to the user profile.");

        using var damaged = new Fixture();
        Directory.CreateDirectory(damaged.ProgramDirectory);
        File.WriteAllText(damaged.PortableFile, "{ incomplete");
        var damagedStore = damaged.Store();
        damagedStore.Save(damagedStore.Load());
        if (File.ReadAllText(damaged.PortableFile) != "{ incomplete")
            throw new Exception("Damaged portable settings were overwritten.");

        using var damagedLegacy = new Fixture();
        Directory.CreateDirectory(Path.GetDirectoryName(damagedLegacy.LegacyFile)!);
        File.WriteAllText(damagedLegacy.LegacyFile, "{ incomplete");
        damagedLegacy.Store().Load();
        if (File.ReadAllText(damagedLegacy.LegacyFile) != "{ incomplete" || !File.Exists(damagedLegacy.PortableFile))
            throw new Exception("Damaged legacy preferences prevented a safe portable startup.");

        Console.WriteLine("Portable settings: full legacy copy, portable priority, atomic saves, new installation, fallback and damaged file preservation: OK");
    }
}
