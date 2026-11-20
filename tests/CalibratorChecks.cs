using System.Net;
using System.Security.Cryptography;
using System.Text;
using Stroik;

internal static class CalibratorChecks
{
    internal static async Task CheckPolicyAndVerification()
    {
        var current = new Version(1, 0, 0);
        var next = new Version(1, 0, 1);
        var subsequent = new Version(1, 0, 2);
        if (!CalibratorUpdater.ShouldOffer(null, next, true, false, null) ||
            CalibratorUpdater.ShouldOffer(null, next, true, true, null) ||
            !CalibratorUpdater.ShouldOffer(null, next, false, true, null) ||
            CalibratorUpdater.ShouldOffer(next, next, false, true, null) ||
            CalibratorUpdater.ShouldOffer(subsequent, next, false, true, null) ||
            CalibratorUpdater.ShouldOffer(current, next, true, true, next) ||
            !CalibratorUpdater.ShouldOffer(current, next, false, true, next) ||
            !CalibratorUpdater.ShouldOffer(current, subsequent, true, true, next))
            throw new Exception("Calibrator prompts do not honor a refusal, manual checks, or safe version comparisons.");

        string checksum = new('A', 64);
        CalibratorRelease Manifest(string tag) => CalibratorUpdater.ParseManifest(Encoding.UTF8.GetBytes(
            $$"""{"schema":1,"version":"1.0.1","releaseTag":"{{tag}}","sha256":"{{checksum}}","size":50000000}"""));
        var published = Manifest("v1.0.24");
        if (published.Version != next || !published.Executable.AbsolutePath.EndsWith("/v1.0.24/Stroik-Kalibrator.exe") ||
            !Manifest("calibrator-v1.0.1").Executable.AbsolutePath.Contains("/calibrator-v1.0.1/"))
            throw new Exception("Calibrator feed did not accept the Windows and independent calibrator release tags.");
        foreach (string tag in new[] { "../untrusted", "android-v1.10", "calibrator-v1.0.2" })
        {
            try { Manifest(tag); throw new Exception("Calibrator feed accepted an invalid tag."); }
            catch (InvalidDataException) { }
        }

        using var fixture = new PortablePreferencesChecks.Fixture();
        Directory.CreateDirectory(fixture.ProgramDirectory);
        string file = Path.Combine(fixture.ProgramDirectory, CalibratorUpdater.ExecutableName);
        using var client = new HttpClient(new DownloadBytes([1, 2, 3, 4]));
        foreach (int size in new[] { 3, 4, 5 })
        {
            try
            {
                await CalibratorUpdater.InstallAsync(fixture.ProgramDirectory, published with { Size = size }, client);
                throw new Exception("Unverified calibrator was installed.");
            }
            catch (InvalidDataException) { }
            if (Directory.EnumerateFileSystemEntries(fixture.ProgramDirectory).Any())
                throw new Exception("Failed calibrator download left a partial file.");
        }
        File.WriteAllText(file, "unknown executable");
        try
        {
            await CalibratorUpdater.InstallAsync(fixture.ProgramDirectory, published, client);
            throw new Exception("Updater tried to overwrite an unknown executable.");
        }
        catch (InvalidDataException) { }
        if (File.ReadAllText(file) != "unknown executable") throw new Exception("Unknown executable was modified.");
        Console.WriteLine("Calibrator prompt policy, separate release tags, size/hash rejection and existing file protection: OK");
    }

    internal static void CheckSettingsControls(TunerForm form, PortablePreferencesChecks.Fixture fixture)
    {
        const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var option = (CheckBox)typeof(TunerForm).GetField("_checkCalibratorOnStartup", flags)!.GetValue(form)!;
        var manual = (Button)typeof(TunerForm).GetField("_checkCalibrator", flags)!.GetValue(form)!;
        var status = (TextBox)typeof(TunerForm).GetField("_calibratorStatus", flags)!.GetValue(form)!;
        if (!option.TabStop || !option.Checked || string.IsNullOrWhiteSpace(option.AccessibleName) || string.IsNullOrWhiteSpace(option.AccessibleDescription) ||
            !manual.TabStop || string.IsNullOrWhiteSpace(manual.AccessibleName) || !status.TabStop || !status.ReadOnly || string.IsNullOrWhiteSpace(status.AccessibleName))
            throw new Exception("Calibrator settings need accessible keyboard controls and readable status.");
        option.Checked = false;
        if (fixture.Store().Load().CheckCalibratorOnStartup || !manual.Enabled)
            throw new Exception("Disabling startup checks did not persist or disabled manual checks.");
        option.Checked = true;

        using var dialog = new CalibratorOfferDialog("Install", "A prompt to read", "pl", true);
        var panel = (TableLayoutPanel)dialog.Controls[0];
        var prompt = (TextBox)panel.GetControlFromPosition(0, 0)!;
        var actions = (FlowLayoutPanel)panel.GetControlFromPosition(0, 1)!;
        var yes = actions.Controls.OfType<Button>().Single(button => button.DialogResult == DialogResult.Yes);
        if (!prompt.TabStop || !prompt.ReadOnly || prompt.Text != "A prompt to read" || prompt.AccessibleName != "Install" ||
            !yes.TabStop || yes.Text != "Tak, pobierz kalibrator" || dialog.AcceptButton is not Button { DialogResult: DialogResult.No } ||
            !ReferenceEquals(dialog.AcceptButton, dialog.CancelButton))
            throw new Exception("Calibrator prompt needs readable text, a clear Yes and a safe default No/Esc.");
        Console.WriteLine("Calibrator settings and prompt keyboard controls, labels, status and safe defaults: OK");
    }

    internal static async Task CheckPublicInstallation(string previousExecutable)
    {
        CalibratorRelease release = await CalibratorUpdater.FetchAsync();
        using var fresh = new PortablePreferencesChecks.Fixture();
        using var upgrade = new PortablePreferencesChecks.Fixture();
        foreach (var fixture in new[] { fresh, upgrade })
        {
            Directory.CreateDirectory(fixture.ProgramDirectory);
            string target = Path.Combine(fixture.ProgramDirectory, CalibratorUpdater.ExecutableName);
            if (fixture == upgrade)
            {
                File.Copy(previousExecutable, target);
                if (CalibratorUpdater.InstalledVersion(fixture.ProgramDirectory) is not Version previous || previous >= release.Version)
                    throw new Exception("Expected a real older calibrator for the upgrade test.");
            }
            await CalibratorUpdater.InstallAsync(fixture.ProgramDirectory, release);
            await using var stream = File.OpenRead(target);
            string hash = Convert.ToHexString(await SHA256.HashDataAsync(stream));
            if (CalibratorUpdater.InstalledVersion(fixture.ProgramDirectory) != release.Version || stream.Length != release.Size ||
                !hash.Equals(release.Sha256, StringComparison.OrdinalIgnoreCase) || Directory.EnumerateFiles(fixture.ProgramDirectory).Count() != 1)
                throw new Exception("Public calibrator install did not match the published size, hash and version.");
            Console.WriteLine($"{(fixture == fresh ? "Fresh install" : "Update from the previous EXE")} {release.Version}: {stream.Length} bytes, SHA-256 {hash}");
        }
    }

    private sealed class DownloadBytes(byte[] bytes) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) });
    }
}
