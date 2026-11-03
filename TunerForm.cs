using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms.Automation;

namespace Stroik;

internal enum ForkShortcutAction { SingleStrike, Pause, Resume }

internal sealed class TunerForm : Form
{
    internal const string PayPalUrl = "https://paypal.me/proszczok";
    internal const string RevolutUrl = "https://revolut.me/r_rosiejka";
    internal const string WebsiteUrl = "https://proszczok.pl/";
    private readonly TabControl _tabs = new() { Dock = DockStyle.Fill, DrawMode = TabDrawMode.OwnerDrawFixed };
    private readonly TabPage _tunerTab = new(), _settingsTab = new();
    private readonly Label _preferencesIntro = MakeLabel();
    private readonly Label _heading = MakeLabel(20), _intro = MakeLabel(), _status = MakeLabel();
    private readonly Label _methodTitle = MakeLabel(), _instrumentTitle = MakeLabel(), _viewTitle = MakeLabel(), _beginnerHint = MakeLabel();
    private readonly Label _resultTitle = MakeLabel(), _details = MakeLabel(), _settingsTitle = MakeLabel(14);
    private readonly Label _concertTitle = MakeLabel(), _concertHelp = MakeLabel(), _sensitivityTitle = MakeLabel();
    private readonly Label _toleranceTitle = MakeLabel(), _octaveTitle = MakeLabel(), _targetTitle = MakeLabel();
    private readonly Label _noteHelp = MakeLabel(), _languageTitle = MakeLabel(), _intervalTitle = MakeLabel(), _forkHelp = MakeLabel();
    private readonly Label _ukuleleTitle = MakeLabel(), _orderTitle = MakeLabel(), _orderHelp = MakeLabel(), _guideText = MakeLabel();
    private readonly Label _visualHelp = MakeLabel(), _calibrationTitle = MakeLabel(), _calibrationHelp = MakeLabel(), _strikeHelp = MakeLabel(), _updateStatus = MakeLabel(), _automaticStatus = MakeLabel();
    private readonly Label _supportTitle = MakeLabel(14);
    private readonly Label _doubleStrikeWindowTitle = MakeLabel();
    private readonly Button _start = new() { AutoSize = true }, _stop = new() { AutoSize = true, Enabled = false };
    private readonly Button _repeat = new() { AutoSize = true, Enabled = false }, _test = new() { AutoSize = true };
    private readonly Button _updates = new() { AutoSize = true }, _strike = new() { AutoSize = true };
    private readonly Button _recentChanges = new() { AutoSize = true }, _about = new() { AutoSize = true };
    private readonly Button _paypal = new() { AutoSize = true }, _revolut = new() { AutoSize = true }, _website = new() { AutoSize = true };
    private readonly Button _next = new() { AutoSize = true };
    private readonly Button _saveProfile = new() { AutoSize = true }, _loadProfile = new() { AutoSize = true };
    private readonly TextBox _result = new() { ReadOnly = true, Multiline = true, Height = 64, TabStop = true };
    private readonly ComboBox _method = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _instrument = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _ukuleleTuning = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _pianoOrder = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _view = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _octave = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _target = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _sensitivity = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _language = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckBox _dark = new() { AutoSize = true }, _showOctave = new() { AutoSize = true }, _autoStrike = new() { AutoSize = true };
    private readonly CheckBox _doubleStrike = new() { AutoSize = true }, _visualMode = new() { AutoSize = true };
    private readonly PitchMeter _meter = new();
    private readonly KeyboardDiagram _keyboard = new();
    private readonly NumericUpDown _concertA = new() { Minimum = 390, Maximum = 466, DecimalPlaces = 1, Increment = 0.1M, Value = 440 };
    private readonly NumericUpDown _tolerance = new() { Minimum = 1, Maximum = 20, Value = 5 };
    private readonly NumericUpDown _interval = new() { Minimum = 1, Maximum = 10, Value = 3 };
    private readonly NumericUpDown _doubleStrikeWindow = new() { Minimum = 0.5M, Maximum = 2.5M, DecimalPlaces = 1, Increment = 0.1M, Value = 1.2M };
    private readonly NumericUpDown _calibration = new() { Minimum = -100, Maximum = 100, DecimalPlaces = 1, Increment = 0.1M };
    private readonly System.Windows.Forms.Timer _strikeTimer = new();
    private readonly TuningFork _fork = new();
    private readonly NoteSession _session = new();
    private readonly DoubleStrikeNavigator _strikeNavigator = new();
    private readonly object _navigationLock = new();
    private readonly short[] _window = new short[8192];
    private Microphone? _microphone;
    private int _windowCount, _analysisFrame, _targetMidi = NoteCatalog.DefaultMidi;
    private int _planAnchorMidi = NoteCatalog.DefaultMidi, _lastResolvedTarget = NoteCatalog.DefaultMidi;
    private volatile UkuleleTuning _ukuleleKind;
    private volatile bool _doubleStrikeEnabled, _visualEnabled;
    private int _doubleStrikeWindowMilliseconds = 1200;
    private bool _automaticStrikesRunning;
    private double _correctionFactor = 1;
    private DateTime _lastVisualAt;
    private bool _advancingTarget;
    private volatile InstrumentKind _instrumentKind = InstrumentKind.Piano;
    private double _reference = 440, _toleranceCents = 5, _minimumRms = 0.008;
    private DateTime _ignoreUntil;
    private string? _lastAnnouncement;
    private string _updateStatusPl = "Aktualizacje nie zostały jeszcze sprawdzone.", _updateStatusEn = "Updates have not been checked yet.";
    private bool _english, _hasResult, _initializing, _changingOctave;
    private double _lastMeasured, _lastReference, _lastTolerance;
    private int _lastTarget;
    private InstrumentKind _lastInstrument;
    private UkuleleTuning _lastUkulele;

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
        Controls.Add(_tabs);
        _tabs.TabPages.AddRange([_tunerTab, _settingsTab]);
        _tabs.SelectedIndex = 0;
        _tunerTab.Controls.Add(content);
        var settingsContent = new TableLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, ColumnCount = 1, Padding = new Padding(18) };
        settingsContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _settingsTab.Controls.Add(settingsContent);
        Add(content, _heading); Add(content, _intro);
        Add(content, _methodTitle); Add(content, _method);
        Add(content, _instrumentTitle); Add(content, _instrument);
        Add(content, _ukuleleTitle); Add(content, _ukuleleTuning);
        Add(content, _viewTitle); Add(content, _view);
        Add(content, _beginnerHint); Add(content, _status);
        var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };
        buttons.Controls.AddRange([_start, _stop, _strike, _next, _repeat, _test]);
        Add(content, buttons);
        Add(content, _autoStrike); Add(content, _automaticStatus); Add(content, _intervalTitle); Add(content, _interval); Add(content, _forkHelp);
        Add(content, _resultTitle); Add(content, _result); Add(content, _details); Add(content, _meter);
        Add(content, _settingsTitle);
        Add(content, _concertTitle); Add(content, _concertA); Add(content, _concertHelp);
        Add(content, _sensitivityTitle); Add(content, _sensitivity);
        Add(content, _toleranceTitle); Add(content, _tolerance);
        Add(content, _showOctave); Add(content, _octaveTitle); Add(content, _octave);
        Add(content, _targetTitle); Add(content, _target); Add(content, _noteHelp);
        Add(content, _orderTitle); Add(content, _pianoOrder); Add(content, _orderHelp);
        Add(content, _doubleStrike); Add(content, _doubleStrikeWindowTitle); Add(content, _doubleStrikeWindow); Add(content, _strikeHelp);
        Add(content, _guideText); Add(content, _keyboard);
        var profileButtons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };
        profileButtons.Controls.AddRange([_saveProfile, _loadProfile]);
        Add(content, profileButtons);
        Add(settingsContent, _preferencesIntro);
        Add(settingsContent, _languageTitle); Add(settingsContent, _language);
        Add(settingsContent, _dark); Add(settingsContent, _visualMode); Add(settingsContent, _visualHelp);
        Add(settingsContent, _calibrationTitle); Add(settingsContent, _calibration); Add(settingsContent, _calibrationHelp);
        Add(settingsContent, _updates); Add(settingsContent, _updateStatus);
        Add(settingsContent, _recentChanges); Add(settingsContent, _about); Add(settingsContent, _website);
        Add(settingsContent, _supportTitle);
        var supportButtons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };
        supportButtons.Controls.AddRange([_paypal, _revolut]);
        Add(settingsContent, supportButtons);

        _initializing = true;
        _language.Items.AddRange(["Polski", "English"]);
        _language.SelectedIndex = settings.English ? 1 : 0;
        _dark.Checked = settings.DarkTheme;
        _visualMode.Checked = settings.VisualMode ?? !ScreenReaderDetector.IsActive();
        _visualEnabled = _visualMode.Checked;
        _doubleStrike.Checked = settings.DoubleStrikeAdvance;
        _doubleStrikeEnabled = settings.DoubleStrikeAdvance;
        _doubleStrikeWindow.Value = settings.DoubleStrikeWindowMilliseconds / 1000M;
        _doubleStrikeWindowMilliseconds = settings.DoubleStrikeWindowMilliseconds;
        _calibration.Value = settings.MicrophoneCorrectionCents;
        _correctionFactor = Math.Pow(2, (double)_calibration.Value / 1200);
        _planAnchorMidi = settings.PlanAnchorMidi;
        PopulateChoices(settings);
        _concertA.Value = (decimal)settings.ConcertA;
        _tolerance.Value = settings.ToleranceCents;
        _interval.Value = settings.StrikeIntervalSeconds;
        _showOctave.Checked = settings.ShowOctaveInClassic;
        _autoStrike.Checked = settings.AutoStrike;
        _automaticStrikesRunning = settings.AutoStrike;
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
        _next.Click += (_, _) => AdvanceTarget(_targetMidi < 0 ? _lastResolvedTarget : _targetMidi);
        _repeat.Click += (_, _) => { if (_lastAnnouncement is not null) Announce(_lastAnnouncement); };
        _test.Click += (_, _) => Announce(T("Stroik. Komunikaty dla czytnika ekranu działają.", "Stroik. Screen reader announcements work."));
        _updates.Click += async (_, _) => await CheckUpdatesAsync(false);
        _recentChanges.Click += (_, _) => ShowRecentChanges();
        _about.Click += (_, _) => ShowAbout();
        _paypal.Click += (_, _) => OpenExternal(PayPalUrl);
        _revolut.Click += (_, _) => OpenExternal(RevolutUrl);
        _website.Click += (_, _) => OpenExternal(WebsiteUrl);
        _saveProfile.Click += (_, _) => SaveProfile();
        _loadProfile.Click += (_, _) => LoadProfile();
        _method.SelectedIndexChanged += (_, _) => { if (_initializing) return; if (_method.SelectedIndex == 0 && _targetMidi < 0) SelectTarget(InstrumentCatalog.DefaultMidi(_instrumentKind, _ukuleleKind)); ApplyMethodUI(); Persist(); };
        _instrument.SelectedIndexChanged += (_, _) => { if (_initializing) return; _instrumentKind = (InstrumentKind)_instrument.SelectedIndex; PopulateTargets(_instrumentKind, InstrumentCatalog.DefaultMidi(_instrumentKind, _ukuleleKind)); _planAnchorMidi = _targetMidi < 0 ? NoteCatalog.DefaultMidi : _targetMidi; ApplyLanguage(); ApplyViewUI(); UpdateGuide(); ResetResult(); Persist(); };
        _ukuleleTuning.SelectedIndexChanged += (_, _) => { if (_initializing) return; _ukuleleKind = (UkuleleTuning)_ukuleleTuning.SelectedIndex; PopulateTargets(_instrumentKind, InstrumentCatalog.DefaultMidi(_instrumentKind, _ukuleleKind)); ApplyLanguage(); UpdateGuide(); ResetResult(); Persist(); };
        _pianoOrder.SelectedIndexChanged += (_, _) => { if (_initializing) return; _planAnchorMidi = _targetMidi < 0 ? NoteCatalog.DefaultMidi : _targetMidi; Persist(); };
        _view.SelectedIndexChanged += (_, _) => { if (_initializing) return; ApplyViewUI(); Persist(); };
        _showOctave.CheckedChanged += (_, _) => { if (_initializing) return; ApplyViewUI(); Persist(); };
        _octave.SelectedIndexChanged += (_, _) => { if (_initializing || _changingOctave || _octave.SelectedIndex < 0) return; int selected = _targetMidi < 0 ? NoteCatalog.DefaultMidi : _targetMidi; int midi = Math.Clamp((_octave.SelectedIndex + 1) * 12 + selected % 12, NoteCatalog.LowestMidi, NoteCatalog.HighestMidi); if (NoteCatalog.Octave(midi) != _octave.SelectedIndex) midi = _octave.SelectedIndex == 0 ? NoteCatalog.LowestMidi : NoteCatalog.HighestMidi; SelectTarget(midi); Persist(); };
        _target.SelectedIndexChanged += (_, _) => { if (_initializing || _target.SelectedItem is not Choice choice) return; lock (_navigationLock) _strikeNavigator.Reset(); _targetMidi = choice.Id; if (!_advancingTarget && _instrumentKind == InstrumentKind.Piano && _targetMidi >= 0) _planAnchorMidi = _targetMidi; SyncOctave(); UpdateGuide(); Persist(); };
        _target.KeyPress += (_, e) => { if (!InstrumentCatalog.HasStrings(_instrumentKind) && char.ToLowerInvariant(e.KeyChar) is 'i' or 's') { SelectTarget(NoteCatalog.NextSharp(_targetMidi < 0 ? NoteCatalog.DefaultMidi : _targetMidi)); e.Handled = true; } };
        _concertA.ValueChanged += (_, _) => { _reference = (double)_concertA.Value; Persist(); };
        _tolerance.ValueChanged += (_, _) => { _toleranceCents = (double)_tolerance.Value; Persist(); };
        _sensitivity.SelectedIndexChanged += (_, _) => { if (_sensitivity.SelectedItem is Choice choice) _minimumRms = choice.Value; Persist(); };
        _language.SelectedIndexChanged += (_, _) => { if (_initializing) return; _english = _language.SelectedIndex == 1; var current = ReadSettings(); PopulateChoices(current); ApplyLanguage(); ApplyViewUI(); ApplyMethodUI(); SetContextMenu(); Persist(); };
        _dark.CheckedChanged += (_, _) => { ApplyTheme(); Persist(); };
        _autoStrike.CheckedChanged += (_, _) => { _automaticStrikesRunning = _autoStrike.Checked; if (!_automaticStrikesRunning) _fork.Stop(); UpdateStrikeTimer(); UpdateAutomaticStatus(); Persist(); };
        _interval.ValueChanged += (_, _) => { UpdateStrikeTimer(); Persist(); };
        _doubleStrike.CheckedChanged += (_, _) => { _doubleStrikeEnabled = _doubleStrike.Checked; _doubleStrikeWindow.Enabled = _doubleStrike.Checked; Persist(); };
        _doubleStrikeWindow.ValueChanged += (_, _) => { Volatile.Write(ref _doubleStrikeWindowMilliseconds, (int)(_doubleStrikeWindow.Value * 1000)); Persist(); };
        _visualMode.CheckedChanged += (_, _) => { _visualEnabled = _visualMode.Checked; ApplyViewUI(); Persist(); };
        _calibration.ValueChanged += (_, _) => { Volatile.Write(ref _correctionFactor, Math.Pow(2, (double)_calibration.Value / 1200)); ResetResult(); Persist(); };
        _strikeTimer.Tick += (_, _) => Strike();
        KeyDown += (_, e) => { if (e.Control && e.KeyCode == Keys.S) { SaveProfile(); e.SuppressKeyPress = true; } else if (e.Control && e.KeyCode == Keys.O) { LoadProfile(); e.SuppressKeyPress = true; } else if (e.Control && e.KeyCode == Keys.N) { AdvanceTarget(_targetMidi < 0 ? _lastResolvedTarget : _targetMidi); e.SuppressKeyPress = true; } };
        Shown += async (_, _) => await CheckUpdatesAsync(true);
        FormClosing += (_, _) => { StopMicrophone(false); _strikeTimer.Stop(); _fork.Dispose(); };
        AcceptButton = _method.SelectedIndex == 0 ? _strike : _start;
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == (Keys.Alt | Keys.U) && _method.SelectedIndex == 0)
        {
            ForkShortcutAction action = DecideForkShortcut(_autoStrike.Checked, _automaticStrikesRunning);
            if (action == ForkShortcutAction.SingleStrike) Strike();
            else
            {
                _automaticStrikesRunning = action == ForkShortcutAction.Resume;
                if (!_automaticStrikesRunning) _fork.Stop();
                UpdateStrikeTimer();
                UpdateAutomaticStatus();
                Announce(_automaticStatus.Text);
            }
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    internal static ForkShortcutAction DecideForkShortcut(bool automaticSelected, bool running) =>
        !automaticSelected ? ForkShortcutAction.SingleStrike :
        running ? ForkShortcutAction.Pause : ForkShortcutAction.Resume;
    private string T(string pl, string en) => _english ? en : pl;
    private void SetUpdateStatus(string pl, string en)
    {
        _updateStatusPl = pl; _updateStatusEn = en;
        _updateStatus.Text = T(pl, en);
    }
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
        _instrument.Items.Clear();
        _instrument.Items.AddRange([T("Pianino / fortepian", "Piano"), T("Gitara", "Guitar"), "Ukulele", T("Skrzypce", "Violin"), T("Gitara basowa", "Bass guitar"), T("Chromatyczny", "Chromatic")]);
        _instrument.SelectedIndex = (int)settings.Instrument;
        _instrumentKind = settings.Instrument;
        _ukuleleTuning.Items.Clear();
        _ukuleleTuning.Items.AddRange([T("Standardowy: od podłogi A–E–C–G", "Standard: floor to ceiling A–E–C–G"),
            T("Zamienione A/G: od podłogi G–E–C–A", "Swapped A/G: floor to ceiling G–E–C–A")]);
        _ukuleleTuning.SelectedIndex = (int)settings.UkuleleTuning;
        _ukuleleKind = settings.UkuleleTuning;
        _pianoOrder.Items.Clear();
        _pianoOrder.Items.AddRange([T("Rosnąco od wybranego dźwięku", "Ascending from selected note"),
            T("Od środka: po 5 półtonów w prawo i lewo", "From center: 5 semitones right, then left"),
            T("Od środka: po 5 całych tonów w prawo i lewo", "From center: 5 whole tones right, then left")]);
        _pianoOrder.SelectedIndex = (int)settings.PianoOrder;
        _octave.Items.Clear();
        for (int i = 0; i <= 8; i++) _octave.Items.Add(NoteCatalog.OctaveLabel(i, _english));
        _octave.SelectedIndex = settings.StartOctave;
        PopulateTargets(settings.Instrument, settings.TargetMidi);
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
    private void PopulateTargets(InstrumentKind kind, int requestedMidi)
    {
        bool wasInitializing = _initializing;
        _initializing = true;
        _target.Items.Clear();
        _target.Items.Add(new Choice(-1, 0, InstrumentCatalog.HasStrings(kind)
            ? T("Automatycznie: najbliższa pusta struna (stroik elektroniczny)", "Automatic: nearest open string (electronic tuner)")
            : T("Automatycznie: najbliższy dźwięk (stroik elektroniczny)", "Automatic: nearest note (electronic tuner)")));
        if (InstrumentCatalog.HasStrings(kind))
        {
            foreach (OpenString open in InstrumentCatalog.Strings(kind, _ukuleleKind))
                _target.Items.Add(new Choice(open.Midi, 0,
                    T($"Struna {open.Number}", $"String {open.Number}") +
                    (kind is InstrumentKind.Guitar or InstrumentKind.Ukulele
                        ? open.Number == 1 ? T(" (przy podłodze): ", " (floor side): ")
                                           : open.Number == InstrumentCatalog.Strings(kind, _ukuleleKind).Count ? T(" (przy suficie): ", " (ceiling side): ") : ": "
                        : ": ") + NoteCatalog.Label(open.Midi, _english)));
        }
        else
        {
            for (int midi = NoteCatalog.LowestMidi; midi <= NoteCatalog.HighestMidi; midi++)
                _target.Items.Add(new Choice(midi, 0, NoteCatalog.Label(midi, _english)));
        }
        int selected = -1;
        for (int i = 0; i < _target.Items.Count; i++)
            if (_target.Items[i] is Choice choice && choice.Id == requestedMidi) { selected = i; break; }
        _target.SelectedIndex = selected >= 0 ? selected : InstrumentCatalog.HasStrings(kind) ? 1 : NoteCatalog.DefaultMidi - NoteCatalog.LowestMidi + 1;
        _targetMidi = ((Choice)_target.SelectedItem!).Id;
        SyncOctave();
        _initializing = wasInitializing;
    }
    private void SelectTarget(int midi)
    {
        for (int i = 0; i < _target.Items.Count; i++)
            if (_target.Items[i] is Choice choice && choice.Id == midi) { _target.SelectedIndex = i; return; }
    }
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
        foreach (Control control in new Control[] { _strike, _autoStrike, _automaticStatus, _intervalTitle, _interval, _forkHelp }) control.Visible = fork;
        foreach (Control control in new Control[] { _start, _stop, _repeat, _test, _resultTitle, _result, _details, _status, _sensitivityTitle, _sensitivity, _toleranceTitle, _tolerance, _doubleStrike, _doubleStrikeWindowTitle, _doubleStrikeWindow, _strikeHelp }) control.Visible = !fork;
        _doubleStrikeWindow.Enabled = !fork && _doubleStrike.Checked;
        _status.Text = _microphone is null ? T("Mikrofon jest wyłączony.", "Microphone is off.") : T("Mikrofon włączony.", "Microphone is on.");
        AcceptButton = fork ? _strike : _start;
        UpdateStrikeTimer();
        UpdateAutomaticStatus();
        ApplyViewUI();
    }
    private void ApplyViewUI()
    {
        bool beginner = _view.SelectedIndex == 1;
        _beginnerHint.Visible = beginner;
        bool fullRange = !InstrumentCatalog.HasStrings(_instrumentKind);
        _showOctave.Visible = !beginner && fullRange;
        _octaveTitle.Visible = fullRange && (beginner || _showOctave.Checked);
        _octave.Visible = _octaveTitle.Visible;
        _concertHelp.Visible = _instrumentKind == InstrumentKind.Piano;
        _ukuleleTitle.Visible = _ukuleleTuning.Visible = _instrumentKind == InstrumentKind.Ukulele;
        _orderTitle.Visible = _pianoOrder.Visible = _orderHelp.Visible = _instrumentKind == InstrumentKind.Piano;
        _guideText.Visible = _instrumentKind == InstrumentKind.Piano;
        _keyboard.Visible = _instrumentKind == InstrumentKind.Piano && _visualMode.Checked;
        _meter.Visible = _visualMode.Checked && _method.SelectedIndex == 1;
    }
    private void UpdateStrikeTimer()
    {
        _strikeTimer.Stop();
        if (_method.SelectedIndex != 0 || !_autoStrike.Checked || !_automaticStrikesRunning || IsDisposed) return;
        _strikeTimer.Interval = (int)_interval.Value * 1000;
        _strikeTimer.Start();
    }
    private void UpdateAutomaticStatus()
    {
        _automaticStatus.Text = !_autoStrike.Checked
            ? T("Tryb ręczny. Alt+U: pojedyncze uderzenie.", "Manual mode. Alt+U: one strike.")
            : _automaticStrikesRunning
                ? T("Automatyczne uderzenia działają. Alt+U: pauza.", "Automatic strikes are running. Alt+U: pause.")
                : T("Automatyczne uderzenia wstrzymane. Alt+U: wznowienie.", "Automatic strikes are paused. Alt+U: resume.");
        _autoStrike.AccessibleDescription = _automaticStatus.Text;
    }
    private void Strike()
    {
        try
        {
            int midi = _targetMidi < 0 ? InstrumentCatalog.DefaultMidi(_instrumentKind, _ukuleleKind) : _targetMidi;
            double frequency = PitchDetector.NoteFromMidi(midi, _reference).Frequency;
            _fork.Strike(frequency);
        }
        catch (Exception error)
        {
            _strikeTimer.Stop();
            _automaticStrikesRunning = false;
            UpdateAutomaticStatus();
            MessageBox.Show(this, error.Message, T("Błąd kamertonu", "Tuning fork error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    private void ApplyLanguage()
    {
        Text = T("Stroik 1.09 — dostępny stroiciel", "Stroik 1.09 — accessible tuner");
        _heading.Text = "Stroik 1.09";
        _tunerTab.Text = T("Stroik", "Tuner");
        _settingsTab.Text = T("Ustawienia", "Settings");
        _tunerTab.AccessibleName = _tunerTab.Text;
        _settingsTab.AccessibleName = _settingsTab.Text;
        _preferencesIntro.Text = T("Język, wygląd, korekta mikrofonu, aktualizacje i informacje o programie.", "Language, appearance, microphone correction, updates and program information.");
        _tabs.Invalidate();
        _intro.Text = T("Wybierz instrument, metodę strojenia i dźwięk. Domyślnie: pianino i g w oktawie razkreślnej.",
            "Choose an instrument, tuning method and note. Defaults: piano and G in the one-line octave.");
        _methodTitle.Text = T("Metoda strojenia:", "Tuning method:");
        _instrumentTitle.Text = T("Instrument:", "Instrument:");
        _viewTitle.Text = T("Widok:", "View:");
        _beginnerHint.Text = _instrumentKind switch
        {
            InstrumentKind.Piano => T(
                "Wskazówka dla początkujących: przy starych pianinach i fortepianach zwykle zaczyna się od środka i pracuje równomiernie w obie strony, np. kwintami. Klucz ustaw możliwie pionowo (godzina 12). Po wytłumieniu bocznych strun najpierw stroi się środkową, potem boczne, aż ustanie dudnienie. Praca przy napiętych strunach może uszkodzić instrument lub zranić; stan zabytkowego instrumentu powinien ocenić technik. Program nie oblicza indywidualnego stroju rozszerzonego oktaw.",
                "Beginner guidance: on older pianos, work from the middle evenly toward both ends, for example in fifths. Keep the tuning lever as upright as practical (12 o'clock). Mute the outer strings, tune the middle string first, then the outer strings until beating stops. High-tension strings can damage the instrument or cause injury; ask a technician to assess a historic piano. This app does not calculate an individual stretched-octave tuning."),
            InstrumentKind.Guitar => T("Gitara: od podłogi struny to E razkreślne, H małe, G małe, D małe, A wielkie, E wielkie. G jest struną 3. Graj jedną pustą strunę naraz i po strojeniu sprawdź je ponownie.", "Guitar: from the floor, the strings are high E, B, G, D, A, low E. G is string 3. Play one open string at a time and recheck after tuning."),
            InstrumentKind.Ukulele => _ukuleleKind == UkuleleTuning.Standard
                ? T("Ukulele standardowe: od podłogi A–E–C–G, wszystkie w oktawie razkreślnej. G jest struną 4 przy suficie. Graj jedną pustą strunę naraz.", "Standard ukulele: floor to ceiling A–E–C–G, all in the one-line octave. G is string 4 on the ceiling side. Play one open string at a time.")
                : T("Twoje ukulele: od podłogi G–E–C–A, wszystkie w oktawie razkreślnej. G jest struną 1 przy podłodze, A struną 4 przy suficie.", "Swapped ukulele: floor to ceiling G–E–C–A, all in the one-line octave. G is string 1 on the floor side, A string 4 on the ceiling side."),
            InstrumentKind.Violin => T("Skrzypce: strój po jednej pustej strunie G–D–A–E. Kręć kołkami ostrożnie; zbyt duży naciąg może zerwać strunę.", "Violin: tune one open G–D–A–E string at a time. Turn pegs carefully; excessive tension can break a string."),
            InstrumentKind.Bass => T("Gitara basowa: graj po jednej pustej strunie, od niskiego E przez A i D do G. Jeśli mikrofon myli oktawę, spróbuj czystszego sygnału lub dźwięku harmonicznego.", "Bass guitar: play one open string at a time, from low E through A and D to G. If the microphone confuses octaves, try a cleaner signal or a harmonic."),
            _ => T("Tryb chromatyczny: dostępne są wszystkie dźwięki od A subkontra do c pięciokreślnego. Graj jeden dźwięk naraz.", "Chromatic mode: all notes from A0 to C8 are available. Play one note at a time.")
        };
        _ukuleleTitle.Text = T("Strój ukulele:", "Ukulele tuning:");
        _orderTitle.Text = T("Kolejność strojenia pianina:", "Piano tuning order:");
        _orderHelp.Text = T("Plan prowadzi tylko po dźwiękach. Wariant całotonowy pomija co drugi półton; pozostałe nuty trzeba później nastroić osobno. Kolejność nie gwarantuje bezpiecznego rozkładu naprężeń — przy starym pianinie skonsultuj technika.", "The plan only guides note selection. Whole-tone mode skips alternate semitones; tune the remaining notes separately. The order does not guarantee safe tension distribution; consult a technician for an old piano.");
        _start.Text = T("&Włącz mikrofon", "&Start microphone");
        _stop.Text = T("W&yłącz mikrofon", "S&top microphone");
        _strike.Text = T("&Uderz kamerton", "&Strike tuning fork");
        _next.Text = T("&Następny dźwięk (Ctrl+N)", "&Next note (Ctrl+N)");
        _repeat.Text = T("&Powtórz wynik", "&Repeat result");
        _test.Text = T("Sprawdź komunikat &NVDA", "&Test screen reader announcement");
        _updates.Text = T("Sprawdź &aktualizacje", "Check for &updates");
        _recentChanges.Text = T("Ostatnie zmiany", "Latest changes");
        _about.Text = T("O programie", "About Stroik");
        _website.Text = T("Strona Proszczoka", "Proszczok website");
        _supportTitle.Text = T("Wsparcie projektu", "Support the project");
        _paypal.Text = T("Wesprzyj przez PayPal", "Support via PayPal");
        _revolut.Text = T("Wesprzyj przez Revolut", "Support via Revolut");
        _updateStatus.Text = T(_updateStatusPl, _updateStatusEn);
        _updateStatus.AccessibleName = T("Stan aktualizacji", "Update status");
        _autoStrike.Text = T("Uderzaj kamerton automatycznie", "Strike tuning fork automatically");
        _doubleStrike.Text = T("Po dobrym wyniku dwa szybkie uderzenia przechodzą do następnego dźwięku", "After an in-tune result, two quick strikes advance to the next note");
        _doubleStrikeWindowTitle.Text = T("Maksymalny odstęp między uderzeniami (0,5–2,5 s; domyślnie 1,2):", "Maximum gap between strikes (0.5–2.5 s; default 1.2):");
        _strikeHelp.Text = T("Po wyniku „dobrze” poczekaj na koniec komunikatu, a następnie dwukrotnie uderz tę samą nutę w ustawionym czasie. Uderzenia mogą mieć różną głośność, ale oba muszą być rozpoznane jako właściwy dźwięk. Dłuższy czas jest wygodniejszy, lecz zwiększa ryzyko przypadkowego przejścia. Ctrl+N przechodzi ręcznie.",
            "After an in-tune result, wait for the announcement to finish, then strike the same note twice within the selected time. Strikes may differ in volume, but both must be recognized at the target pitch. A longer time is easier to use but raises the chance of accidental advancement. Ctrl+N advances manually.");
        _intervalTitle.Text = T("Odstęp między uderzeniami w sekundach:", "Seconds between strikes:");
        _forkHelp.Text = T("Kamerton wydaje krótki, gasnący ton. Jeśli nie masz pewności co do strojenia ze słuchu, najpierw ćwicz na niedrogim, łatwym do obsługi instrumencie, np. ukulele lub gitarze. Zerwanie struny nadal może zranić.",
            "The tuning fork plays a short, decaying tone. If unsure about tuning by ear, first practise on a modest, easy-to-handle instrument such as a ukulele or guitar. A broken string can still cause injury.");
        _resultTitle.Text = T("Ostatni wynik:", "Last result:");
        _settingsTitle.Text = T("Parametry strojenia", "Tuning parameters");
        _concertTitle.Text = T("Częstotliwość odniesienia A w hercach (standard 440 Hz):", "A reference frequency in hertz (standard 440 Hz):");
        _concertHelp.Text = T("Starszy instrument może wymagać niższego stroju, np. 430–435 Hz, ale wiek nie wyznacza bezpiecznej wartości. Przed podniesieniem naciągu starego pianina do 440 Hz poproś technika o ocenę stanu, konstrukcji i właściwej wysokości stroju.",
            "An older instrument may need a lower pitch, such as 430–435 Hz, but age alone does not determine a safe value. Before raising an old piano to 440 Hz, ask a piano technician to assess its condition, design and suitable pitch.");
        _sensitivityTitle.Text = T("Czułość mikrofonu:", "Microphone sensitivity:");
        _toleranceTitle.Text = T("Próg oceny „dobrze” w centach:", "In-tune tolerance in cents:");
        _showOctave.Text = T("Pokaż wybór oktawy", "Show octave selector");
        _octaveTitle.Text = T("Wybierz oktawę:", "Choose octave:");
        _targetTitle.Text = InstrumentCatalog.HasStrings(_instrumentKind)
            ? T("Struna / dźwięk docelowy:", "String / target note:")
            : T("Dźwięk docelowy:", "Target note:");
        _noteHelp.Text = InstrumentCatalog.HasStrings(_instrumentKind)
            ? T("Strzałkami wybierz pustą strunę. Tryb automatyczny porównuje dźwięk z najbliższą struną. Ukulele ma dwa układy G/A; dla innych strojów użyj trybu chromatycznego.",
                "Use the arrow keys to select an open string. Automatic mode compares pitch with the nearest open string. Ukulele has two G/A layouts; use chromatic mode for other tunings.")
            : T("Strzałki przechodzą płynnie między oktawami. W liście dźwięków naciśnij I lub S, aby przejść do następnego dźwięku z krzyżykiem (cis, dis, fis, gis). Zakres listy: A subkontra – c pięciokreślne.",
                "Arrow keys move through octave boundaries. In the note list, press I or S for the next sharp note. The list covers the full 88-key piano range, A0–C8.");
        _saveProfile.Text = T("Zapisz plik ustawień (Ctrl+S)", "Save settings file (Ctrl+S)");
        _loadProfile.Text = T("Wczytaj plik ustawień (Ctrl+O)", "Open settings file (Ctrl+O)");
        _languageTitle.Text = T("Język:", "Language:");
        _visualMode.Text = T("Pokazuj wskaźnik na żywo i klawiaturę", "Show live meter and keyboard");
        _visualHelp.Text = T("Przy wykrytym czytniku ekranu grafika jest domyślnie wyłączona. Wykrywanie nie jest pewne; tę opcję można zawsze zmienić ręcznie.", "Graphics default to off when a screen reader is detected. Detection is not certain; this option can always be changed manually.");
        _calibrationTitle.Text = T("Ręczna korekta pomiaru mikrofonu w centach (−100 do +100):", "Manual microphone pitch correction in cents (−100 to +100):");
        _calibrationHelp.Text = T("Użyj tylko po porównaniu ze znanym wzorcem. Dodatnia wartość podwyższa odczyt, ujemna obniża. Błędnie nastrojony instrument nie oznacza błędu mikrofonu.", "Use only after comparing with a known reference. A positive value raises the reading; a negative value lowers it. An out-of-tune instrument does not imply a microphone error.");
        _dark.Text = T("Ciemny motyw", "Dark theme");
        foreach (Button button in new[] { _start, _stop, _repeat, _test, _updates, _recentChanges, _about, _website, _paypal, _revolut, _strike, _next, _saveProfile, _loadProfile }) button.AccessibleName = button.Text.Replace("&", "");
        _result.AccessibleName = T("Ostatni wynik strojenia", "Last tuning result");
        _details.AccessibleName = T("Szczegóły pomiaru", "Measurement details");
        _method.AccessibleName = _methodTitle.Text; _instrument.AccessibleName = _instrumentTitle.Text; _view.AccessibleName = _viewTitle.Text;
        _ukuleleTuning.AccessibleName = _ukuleleTitle.Text; _pianoOrder.AccessibleName = _orderTitle.Text;
        _octave.AccessibleName = _octaveTitle.Text; _target.AccessibleName = _targetTitle.Text;
        _concertA.AccessibleName = _concertTitle.Text; _tolerance.AccessibleName = _toleranceTitle.Text;
        _sensitivity.AccessibleName = _sensitivityTitle.Text; _language.AccessibleName = _languageTitle.Text;
        _interval.AccessibleName = _intervalTitle.Text; _showOctave.AccessibleName = _showOctave.Text;
        _autoStrike.AccessibleName = _autoStrike.Text; _dark.AccessibleName = _dark.Text;
        _automaticStatus.AccessibleName = T("Stan kamertonu", "Tuning fork status");
        UpdateAutomaticStatus();
        _doubleStrike.AccessibleName = _doubleStrike.Text; _visualMode.AccessibleName = _visualMode.Text;
        _doubleStrikeWindow.AccessibleName = _doubleStrikeWindowTitle.Text;
        _calibration.AccessibleName = _calibrationTitle.Text;
        _meter.AccessibleName = T("Wizualny wskaźnik odchyłki stroju", "Visual tuning deviation meter");
        _keyboard.AccessibleName = T("Wirtualna klawiatura z zaznaczonym dźwiękiem", "Virtual keyboard with selected note");
        _meter.SetLanguage(_english);
        UpdateGuide();
        if (_hasResult) ShowResult(_lastMeasured, _lastReference, _lastTarget, _lastTolerance, _lastInstrument, _lastUkulele, false);
        else _result.Text = T("Wynik pojawi się po ucichnięciu dźwięku.", "The result appears when the note stops.");
    }
    private Preferences ReadSettings() => new()
    {
        English = _english, DarkTheme = _dark.Checked, Method = (TuningMethod)Math.Max(0, _method.SelectedIndex),
        View = (TunerView)Math.Max(0, _view.SelectedIndex), Instrument = _instrumentKind, ConcertA = (double)_concertA.Value,
        UkuleleTuning = _ukuleleKind, PianoOrder = (PianoOrder)Math.Max(0, _pianoOrder.SelectedIndex),
        PlanAnchorMidi = _planAnchorMidi, DoubleStrikeAdvance = _doubleStrike.Checked,
        DoubleStrikeWindowMilliseconds = (int)(_doubleStrikeWindow.Value * 1000),
        MicrophoneCorrectionCents = _calibration.Value, VisualMode = _visualMode.Checked,
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
        _visualMode.Checked = settings.VisualMode ?? !ScreenReaderDetector.IsActive();
        _visualEnabled = _visualMode.Checked;
        _doubleStrike.Checked = settings.DoubleStrikeAdvance;
        _doubleStrikeEnabled = settings.DoubleStrikeAdvance;
        _doubleStrikeWindow.Value = settings.DoubleStrikeWindowMilliseconds / 1000M;
        Volatile.Write(ref _doubleStrikeWindowMilliseconds, settings.DoubleStrikeWindowMilliseconds);
        _calibration.Value = settings.MicrophoneCorrectionCents;
        Volatile.Write(ref _correctionFactor, Math.Pow(2, (double)_calibration.Value / 1200));
        _planAnchorMidi = settings.PlanAnchorMidi;
        _concertA.Value = (decimal)settings.ConcertA;
        _tolerance.Value = settings.ToleranceCents;
        _interval.Value = settings.StrikeIntervalSeconds;
        _showOctave.Checked = settings.ShowOctaveInClassic;
        _autoStrike.Checked = settings.AutoStrike;
        _automaticStrikesRunning = settings.AutoStrike;
        PopulateChoices(settings);
        _reference = (double)_concertA.Value;
        _toleranceCents = (double)_tolerance.Value;
        _initializing = false;
        ApplyLanguage(); ApplyViewUI(); ApplyMethodUI(); ApplyTheme(); UpdateGuide(); ResetResult(); Persist();
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
    private void DrawTab(DrawItemEventArgs e)
    {
        bool selected = e.Index == _tabs.SelectedIndex;
        Color background = _dark.Checked
            ? (selected ? Color.FromArgb(48, 48, 48) : Color.FromArgb(36, 36, 36))
            : (selected ? SystemColors.ControlLightLight : SystemColors.Control);
        using var brush = new SolidBrush(background);
        e.Graphics.FillRectangle(brush, e.Bounds);
        TextRenderer.DrawText(e.Graphics, _tabs.TabPages[e.Index].Text, _tabs.Font, e.Bounds,
            _dark.Checked ? Color.WhiteSmoke : SystemColors.ControlText,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        if ((e.State & DrawItemState.Focus) != 0) ControlPaint.DrawFocusRectangle(e.Graphics, e.Bounds);
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
        _tabs.Invalidate();
        _meter.Invalidate(); _keyboard.Invalidate();
        if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
        {
            int enabled = _dark.Checked ? 1 : 0;
            if (DwmSetWindowAttribute(Handle, 20, ref enabled, sizeof(int)) != 0)
                DwmSetWindowAttribute(Handle, 19, ref enabled, sizeof(int));
        }
    }
    private void UpdateGuide()
    {
        if (_instrumentKind != InstrumentKind.Piano) return;
        int midi = _targetMidi >= 0 ? _targetMidi : _lastResolvedTarget;
        if (midi is < NoteCatalog.LowestMidi or > NoteCatalog.HighestMidi) midi = NoteCatalog.DefaultMidi;
        _guideText.Text = KeyboardGuide.Describe(midi, _english);
        _keyboard.SelectNote(midi);
    }
    private void AdvanceTarget(int current)
    {
        if (current is < NoteCatalog.LowestMidi or > NoteCatalog.HighestMidi)
            current = InstrumentCatalog.DefaultMidi(_instrumentKind, _ukuleleKind);
        int? next;
        if (_instrumentKind == InstrumentKind.Piano)
        {
            PianoOrder order = (PianoOrder)Math.Max(0, _pianoOrder.SelectedIndex);
            if (!TuningPlan.Build(_planAnchorMidi, order).Contains(current)) _planAnchorMidi = current;
            next = TuningPlan.Next(_planAnchorMidi, current, order);
        }
        else if (InstrumentCatalog.HasStrings(_instrumentKind))
        {
            IReadOnlyList<OpenString> strings = InstrumentCatalog.Strings(_instrumentKind, _ukuleleKind);
            int index = -1;
            for (int i = 0; i < strings.Count; i++) if (strings[i].Midi == current) { index = i; break; }
            next = index >= 0 && index + 1 < strings.Count ? strings[index + 1].Midi : null;
        }
        else next = current < NoteCatalog.HighestMidi ? current + 1 : null;
        if (next is null)
        {
            Announce(T("To koniec wybranej kolejności strojenia.", "End of the selected tuning order."));
            return;
        }
        _advancingTarget = true;
        try { SelectTarget(next.Value); }
        finally { _advancingTarget = false; }
        UpdateGuide();
        ResetResult();
        Persist();
        Announce(T($"Następny dźwięk: {NoteCatalog.Label(next.Value, false)}.",
            $"Next note: {NoteCatalog.Label(next.Value, true)}."));
    }

    private void StartMicrophone()
    {
        if (_microphone is not null) return;
        var microphone = new Microphone();
        try
        {
            _session.Reset(); lock (_navigationLock) _strikeNavigator.Reset();
            _windowCount = 0; _analysisFrame = 0; _ignoreUntil = DateTime.MinValue;
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
        _session.Reset(); lock (_navigationLock) _strikeNavigator.Reset();
        _windowCount = 0; _meter.SetReading(null);
        _start.Enabled = true; _stop.Enabled = false;
        _status.Text = T("Mikrofon jest wyłączony.", "Microphone is off.");
        if (announce && !IsDisposed && !Disposing) { _start.Focus(); Announce(_status.Text); }
    }
    // The audio thread emits final spoken results and throttled, silent visual readings.
    private void ProcessFrame(short[] samples)
    {
        DateTime now = DateTime.UtcNow;
        if (now < _ignoreUntil) { _windowCount = 0; return; }
        double rms = PitchDetector.Rms(samples);
        bool sound = rms >= _minimumRms;
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
        double correction = Volatile.Read(ref _correctionFactor);
        if (frequency is > 0) frequency *= correction;
        int? advance = null;
        bool needsQuickPitch = false;
        if (_doubleStrikeEnabled)
        {
            lock (_navigationLock)
            {
                advance = _strikeNavigator.Observe(rms, _minimumRms, frequency, now,
                    Volatile.Read(ref _doubleStrikeWindowMilliseconds));
                needsQuickPitch = advance is null && _strikeNavigator.IsAwaitingPitch && frequency is null && _windowCount >= 4096;
            }
            if (needsQuickPitch)
            {
                var quick = PitchDetector.Estimate(_window.AsSpan(0, 4096), _microphone?.SampleRate ?? 48000);
                if (quick is { Confidence: >= 0.75 })
                {
                    lock (_navigationLock)
                        advance = _strikeNavigator.Observe(rms, _minimumRms, quick.Value.Frequency * correction, now,
                            Volatile.Read(ref _doubleStrikeWindowMilliseconds));
                }
            }
        }
        else lock (_navigationLock) _strikeNavigator.Reset();
        if (advance is int midi)
        {
            _ignoreUntil = now.AddMilliseconds(1500);
            _session.Reset(); _windowCount = 0;
            try { BeginInvoke((Action)(() => AdvanceTarget(midi))); }
            catch (InvalidOperationException) { }
            return;
        }
        if (_visualEnabled && frequency is > 0 && now - _lastVisualAt >= TimeSpan.FromMilliseconds(120))
        {
            _lastVisualAt = now;
            double measured = frequency.Value;
            try { BeginInvoke((Action)(() => UpdateMeter(measured))); }
            catch (InvalidOperationException) { }
        }
        double? completed = _session.Observe(sound, frequency, now);
        if (completed is not > 0) return;
        double result = completed.Value, reference = _reference, tolerance = _toleranceCents;
        int target = _targetMidi;
        InstrumentKind instrument = _instrumentKind;
        UkuleleTuning ukulele = _ukuleleKind;
        int resolved = ResolveTarget(target, instrument, ukulele, result, reference);
        double targetFrequency = PitchDetector.NoteFromMidi(resolved, reference).Frequency;
        bool inTune = Math.Abs(1200 * Math.Log2(result / targetFrequency)) <= tolerance;
        bool counting;
        lock (_navigationLock)
        {
            counting = _strikeNavigator.IsCounting;
            if (!counting)
            {
                if (_doubleStrikeEnabled && inTune) _strikeNavigator.Arm(resolved, targetFrequency, now);
                else _strikeNavigator.Reset();
            }
        }
        if (counting) return;
        _ignoreUntil = now.AddMilliseconds(1500);
        try { BeginInvoke((Action)(() => ShowResult(result, reference, target, tolerance, instrument, ukulele))); }
        catch (InvalidOperationException) { }
    }
    private static int ResolveTarget(int target, InstrumentKind instrument, UkuleleTuning ukulele,
        double measured, double reference) => target >= 0 ? target :
        InstrumentCatalog.HasStrings(instrument)
            ? InstrumentCatalog.NearestOpenString(instrument, measured, reference, ukulele)
            : PitchDetector.Nearest(measured, reference).Midi;
    private void UpdateMeter(double measured)
    {
        if (!_visualMode.Checked || _method.SelectedIndex != 1) return;
        int target = ResolveTarget(_targetMidi, _instrumentKind, _ukuleleKind, measured, _reference);
        double expected = PitchDetector.NoteFromMidi(target, _reference).Frequency;
        _meter.SetReading(1200 * Math.Log2(measured / expected));
    }
    private void ShowResult(double measured, double reference, int targetMidi, double tolerance, InstrumentKind instrument, UkuleleTuning ukulele, bool announce = true)
    {
        if (IsDisposed) return;
        _hasResult = true; _lastMeasured = measured; _lastReference = reference; _lastTarget = targetMidi; _lastTolerance = tolerance; _lastInstrument = instrument; _lastUkulele = ukulele;
        int resolved = ResolveTarget(targetMidi, instrument, ukulele, measured, reference);
        _lastResolvedTarget = resolved;
        Note note = PitchDetector.NoteFromMidi(resolved, reference);
        double hertz = measured - note.Frequency;
        double cents = 1200 * Math.Log2(measured / note.Frequency);
        CultureInfo culture = CultureInfo.GetCultureInfo(_english ? "en-US" : "pl-PL");
        string name = note.Midi is >= NoteCatalog.LowestMidi and <= NoteCatalog.HighestMidi
            ? NoteCatalog.Label(note.Midi, _english) : note.Name;
        if (InstrumentCatalog.HasStrings(instrument))
        {
            OpenString open = InstrumentCatalog.Strings(instrument, ukulele).FirstOrDefault(s => s.Midi == note.Midi);
            if (open.Number > 0) name = T($"struna {open.Number}: ", $"string {open.Number}: ") + name;
        }
        string hz = Math.Abs(hertz).ToString("0.0", culture);
        string direction = Math.Abs(cents) <= tolerance ? T("dobrze", "in tune") : hertz < 0 ? T("za nisko", "too low") : T("za wysoko", "too high");
        string offset = hertz < -0.05 ? $"minus {hz}" : hertz > 0.05 ? $"plus {hz}" : "zero";
        string announcement = T($"{name}, {direction}, {offset} herca.", $"{name}, {direction}, {offset} hertz.");
        _result.Text = announcement;
        _details.Text = T($"Zmierzono: {measured.ToString("0.0", culture)} Hz. Cel: {note.Frequency.ToString("0.0", culture)} Hz. Odchyłka: {cents.ToString("+0.0;-0.0;0.0", culture)} centów.",
            $"Measured: {measured.ToString("0.0", culture)} Hz. Target: {note.Frequency.ToString("0.0", culture)} Hz. Difference: {cents.ToString("+0.0;-0.0;0.0", culture)} cents.");
        _lastAnnouncement = announcement; _repeat.Enabled = true;
        if (instrument == InstrumentKind.Piano && targetMidi < 0) UpdateGuide();
        _meter.SetReading(cents);
        if (announce) Announce(announcement);
    }
    private void ResetResult()
    {
        _hasResult = false;
        _lastAnnouncement = null;
        _repeat.Enabled = false;
        _details.Text = "";
        _meter.SetReading(null);
        _result.Text = T("Wynik pojawi się po ucichnięciu dźwięku.", "The result appears when the note stops.");
    }
    private void Announce(string message) => _result.AccessibilityObject.RaiseAutomationNotification(
        AutomationNotificationKind.ActionCompleted, AutomationNotificationProcessing.ImportantMostRecent, message);
    private void ShowRecentChanges()
    {
        try
        {
            Version assemblyVersion = typeof(TunerForm).Assembly.GetName().Version ?? new Version(1, 0, 9);
            Version version = new(assemblyVersion.Major, assemblyVersion.Minor, Math.Max(0, assemblyVersion.Build));
            using var dialog = new ReleaseNotesDialog(version, ReleaseNotes.Current(_english), _english, _dark.Checked, false);
            dialog.ShowDialog(this);
        }
        catch (InvalidDataException error)
        {
            MessageBox.Show(this, error.Message, T("Nie można pokazać zmian", "Cannot show changes"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    private void ShowAbout()
    {
        MessageBox.Show(this, AboutText(_english), T("O programie", "About Stroik"), MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    internal static string AboutText(bool english) => english
        ? "Stroik 1.09 — an accessible instrument tuner for blind, partially sighted and sighted musicians. It helps tune piano, guitar, ukulele and other instruments using a microphone or a tuning fork tone.\n\nCreated by: Proszczok with Codex.\nOriginal idea: Witold."
        : "Stroik 1.09 — dostępny stroiciel instrumentów dla osób niewidomych, słabowidzących i widzących. Pomaga stroić pianino, gitarę, ukulele i inne instrumenty za pomocą mikrofonu lub dźwięku kamertonu.\n\nWykonawca: Proszczok z Codex.\nPomysłodawca: Witold.";
    private void OpenExternal(string url)
    {
        try
        {
            _ = Process.Start(new ProcessStartInfo(url) { UseShellExecute = true })
                ?? throw new InvalidOperationException("Cannot open the web browser.");
        }
        catch (Exception error) when (error is Win32Exception or InvalidOperationException)
        {
            MessageBox.Show(this, error.Message, T("Nie można otworzyć strony", "Cannot open page"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    private async Task CheckUpdatesAsync(bool quiet)
    {
        bool prompted = false;
        _updates.Enabled = false;
        SetUpdateStatus("Sprawdzanie aktualizacji…", "Checking for updates…");
        try
        {
            ReleaseInfo? release = await Updater.FindNewReleaseAsync();
            if (release is null)
            {
                SetUpdateStatus("Masz najnowszą wersję.", "You have the latest version.");
                if (!quiet) MessageBox.Show(this, T("Masz najnowszą wersję.", "You have the latest version."), T("Aktualizacje", "Updates"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            prompted = true;
            SetUpdateStatus($"Dostępna wersja {release.Version}.", $"Version {release.Version} is available.");
            using (var dialog = new ReleaseNotesDialog(release.Version,
                ReleaseNotes.ForLanguage(release.Notes, _english), _english, _dark.Checked, true))
            {
                if (dialog.ShowDialog(this) != DialogResult.Yes) return;
            }
            UseWaitCursor = true;
            SetUpdateStatus("Pobieranie i weryfikacja aktualizacji…", "Downloading and verifying the update…");
            string downloaded = await Updater.DownloadVerifiedAsync(release);
            UseWaitCursor = false;
            Updater.LaunchReplacement(downloaded);
            SetUpdateStatus("Instalowanie aktualizacji…", "Installing the update…");
            Close();
        }
        catch (Exception error)
        {
            UseWaitCursor = false;
            SetUpdateStatus("Nie udało się sprawdzić lub zainstalować aktualizacji. Spróbuj ponownie przyciskiem Sprawdź aktualizacje. Szczegóły: " + error.Message,
                "Could not check or install the update. Retry with Check for updates. Details: " + error.Message);
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
