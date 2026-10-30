using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms.Automation;

namespace Stroik;

internal sealed class TunerForm : Form
{
    private readonly Button _start = new() { AutoSize = true };
    private readonly Button _stop = new() { AutoSize = true, Enabled = false };
    private readonly Button _repeat = new() { AutoSize = true, Enabled = false };
    private readonly Button _test = new() { AutoSize = true };
    private readonly Button _updates = new() { AutoSize = true };
    private readonly Label _heading = MakeLabel(20);
    private readonly Label _intro = MakeLabel();
    private readonly Label _status = MakeLabel();
    private readonly Label _resultTitle = MakeLabel();
    private readonly TextBox _result = new() { ReadOnly = true, Multiline = true, Height = 64, TabStop = true };
    private readonly Label _details = MakeLabel();
    private readonly Label _settingsTitle = MakeLabel(14);
    private readonly Label _targetTitle = MakeLabel();
    private readonly Label _concertTitle = MakeLabel();
    private readonly Label _toleranceTitle = MakeLabel();
    private readonly Label _sensitivityTitle = MakeLabel();
    private readonly Label _noteHelp = MakeLabel();
    private readonly Label _languageTitle = MakeLabel();
    private readonly ComboBox _language = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckBox _dark = new() { AutoSize = true };
    private readonly ComboBox _target = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown _concertA = new() { Minimum = 415, Maximum = 466, DecimalPlaces = 1, Increment = 0.1M, Value = 440 };
    private readonly NumericUpDown _tolerance = new() { Minimum = 1, Maximum = 20, Value = 5 };
    private readonly ComboBox _sensitivity = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NoteSession _session = new();
    private readonly short[] _window = new short[8192];
    private Microphone? _microphone;
    private int _windowCount;
    private double _reference = 440, _toleranceCents = 5, _minimumRms = 0.008;
    private int _targetMidi = -1;
    private DateTime _ignoreUntil;
    private string? _lastAnnouncement;
    private bool _english, _hasResult;
    private double _lastMeasured, _lastReference, _lastTolerance;
    private int _lastTarget;

    internal TunerForm()
    {
        var settings = Preferences.Load();
        _english = settings.English;
        MinimumSize = new Size(520, 650);
        Size = new Size(720, 780);
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Dpi;
        var content = new TableLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, ColumnCount = 1, Padding = new Padding(18) };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Controls.Add(content);
        Add(content, _heading); Add(content, _intro); Add(content, _status);
        var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };
        buttons.Controls.AddRange([_start, _stop, _repeat, _test, _updates]);
        Add(content, buttons);
        Add(content, _resultTitle); Add(content, _result); Add(content, _details);
        Add(content, _settingsTitle); Add(content, _targetTitle); Add(content, _target);
        Add(content, _concertTitle); Add(content, _concertA); Add(content, _toleranceTitle); Add(content, _tolerance);
        Add(content, _sensitivityTitle); Add(content, _sensitivity); Add(content, _noteHelp);
        Add(content, _languageTitle); Add(content, _language); Add(content, _dark);
        _language.Items.AddRange(["Polski", "English"]);
        _language.SelectedIndex = _english ? 1 : 0;
        _dark.Checked = settings.DarkTheme;
        PopulateChoices(); ApplyLanguage(); ApplyTheme();

        _start.Click += (_, _) => StartMicrophone();
        _stop.Click += (_, _) => StopMicrophone();
        _repeat.Click += (_, _) => { if (_lastAnnouncement is not null) Announce(_lastAnnouncement); };
        _test.Click += (_, _) => Announce(T("Stroik. Komunikaty dla czytnika ekranu działają.", "Stroik. Screen reader announcements work."));
        _updates.Click += async (_, _) => await CheckUpdatesAsync(false);
        _target.SelectedIndexChanged += (_, _) => _targetMidi = ((Choice)_target.SelectedItem!).Id;
        _concertA.ValueChanged += (_, _) => _reference = (double)_concertA.Value;
        _tolerance.ValueChanged += (_, _) => _toleranceCents = (double)_tolerance.Value;
        _sensitivity.SelectedIndexChanged += (_, _) => _minimumRms = ((Choice)_sensitivity.SelectedItem!).Value;
        _language.SelectedIndexChanged += (_, _) => { _english = _language.SelectedIndex == 1; PopulateChoices(); ApplyLanguage(); SaveSettings(); };
        _dark.CheckedChanged += (_, _) => { ApplyTheme(); SaveSettings(); };
        Shown += async (_, _) => await CheckUpdatesAsync(true);
        FormClosing += (_, _) => StopMicrophone(false);
        AcceptButton = _start;
    }

    private string T(string pl, string en) => _english ? en : pl;
    private void SaveSettings() => Preferences.Save(_english, _dark.Checked);
    private static string EnglishName(int midi)
    {
        string[] names = ["C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"];
        return $"{names[midi % 12]}{midi / 12 - 1}";
    }
    private void PopulateChoices()
    {
        int target = _target.SelectedItem is Choice old ? old.Id : -1;
        int sensitivity = _sensitivity.SelectedIndex < 0 ? 1 : _sensitivity.SelectedIndex;
        _target.Items.Clear();
        _target.Items.Add(new Choice(-1, 0, T("Automatycznie: najbliższy dźwięk", "Automatic: nearest note")));
        for (int midi = 33; midi <= 86; midi++)
            _target.Items.Add(new Choice(midi, 0, _english ? EnglishName(midi) : PitchDetector.NoteFromMidi(midi, 440).Name));
        _target.SelectedIndex = target < 33 ? 0 : target - 32;
        _sensitivity.Items.Clear();
        _sensitivity.Items.Add(new Choice(0, 0.004, T("Wysoka, dla cichych instrumentów", "High, for quiet instruments")));
        _sensitivity.Items.Add(new Choice(1, 0.008, T("Normalna", "Normal")));
        _sensitivity.Items.Add(new Choice(2, 0.016, T("Niska, przy hałasie w tle", "Low, for background noise")));
        _sensitivity.SelectedIndex = sensitivity;
    }
    private void ApplyLanguage()
    {
        Text = T("Stroik 1.0 — dostępny stroiciel", "Stroik 1.0 — accessible tuner");
        _heading.Text = "Stroik 1.0";
        _intro.Text = T("Zagraj pojedynczy dźwięk, potem go wycisz. Czytnik ekranu ogłosi wynik po około pół sekundy ciszy.",
            "Play one note, then let it stop. Your screen reader announces the result after about half a second of silence.");
        _status.Text = _microphone is null ? T("Mikrofon jest wyłączony.", "Microphone is off.") :
            T("Mikrofon włączony. Zagraj dźwięk i go wycisz.", "Microphone is on. Play a note and let it stop.");
        _start.Text = T("&Włącz mikrofon", "&Start microphone");
        _stop.Text = T("W&yłącz mikrofon", "S&top microphone");
        _repeat.Text = T("&Powtórz wynik", "&Repeat result");
        _test.Text = T("Sprawdź komunikat &NVDA", "&Test screen reader announcement");
        _updates.Text = T("Sprawdź &aktualizacje", "Check for &updates");
        _resultTitle.Text = T("Ostatni wynik:", "Last result:");
        _settingsTitle.Text = T("Ustawienia", "Settings");
        _targetTitle.Text = T("Dźwięk docelowy:", "Target note:");
        _concertTitle.Text = T("A4, częstotliwość odniesienia w hercach:", "A4 reference frequency in hertz:");
        _toleranceTitle.Text = T("Próg oceny „dobrze” w centach:", "In-tune tolerance in cents:");
        _sensitivityTitle.Text = T("Czułość mikrofonu:", "Microphone sensitivity:");
        _noteHelp.Text = T("Polskie H to angielskie B; polskie B to angielskie B-flat.", "English B is Polish H; English B-flat is Polish B.");
        _languageTitle.Text = T("Język:", "Language:");
        _dark.Text = T("Ciemny motyw", "Dark theme");
        foreach (Button button in new[] { _start, _stop, _repeat, _test, _updates }) button.AccessibleName = button.Text.Replace("&", "");
        _status.AccessibleName = T("Stan mikrofonu", "Microphone status");
        _result.AccessibleName = T("Ostatni wynik strojenia", "Last tuning result");
        _details.AccessibleName = T("Szczegóły pomiaru", "Measurement details");
        _target.AccessibleName = _targetTitle.Text;
        _concertA.AccessibleName = _concertTitle.Text;
        _tolerance.AccessibleName = _toleranceTitle.Text;
        _sensitivity.AccessibleName = _sensitivityTitle.Text;
        _language.AccessibleName = _languageTitle.Text;
        _dark.AccessibleName = _dark.Text;
        if (_hasResult) ShowResult(_lastMeasured, _lastReference, _lastTarget, _lastTolerance, false);
        else _result.Text = T("Wynik pojawi się po ucichnięciu dźwięku.", "The result appears when the note stops.");
    }
    private void ApplyTheme()
    {
        Color background = _dark.Checked ? Color.FromArgb(30, 30, 30) : SystemColors.Control;
        Color foreground = _dark.Checked ? Color.WhiteSmoke : SystemColors.ControlText;
        Color field = _dark.Checked ? Color.FromArgb(48, 48, 48) : SystemColors.Window;
        foreach (Control control in Walk(this))
        {
            control.BackColor = control is TextBox or ComboBox or NumericUpDown ? field : background;
            control.ForeColor = foreground;
            if (control is Button button) button.UseVisualStyleBackColor = !_dark.Checked;
        }
        if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
        {
            int enabled = _dark.Checked ? 1 : 0;
            if (DwmSetWindowAttribute(Handle, 20, ref enabled, sizeof(int)) != 0)
                DwmSetWindowAttribute(Handle, 19, ref enabled, sizeof(int));
        }
    }
    private static IEnumerable<Control> Walk(Control root)
    {
        yield return root;
        foreach (Control child in root.Controls) foreach (Control nested in Walk(child)) yield return nested;
    }
    private static Label MakeLabel(float size = 0) => new()
    {
        AutoSize = true, Dock = DockStyle.Fill, Padding = new Padding(0, 5, 0, 5),
        Font = size > 0 ? new Font(FontFamily.GenericSansSerif, size, FontStyle.Bold) : SystemFonts.DefaultFont
    };
    private static void Add(TableLayoutPanel panel, Control control)
    {
        int row = panel.RowCount++;
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        control.Margin = new Padding(0, 2, 0, 6);
        if (control is not FlowLayoutPanel) control.Dock = DockStyle.Fill;
        panel.Controls.Add(control, 0, row);
    }
    private void StartMicrophone()
    {
        if (_microphone is not null) return;
        var microphone = new Microphone();
        try
        {
            _session.Reset(); _windowCount = 0; _ignoreUntil = DateTime.MinValue;
            microphone.FrameReady += ProcessFrame;
            microphone.Start(); _microphone = microphone;
            _start.Enabled = false; _stop.Enabled = true;
            ApplyLanguage(); _stop.Focus(); Announce(_status.Text);
        }
        catch (Exception error)
        {
            microphone.Dispose();
            MessageBox.Show(this, error.Message, T("Nie można włączyć mikrofonu", "Cannot start microphone"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    private void StopMicrophone(bool announce = true)
    {
        if (_microphone is null) return;
        Microphone microphone = _microphone; _microphone = null;
        microphone.FrameReady -= ProcessFrame; microphone.Dispose();
        _session.Reset(); _windowCount = 0;
        _start.Enabled = true; _stop.Enabled = false;
        _status.Text = T("Mikrofon jest wyłączony.", "Microphone is off.");
        if (announce && !IsDisposed && !Disposing) { _start.Focus(); Announce(_status.Text); }
    }
    // This method runs on the audio capture thread. Only final results reach the UI thread.
    private void ProcessFrame(short[] samples)
    {
        DateTime now = DateTime.UtcNow;
        if (now < _ignoreUntil) { _windowCount = 0; return; }
        bool sound = PitchDetector.Rms(samples) >= _minimumRms;
        double? frequency = null;
        if (sound)
        {
            int overflow = Math.Max(0, _windowCount + samples.Length - _window.Length);
            if (overflow > 0) { Array.Copy(_window, overflow, _window, 0, _windowCount - overflow); _windowCount -= overflow; }
            int count = Math.Min(samples.Length, _window.Length);
            Array.Copy(samples, samples.Length - count, _window, _windowCount, count);
            _windowCount += count;
            if (_windowCount == _window.Length)
            {
                var estimate = PitchDetector.Estimate(_window, _microphone?.SampleRate ?? 48000);
                if (estimate is { Confidence: >= 0.75 }) frequency = estimate.Value.Frequency;
            }
        }
        else _windowCount = 0;
        double? completed = _session.Observe(sound, frequency, now);
        if (completed is > 0)
        {
            _ignoreUntil = now.AddMilliseconds(1500);
            double measured = completed.Value, reference = _reference, tolerance = _toleranceCents;
            int target = _targetMidi;
            try { BeginInvoke((Action)(() => ShowResult(measured, reference, target, tolerance))); }
            catch (InvalidOperationException) { /* Form closed. */ }
        }
    }
    private void ShowResult(double measured, double reference, int targetMidi, double tolerance, bool announce = true)
    {
        if (IsDisposed) return;
        _hasResult = true; _lastMeasured = measured; _lastReference = reference; _lastTarget = targetMidi; _lastTolerance = tolerance;
        Note note = targetMidi < 0 ? PitchDetector.Nearest(measured, reference) : PitchDetector.NoteFromMidi(targetMidi, reference);
        double hertz = measured - note.Frequency;
        double cents = 1200 * Math.Log2(measured / note.Frequency);
        CultureInfo culture = CultureInfo.GetCultureInfo(_english ? "en-US" : "pl-PL");
        string name = _english ? EnglishName(note.Midi) : note.Name;
        string hz = Math.Abs(hertz).ToString("0.0", culture);
        string direction = Math.Abs(cents) <= tolerance ? T("dobrze", "in tune") : hertz < 0 ? T("za nisko", "too low") : T("za wysoko", "too high");
        string offset = hertz < -0.05 ? $"minus {hz}" : hertz > 0.05 ? $"plus {hz}" : T("zero", "zero");
        string announcement = T($"{name}, {direction}, {offset} herca.", $"{name}, {direction}, {offset} hertz.");
        _result.Text = announcement;
        _details.Text = T($"Zmierzono: {measured.ToString("0.0", culture)} Hz. Cel: {note.Frequency.ToString("0.0", culture)} Hz. Odchyłka: {cents.ToString("+0.0;-0.0;0.0", culture)} centów.",
            $"Measured: {measured.ToString("0.0", culture)} Hz. Target: {note.Frequency.ToString("0.0", culture)} Hz. Difference: {cents.ToString("+0.0;-0.0;0.0", culture)} cents.");
        _lastAnnouncement = announcement; _repeat.Enabled = true;
        if (announce) Announce(announcement);
    }
    private void Announce(string message) => _result.AccessibilityObject.RaiseAutomationNotification(
        AutomationNotificationKind.ActionCompleted, AutomationNotificationProcessing.ImportantMostRecent, message);
    private async Task CheckUpdatesAsync(bool quiet)
    {
        _updates.Enabled = false;
        try
        {
            ReleaseInfo? release = await Updater.FindNewReleaseAsync();
            if (release is null)
            {
                if (!quiet) MessageBox.Show(this, T("Masz najnowszą wersję.", "You have the latest version."), T("Aktualizacje", "Updates"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (MessageBox.Show(this,
                T($"Dostępna jest wersja {release.Version}. Pobrać ją, zamknąć program i podmienić plik EXE?",
                  $"Version {release.Version} is available. Download it, close the app, and replace the EXE file?"),
                T("Nowa wersja Stroika", "New Stroik version"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            UseWaitCursor = true;
            string downloaded = await Updater.DownloadVerifiedAsync(release);
            UseWaitCursor = false;
            Updater.LaunchReplacement(downloaded);
            Close();
        }
        catch (Exception error)
        {
            UseWaitCursor = false;
            if (!quiet) MessageBox.Show(this, error.Message, T("Błąd aktualizacji", "Update error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { _updates.Enabled = true; }
    }
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr window, int attribute, ref int value, int valueSize);
    private sealed record Choice(int Id, double Value, string Description)
    {
        public override string ToString() => Description;
    }
}
