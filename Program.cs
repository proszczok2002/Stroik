namespace Stroik;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        if (args.Length is 3 or 4 && args[0] == "--apply-update" && int.TryParse(args[2], out int oldPid))
        {
            try { Updater.ApplyReplacement(args[1], oldPid, args.Length == 4 ? args[3] : null); }
            catch (Exception error) { UpdateInstaller.Log("Update helper error", error); MessageBox.Show(error.Message + "\n\nDziennik / Log: " + UpdateInstaller.LogPath, "Stroik — aktualizacja / update", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            return;
        }
        int helperPid = 0;
        bool cleanup = args.Length is 3 or 4 && args[0] == "--cleanup-update" && int.TryParse(args[2], out helperPid);
        // When upgraded by an older version, the running helper can still be in AppData.
        bool waitForLegacyHelper = cleanup && UpdateInstaller.IsLegacyUpdateDirectory(args[1]);
        var preferences = PreferencesStore.ForApplication(migrateOnLoad: false);
        var form = new TunerForm(preferences);
        form.Shown += async (_, _) =>
        {
            Task<bool> cleanupTask = cleanup ? Updater.CleanupAfterUpdate(args[1], helperPid, args.Length == 4 ? args[3] : null) : Task.FromResult(true);
            if (!waitForLegacyHelper || await cleanupTask)
                await Task.Run(preferences.CompleteMigration);
            else
                preferences.DeferMigration();
            form.WarnPortableStorage();
        };
        Application.Run(form);
    }
}
