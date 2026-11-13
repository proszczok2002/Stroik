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
        var form = new TunerForm();
        if (args.Length is 3 or 4 && args[0] == "--cleanup-update" && int.TryParse(args[2], out int helperPid))
            form.Shown += (_, _) => Updater.CleanupAfterUpdate(args[1], helperPid, args.Length == 4 ? args[3] : null);
        Application.Run(form);
    }
}
