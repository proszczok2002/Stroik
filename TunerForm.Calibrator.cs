using System.Windows.Forms.Automation;

namespace Stroik;

internal sealed partial class TunerForm
{
    private readonly CheckBox _checkCalibratorOnStartup = new() { AutoSize = true };
    private readonly Button _checkCalibrator = new() { AutoSize = true };
    private readonly TextBox _calibratorStatus = new() { ReadOnly = true, Dock = DockStyle.Fill };
    private bool _calibratorBusy, _calibratorInstallOffered;
    private Version? _calibratorUpdateOffered;

    private void AddCalibratorSettings(TableLayoutPanel panel)
    {
        Add(panel, _checkCalibratorOnStartup);
        Add(panel, _checkCalibrator);
        Add(panel, _calibratorStatus);
        _checkCalibratorOnStartup.CheckedChanged += (_, _) => Persist();
        _checkCalibratorOnStartup.Click += (_, _) => AnnounceCheckBoxState(_checkCalibratorOnStartup);
        _checkCalibrator.Click += async (_, _) => await CheckCalibratorAsync(startup: false);
    }

    private void ApplyCalibratorPreferences(Preferences settings)
    {
        _checkCalibratorOnStartup.Checked = settings.CheckCalibratorOnStartup;
        _calibratorInstallOffered = settings.CalibratorInstallOffered;
        Version.TryParse(settings.CalibratorUpdateOffered, out _calibratorUpdateOffered);
    }

    private void ApplyCalibratorLanguage()
    {
        _checkCalibratorOnStartup.AccessibleName = _checkCalibratorOnStartup.Text = T("Sprawdzaj kalibrator przy uruchomieniu", "Check for the calibrator at startup");
        _checkCalibratorOnStartup.AccessibleDescription = T("Stroik pyta raz o pobranie brakującego kalibratora i raz o każdą nowszą wersję. Ręczne sprawdzanie jest dostępne niezależnie od tej opcji.",
            "Stroik offers a missing calibrator once and each newer version once. You can always check manually.");
        _checkCalibrator.AccessibleName = _checkCalibrator.Text = T("Sprawdź kalibrator teraz", "Check calibrator now");
        _calibratorStatus.AccessibleName = T("Stan pobierania i aktualizacji kalibratora", "Calibrator download and update status");
        if (string.IsNullOrEmpty(_calibratorStatus.Text))
            _calibratorStatus.Text = T("Kalibrator jest opcjonalny.", "The calibrator is optional.");
    }

    private void ReportCalibrator(string message)
    {
        _calibratorStatus.Text = message;
        if (IsHandleCreated)
            _checkCalibrator.AccessibilityObject.RaiseAutomationNotification(AutomationNotificationKind.ActionCompleted,
                AutomationNotificationProcessing.ImportantMostRecent, message);
    }

    internal async Task CheckCalibratorAsync(bool startup)
    {
        if (_closing || IsDisposed || _calibratorBusy || _updateChecks.IsRunning || (startup && !_checkCalibratorOnStartup.Checked)) return;
        _calibratorBusy = true;
        bool confirmed = false;
        try
        {
            string directory = AppContext.BaseDirectory;
            Version? installed = CalibratorUpdater.InstalledVersion(directory);
            if (startup && installed is null && _calibratorInstallOffered) return;
            if (!startup) ReportCalibrator(T("Sprawdzam wersję kalibratora…", "Checking the calibrator version…"));
            CalibratorRelease release = await CalibratorUpdater.FetchAsync();
            if (_closing || IsDisposed || (startup && (!_checkCalibratorOnStartup.Checked || !ContainsFocus))) return;
            if (!CalibratorUpdater.ShouldOffer(installed, release.Version, startup, _calibratorInstallOffered, _calibratorUpdateOffered))
            {
                if (!startup) ReportCalibrator(Tf($"Kalibrator {installed} jest aktualny.", $"Calibrator {installed} is up to date."));
                return;
            }

            string title = installed is null
                ? T("Pobieranie kalibratora", "Calibrator download")
                : T("Aktualizacja kalibratora", "Calibrator update");
            string message = installed is null
                ? Tf($"Brakuje opcjonalnego kalibratora kamertonu. Czy pobrać wersję {release.Version} z GitHuba i zapisać obok Stroik.exe?", $"The optional fork calibrator is missing. Download version {release.Version} from GitHub and save it next to Stroik.exe?")
                : Tf($"Masz kalibrator {installed}. Czy pobrać wersję {release.Version} z GitHuba i zastąpić plik obok Stroik.exe?", $"You have calibrator {installed}. Download version {release.Version} from GitHub and replace the file next to Stroik.exe?");
            using var offer = new CalibratorOfferDialog(title, message, Locale, _dark.Checked);
            bool yes = offer.ShowDialog(this) == DialogResult.Yes;
            if (installed is null) _calibratorInstallOffered = true;
            else _calibratorUpdateOffered = release.Version;
            Persist();
            if (!yes)
            {
                if (!startup) ReportCalibrator(T("Pobieranie kalibratora pominięte.", "Calibrator download skipped."));
                return;
            }

            confirmed = true;
            ReportCalibrator(T("Pobieram kalibrator i sprawdzam sumę SHA-256…", "Downloading the calibrator and verifying SHA-256…"));
            await CalibratorUpdater.InstallAsync(directory, release);
            if (_closing || IsDisposed) return;
            ReportCalibrator(Tf($"Kalibrator {release.Version} jest gotowy obok Stroik.exe.", $"Calibrator {release.Version} is ready next to Stroik.exe."));
            MessageBox.Show(this, _calibratorStatus.Text, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or HttpRequestException or OperationCanceledException or
            System.Text.Json.JsonException or InvalidDataException or InvalidOperationException or KeyNotFoundException or FormatException)
        {
            if (_closing || IsDisposed || (startup && !confirmed)) return;
            UpdateInstaller.Log("Calibrator check or install failed", error);
            string text = confirmed
                ? T("Nie udało się zapisać kalibratora. Zamknij kalibrator, jeśli jest uruchomiony, i sprawdź prawo zapisu w folderze. Możesz ponowić próbę w Ustawieniach.",
                    "Could not save the calibrator. Close it if it is running and check that the folder is writable. You can retry in Settings.")
                : T("Nie udało się sprawdzić kalibratora. Sprawdź połączenie i ponów próbę w Ustawieniach.", "Could not check the calibrator. Check the connection and retry in Settings.");
            ReportCalibrator(text);
            MessageBox.Show(this, text, T("Kalibrator kamertonu", "Fork calibrator"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        finally { _calibratorBusy = false; }
    }
}

internal sealed class CalibratorOfferDialog : Form
{
    internal CalibratorOfferDialog(string title, string message, string language, bool dark)
    {
        Text = title;
        AccessibleName = title;
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        MinimizeBox = MaximizeBox = false;
        Size = new Size(540, 245);
        MinimumSize = new Size(410, 220);
        AutoScaleMode = AutoScaleMode.Dpi;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(12) };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var prompt = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
            AccessibleName = title, Text = message, TabIndex = 0 };
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, TabIndex = 1 };
        var yes = new Button { AutoSize = true, DialogResult = DialogResult.Yes, TabIndex = 0,
            Text = UiLanguages.Get(language, "Tak, pobierz kalibrator", "Yes, download the calibrator") };
        var no = new Button { AutoSize = true, DialogResult = DialogResult.No, TabIndex = 1,
            Text = UiLanguages.Get(language, "Nie", "No") };
        layout.Controls.Add(prompt, 0, 0);
        buttons.Controls.AddRange([yes, no]);
        layout.Controls.Add(buttons, 0, 1);
        Controls.Add(layout);
        AcceptButton = CancelButton = no;
        if (dark)
        {
            BackColor = layout.BackColor = buttons.BackColor = Color.FromArgb(30, 30, 30);
            prompt.BackColor = yes.BackColor = no.BackColor = Color.FromArgb(48, 48, 48);
            prompt.ForeColor = yes.ForeColor = no.ForeColor = Color.WhiteSmoke;
        }
        Shown += (_, _) => { prompt.SelectionLength = 0; prompt.SelectionStart = 0; prompt.Focus(); };
    }
}
