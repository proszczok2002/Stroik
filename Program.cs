namespace Stroik;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        if (args.Length == 3 && args[0] == "--apply-update" && int.TryParse(args[2], out int oldPid))
        {
            try { Updater.ApplyReplacement(args[1], oldPid); }
            catch (Exception error) { MessageBox.Show(error.Message, "Stroik update", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            return;
        }
        if (args.Length == 3 && args[0] == "--cleanup-update" && int.TryParse(args[2], out int helperPid))
            Updater.CleanupAfterUpdate(args[1], helperPid);
        Application.Run(new TunerForm());
    }
}
