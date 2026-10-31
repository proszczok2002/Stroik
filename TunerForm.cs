using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms.Automation;

namespace Stroik;

internal sealed class TunerForm : Form
{
    private readonly Label _heading = MakeLabel(20), _intro = MakeLabel(), _status = MakeLabel();
    private readonly Label _methodTitle = MakeLabel(), _viewTitle = MakeLabel(), _beginnerHint = MakeLabel();
    private readonly Label _resultTitle = MakeLabel(), _details = MakeLabel(), _settingsTitle = MakeLabel(14);
    private readonly Label _concertTitle = MakeLabel(), _concertHelp = MakeLabel(), _sensitivityTitle = MakeLabel();
    private readonly Label _toleranceTitle = MakeLabel(), _octaveTitle = MakeLabel(), _targetTitle = MakeLabel();
    private readonly Label _noteHelp = MakeLabel(), _languageTitle = MakeLabel(), _intervalTitle = MakeLabel(), _forkHelp = MakeLabel();
    private readonly Button _start = new() { AutoSize = true }, _stop = new() { AutoSize = true, Enabled = false };
    private readonly Button _repeat = new() { AutoSize = true, Enabled = false }, _test = new() { AutoSize = true };
    private readonly Button _updates = new() { AutoSize = true }, _strike = new() { AutoSize = true };
    private readonly Button _saveProfile = new() { AutoSize = true }, _loadProfile = new() { AutoSize = true };
    private readonly TextBox _result = new() { ReadOnly = true, Multiline = true, Height = 64, TabStop = true };
    private readonly ComboBox _method = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _view = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _octave = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _target = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _sensitivity = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _language = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckBox _dark = new() { AutoSize = true }, _showOctave = new() { AutoSize = true }, _autoStrike = new() { AutoSize = true };
    private readonly NumericUpDown _concertA = new() { Minimum = 390, Maximum = 466, DecimalPlaces = 1, Increment = 0.1M, Value = 440 };
    private readonly NumericUpDown _tolerance = new() { Minimum = 1, Maximum = 20, Value = 5 };
    private readonly NumericUpDown _interval = new() { Minimum = 1, Maximum = 10, Value = 3 };
    private readonly System.Windows.Forms.Timer _strikeTimer = new();
    private readonly TuningFork _fork = new();
    private readonly NoteSession _session = new();
    private readonly short[] _window = new short[8192];
    private Microphone? _microphone;
    private int _windowCount, _analysisFrame, _targetMidi = NoteCatalog.DefaultMidi;
    private double _reference = 440, _toleranceCents = 5, _minimumRms = 0.008;
    private DateTime _ignoreUntil;
    private string? _lastAnnouncement;
    private bool _english, _hasResult, _initializing, _changingOctave;
    private double _lastMeasured, _lastReference, _lastTolerance;
    private int _lastTarget;

    internal TunerForm()
    {
        Preferences settings = Preferences.Load();
        _english = settings.English;
        MinimumSize = new Size(540, 700);
        Size = new Size(760, 850);
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Dpi;
        KeyPreview = true;

        var content = new TableLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, ColumnCount = 1, Padding = new Padding(18) };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Controls.Add(content);
        Add(content, _heading); Add(content, _intro);
        Add(content, _methodTitle); Add(content, _method); Add(content, _viewTitle); Add(content, _view);
        Add(content, _beginnerHint); Add(content, _status);
        var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };
        buttons.Controls.AddRange([_start, _stop, _strike, _repeat, _test, _updates]);
        Add(content, buttons);
        Add(content, _autoStrike); Add(content, _intervalTitle); Add(content, _interval); Add(content, _forkHelp);
        Add(content, _resultTitle); Add(content, _result); Add(content, _details);
        Add(content, _settingsTitle);
        Add(content, _concertTitle); Add(content, _concertA); Add(content, _concertHelp);
        Add(content, _sensitivityTitle); Add(content, _sensitivity);
        Add(content, _toleranceTitle); Add(content, _tolerance);
        Add(content, _showOctave); Add(content, _octaveTitle); Add(content, _octave);
        Add(content, _targetTitle); Add(content, _target); Add(content, _noteHelp);
        var profileButtons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };
        profileButtons.Controls.AddRange([_saveProfile, _loadProfile]);
        Add(content, profileButtons);
        Add(content, _languageTitle); Add(content, _language); Add(content, _dark);

        _initializing = true;
        _language.Items.AddRange(["Polski", "English"]);
        _language.SelectedIndex = settings.English ? 1 : 0;
        _dark.Checked = settings.DarkTheme;
        PopulateChoices(settings);
        _concertA.Value = (decimal)settings.ConcertA;
        _tolerance.Value = settings.ToleranceCents;
        _interval.Value = settings.StrikeIntervalSeconds;
        _showOctave.Checked = settings.ShowOctaveInClassic;
        _autoStrike.Checked = settings.AutoStrike;
        _reference = (double)_concertA.Value;
        _toleranceCents = (double)_tolerance.Value;
        _initializing = false;
        ApplyLanguage();
        ApplyViewUI();
        ApplyMethodUI();
        ApplyTheme();
        SetContextMenu();

        _start.Click += (_, _) => StartMicrophone();
        _stop.Click += (_, _) => StopMicrophone();
        _strike.Click += (_, _) => Strike();
        _repeat.Click += (_, _) => { if (_lastAnnouncement is not null) Announce(_lastAnnouncement); };
        _test.Click += (_, _) => Announce(T("Stroik. Komunikaty dla czytnika ekranu działają.", "Stroik. Screen reader announcements work."));
        _updates.Click += async (_, _) => await CheckUpdatesAsync(false);
        _saveProfile.Click += (_, _) => SaveProfile();
        _loadProfile.Click += (_, _) => LoadProfile();
        _method.SelectedIndexChanged += (_, _) => { if (_initializing) return; if (_method.SelectedIndex == 0 && _targetMidi < 0) SelectTarget(NoteCatalog.DefaultMidi); ApplyMethodUI(); Persist(); };
        _view.SelectedIndexChanged += (_, _) => { if (_initializing) return; ApplyViewUI(); Persist(); };
        _showOctave.CheckedChanged += (_, _) => { if (_initializing) return; ApplyViewUI(); Persist(); };
        _octave.SelectedIndexChanged += (_, _) => { if (_initializing || _changingOctave || _octave.SelectedIndex < 0) return; int selected = _targetMidi < 0 ? NoteCatalog.DefaultMidi : _targetMidi; int midi = Math.Clamp((_octave.SelectedIndex + 1) * 12 + selected % 12, NoteCatalog.LowestMidi, NoteCatalog.HighestMidi); if (NoteCatalog.Octave(midi) != _octave.SelectedIndex) midi = _octave.SelectedIndex == 0 ? NoteCatalog.LowestMidi : NoteCatalog.HighestMidi; SelectTarget(midi); Persist(); };
        _target.SelectedIndexChanged += (_, _) => { if (_initializing || _target.SelectedItem is not Choice choice) return; _targetMidi = choice.Id; SyncOctave(); Persist(); };
        _target.KeyPress += (_, e) => { if (char.ToLowerInvariant(e.KeyChar) is 'i' or 's') { SelectTarget(NoteCatalog.NextSharp(_targetMidi < 0 ? NoteCatalog.DefaultMidi : _targetMidi)); e.Handled = true; } };
        _concertA.ValueChanged += (_, _) => { _reference = (double)_concertA.Value; Persist(); };
        _tolerance.ValueChanged += (_, _) => { _toleranceCents = (double)_tolerance.Value; Persist(); };
        _sensitivity.SelectedIndexChanged += (_, _) => { if (_sensitivity.SelectedItem is Choice choice) _minimumRms = choice.Value; Persist(); };
        _language.SelectedIndexChanged += (_, _) => { if (_initializing) return; _english = _language.SelectedIndex == 1; var current = ReadSettings(); PopulateChoices(current); ApplyLanguage(); ApplyViewUI(); ApplyMethodUI(); SetContextMenu(); Persist(); };
        _dark.CheckedChanged += (_, _) => { ApplyTheme(); Persist(); };
        _autoStrike.CheckedChanged += (_, _) => { UpdateStrikeTimer(); Persist(); };
        _interval.ValueChanged += (_, _) => { UpdateStrikeTimer(); Persist(); };
        _strikeTimer.Tick += (_, _) => Strike();
        KeyDown += (_, e) => { if (e.Control && e.KeyCode == Keys.S) { SaveProfile(); e.SuppressKeyPress = true; } else if (e.Control && e.KeyCode == Keys.O) { LoadProfile(); e.SuppressKeyPress = true; } };
        Shown += async (_, _) => await CheckUpdatesAsync(true);
        FormClosing += (_, _) => { StopMicrophone(false); _strikeTimer.Stop(); _fork.Dispose(); };
        AcceptButton = _method.SelectedIndex == 0 ? _strike : _start;
    }

    private string T(string pl, string en) => _english ? en : pl;
    private static Label MakeLabel(float size = 0) => new() { AutoSize = true, Dock = DockStyle.Fill, Padding = new Padding(0, 5, 0, 5), Font = size > 0 ? new Font(FontFamily.GenericSansSerif, size, FontStyle.Bold) : SystemFonts.DefaultFont };
    private static void Add(TableLayoutPanel panel, Control control)
    {
        int row = panel.RowCount++;
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        control.Margin = new Padding(0, 2, 0, 6);
        if (control is not FlowLayoutPanel) control.Dock = DockStyle.Fill;
        panel.Controls.Add(control, 0, row);
    }
    private static IEnumerable<Control> Walk(Control root)
    {
        yield return root;
        foreach (Control child in root.Controls) foreach (Control nested in Walk(child)) yield return nested;
    }
    private void PopulateChoices(Preferences settings)
    {
        _initializing = true;
        _method.Items.Clear();
        _method.Items.AddRange([T("Kamerton", "Tuning fork"), T("Stroik elektroniczny", "Electronic tuner")]);
        _method.SelectedIndex = (int)settings.Method;
        _view.Items.Clear();
        _view.Items.AddRange([T("Widok klasyczny", "Classic view"), T("Widok dla początkujących", "Beginner view")]);
        _view.SelectedIndex = (int)settings.View;
        _octave.Items.Clear();
        for (int i = 0; i <= 8; i++) _octave.Items.Add(NoteCatalog.OctaveLabel(i, _english));
        _octave.SelectedIndex = settings.StartOctave;
        _target.Items.Clear();
        _target.Items.Add(new Choice(-1, 0, T("Automatycznie (stroik elektroniczny)", "Automatic (electronic tuner)")));
        for (int midi = NoteCatalog.LowestMidi; midi <= NoteCatalog.HighestMidi; midi++)
            _target.Items.Add(new Choice(midi, 0, NoteCatalog.Label(midi, _english)));
        SelectTarget(settings.TargetMidi);
        _targetMidi = settings.TargetMidi;
        _sensitivity.Items.Clear();
        _sensitivity.Items.Add(new Choice(0, 0.004, T("Wysoka, dla cichych instrumentów", "High, for quiet instruments")));
        _sensitivity.Items.Add(new Choice(1, 0.008, T("Normalna", "Normal")));
        _sensitivity.Items.Add(new Choice(2, 0.016, T("Niska, przy hałasie w tle", "Low, for background noise")));
        _sensitivity.SelectedIndex = settings.SensitivityIndex;
        _minimumRms = ((Choice)_sensitivity.SelectedItem!).Value;
        _reference = (double)_concertA.Value;
        _toleranceCents = (double)_tolerance.Value;
        _initializing = false;
    }
    private void SelectTarget(int midi) => _target.SelectedIndex = midi < 0 ? 0 : midi - NoteCatalog.LowestMidi + 1;
    private void SyncOctave()
    {
        if (_targetMidi < 0) return;
        _changingOctave = true;
        _octave.SelectedIndex = NoteCatalog.Octave(_targetMidi);
        _changingOctave = false;
    }
    private void ApplyMethodUI()
    {
        bool fork = _method.SelectedIndex == 0;
        if (fork) StopMicrophone(false);
        else { _strikeTimer.Stop(); _fork.Stop(); }
        foreach (Control control in new Control[] { _strike, _autoStrike, _intervalTitle, _interval, _forkHelp }) control.Visible = fork;
        foreach (Control control in new Control[] { _start, _stop, _repeat, _test, _resultTitle, _result, _details, _status, _sensitivityTitle, _sensitivity, _toleranceTitle, _tolerance }) control.Visible = !fork;
        _status.Text = _microphone is null ? T("Mikrofon jest wyłączony.", "Microphone is off.") : T("Mikrofon włączony.", "Microphone is on.");
        AcceptButton = fork ? _strike : _start;
        UpdateStrikeTimer();
    }
    private void ApplyViewUI()
    {
        bool beginner = _view.SelectedIndex == 1;
        _beginnerHint.Visible = beginner;
        _showOctave.Visible = !beginner;
        _octaveTitle.Visible = beginner || _showOctave.Checked;
        _octave.Visible = _octaveTitle.Visible;
    }
    private void UpdateStrikeTimer()
    {
        _strikeTimer.Stop();
        if (_method.SelectedIndex != 0 || !_autoStrike.Checked || IsDisposed) return;
        _strikeTimer.Interval = (int)_interval.Value * 1000;
        _strikeTimer.Start();
    }
    private void Strike()
    {
        try
        {
            int midi = _targetMidi < 0 ? NoteCatalog.DefaultMidi : _targetMidi;
            double frequency = PitchDetector.NoteFromMidi(midi, _reference).Frequency;
            _fork.Strike(frequency);
        }
        catch (Exception error)
        {
            _strikeTimer.Stop();
            MessageBox.Show(this, error.Message, T("Błąd kamertonu", "Tuning fork error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    private void ApplyLanguage()
    {
        Text = T("Stroik 1.01 — dostępny stroiciel", "Stroik 1.01 — accessible tuner");
        _heading.Text = "Stroik 1.01";
        _intro.Text = T("Wybierz kamerton lub stroik elektroniczny. Dźwięk docelowy: domyślnie g w oktawie razkreślnej.",
            "Choose a tuning fork or electronic tuner. The default target is G in the one-line octave.");
        _methodTitle.Text = T("Metoda strojenia:", "Tuning method:");
        _viewTitle.Text = T("Widok:", "View:");
        _beginnerHint.Text = T(
            "Wskazówka dla początkujących: przy starych pianinach i fortepianach zwykle zaczyna się od środka i pracuje równomiernie w obie strony, np. kwintami. Klucz ustaw możliwie pionowo (godzina 12). Po wytłumieniu bocznych strun najpierw stroi się środkową, potem boczne, aż ustanie dudnienie. Praca przy napiętych strunach może uszkodzić instrument lub zranić; stan zabytkowego instrumentu powinien ocenić technik.",
            "Beginner guidance: on older pianos, work from the middle evenly toward both ends, for example in fifths. Keep the tuning lever as upright as practical (12 o'clock). Mute the outer strings, tune the middle string first, then the outer strings until beating stops. High-tension strings can damage the instrument or cause injury; ask a technician to assess a historic piano.");
        _start.Text = T("&Włącz mikrofon", "&Start microphone");
        _stop.Text = T("W&yłącz mikrofon", "S&top microphone");
        _strike.Text = T("&Uderz kamerton", "&Strike tuning fork");
        _repeat.Text = T("&Powtórz wynik", "&Repeat result");
        _test.Text = T("Sprawdź komunikat &NVDA", "&Test screen reader announcement");
        _updates.Text = T("Sprawdź &aktualizacje", "Check for &updates");
        _autoStrike.Text = T("Uderzaj kamerton automatycznie", "Strike tuning fork automatically");
        _intervalTitle.Text = T("Odstęp między uderzeniami w sekundach:", "Seconds between strikes:");
        _forkHelp.Text = T("Kamerton wydaje krótki, gasnący ton. Jeśli nie masz pewności co do strojenia ze słuchu, najpierw ćwicz na niedrogim, łatwym do obsługi instrumencie, np. ukulele lub gitarze. Zerwanie struny nadal może zranić.",
            "The tuning fork plays a short, decaying tone. If unsure about tuning by ear, first practise on a modest, easy-to-handle instrument such as a ukulele or guitar. A broken string can still cause injury.");
        _resultTitle.Text = T("Ostatni wynik:", "Last result:");
        _settingsTitle.Text = T("Ustawienia", "Settings");
        _concertTitle.Text = T("Częstotliwość odniesienia A w hercach (standard 440 Hz):", "A reference frequency in hertz (standard 440 Hz):");
        _concertHelp.Text = T("Starszy instrument może wymagać niższego stroju, np. 430–435 Hz, ale wiek nie wyznacza bezpiecznej wartości. Przed podniesieniem naciągu starego pianina do 440 Hz poproś technika o ocenę stanu, konstrukcji i właściwej wysokości stroju.",
            "An older instrument may need a lower pitch, such as 430–435 Hz, but age alone does not determine a safe value. Before raising an old piano to 440 Hz, ask a piano technician to assess its condition, design and suitable pitch.");
        _sensitivityTitle.Text = T("Czułość mikrofonu:", "Microphone sensitivity:");
        _toleranceTitle.Text = T("Próg oceny „dobrze” w centach:", "In-tune tolerance in cents:");
        _showOctave.Text = T("Pokaż wybór oktawy", "Show octave selector");
        _octaveTitle.Text = T("Wybierz oktawę:", "Choose octave:");
        _targetTitle.Text = T("Dźwięk docelowy:", "Target note:");
        _noteHelp.Text = T("Strzałki przechodzą płynnie między oktawami. W liście dźwięków naciśnij I lub S, aby przejść do następnego dźwięku z krzyżykiem (cis, dis, fis, gis). Zakres listy: A subkontra – c pięciokreślne.",
            "Arrow keys move through octave boundaries. In the note list, press I or S for the next sharp note. The list covers the full 88-key piano range, A0–C8.");
        _saveProfile.Text = T("Zapisz plik ustawień (Ctrl+S)", "Save settings file (Ctrl+S)");
        _loadProfile.Text = T("Wczytaj plik ustawień (Ctrl+O)", "Open settings file (Ctrl+O)");
        _languageTitle.Text = T("Język:", "Language:");
        _dark.Text = T("Ciemny motyw", "Dark theme");
        foreach (Button button in new[] { _start, _stop, _repeat, _test, _updates, _strike, _saveProfile, _loadProfile }) button.AccessibleName = button.Text.Replace("&", "");
        _result.AccessibleName = T("Ostatni wynik strojenia", "Last tuning result");
        _details.AccessibleName = T("Szczegóły pomiaru", "Measurement details");
        _method.AccessibleName = _methodTitle.Text; _view.AccessibleName = _viewTitle.Text;
        _octave.AccessibleName = _octaveTitle.Text; _target.AccessibleName = _targetTitle.Text;
        _concertA.AccessibleName = _concertTitle.Text; _tolerance.AccessibleName = _toleranceTitle.Text;
        _sensitivity.AccessibleName = _sensitivityTitle.Text; _language.AccessibleName = _languageTitle.Text;
        _interval.AccessibleName = _intervalTitle.Text; _showOctave.AccessibleName = _showOctave.Text;
        _autoStrike.AccessibleName = _autoStrike.Text; _dark.AccessibleName = _dark.Text;
        if (_hasResult) ShowResult(_lastMeasured, _lastReference, _lastTarget, _lastTolerance, false);
        else _result.Text = T("Wynik pojawi się po ucichnięciu dźwięku.", "The result appears when the note stops.");
    }
    private Preferences ReadSettings() => new()
    {
        English = _english, DarkTheme = _dark.Checked, Method = (TuningMethod)Math.Max(0, _method.SelectedIndex),
        View = (TunerView)Math.Max(0, _view.SelectedIndex), ConcertA = (double)_concertA.Value,
        SensitivityIndex = Math.Max(0, _sensitivity.SelectedIndex), ToleranceCents = (int)_tolerance.Value,
        TargetMidi = _targetMidi, StartOctave = Math.Max(0, _octave.SelectedIndex),
        ShowOctaveInClassic = _showOctave.Checked, AutoStrike = _autoStrike.Checked, StrikeIntervalSeconds = (int)_interval.Value
    };
    private void Persist() { if (!_initializing) Preferences.Save(ReadSettings()); }
    private void ApplySettings(Preferences settings)
    {
        _initializing = true;
        _english = settings.English;
        _language.SelectedIndex = settings.English ? 1 : 0;
        _dark.Checked = settings.DarkTheme;
        _concertA.Value = (decimal)settings.ConcertA;
        _tolerance.Value = settings.ToleranceCents;
        _interval.Value = settings.StrikeIntervalSeconds;
        _showOctave.Checked = settings.ShowOctaveInClassic;
        _autoStrike.Checked = settings.AutoStrike;
        PopulateChoices(settings);
        _reference = (double)_concertA.Value;
        _toleranceCents = (double)_tolerance.Value;
        _initializing = false;
        ApplyLanguage(); ApplyViewUI(); ApplyMethodUI(); ApplyTheme(); Persist();
    }
    private void SetContextMenu()
    {
        ContextMenuStrip? previous = ContextMenuStrip;
        var menu = new ContextMenuStrip();
        menu.Items.Add(T("Zapisz plik ustawień", "Save settings file"), null, (_, _) => SaveProfile());
        menu.Items.Add(T("Wczytaj plik ustawień", "Open settings file"), null, (_, _) => LoadProfile());
        foreach (Control control in Walk(this)) control.ContextMenuStrip = menu;
        previous?.Dispose();
    }
    private void SaveProfile()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = T("Profil Stroika (*.json)|*.json", "Stroik profile (*.json)|*.json"),
            DefaultExt = "json", AddExtension = true,
            FileName = T("profil_stroika.json", "stroik_profile.json"),
            Title = T("Zapisz plik ustawień", "Save settings file")
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try { Preferences.SaveToFile(dialog.FileName, ReadSettings()); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or InvalidDataException)
        { MessageBox.Show(this, error.Message, T("Nie można zapisać", "Cannot save"), MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
    private void LoadProfile()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = T("Profil Stroika (*.json)|*.json", "Stroik profile (*.json)|*.json"),
            Title = T("Wczytaj plik ustawień", "Open settings file")
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try { ApplySettings(Preferences.ReadFile(dialog.FileName)); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or System.Text.Json.JsonException or InvalidDataException)
        { MessageBox.Show(this, error.Message, T("Nie można wczytać", "Cannot open"), MessageBoxButtons.OK, MessageBoxIcon.Error); }
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
    private void StartMicrophone()
    {
        if (_microphone is not null) return;
        var microphone = new Microphone();
        try
        {
            _session.Reset(); _windowCount = 0; _analysisFrame = 0; _ignoreUntil = DateTime.MinValue;
            microphone.FrameReady += ProcessFrame;
            microphone.Start(); _microphone = microphone;
            _start.Enabled = false; _stop.Enabled = true;
            _status.Text = T("Mikrofon włączony. Zagraj dźwięk i go wycisz.", "Microphone is on. Play a note and let it stop.");
            _stop.Focus(); Announce(_status.Text);
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
    // Runs on the audio capture thread. Only final results reach the UI thread.
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
            if (_windowCount == _window.Length && ++_analysisFrame % 3 == 0)
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
            catch (InvalidOperationException) { }
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
        string name = note.Midi is >= NoteCatalog.LowestMidi and <= NoteCatalog.HighestMidi
            ? NoteCatalog.Label(note.Midi, _english) : note.Name;
        string hz = Math.Abs(hertz).ToString("0.0", culture);
        string direction = Math.Abs(cents) <= tolerance ? T("dobrze", "in tune") : hertz < 0 ? T("za nisko", "too low") : T("za wysoko", "too high");
        string offset = hertz < -0.05 ? $"minus {hz}" : hertz > 0.05 ? $"plus {hz}" : "zero";
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
        bool prompted = false;
        _updates.Enabled = false;
        try
        {
            ReleaseInfo? release = await Updater.FindNewReleaseAsync();
            if (release is null)
            {
                if (!quiet) MessageBox.Show(this, T("Masz najnowszą wersję.", "You have the latest version."), T("Aktualizacje", "Updates"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            prompted = true;
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
            if (!quiet || prompted) MessageBox.Show(this, error.Message, T("Błąd aktualizacji", "Update error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
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
