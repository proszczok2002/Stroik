using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms.Automation;

namespace Stroik;

internal enum ForkShortcutAction { SingleStrike, Pause, Resume }

internal sealed partial class TunerForm : Form
{
    internal const string PayPalUrl = "https://paypal.me/proszczok";
    internal const string RevolutUrl = "https://revolut.me/r_rosiejka";
    internal const string WebsiteUrl = "https://proszczok.pl/";
    internal static string DisplayVersion => "1." + (System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.Build ?? 0).ToString("00", CultureInfo.InvariantCulture);
    private readonly TabControl _tabs = new() { Dock = DockStyle.Fill, DrawMode = TabDrawMode.OwnerDrawFixed };
    private readonly TabPage _tunerTab = new(), _settingsTab = new();
    private readonly Label _preferencesIntro = MakeLabel();
    private readonly Label _heading = MakeLabel(20), _intro = MakeLabel(), _status = MakeLabel();
    private readonly Label _methodTitle = MakeLabel(), _instrumentTitle = MakeLabel(), _viewTitle = MakeLabel();
    private readonly TextBox _beginnerHint = new() { ReadOnly = true, Multiline = true, ScrollBars = ScrollBars.Vertical, Height = 150, TabStop = true };
    private readonly TextBox _methodHelp = new() { ReadOnly = true, Multiline = true, ScrollBars = ScrollBars.Vertical, Height = 100, TabStop = true };
    private readonly Label _hybridPauseTitle = MakeLabel();
    private readonly EditableNumber _hybridPause = new() { Minimum = 0, Maximum = 60, Value = 0 };
    private bool _waitingForGuidance;
    private int _guidanceEpoch;
    private readonly Label _resultTitle = MakeLabel(), _details = MakeLabel(), _settingsTitle = MakeLabel(14);
    private readonly Label _concertTitle = MakeLabel(), _concertHelp = MakeLabel(), _sensitivityTitle = MakeLabel();
    private readonly Label _toleranceTitle = MakeLabel(), _octaveTitle = MakeLabel(), _targetTitle = MakeLabel();
    private readonly Label _noteHelp = MakeLabel(), _languageTitle = MakeLabel(), _intervalTitle = MakeLabel(), _forkHelp = MakeLabel();
    private readonly Label _ukuleleTitle = MakeLabel(), _orderTitle = MakeLabel(), _orderHelp = MakeLabel(), _guideText = MakeLabel();
    private readonly Label _visualHelp = MakeLabel(), _calibrationTitle = MakeLabel(), _calibrationHelp = MakeLabel(), _strikeHelp = MakeLabel(), _updateStatus = MakeLabel(), _automaticStatus = MakeLabel();
    private readonly Label _supportTitle = MakeLabel(14);
    private readonly Label _doubleStrikeWindowTitle = MakeLabel();
    private readonly Label _consonanceTitle = MakeLabel(), _consonanceHelp = MakeLabel(), _updateMinutesTitle = MakeLabel(), _playingHelp = MakeLabel();
    private readonly ComboBox _consonance = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _intervalInput = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckBox _forkTap = new() { AutoSize = true }, _monitor = new() { AutoSize = true }, _automaticUpdates = new() { AutoSize = true };
    private readonly EditableNumber _updateMinutes = new() { Minimum = 1, Maximum = 1440, Value = 5 };
    private readonly System.Windows.Forms.Timer _practiceTimer = new() { Interval = 100 };
    private readonly PracticeCycle _practice = new();
    private readonly SpeechOutput _speech = new();
    private readonly TapNavigator _tapNavigator = new();
    private AudioMonitor? _audioMonitor;
    private volatile TuningMethod _activeMethod;
    private volatile Consonance _consonanceKind;
    private readonly ComboBox _resultUnits = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label _resultUnitsTitle = MakeLabel(14);
    private Consonance SelectedConsonance => (Consonance)Math.Max(0, _consonance.SelectedIndex);
    private int _pendingNoteKey;
    private long _pendingNoteUntil;
    private volatile bool _together, _forkTapEnabled, _monitorEnabled, _acceptAudio = true;
    private DateTime _forkQuietUntil;
    private readonly short[] _pairWindow = new short[65536];
    private int _pairCount;
    private double? _lowerIntervalFrequency;
    private DateTime _lastPairAt;
    private readonly Button _start = new() { AutoSize = true };
    private readonly Button _repeat = new() { AutoSize = true, Enabled = false }, _test = new() { AutoSize = true };
    private readonly Button _updates = new() { AutoSize = true }, _strike = new() { AutoSize = true };
    private readonly Button _recentChanges = new() { AutoSize = true }, _about = new() { AutoSize = true };
    private readonly Button _paypal = new() { AutoSize = true }, _revolut = new() { AutoSize = true }, _website = new() { AutoSize = true };
    private readonly Button _next = new() { AutoSize = true }, _previous = new() { AutoSize = true };
    private readonly Button _saveProfile = new() { AutoSize = true }, _loadProfile = new() { AutoSize = true };
    private readonly TextBox _result = new() { ReadOnly = true, Multiline = true, Height = 64, TabStop = true };
    private readonly ComboBox _method = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _instrument = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _ukuleleTuning = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _pianoOrder = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _view = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _octave = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _target = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private NoteKeyFilter? _noteKeyFilter;
    private readonly ComboBox _sensitivity = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _language = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckBox _dark = new() { AutoSize = true }, _showOctave = new() { AutoSize = true }, _autoStrike = new() { AutoSize = true };
    private readonly CheckBox _doubleStrike = new() { AutoSize = true }, _visualMode = new() { AutoSize = true };
    private readonly CheckBox _voiceInVisual = new() { AutoSize = true, Checked = true };
    private readonly PitchMeter _meter = new();
    private readonly KeyboardDiagram _keyboard = new();
    private readonly EditableNumber _concertA = new() { Minimum = 390, Maximum = 466, DecimalPlaces = 1, Increment = 0.1M, Value = 440 };
    private readonly EditableNumber _tolerance = new() { Minimum = 1, Maximum = 20, Value = 5 };
    private readonly EditableNumber _interval = new() { Minimum = 1, Maximum = 10, Value = 3 };
    private readonly EditableNumber _doubleStrikeWindow = new() { Minimum = 0.5M, Maximum = 2.5M, DecimalPlaces = 1, Increment = 0.1M, Value = 1.2M };
    private readonly EditableNumber _calibration = new() { Minimum = -100, Maximum = 100, DecimalPlaces = 1, Increment = 0.1M };
    private readonly System.Windows.Forms.Timer _strikeTimer = new();
    private readonly System.Windows.Forms.Timer _updateTimer = new() { Interval = UpdateCheckState.IntervalMilliseconds };
    private readonly UpdateCheckState _updateChecks = new();
    private bool _closing;
    private readonly TuningFork _fork = new();
    private readonly NoteSession _session = new();
    private readonly DoubleStrikeNavigator _strikeNavigator = new();
    private readonly object _navigationLock = new();
    private readonly short[] _window = new short[8192];
    private Microphone? _microphone;
    private int _windowCount, _targetMidi = NoteCatalog.DefaultMidi;
    private int _measurementGeneration, _processedGeneration;
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

    private readonly PreferencesStore _preferencesStore;

    internal TunerForm(PreferencesStore preferencesStore)
    {
        _preferencesStore = preferencesStore;
        Preferences settings = _preferencesStore.Load();
        _languageCode = string.IsNullOrEmpty(settings.Language) ? (settings.English ? "en" : "pl") : settings.Language;
        _english = _languageCode != "pl";
        MinimumSize = new Size(540, 700);
        Size = new Size(760, 850);
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Dpi;
        KeyPreview = true;
        _noteKeyFilter = new NoteKeyFilter(_target, NavigateTargetKey);
        Application.AddMessageFilter(_noteKeyFilter);

        var content = new TableLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, ColumnCount = 1, Padding = new Padding(18) };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Controls.Add(_tabs);
        _tabs.TabPages.AddRange([_tunerTab, _settingsTab]);
        _tabs.SelectedIndex = 0;
        _tunerTab.Controls.Add(content);
        var settingsContent = new TableLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, ColumnCount = 1, Padding = new Padding(18) };
        settingsContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _settingsTab.Controls.Add(settingsContent);
        BuildMappingUI(settingsContent);
        LoadAudioPreferences(settings);
        Add(content, _heading); Add(content, _intro);
        Add(content, _viewTitle); Add(content, _view);

        Add(content, _instrumentTitle); Add(content, _instrument);
        Add(content, _ukuleleTitle); Add(content, _ukuleleTuning);
        Add(content, _concertTitle); Add(content, _concertA); Add(content, _concertHelp);
        Add(content, _orderTitle); Add(content, _pianoOrder); Add(content, _orderHelp);
        Add(content, _doubleStrike); Add(content, _forkTap);
        Add(content, _targetTitle); Add(content, _target); Add(content, _showOctave); Add(content, _octaveTitle); Add(content, _octave);
        Add(content, _guideText); Add(content, _noteHelp);
        Add(content, _methodTitle); Add(content, _method); Add(content, _methodHelp);
        Add(content, _consonanceTitle); Add(content, _consonance); Add(content, _intervalInput); Add(content, _consonanceHelp);
        Add(content, _monitor);
        Add(content, _beginnerHint); Add(content, _playingHelp); Add(content, _status);
        var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };
        buttons.Controls.AddRange([_previous, _next, _repeat, _test]);
        Add(content, buttons);
        Add(content, _hybridPauseTitle); Add(content, _hybridPause);
        Add(content, _resultTitle); Add(content, _result); Add(content, _details); Add(content, _meter);
        Add(content, _settingsTitle);
        Add(content, _sensitivityTitle); Add(content, _sensitivity);
        Add(content, _toleranceTitle); Add(content, _tolerance);
        Add(content, _doubleStrikeWindowTitle); Add(content, _doubleStrikeWindow); Add(content, _strikeHelp);
        Add(content, _keyboard);
        var profileButtons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };
        profileButtons.Controls.AddRange([_saveProfile, _loadProfile]);
        Add(content, profileButtons);
        Add(content, _start); Add(content, _strike);
        Add(content, _autoStrike); Add(content, _automaticStatus); Add(content, _intervalTitle); Add(content, _interval); Add(content, _forkHelp);
        Add(settingsContent, _preferencesIntro);
        Add(settingsContent, _resultUnitsTitle); Add(settingsContent, _resultUnits);
        Add(settingsContent, _languageTitle); Add(settingsContent, _language);
        Add(settingsContent, _dark); Add(settingsContent, _visualMode); Add(settingsContent, _voiceInVisual); Add(settingsContent, _visualHelp);
        Add(settingsContent, _calibrationTitle); Add(settingsContent, _calibration); Add(settingsContent, _calibrationHelp);
        Add(settingsContent, _automaticUpdates); Add(settingsContent, _updateMinutesTitle); Add(settingsContent, _updateMinutes);
        Add(settingsContent, _updates); Add(settingsContent, _updateStatus);
        AddCalibratorSettings(settingsContent);
        Add(settingsContent, _recentChanges); Add(settingsContent, _about); Add(settingsContent, _licenses); Add(settingsContent, _website);
        Add(settingsContent, _supportTitle);
        var supportButtons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };
        supportButtons.Controls.AddRange([_paypal, _revolut]);
        Add(settingsContent, supportButtons);

        _initializing = true;
        _language.Items.AddRange(UiLanguages.Names);
        _language.SelectedIndex = Array.IndexOf(UiLanguages.Codes, _languageCode);
        _dark.Checked = settings.DarkTheme;
        _visualMode.Checked = settings.VisualMode ?? !ScreenReaderDetector.IsActive();
        _voiceInVisual.Checked = settings.VoiceInVisualMode;
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
        _hybridPause.Value = settings.HybridPauseSeconds;
        _showOctave.Checked = settings.ShowOctaveInClassic;
        _autoStrike.Checked = settings.AutoStrike;
        _forkTap.Checked = settings.ForkTapAdvance;
        _monitor.Checked = settings.MicrophoneMonitor;
        _automaticUpdates.Checked = settings.AutomaticUpdates;
        ApplyCalibratorPreferences(settings);
        _updateMinutes.Value = settings.UpdateMinutes;
        _automaticStrikesRunning = settings.AutoStrike;
        _reference = (double)_concertA.Value;
        _toleranceCents = (double)_tolerance.Value;
        _initializing = false;
        ApplyLanguage();
        ApplyViewUI();
        ApplyMethodUI();
        ApplyTheme();
        SetContextMenu();

        _start.Click += (_, _) => ToggleMicrophone();
        _strike.Click += (_, _) => Strike();
        _previous.Click += (_, _) => MoveTarget(_targetMidi < 0 ? _lastResolvedTarget : _targetMidi, -1);
        _next.Click += (_, _) => AdvanceTarget(_targetMidi < 0 ? _lastResolvedTarget : _targetMidi);
        _repeat.Click += (_, _) => { if (_lastAnnouncement is not null) Announce(_lastAnnouncement); };
        _test.Click += (_, _) => Announce(T("Stroik. Komunikaty dla czytnika ekranu działają.", "Stroik. Screen reader announcements work."));
        _updates.Click += async (_, _) => await CheckUpdatesAsync(false);
        _recentChanges.Click += (_, _) => ShowRecentChanges();
        _about.Click += (_, _) => ShowAbout();
        _licenses.Click += (_, _) => ShowLicenses();
        _paypal.Click += (_, _) => OpenExternal(PayPalUrl);
        _revolut.Click += (_, _) => OpenExternal(RevolutUrl);
        _website.Click += (_, _) => OpenExternal(WebsiteUrl);
        _saveProfile.Click += (_, _) => SaveProfile();
        _loadProfile.Click += (_, _) => LoadProfile();
        _method.SelectedIndexChanged += (_, _) => { if (_initializing) return; StopMicrophone(false); if (_method.SelectedIndex == 0 && _targetMidi < 0) SelectTarget(InstrumentCatalog.DefaultMidi(_instrumentKind, _ukuleleKind)); ApplyMethodUI(); Persist(); };
        _method.SelectionChangeCommitted += (_, _) => { if (_activeMethod == TuningMethod.TuningFork && _forkTap.Checked && Visible) StartMicrophone(); };
        _instrument.SelectedIndexChanged += (_, _) => { if (_initializing) return; _instrumentKind = (InstrumentKind)_instrument.SelectedIndex; PopulateTargets(_instrumentKind, _instrumentKind == InstrumentKind.Piano && _pianoOrder.SelectedIndex == 0 ? NoteCatalog.LowestMidi : InstrumentCatalog.DefaultMidi(_instrumentKind, _ukuleleKind)); _planAnchorMidi = _targetMidi < 0 ? NoteCatalog.DefaultMidi : _targetMidi; ApplyLanguage(); ApplyViewUI(); UpdateGuide(); ResetResult(); Persist(); };
        _ukuleleTuning.SelectedIndexChanged += (_, _) => { if (_initializing) return; _ukuleleKind = (UkuleleTuning)_ukuleleTuning.SelectedIndex; PopulateTargets(_instrumentKind, InstrumentCatalog.DefaultMidi(_instrumentKind, _ukuleleKind)); ApplyLanguage(); UpdateGuide(); ResetResult(); Persist(); };
        _pianoOrder.SelectedIndexChanged += (_, _) => { if (_initializing) return; if (_instrumentKind == InstrumentKind.Piano && _pianoOrder.SelectedIndex == 0) SelectTarget(NoteCatalog.LowestMidi);
            _planAnchorMidi = _targetMidi < 0 ? NoteCatalog.DefaultMidi : _targetMidi; UpdateConsonance(); Persist(); };
        _view.SelectedIndexChanged += (_, _) => { if (_initializing) return; PopulateTargets(_instrumentKind, _targetMidi); ApplyViewUI(); Persist(); };
        _showOctave.CheckedChanged += (_, _) => { if (_initializing) return; ApplyViewUI(); Persist(); };
        _octave.SelectedIndexChanged += (_, _) => { if (_initializing || _changingOctave || _octave.SelectedIndex < 0) return; int selected = _targetMidi < 0 ? NoteCatalog.DefaultMidi : _targetMidi; int midi = Math.Clamp((_octave.SelectedIndex + 1) * 12 + selected % 12, NoteCatalog.LowestMidi, NoteCatalog.HighestMidi); if (NoteCatalog.Octave(midi) != _octave.SelectedIndex) midi = _octave.SelectedIndex == 0 ? NoteCatalog.LowestMidi : NoteCatalog.HighestMidi; SelectTarget(midi); Persist(); };
        _target.SelectedIndexChanged += (_, _) => { if (_initializing || _target.SelectedItem is not Choice choice) return; lock (_navigationLock) _strikeNavigator.Reset(); _targetMidi = choice.Id; if (!_advancingTarget && !InstrumentCatalog.HasStrings(_instrumentKind) && _targetMidi >= 0) _planAnchorMidi = _targetMidi; SyncOctave(); UpdateGuide(); UpdateConsonance(); ResetResult(); RefreshForkAfterSelection(); Persist(); };
        _target.KeyPress += (_, e) => { if (!InstrumentCatalog.HasStrings(_instrumentKind) && char.ToLowerInvariant(e.KeyChar) is 'i' or 's') { SelectTarget(NoteCatalog.NextSharp(_targetMidi < 0 ? NoteCatalog.DefaultMidi : _targetMidi)); e.Handled = true; } };
        _concertA.ValueChanged += (_, _) => { _reference = (double)_concertA.Value; ResetResult(); RefreshForkAfterSelection(); Persist(); };
        _tolerance.ValueChanged += (_, _) => { _toleranceCents = (double)_tolerance.Value; Persist(); };
        _sensitivity.SelectedIndexChanged += (_, _) => { if (_sensitivity.SelectedItem is Choice choice) _minimumRms = choice.Value; ResetResult(); Persist(); };
        _language.SelectedIndexChanged += (_, _) => { if (_initializing) return; _languageCode = UiLanguages.Codes[Math.Max(0, _language.SelectedIndex)]; _english = _languageCode != "pl"; var current = ReadSettings(); PopulateChoices(current); ApplyLanguage(); ApplyViewUI(); ApplyMethodUI(); SetContextMenu(); Persist(); };
        _dark.CheckedChanged += (_, _) => { ApplyTheme(); Persist(); };
        _autoStrike.CheckedChanged += (_, _) => { _automaticStrikesRunning = _autoStrike.Checked; if (!_automaticStrikesRunning) _fork.Stop(); UpdateStrikeTimer(); UpdateAutomaticStatus(); Persist(); };
        _interval.ValueChanged += (_, _) => { UpdateStrikeTimer(); Persist(); };
        _hybridPause.ValueChanged += (_, _) => Persist();
        _doubleStrike.CheckedChanged += (_, _) => { _doubleStrikeEnabled = _doubleStrike.Checked; _doubleStrikeWindow.Enabled = _doubleStrike.Checked || _forkTap.Checked; Persist(); };
        _doubleStrikeWindow.ValueChanged += (_, _) => { Volatile.Write(ref _doubleStrikeWindowMilliseconds, (int)(_doubleStrikeWindow.Value * 1000)); Persist(); };
        _visualMode.CheckedChanged += (_, _) => { _visualEnabled = _visualMode.Checked; ApplyViewUI(); Persist(); };
        _resultUnits.SelectedIndexChanged += (_, _) => Persist();
        _voiceInVisual.CheckedChanged += (_, _) => Persist();
        _consonance.SelectedIndexChanged += (_, _) => { if (_initializing) return; UpdateConsonance(); ResetResult(); ApplyViewUI(); UpdateIntervalHelp(); RefreshForkAfterSelection(); Persist(); };
        _intervalInput.SelectedIndexChanged += (_, _) => { if (_initializing) return; _together = _intervalInput.SelectedIndex == 1; ResetResult(); UpdateIntervalHelp(); Persist(); };
        _forkTap.CheckedChanged += (_, _) =>
        {
            _forkTapEnabled = _forkTap.Checked;
            if (!_initializing)
            {
                bool wasListening = _microphone is not null;
                StopMicrophone(false); ApplyMethodUI();
                if (_activeMethod == TuningMethod.TuningFork)
                {
                    if (_forkTap.Checked && Visible) StartMicrophone();
                    if (_autoStrike.Checked && _automaticStrikesRunning) Strike();
                    UpdateStrikeTimer();
                }
                else if (wasListening) StartMicrophone();
            }
            Persist();
        };
        _monitor.CheckedChanged += (_, _) => { _monitorEnabled = _monitor.Checked; if (!_monitor.Checked) { _audioMonitor?.Dispose(); _audioMonitor = null; } else if (_microphone is not null && _activeMethod == TuningMethod.HybridHeadphones) _audioMonitor = CreateMonitor(); Persist(); };
        _automaticUpdates.CheckedChanged += (_, _) => { ConfigureUpdateTimer(); Persist(); };
        _checkCalibratorOnStartup.CheckedChanged += (_, _) => Persist();
        _updateMinutes.ValueChanged += (_, _) => { ConfigureUpdateTimer(); Persist(); };
        _practiceTimer.Tick += (_, _) =>
        {
            if (_microphone is null || _activeMethod != TuningMethod.HybridSpeakers || _waitingForGuidance || _waitingForResultSpeech) return;
            var now = DateTime.UtcNow;
            if (_speech.IsSpeaking || NvdaReader.IsSpeaking) _practice.Speech(now, (int)_hybridPause.Value);
            else if (_practice.NeedsFork(now)) Strike();
        };
        foreach (CheckBox checkBox in new[] { _autoStrike, _doubleStrike, _showOctave, _visualMode, _voiceInVisual, _dark, _forkTap, _monitor, _automaticUpdates })
            checkBox.Click += (_, _) => AnnounceCheckBoxState(checkBox);
        _calibration.ValueChanged += (_, _) => { Volatile.Write(ref _correctionFactor, Math.Pow(2, (double)_calibration.Value / 1200)); ResetResult(); Persist(); };
        _strikeTimer.Tick += (_, _) => Strike();
        KeyDown += (_, e) => { if (e.Control && e.KeyCode == Keys.S) { SaveProfile(); e.SuppressKeyPress = true; } else if (e.Control && e.KeyCode == Keys.O) { LoadProfile(); e.SuppressKeyPress = true; } else if (e.Control && e.KeyCode == Keys.N) { AdvanceTarget(_targetMidi < 0 ? _lastResolvedTarget : _targetMidi); e.SuppressKeyPress = true; } };
        _updateTimer.Tick += async (_, _) => await CheckUpdatesAsync(true);
        Shown += async (_, _) =>
        {
            if (_activeMethod == TuningMethod.TuningFork && _forkTap.Checked) StartMicrophone();
            ConfigureUpdateTimer();
            if (_automaticUpdates.Checked) await CheckUpdatesAsync(true);
            await CheckCalibratorAsync(startup: true);
        };
        FormClosing += (_, _) => { _closing = true; StopMapping(false); _updateTimer.Stop(); StopMicrophone(false); _strikeTimer.Stop(); _fork.Dispose(); };
        AcceptButton = _method.SelectedIndex == 0 ? _strike : _start;
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (_tabs.SelectedTab == _mappingTab)
        {
            if (keyData == (Keys.Control | Keys.S)) { SaveMapping(); return true; }
            if (keyData == (Keys.Control | Keys.O)) { OpenTuningFile(false); return true; }
            if (keyData == (Keys.Control | Keys.N) || keyData == (Keys.Alt | Keys.N)) { NextMappingNote(); return true; }
            if (keyData == (Keys.Alt | Keys.U)) return true;
        }
        if (keyData == (Keys.Alt | Keys.N) || keyData == (Keys.Control | Keys.N) ||
            keyData == (Keys.Alt | Keys.Shift | Keys.N) || keyData == (Keys.Control | Keys.Shift | Keys.N))
        {
            MoveTarget(_targetMidi < 0 ? _lastResolvedTarget : _targetMidi, (keyData & Keys.Shift) != 0 ? -1 : 1);
            return true;
        }
        if ((_target.Focused || _target.DroppedDown) && NavigateTargetKey(keyData)) return true;
        if (_tabs.SelectedTab == _tunerTab && (keyData & Keys.Modifiers) == Keys.Alt)
        {
            Control? field = (keyData & Keys.KeyCode) switch
            {
                Keys.W => _view, Keys.I => _instrument, Keys.C => _concertA, Keys.K => _pianoOrder,
                Keys.D => _target, Keys.O => _octave, Keys.T => _method, Keys.Y => _consonance,
                Keys.B => _activeMethod == TuningMethod.Electronic ? _doubleStrike : _forkTap,
                Keys.G => _doubleStrikeWindow, Keys.H => _monitor, Keys.F => _autoStrike,
                Keys.P => _hybridPause, Keys.L => _sensitivity, Keys.J => _tolerance,
                Keys.R => _intervalInput, Keys.Z => _interval,
                _ => null
            };
            if (field is { Visible: true, Enabled: true })
            {
                field.Focus();
                if (field is CheckBox box) { box.Checked = !box.Checked; AnnounceCheckBoxState(box); }
                return true;
            }
            if (keyData == (Keys.Alt | Keys.M)) { ToggleMicrophone(); return true; }
        }
        if (keyData == Keys.F1 && _view.SelectedIndex == 1)
        {
            MessageBox.Show(this, _methodHelp.Text + "\n\n" + _beginnerHint.Text, T("Wskazówki strojenia", "Tuning guidance"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }
        if (keyData == (Keys.Control | Keys.Q))
        {
            Close();
            return true;
        }
        if (keyData == (Keys.Alt | Keys.U) && _method.SelectedIndex != 1)
        {
            ForkShortcutAction action = DecideForkShortcut(_activeMethod != TuningMethod.HybridSpeakers && _autoStrike.Checked, _automaticStrikesRunning);
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

    private bool NavigateTargetKey(Keys key)
    {
        if (InstrumentCatalog.HasStrings(_instrumentKind)) return false;
        int current = _targetMidi < 0 ? NoteCatalog.DefaultMidi : _targetMidi;
        if (key is (Keys.Control | Keys.Up) or (Keys.Control | Keys.Down))
        {
            int midi = Math.Clamp(current + (key == (Keys.Control | Keys.Up) ? 12 : -12), NoteCatalog.LowestMidi, NoteCatalog.HighestMidi);
            SelectTarget(midi);
            AnnounceTarget();
            return true;
        }
        int pitch = key switch { Keys.C => 0, Keys.D => 2, Keys.E => 4, Keys.F => 5,
            Keys.G => 7, Keys.A => 9, Keys.B or Keys.H => 11, _ => -1 };
        if (pitch >= 0)
        {
            _pendingNoteKey = pitch;
            _pendingNoteUntil = Environment.TickCount64 + 1800;
            return true;
        }
        int octave = key >= Keys.D0 && key <= Keys.D8 ? key - Keys.D0
            : key >= Keys.NumPad0 && key <= Keys.NumPad8 ? key - Keys.NumPad0 : -1;
        if (octave >= 0 && Environment.TickCount64 <= _pendingNoteUntil)
        {
            int midi = (octave + 1) * 12 + _pendingNoteKey;
            _pendingNoteUntil = 0;
            if (midi is >= NoteCatalog.LowestMidi and <= NoteCatalog.HighestMidi) { SelectTarget(midi); AnnounceTarget(); }
            return true;
        }
        if (key is not (Keys.Up or Keys.Down or Keys.Left or Keys.Right)) return false;
        int next = key is Keys.Up or Keys.Down ? NoteCatalog.NextNatural(current, key == Keys.Up ? -1 : 1)
            : NoteCatalog.NextBlack(current, key == Keys.Left ? -1 : 1);
        SelectTarget(next);
        AnnounceTarget();
        if (next == current) RefreshForkAfterSelection();
        return true;
    }

    private void AnnounceTarget()
    {
        string name = NoteCatalog.Spoken(_targetMidi, Locale);
        if (_instrumentKind == InstrumentKind.Piano && _view.SelectedIndex == (int)TunerView.Beginner)
            name += ": " + KeyboardGuide.Position(_targetMidi, Locale);
        _target.AccessibleDescription = name;
        if (!ScreenReaderDetector.IsActive()) _speech.Speak(name, Locale);
        else _target.AccessibilityObject.RaiseAutomationNotification(AutomationNotificationKind.ActionCompleted,
            AutomationNotificationProcessing.ImportantAll, name);
    }

    internal static ForkShortcutAction DecideForkShortcut(bool automaticSelected, bool running) =>
        !automaticSelected ? ForkShortcutAction.SingleStrike :
        running ? ForkShortcutAction.Pause : ForkShortcutAction.Resume;
    private string _languageCode = "pl";
    private string Locale => _languageCode is "pl" or "en" ? (_english ? "en" : "pl") : _languageCode;
    private string T(string pl, string en) => UiLanguages.Get(Locale, pl, en);
    private string Tf(FormattableString pl, FormattableString en) => UiLanguages.Format(Locale, pl, en);
    private FormattableString? _statusPlFormat, _statusEnFormat;
    private void SetUpdateStatusF(FormattableString pl, FormattableString en) { _statusPlFormat = pl; _statusEnFormat = en; _updateStatus.Text = Tf(pl, en); }
    private void SetUpdateStatus(string pl, string en)
    {
        _statusPlFormat = _statusEnFormat = null; _updateStatusPl = pl; _updateStatusEn = en;
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
        _method.Items.AddRange([T("Kamerton", "Tuning fork"), T("Stroik elektroniczny", "Electronic tuner"), T("Hybrydowy — głośniki (naprzemiennie)", "Hybrid — speakers (alternating)"), T("Hybrydowy — słuchawki", "Hybrid — headphones")]);
        _method.SelectedIndex = (int)settings.Method;
        _activeMethod = settings.Method;
        _consonance.Items.Clear();
        _consonance.Items.AddRange([T("Wyłączone", "Off"), T("Kwinty", "Fifths"), T("Oktawy", "Octaves"), PeriodicFifthLabel(settings.View == TunerView.Beginner)]);
        _consonance.SelectedIndex = (int)settings.Consonance;
        UpdateConsonance();
        _resultUnits.Items.Clear();
        _resultUnits.Items.AddRange([T("Tylko centy", "Cents only"), T("Tylko herce", "Hertz only"),
            T("Centy i herce", "Cents then hertz"), T("Herce i centy", "Hertz then cents")]);
        _resultUnits.SelectedIndex = (int)settings.ResultUnits;
        _intervalInput.Items.Clear();
        _intervalInput.Items.AddRange([T("Dźwięki osobno: dolny, potem górny", "Separate notes: lower, then upper"), T("Dźwięki razem — eksperymentalnie", "Together — experimental")]);
        _intervalInput.SelectedIndex = settings.SimultaneousInterval ? 1 : 0;
        _together = settings.SimultaneousInterval;
        _view.Items.Clear();
        _view.Items.AddRange([T("Widok klasyczny", "Classic view"), T("Widok dla początkujących", "Beginner view")]);
        _view.SelectedIndex = (int)settings.View;
        _instrument.Items.Clear();
        _instrument.Items.AddRange([T("Fortepian", "Piano"), T("Gitara", "Guitar"), T("Ukulele", "Ukulele"), T("Skrzypce", "Violin"), T("Gitara basowa", "Bass guitar"), T("Chromatyczny", "Chromatic")]);
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
        for (int i = 0; i <= 8; i++) _octave.Items.Add(NoteCatalog.OctaveLabel(i, Locale));
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
                    Tf($"Struna {open.Number}", $"String {open.Number}") +
                    (kind is InstrumentKind.Guitar or InstrumentKind.Ukulele
                        ? open.Number == 1 ? T(" (przy podłodze): ", " (floor side): ")
                                           : open.Number == InstrumentCatalog.Strings(kind, _ukuleleKind).Count ? T(" (przy suficie): ", " (ceiling side): ") : ": "
                        : ": ") + NoteCatalog.Label(open.Midi, Locale)));
        }
        else
        {
            for (int midi = NoteCatalog.LowestMidi; midi <= NoteCatalog.HighestMidi; midi++)
                _target.Items.Add(new Choice(midi, 0, kind == InstrumentKind.Piano && _view.SelectedIndex == (int)TunerView.Beginner
                    ? KeyboardGuide.Describe(midi, Locale) : NoteCatalog.Label(midi, Locale)));
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
        _target.AccessibleDescription = _targetMidi < 0 ? null : NoteCatalog.Spoken(_targetMidi, Locale);
        if (_targetMidi < 0) return;
        _changingOctave = true;
        _octave.SelectedIndex = NoteCatalog.Octave(_targetMidi);
        _changingOctave = false;
    }
    private void ApplyMethodUI()
    {
        _activeMethod = (TuningMethod)Math.Max(0, _method.SelectedIndex);
        bool fork = _activeMethod == TuningMethod.TuningFork;
        bool hybrid = _activeMethod is TuningMethod.HybridSpeakers or TuningMethod.HybridHeadphones;
        bool canListen = !fork;
        _forkTapEnabled = _forkTap.Checked; _monitorEnabled = _monitor.Checked;
        _strikeTimer.Stop(); _fork.Stop();
        _strike.Visible = fork;
        _forkHelp.Visible = fork;
        foreach (Control control in new Control[] { _autoStrike, _automaticStatus, _intervalTitle, _interval }) control.Visible = fork;
        foreach (Control control in new Control[] { _start, _status, _sensitivityTitle, _sensitivity }) control.Visible = canListen;
        foreach (Control control in new Control[] { _repeat, _test, _resultTitle, _result, _details, _toleranceTitle, _tolerance, _doubleStrike, _doubleStrikeWindowTitle, _doubleStrikeWindow, _strikeHelp }) control.Visible = !fork;
        _forkTap.Visible = fork || hybrid;
        _monitor.Visible = _activeMethod == TuningMethod.HybridHeadphones;
        _doubleStrike.Visible = _activeMethod == TuningMethod.Electronic;
        _doubleStrikeWindow.Visible = _doubleStrikeWindowTitle.Visible = _activeMethod == TuningMethod.Electronic || _forkTap.Checked;
        _doubleStrikeWindow.Enabled = _doubleStrike.Checked || _forkTap.Checked;
        _status.Text = _microphone is null ? T("Mikrofon jest wyłączony.", "Microphone is off.") : T("Mikrofon włączony.", "Microphone is on.");
        AcceptButton = fork ? _strike : _start;
        UpdateStrikeTimer(); UpdateAutomaticStatus(); ApplyViewUI(); UpdateIntervalHelp();
    }
    private void ApplyViewUI()
    {
        bool beginner = _view.SelectedIndex == 1;
        if (_consonance.Items.Count == 4)
        {
            string label = PeriodicFifthLabel(beginner);
            if (!Equals(_consonance.Items[3], label))
            {
                bool initializing = _initializing;
                _initializing = true;
                int selected = _consonance.SelectedIndex;
                _consonance.Items[3] = label;
                _consonance.SelectedIndex = selected;
                _initializing = initializing;
            }
        }
        _saveProfile.Text = _saveProfile.AccessibleName = T("Zapisz plik ustawień", "Save settings file") + (beginner ? " (Ctrl+S)" : "");
        _loadProfile.Text = _loadProfile.AccessibleName = T("Wczytaj plik ustawień", "Open settings file") + (beginner ? " (Ctrl+O)" : "");
        _methodHelp.Visible = beginner;
        _intro.Visible = _beginnerHint.Visible = _playingHelp.Visible = _noteHelp.Visible = beginner;
        _forkHelp.Visible = beginner && _activeMethod == TuningMethod.TuningFork;
        _strikeHelp.Visible = beginner && _activeMethod == TuningMethod.Electronic;
        bool fullRange = !InstrumentCatalog.HasStrings(_instrumentKind);
        _showOctave.Visible = false;
        _octaveTitle.Visible = _octave.Visible = fullRange;
        _concertHelp.Visible = beginner && _instrumentKind == InstrumentKind.Piano;
        _ukuleleTitle.Visible = _ukuleleTuning.Visible = _instrumentKind == InstrumentKind.Ukulele;
        _orderTitle.Visible = _pianoOrder.Visible = _instrumentKind == InstrumentKind.Piano;
        _orderHelp.Visible = _guideText.Visible = beginner && _instrumentKind == InstrumentKind.Piano;
        _concertTitle.Text = T("Częstotliwość, Hz:", "Frequency, Hz:");
        _concertA.AccessibleName = _concertTitle.Text;
        _keyboard.Visible = _instrumentKind == InstrumentKind.Piano && _visualMode.Checked;
        _voiceInVisual.Visible = _visualMode.Checked;
        _previous.Visible = _next.Visible = _visualMode.Checked;
        UpdateConsonance();
        _meter.Visible = _visualMode.Checked && _activeMethod != TuningMethod.TuningFork && _consonanceKind == Consonance.Off;
        _intervalInput.Visible = SelectedConsonance != Consonance.Off && _activeMethod != TuningMethod.TuningFork;
        _consonanceHelp.Visible = beginner && SelectedConsonance != Consonance.Off;
        _hybridPauseTitle.Visible = _hybridPause.Visible = _activeMethod == TuningMethod.HybridSpeakers;
        foreach (var input in new[] { _concertA, _interval, _tolerance, _doubleStrikeWindow, _calibration, _updateMinutes, _hybridPause })
            input.AccessibleDescription = beginner ? T("Strzałki górna i dolna zmieniają wartość.", "Up and Down arrows change the value.") : null;
        _doubleStrike.AccessibleDescription = _forkTap.AccessibleDescription = beginner ? "Alt+B" : null;
        // Keep the automatic fork action and the interval separate.
        _interval.AccessibleDescription = beginner ? _interval.AccessibleDescription + " Alt+Z" : null;
        UpdateAutomaticStatus(); UpdateMethodHelp();
        UpdateIntervalHelp();
    }
    private string PeriodicFifthLabel(bool beginner) => beginner
        ? T("Sprawdzaj współbrzmienie co 5 dźwięków wybranej kolejności strojenia", "Check consonance every five notes in the selected tuning order")
        : T("Sprawdzaj współbrzmienie co kwintę wybranej kolejności strojenia", "Check a fifth every five notes in the selected tuning order");

    private void UpdateConsonance()
    {
        _consonanceKind = SelectedConsonance;
        if (_consonanceKind != Consonance.PeriodicFifth) return;
        int position;
        if (InstrumentCatalog.HasStrings(_instrumentKind))
            position = InstrumentCatalog.Strings(_instrumentKind, _ukuleleKind).ToList().FindIndex(s => s.Midi == _targetMidi);
        else if (_instrumentKind == InstrumentKind.Piano)
            position = TuningPlan.Build(_planAnchorMidi, (PianoOrder)Math.Max(0, _pianoOrder.SelectedIndex)).ToList().IndexOf(_targetMidi);
        else
            position = _targetMidi - _planAnchorMidi;
        _consonanceKind = position >= 0 && (position + 1) % 5 == 0 && _targetMidi <= 101 ? Consonance.Fifth : Consonance.Off;
        _meter.Visible = _visualMode.Checked && _activeMethod != TuningMethod.TuningFork && _consonanceKind == Consonance.Off;
    }

    private void UpdateStrikeTimer()
    {
        _strikeTimer.Stop();
        if (_activeMethod != TuningMethod.TuningFork || !_autoStrike.Checked || !_automaticStrikesRunning || IsDisposed) return;
        _strikeTimer.Interval = (int)_interval.Value * 1000;
        if (_activeMethod == TuningMethod.TuningFork && _forkTap.Checked) _strikeTimer.Interval = Math.Max(6500, _strikeTimer.Interval);
        _strikeTimer.Start();
    }
    private void UpdateAutomaticStatus()
    {
        _automaticStatus.Text = !_autoStrike.Checked
            ? T("Tryb ręczny. Alt+U: pojedyncze uderzenie.", "Manual mode. Alt+U: one strike.")
            : _automaticStrikesRunning
                ? T("Automatyczne uderzenia działają. Alt+U: pauza.", "Automatic strikes are running. Alt+U: pause.")
                : T("Automatyczne uderzenia wstrzymane. Alt+U: wznowienie.", "Automatic strikes are paused. Alt+U: resume.");
        _autoStrike.AccessibleDescription = null;
        _automaticStatus.Visible = _view.SelectedIndex == (int)TunerView.Beginner && _activeMethod == TuningMethod.TuningFork;
    }
    private void Strike()
    {
        Interlocked.Exchange(ref _speakerAdvanceUntil, 0);
        try
        {
            int midi = _targetMidi < 0 ? InstrumentCatalog.DefaultMidi(_instrumentKind, _ukuleleKind) : _targetMidi;
            double frequency = TargetFrequency(midi, _reference);
            if (midi > IntervalAnalysis.HighestTarget(_consonanceKind))
            {
                StopOutOfRangeFork();
                return;
            }
            int upperMidi = midi + IntervalAnalysis.Semitones(_consonanceKind);
            double? upper = _consonanceKind == Consonance.Off ? null : TargetFrequency(upperMidi, _reference);
            _fork.Strike(frequency, upper, ForkCalibration.Effective(_forkCalibration, midi), ForkCalibration.Effective(_forkCalibration, upperMidi), _forkVolume.Value,
                "Stroik — " + T("Kamerton", "Tuning fork"));
            _forkQuietUntil = DateTime.UtcNow.AddMilliseconds(3400);
            if (_activeMethod == TuningMethod.HybridSpeakers && _microphone is not null)
            {
                _acceptAudio = false;
                _practice.Fork(DateTime.UtcNow);
            }
        }
        catch (Exception error)
        {
            _strikeTimer.Stop();
            _automaticStrikesRunning = false;
            UpdateAutomaticStatus();
            MessageBox.Show(this, error.Message, T("Błąd kamertonu", "Tuning fork error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    private void RefreshForkAfterSelection()
    {
        if (_initializing || _activeMethod != TuningMethod.TuningFork) return;
        // ResetResult already plays the new reference when tap-listening is active.
        if (_microphone is null) Strike();
        UpdateStrikeTimer();
    }
    private void ApplyLanguage()
    {
        Text = Tf($"Stroik {DisplayVersion} — dostępny stroiciel", $"Stroik {DisplayVersion} — accessible tuner");
        _heading.Text = "Stroik " + DisplayVersion;
        _licenses.Text = T("Licencje składników", "Third-party licenses"); _licenses.AccessibleName = _licenses.Text;
        _tunerTab.Text = T("Stroik", "Tuner");
        _settingsTab.Text = T("Ustawienia", "Settings");
        ApplyMappingLanguage();
        _tunerTab.AccessibleName = _tunerTab.Text;
        _settingsTab.AccessibleName = _settingsTab.Text;
        _preferencesIntro.Text = T("Język, wygląd, korekta mikrofonu, aktualizacje i informacje o programie.", "Language, appearance, microphone correction, updates and program information.");
        _tabs.Invalidate();
        _intro.Text = T("Wybierz instrument, metodę strojenia i dźwięk. Alt+W otwiera widok, Alt+I instrument, Alt+C częstotliwość, Alt+K kolejność, Alt+D dźwięk, Alt+O oktawę, Alt+T metodę, Alt+Y współbrzmienie. Alt+M włącza i wyłącza mikrofon.",
            "Choose an instrument, tuning method and note. Alt+W opens view, Alt+I instrument, Alt+C frequency, Alt+K order, Alt+D note, Alt+O octave, Alt+T method, Alt+Y consonance. Alt+M starts and stops the microphone.");
        _methodTitle.Text = T("Metoda strojenia:", "Tuning method:");
        _instrumentTitle.Text = T("Instrument:", "Instrument:");
        _viewTitle.Text = T("Widok:", "View:");
        _beginnerHint.Text = _instrumentKind switch
        {
            InstrumentKind.Piano => T(
                "Wskazówka dla początkujących: przy starych pianinach i fortepianach zwykle zaczyna się od środka i pracuje równomiernie w obie strony, np. kwintami. Klucz ustaw możliwie pionowo, na godzinę dwunastą. Wsuń kołek jak najgłębiej w gniazdo klucza, aby klucz znajdował się blisko ramy. Użyj klinów do wytłumienia bocznych strun. Najpierw nastrój środkową, potem boczne, aż ustanie dudnienie. Praca przy napiętych strunach może uszkodzić instrument lub zranić. Stan zabytkowego instrumentu powinien ocenić technik. Stroik pokazuje strój matematyczny bez indywidualnego rozciągania oktaw.",
                "Beginner guidance: on older pianos, work from the middle evenly toward both ends, for example in fifths. Keep the tuning lever as upright as practical, at twelve o'clock. Seat the pin fully in the lever socket so the lever is close to the frame. Use wedges to mute the outer strings. Tune the middle string first, then the outer strings until beating stops. High-tension strings can damage the instrument or cause injury. Ask a technician to assess a historic piano. Stroik shows mathematical tuning without individual octave stretching."),
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
        _start.Text = _microphone is null ? T("Włącz mikrofon", "Start microphone") : T("Wyłącz mikrofon", "Stop microphone");
        _strike.Text = T("&Uderz kamerton", "&Strike tuning fork");
        _previous.Text = T("Poprzedni dźwięk", "Previous note");
        _next.Text = T("&Następny dźwięk", "&Next note");
        _repeat.Text = T("&Powtórz wynik", "&Repeat result");
        _test.Text = T("Sprawdź komunikat &NVDA", "&Test screen reader announcement");
        _updates.Text = T("Sprawdź &aktualizacje", "Check for &updates");
        ApplyCalibratorLanguage();
        _consonanceTitle.Text = T("Sprawdź współbrzmienie:", "Check consonance:");
        _consonance.AccessibleName = _consonanceTitle.Text;
        _intervalInput.AccessibleName = T("Sposób pomiaru współbrzmienia", "Consonance measurement method");
        _forkTap.Text = T("Dwukrotne stuknięcie przechodzi do kolejnego dźwięku zgodnego z wybraną kolejnością", "Two strikes advance to the next note in the selected order");
        _monitor.Text = T("Odsłuch mikrofonu w słuchawkach (opóźnienie zależy od sprzętu)", "Monitor microphone in headphones (hardware dependent delay)");
        _monitor.AccessibleDescription = T("Najpierw załóż słuchawki. Odsłuch przez głośniki może wywołać sprzężenie.", "Put headphones on first. Monitoring through speakers can cause feedback.");
        _automaticUpdates.Text = T("Automatycznie sprawdzaj aktualizacje", "Automatically check for updates");
        _updateMinutesTitle.Text = T("Sprawdzaj co ile minut (1–1440):", "Check every how many minutes (1–1440):");
        _updateMinutes.AccessibleName = _updateMinutesTitle.Text;
        foreach (CheckBox box in new[] { _forkTap, _monitor, _automaticUpdates }) box.AccessibleName = box.Text;
        _playingHelp.Text = T("Zbyt słabe lub zbyt mocne uderzenia mogą powodować przekłamane odczyty. Graj równomiernie, bez przesterowania mikrofonu. W trybie słuchawkowym używaj słuchawek przed włączeniem odsłuchu, aby uniknąć sprzężenia.", "Strikes that are too weak or too strong can cause inaccurate readings. Play evenly without microphone clipping. Use headphones before enabling monitoring to avoid feedback.");
        _recentChanges.Text = T("Ostatnie zmiany", "Latest changes");
        _about.Text = T("O programie", "About Stroik");
        _website.Text = T("Strona Proszczoka", "Proszczok website");
        _supportTitle.Text = T("Wsparcie projektu", "Support the project");
        _paypal.Text = T("Wesprzyj przez PayPal", "Support via PayPal");
        _revolut.Text = T("Wesprzyj przez Revolut", "Support via Revolut");
        _updateStatus.Text = _statusPlFormat is null ? T(_updateStatusPl, _updateStatusEn) : Tf(_statusPlFormat, _statusEnFormat!);
        _updateStatus.AccessibleName = T("Stan aktualizacji", "Update status");
        _autoStrike.Text = T("Uderzaj kamerton automatycznie", "Strike tuning fork automatically");
        _doubleStrike.Text = T("Po dobrym wyniku dwa szybkie uderzenia przechodzą do następnego dźwięku", "After an in-tune result, two quick strikes advance to the next note");
        _doubleStrikeWindowTitle.Text = T("Maksymalny odstęp między uderzeniami (0,5–2,5 s; domyślnie 1,2):", "Maximum gap between strikes (0.5–2.5 s; default 1.2):");
        _strikeHelp.Text = T("Po wyniku „dobrze” poczekaj na koniec komunikatu, a następnie dwukrotnie uderz tę samą nutę w ustawionym czasie. Uderzenia mogą mieć różną głośność, ale oba muszą być rozpoznane jako właściwy dźwięk. Dłuższy czas jest wygodniejszy, lecz zwiększa ryzyko przypadkowego przejścia. Ctrl+N przechodzi ręcznie.",
            "After an in-tune result, wait for the announcement to finish, then strike the same note twice within the selected time. Strikes may differ in volume, but both must be recognized at the target pitch. A longer time is easier to use but raises the chance of accidental advancement. Ctrl+N advances manually.");
        _methodHelp.AccessibleName = T("Zastosowanie metody strojenia", "Tuning method purpose");
        _beginnerHint.AccessibleName = T("Wskazówki dla wybranego instrumentu", "Selected instrument guidance");
        _hybridPauseTitle.Text = T("Przerwa po komunikacie w hybrydzie, sekundy (0–60):", "Hybrid pause after spoken result, seconds (0–60):");
        _hybridPause.AccessibleName = _hybridPauseTitle.Text;
        _intervalTitle.Text = T("Odstęp między uderzeniami w sekundach:", "Seconds between strikes:");
        _forkHelp.Text = T("Kamerton wydaje krótki, gasnący ton. Jeśli nie masz pewności co do strojenia ze słuchu, najpierw ćwicz na niedrogim, łatwym do obsługi instrumencie, np. ukulele lub gitarze. Zerwanie struny nadal może zranić.",
            "The tuning fork plays a short, decaying tone. If unsure about tuning by ear, first practise on a modest, easy-to-handle instrument such as a ukulele or guitar. A broken string can still cause injury.");
        _resultTitle.Text = T("Ostatni wynik:", "Last result:");
        _settingsTitle.Text = T("Parametry strojenia", "Tuning parameters");
        _concertTitle.Text = T("Częstotliwość, Hz:", "Frequency, Hz:");
        _concertHelp.Text = T("Dla pianin powojennych A4 = 440 Hz to zwykły punkt odniesienia. Pianina przedwojenne mogły być przewidziane do niższego stroju. Przed podniesieniem stroju starego pianina poproś technika o ocenę instrumentu.",
            "A4 = 440 Hz is the usual reference for postwar pianos. Prewar pianos may have been designed for lower pitch. Ask a technician to assess an old piano before raising its pitch.");
        _sensitivityTitle.Text = T("Czułość mikrofonu:", "Microphone sensitivity:");
        _resultUnitsTitle.Text = _resultUnits.AccessibleName = T("Personalizacja komunikatów strojenia", "Customize tuning announcements");
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
        if (!InstrumentCatalog.HasStrings(_instrumentKind))
            _noteHelp.Text += " " + T("Wpisz na liście np. C4. Ctrl+strzałka w górę lub w dół zmienia oktawę.",
                "Type a note such as C4 in the list. Ctrl+Up or Ctrl+Down changes the octave.");
        _languageTitle.Text = T("Język:", "Language:");
        _visualMode.Text = T("Pokazuj wskaźnik na żywo i klawiaturę", "Show live meter and keyboard");
        _voiceInVisual.Text = T("Odczyt głosowy w trybie wizualnym", "Spoken feedback in visual mode");
        _voiceInVisual.AccessibleName = _voiceInVisual.Text;
        _voiceInVisual.AccessibleDescription = T("Jeśli chcesz wyłączyć komunikaty głosowe w trybie wizualnym, odznacz tę opcję w Ustawieniach. Nie wyłącza nawigacji czytnika ekranu.", "Clear this option in Settings to disable application announcements in visual mode. Screen reader navigation remains available.");
        _visualHelp.Text = T("Jeśli chcesz wyłączyć komunikaty głosowe, odznacz opcję „Odczyt głosowy w trybie wizualnym” w Ustawieniach. Przy wykrytym czytniku ekranu grafika jest domyślnie wyłączona; opcję można zmienić ręcznie.", "To disable spoken feedback, clear ‘Spoken feedback in visual mode’ in Settings. Graphics default to off when a screen reader is detected; you can change this manually.");
        _calibrationTitle.Text = T("Ręczna korekta pomiaru mikrofonu w centach (−100 do +100):", "Manual microphone pitch correction in cents (−100 to +100):");
        _calibrationHelp.Text = T("Użyj tylko po porównaniu ze znanym wzorcem. Dodatnia wartość podwyższa odczyt, ujemna obniża. Błędnie nastrojony instrument nie oznacza błędu mikrofonu.", "Use only after comparing with a known reference. A positive value raises the reading; a negative value lowers it. An out-of-tune instrument does not imply a microphone error.");
        _dark.Text = T("Ciemny motyw", "Dark theme");
        foreach (Button button in new[] { _previous, _repeat, _test, _updates, _recentChanges, _about, _website, _paypal, _revolut, _strike, _next, _saveProfile, _loadProfile }) button.AccessibleName = button.Text.Replace("&", "");
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
        _meter.SetLanguage(Locale);
        UpdateGuide();
        if (_hasResult) ShowResult(_lastMeasured, _lastReference, _lastTarget, _lastTolerance, _lastInstrument, _lastUkulele, false);
        else _result.Text = T("Wynik pojawi się po ucichnięciu dźwięku.", "The result appears when the note stops.");
    }
    private Preferences ReadSettings() => new()
    {
        English = _english,
        Language = Locale,
        DarkTheme = _dark.Checked,
        Method = (TuningMethod)Math.Max(0, _method.SelectedIndex),
        View = (TunerView)Math.Max(0, _view.SelectedIndex),
        Instrument = _instrumentKind,
        ConcertA = (double)_concertA.Value,
        UkuleleTuning = _ukuleleKind,
        PianoOrder = (PianoOrder)Math.Max(0, _pianoOrder.SelectedIndex),
        PlanAnchorMidi = _planAnchorMidi,
        DoubleStrikeAdvance = _doubleStrike.Checked,
        DoubleStrikeWindowMilliseconds = (int)(_doubleStrikeWindow.Value * 1000),
        MicrophoneCorrectionCents = _calibration.Value,
        VisualMode = _visualMode.Checked,
        VoiceInVisualMode = _voiceInVisual.Checked,
        SensitivityIndex = Math.Max(0, _sensitivity.SelectedIndex),
        ToleranceCents = (int)_tolerance.Value,
        TargetMidi = _targetMidi,
        StartOctave = Math.Max(0, _octave.SelectedIndex),
        AutomaticUpdates = _automaticUpdates.Checked,
        CheckCalibratorOnStartup = _checkCalibratorOnStartup.Checked,
        CalibratorInstallOffered = _calibratorInstallOffered,
        CalibratorUpdateOffered = _calibratorUpdateOffered?.ToString(),
        UpdateMinutes = (int)_updateMinutes.Value,
        Consonance = SelectedConsonance,
        ResultUnits = (ResultUnits)Math.Max(0, _resultUnits.SelectedIndex),
        SimultaneousInterval = _together,
        ForkTapAdvance = _forkTap.Checked,
        MicrophoneMonitor = _monitor.Checked,
        ShowOctaveInClassic = _showOctave.Checked,
        AutoStrike = _autoStrike.Checked,
        StrikeIntervalSeconds = (int)_interval.Value,
        HybridPauseSeconds = (int)_hybridPause.Value,
        ForkVolume = _forkVolume.Value,
        MonitorVolume = _monitorVolume.Value,
        InstrumentMap = _instrumentMap,
        UseInstrumentMap = _useMap,
        ForkCalibration = _forkCalibration
    };
    private bool _storageWarningShown;

    private void Persist()
    {
        if (!_initializing && !_preferencesStore.Save(ReadSettings()) && Visible && !_closing)
            WarnPortableStorage(saveFailed: true);
    }

    internal void WarnPortableStorage(bool saveFailed = false)
    {
        if (_storageWarningShown || _closing || IsDisposed || !Visible) return;
        if (!saveFailed && _preferencesStore.CanSave && !_preferencesStore.MigrationPending) return;
        _storageWarningShown = true;
        string message = !_preferencesStore.MigrationPending && (saveFailed || !_preferencesStore.CanSave)
            ? T("Nie można zapisać ustawień obok Stroik.exe. Sprawdź uprawnienia folderu programu i stan pliku Stroik.settings.json.",
                "Could not save settings beside Stroik.exe. Check the program folder permissions and the Stroik.settings.json file.")
            : T("Migracja z AppData nie została ukończona. Zamknij starsze kopie Stroika i sprawdź, czy folder programu jest zapisywalny. Migracja zostanie ponowiona przy następnym uruchomieniu.",
                "Migration from AppData is incomplete. Close older copies of Stroik and check that the program folder is writable. Migration will retry on the next launch.");
        MessageBox.Show(this, message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
    private void ApplySettings(Preferences settings)
    {
        StopMicrophone(false);
        StopMapping(false);
        _initializing = true;
        LoadAudioPreferences(settings);
        _languageCode = string.IsNullOrEmpty(settings.Language) ? (settings.English ? "en" : "pl") : settings.Language;
        _english = _languageCode != "pl";
        _language.SelectedIndex = Array.IndexOf(UiLanguages.Codes, _languageCode);
        _dark.Checked = settings.DarkTheme;
        _visualMode.Checked = settings.VisualMode ?? !ScreenReaderDetector.IsActive();
        _voiceInVisual.Checked = settings.VoiceInVisualMode;
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
        _hybridPause.Value = settings.HybridPauseSeconds;
        _showOctave.Checked = settings.ShowOctaveInClassic;
        _autoStrike.Checked = settings.AutoStrike;
        _forkTap.Checked = settings.ForkTapAdvance;
        _monitor.Checked = settings.MicrophoneMonitor;
        _automaticUpdates.Checked = settings.AutomaticUpdates;
        ApplyCalibratorPreferences(settings);
        _updateMinutes.Value = settings.UpdateMinutes;
        _automaticStrikesRunning = settings.AutoStrike;
        PopulateChoices(settings);
        _reference = (double)_concertA.Value;
        _toleranceCents = (double)_tolerance.Value;
        _initializing = false;
        ApplyLanguage(); ApplyViewUI(); ApplyMethodUI(); ApplyTheme(); UpdateGuide(); ResetResult(); ConfigureUpdateTimer(); Persist();
    }
    private void SetContextMenu()
    {
        ContextMenuStrip? previous = ContextMenuStrip;
        var menu = new ContextMenuStrip();
        menu.Items.Add(T("Zapisz plik ustawień", "Save settings file"), null, (_, _) => SaveProfile());
        menu.Items.Add(T("Wczytaj plik ustawień", "Open settings file"), null, (_, _) => LoadProfile());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(T("Zamknij program (Ctrl+Q)", "Quit (Ctrl+Q)"), null, (_, _) => Close());
        foreach (Control control in Walk(this)) control.ContextMenuStrip = menu;
        previous?.Dispose();
    }
    private void SaveProfile()
    {
        using var dialog = new SaveFileDialog
        {
            InitialDirectory = PortableData.ProgramDirectory,
            RestoreDirectory = true,
            Filter = T("Profil Stroika (*.json)|*.json", "Stroik profile (*.json)|*.json"),
            DefaultExt = "json",
            AddExtension = true,
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
            InitialDirectory = PortableData.ProgramDirectory,
            RestoreDirectory = true,
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
        if (_instrumentKind != InstrumentKind.Piano) { _guideText.Text = ""; return; }
        int midi = _targetMidi >= 0 ? _targetMidi : _lastResolvedTarget;
        if (midi is < NoteCatalog.LowestMidi or > NoteCatalog.HighestMidi) midi = NoteCatalog.DefaultMidi;
        _guideText.Text = KeyboardGuide.Describe(midi, Locale);
        _keyboard.SelectNote(midi);
    }
    private void AdvanceTarget(int current) => MoveTarget(current, 1);
    private void MoveTarget(int current, int direction)
    {
        if (current is < NoteCatalog.LowestMidi or > NoteCatalog.HighestMidi)
            current = InstrumentCatalog.DefaultMidi(_instrumentKind, _ukuleleKind);
        int? next;
        if (_instrumentKind == InstrumentKind.Piano)
        {
            PianoOrder order = (PianoOrder)Math.Max(0, _pianoOrder.SelectedIndex);
            if (!TuningPlan.Build(_planAnchorMidi, order).Contains(current)) _planAnchorMidi = current;
            next = TuningPlan.Move(_planAnchorMidi, current, order, direction,
                _activeMethod == TuningMethod.Electronic ? NoteCatalog.HighestMidi : IntervalAnalysis.HighestTarget(SelectedConsonance == Consonance.PeriodicFifth ? Consonance.Off : _consonanceKind));
        }
        else if (InstrumentCatalog.HasStrings(_instrumentKind))
        {
            IReadOnlyList<OpenString> strings = InstrumentCatalog.Strings(_instrumentKind, _ukuleleKind);
            int index = -1;
            for (int i = 0; i < strings.Count; i++) if (strings[i].Midi == current) { index = i; break; }
            next = index >= 0 && index + direction >= 0 && index + direction < strings.Count ? strings[index + direction].Midi : null;
        }
        else next = current + direction is >= NoteCatalog.LowestMidi and <= NoteCatalog.HighestMidi ? current + direction : null;
        if (next is null || (_activeMethod != TuningMethod.Electronic && next > IntervalAnalysis.HighestTarget(SelectedConsonance == Consonance.PeriodicFifth ? Consonance.Off : _consonanceKind)))
        {
            if (direction > 0) { StopMicrophone(false); _fork.Stop(); _automaticStrikesRunning = false; _strikeTimer.Stop(); UpdateAutomaticStatus(); }
            Announce(direction > 0 ? T("To koniec wybranej kolejności strojenia. Strojenie zatrzymane.", "End of the selected tuning order. Tuning stopped.") : T("To początek wybranej kolejności.", "Start of the selected order."));
            return;
        }
        _advancingTarget = true;
        try { SelectTarget(next.Value); }
        finally { _advancingTarget = false; }
        UpdateGuide();
        Persist();
        AnnounceTarget();
    }

    private void StopOutOfRangeFork()
    {
        _fork.Stop();
        _automaticStrikesRunning = false;
        _strikeTimer.Stop();
        _practiceTimer.Stop();
        _practice.Stop();
        UpdateAutomaticStatus();
        _status.Text = T("Kamerton wstrzymany: dla tego współbrzmienia wybierz niższy dźwięk albo wyłącz współbrzmienia.",
            "Fork paused: choose a lower target for this interval, or turn consonance off.");
        Announce(_status.Text);
    }

    private void ToggleMicrophone()
    {
        if (_microphone is null) StartMicrophone();
        else StopMicrophone();
    }

    private void StartMicrophone()
    {
        if (_microphone is not null) return;
        var microphone = new Microphone();
        try
        {
            _session.Reset(); lock (_navigationLock) _strikeNavigator.Reset();
            _windowCount = 0; _pairCount = 0; _lowerIntervalFrequency = null; _tapNavigator.Reset(); _ignoreUntil = DateTime.MinValue;
            _processedGeneration = Volatile.Read(ref _measurementGeneration);
            microphone.FrameReady += ProcessFrame;
            microphone.Failed += error =>
            {
                try
                {
                    BeginInvoke((Action)(() =>
                    {
                        if (!ReferenceEquals(_microphone, microphone) || IsDisposed) return;
                        StopMicrophone(false);
                        _status.Text = T("Przechwytywanie przerwane. Włącz mikrofon ponownie. Szczegóły: ",
                            "Capture interrupted. Start the microphone again. Details: ") + error.Message;
                        Announce(_status.Text);
                    }));
                }
                catch (InvalidOperationException) { }
            };
            if (_activeMethod == TuningMethod.TuningFork && _forkQuietUntil < DateTime.UtcNow.AddMilliseconds(600)) _forkQuietUntil = DateTime.UtcNow.AddMilliseconds(600);
            microphone.Start(); _microphone = microphone;
            _acceptAudio = true;
            if (_activeMethod == TuningMethod.HybridHeadphones && _monitor.Checked) _audioMonitor = CreateMonitor();
            bool hybrid = _activeMethod is TuningMethod.HybridSpeakers or TuningMethod.HybridHeadphones;
            if (hybrid && _view.SelectedIndex == (int)TunerView.Beginner)
            {
                _waitingForGuidance = true; _acceptAudio = false; _practice.Stop();
                // Read after focus/status changes below, so their announcements cannot interrupt it.
            }
            else if (hybrid) Strike();
            if (_activeMethod == TuningMethod.HybridSpeakers) _practiceTimer.Start();
            _start.Text = T("Wyłącz mikrofon", "Stop microphone");
            _status.Text = _activeMethod == TuningMethod.HybridSpeakers
                ? T("Hybryda: poczekaj na wybrzmienie wzorca, potem graj i wycisz instrument.", "Hybrid: wait for the reference to decay, then play and mute your instrument.")
                : T("Mikrofon włączony. Zagraj dźwięk i go wycisz.", "Microphone is on. Play a note and let it stop.");
            if (_activeMethod != TuningMethod.TuningFork) { _start.Focus(); if (!hybrid) Announce(_status.Text); }
            if (_waitingForGuidance) ReadGuidanceBeforeFork();
        }
        catch (Exception error)
        {
            microphone.Dispose();
            MessageBox.Show(this, error.Message, T("Nie można włączyć mikrofonu", "Cannot start microphone"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    private void StopMicrophone(bool announce = true)
    {
        _guidanceEpoch++; _waitingForGuidance = false;
        _resultSpeechEpoch++; _waitingForResultSpeech = false;
        Interlocked.Exchange(ref _speakerAdvanceUntil, 0);
        _speech.Stop();
        if (_microphone is null) return;
        Microphone microphone = _microphone; _microphone = null;
        Interlocked.Increment(ref _measurementGeneration);
        microphone.FrameReady -= ProcessFrame; microphone.Dispose();
        _practiceTimer.Stop(); _practice.Stop(); _acceptAudio = true;
        _audioMonitor?.Dispose(); _audioMonitor = null;
        if (_activeMethod is TuningMethod.HybridSpeakers or TuningMethod.HybridHeadphones) _fork.Stop();
        _session.Reset(); lock (_navigationLock) _strikeNavigator.Reset();
        _windowCount = 0; _meter.SetReading(null);
        _start.Text = T("Włącz mikrofon", "Start microphone");
        _status.Text = T("Mikrofon jest wyłączony.", "Microphone is off.");
        if (announce && !IsDisposed && !Disposing) { _start.Focus(); Announce(_status.Text); }
    }
    // The audio thread emits final spoken results and throttled, silent visual readings.
    private void ProcessFrame(short[] samples)
        => ProcessFrameAt(samples, DateTime.UtcNow);
    private void ProcessFrameAt(short[] samples, DateTime now)
    {
        if (_closing || _waitingForGuidance) return;
        int generation = Volatile.Read(ref _measurementGeneration);
        if (_processedGeneration != generation)
        {
            _session.Reset(); _tapNavigator.Reset(); _windowCount = 0; _pairCount = 0; _lowerIntervalFrequency = null; _ignoreUntil = DateTime.MinValue;
            lock (_navigationLock) _strikeNavigator.Reset();
            _processedGeneration = generation;
        }
        double reference = _reference, tolerance = _toleranceCents;
        int target = _targetMidi;
        InstrumentKind instrument = _instrumentKind;
        UkuleleTuning ukulele = _ukuleleKind;
        if (_activeMethod == TuningMethod.HybridHeadphones && _monitorEnabled) _audioMonitor?.Write(samples, _microphone?.SampleRate ?? 48000, _monitorLevel);
        if (_activeMethod == TuningMethod.HybridSpeakers && Interlocked.Read(ref _speakerAdvanceUntil) != 0)
        {
            long until = Interlocked.Read(ref _speakerAdvanceUntil);
            if (now.Ticks < until && _tapNavigator.Observe(TapNavigator.TransientLevel(samples), _minimumRms, now,
                Volatile.Read(ref _doubleStrikeWindowMilliseconds)))
            {
                Interlocked.Exchange(ref _speakerAdvanceUntil, 0);
                int from = target < 0 ? _lastResolvedTarget : target;
                try { BeginInvoke((Action)(() => { if (generation == Volatile.Read(ref _measurementGeneration)) AdvanceTarget(from); })); }
                catch (InvalidOperationException) { }
            }
            return;
        }
        if (_activeMethod == TuningMethod.HybridSpeakers && !_acceptAudio)
        {
            if (!_practice.Listening(now)) { _windowCount = 0; _pairCount = 0; _session.Reset(); _tapNavigator.Reset(); return; }
            _acceptAudio = true;
        }
        if (_activeMethod == TuningMethod.TuningFork && now < _forkQuietUntil) { _windowCount = 0; _session.Reset(); _tapNavigator.Reset(); return; }
        if (now < _ignoreUntil) { _windowCount = 0; return; }
        if (_waitingForGuidance || _waitingForResultSpeech) { _windowCount = 0; _pairCount = 0; _session.Reset(); _tapNavigator.Reset(); return; }
        bool forkNavigation = _forkTapEnabled && _activeMethod == TuningMethod.TuningFork;
        if (forkNavigation && (NvdaReader.IsSpeaking || _speech.IsSpeaking))
        {
            _tapNavigator.Reset();
            return;
        }
        if (forkNavigation && _tapNavigator.ObserveImpulses(samples, _microphone?.SampleRate ?? 48000,
            _minimumRms, now, Volatile.Read(ref _doubleStrikeWindowMilliseconds)))
        {
            int nextFrom = target < 0 ? InstrumentCatalog.DefaultMidi(instrument, ukulele) : target;
            _ignoreUntil = now.AddSeconds(2);
            try { BeginInvoke((Action)(() => { if (generation == Volatile.Read(ref _measurementGeneration)) AdvanceTarget(nextFrom); })); }
            catch (InvalidOperationException) { }
            return;
        }
        if (_consonanceKind != Consonance.Off && _activeMethod != TuningMethod.TuningFork && _together)
        {
            ProcessIntervalTogether(samples, now, generation);
            return;
        }
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
            if (_windowCount == _window.Length)
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
        if (ConfirmedNavigationEnabled)
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
            try { BeginInvoke((Action)(() => { if (generation == Volatile.Read(ref _measurementGeneration)) AdvanceTarget(midi); })); }
            catch (InvalidOperationException) { }
            return;
        }
        if (_visualEnabled && frequency is > 0 && now - _lastVisualAt >= TimeSpan.FromMilliseconds(120))
        {
            _lastVisualAt = now;
            double measured = frequency.Value;
            try { BeginInvoke((Action)(() => { if (generation == Volatile.Read(ref _measurementGeneration)) UpdateMeter(measured); })); }
            catch (InvalidOperationException) { }
        }
        double? completed = _session.Observe(sound, frequency, now);
        if (completed is not > 0 || generation != Volatile.Read(ref _measurementGeneration)) return;
        if (_activeMethod == TuningMethod.TuningFork) return;
        if (_consonanceKind != Consonance.Off)
        {
            double value = completed.Value;
            try { BeginInvoke((Action)(() => { if (generation == Volatile.Read(ref _measurementGeneration)) ReceiveIntervalNote(value); })); }
            catch (InvalidOperationException) { }
            return;
        }
        double result = completed.Value;
        int resolved = ResolveTarget(target, instrument, ukulele, result, reference);
        double targetFrequency = TargetFrequency(resolved, reference);
        bool inTune = Math.Abs(1200 * Math.Log2(result / targetFrequency)) <= tolerance;
        bool counting;
        lock (_navigationLock)
        {
            counting = _strikeNavigator.IsCounting;
            if (!counting)
            {
                _strikeNavigator.Reset();
            }
        }
        if (counting && inTune) return;
        if (!inTune) lock (_navigationLock) _strikeNavigator.Reset();
        _ignoreUntil = now.AddMilliseconds(1500);
        _waitingForResultSpeech = true;
        try { BeginInvoke((Action)(() => { if (generation == Volatile.Read(ref _measurementGeneration)) ShowResult(result, reference, target, tolerance, instrument, ukulele); })); }
        catch (InvalidOperationException) { }
    }
    private int ResolveTarget(int target, InstrumentKind instrument, UkuleleTuning ukulele, double measured, double reference)
    {
        if (target >= 0) return target;
        var map = _instrumentMap;
        if (map is null)
            return InstrumentCatalog.HasStrings(instrument)
                ? InstrumentCatalog.NearestOpenString(instrument, measured, reference, ukulele)
                : PitchDetector.Nearest(measured, reference).Midi;
        IEnumerable<int> candidates = InstrumentCatalog.HasStrings(instrument)
            ? InstrumentCatalog.Strings(instrument, ukulele).Select(item => item.Midi) : Enumerable.Range(21, 88);
        return candidates.MinBy(midi => Math.Abs(Math.Log(measured / map.Frequency(midi, reference))));
    }
    private void UpdateMeter(double measured)
    {
        if (!_visualMode.Checked || _activeMethod == TuningMethod.TuningFork || _consonanceKind != Consonance.Off) return;
        int target = ResolveTarget(_targetMidi, _instrumentKind, _ukuleleKind, measured, _reference);
        double expected = TargetFrequency(target, _reference);
        double cents = 1200 * Math.Log2(measured / expected);
        _meter.SetReading(cents);
        _keyboard.SelectNote(target); _keyboard.SetReading(cents, _toleranceCents);
    }
    private void ShowResult(double measured, double reference, int targetMidi, double tolerance, InstrumentKind instrument, UkuleleTuning ukulele, bool announce = true)
    {
        if (IsDisposed) return;
        _hasResult = true; _lastMeasured = measured; _lastReference = reference; _lastTarget = targetMidi; _lastTolerance = tolerance; _lastInstrument = instrument; _lastUkulele = ukulele;
        int resolved = ResolveTarget(targetMidi, instrument, ukulele, measured, reference);
        _lastResolvedTarget = resolved;
        Note note = PitchDetector.NoteFromMidi(resolved, reference) with { Frequency = TargetFrequency(resolved, reference) };
        double hertz = measured - note.Frequency;
        double cents = 1200 * Math.Log2(measured / note.Frequency);
        CultureInfo culture = UiLanguages.Culture(Locale);
        string name = note.Midi is >= NoteCatalog.LowestMidi and <= NoteCatalog.HighestMidi
            ? NoteCatalog.Label(note.Midi, Locale) : note.Name;
        if (InstrumentCatalog.HasStrings(instrument))
        {
            OpenString open = InstrumentCatalog.Strings(instrument, ukulele).FirstOrDefault(s => s.Midi == note.Midi);
            if (open.Number > 0) name = Tf($"struna {open.Number}: ", $"string {open.Number}: ") + name;
        }
        string direction = Math.Abs(cents) <= tolerance ? T("dobrze", "in tune") : hertz < 0 ? T("za nisko", "too low") : T("za wysoko", "too high");
        string inHertz = Tf($"{hertz:+0.0;-0.0;0.0} herca", $"{hertz:+0.0;-0.0;0.0} hertz");
        string inCents = Tf($"{cents:+0.0;-0.0;0.0} centy", $"{cents:+0.0;-0.0;0.0} cents");
        string deviation = (ResultUnits)_resultUnits.SelectedIndex switch
        {
            ResultUnits.Cents => inCents,
            ResultUnits.CentsThenHertz => inCents + ", " + inHertz,
            ResultUnits.HertzThenCents => inHertz + ", " + inCents,
            _ => inHertz
        };
        string announcement = $"{name}, {direction}, {deviation}.";
        if (_view.SelectedIndex == 1 && Math.Abs(cents) <= tolerance && _activeMethod == TuningMethod.HybridSpeakers && _forkTapEnabled)
            announcement += " " + T("Stuknij dwa razy, aby przejść dalej.", "Tap twice to advance.");
        _result.Text = announcement;
        _details.Text = Tf($"Zmierzono: {measured.ToString("0.0", culture)} Hz. Cel: {note.Frequency.ToString("0.0", culture)} Hz. Odchyłka: {cents.ToString("+0.0;-0.0;0.0", culture)} centów.",
            $"Measured: {measured.ToString("0.0", culture)} Hz. Target: {note.Frequency.ToString("0.0", culture)} Hz. Difference: {cents.ToString("+0.0;-0.0;0.0", culture)} cents.");
        _lastAnnouncement = announcement; _repeat.Enabled = true;
        if (instrument == InstrumentKind.Piano && targetMidi < 0) UpdateGuide();
        _meter.SetReading(cents);
        _keyboard.SelectNote(resolved); _keyboard.SetReading(cents, tolerance);
        if (announce) ReadResult(announcement, Math.Abs(cents) <= tolerance ? resolved : -1, note.Frequency, tolerance);
    }
    private void ResetResult()
    {
        Interlocked.Exchange(ref _speakerAdvanceUntil, 0);
        _resultSpeechEpoch++; _waitingForResultSpeech = false;
        Interlocked.Increment(ref _measurementGeneration);
        _lowerIntervalFrequency = null;
        UpdateIntervalHelp();
        _hasResult = false;
        _lastAnnouncement = null;
        _repeat.Enabled = false;
        _details.Text = "";
        _meter.SetReading(null);
        _keyboard.SetReading(null, _toleranceCents);
        _result.Text = T("Wynik pojawi się po ucichnięciu dźwięku.", "The result appears when the note stops.");
        if (!_initializing && _microphone is not null && _activeMethod != TuningMethod.Electronic && !_waitingForGuidance) Strike();
    }
    private void Announce(string message)
    {
        if (_visualMode.Checked && !_voiceInVisual.Checked) return;
        if (!ScreenReaderDetector.IsActive())
        {
            if (!_speech.Speak(message, Locale)) _details.Text = T("Synteza mowy niedostępna: ", "Speech unavailable: ") + _speech.LastError;
            return;
        }
        _result.AccessibilityObject.RaiseAutomationNotification(
            AutomationNotificationKind.ActionCompleted, AutomationNotificationProcessing.ImportantAll, message);
    }
    internal static string CheckBoxAnnouncement(string label, bool isChecked, bool english) =>
        english ? $"{label}: {(isChecked ? "checked" : "unchecked")}."
            : $"{label}: {(isChecked ? "zaznaczone" : "odznaczone")}.";
    private void AnnounceCheckBoxState(CheckBox checkBox)
    {
        if (!IsHandleCreated || IsDisposed || Disposing) return;
        string label = checkBox.AccessibleName ?? checkBox.Text.Replace("&", "");
        string message = label + ": " + (checkBox.Checked ? T("zaznaczone", "checked") : T("odznaczone", "unchecked")) + ".";
        BeginInvoke((Action)(() =>
        {
            if (!IsDisposed && !checkBox.IsDisposed && checkBox.IsHandleCreated)
                checkBox.AccessibilityObject.RaiseAutomationNotification(
                    AutomationNotificationKind.ActionCompleted, AutomationNotificationProcessing.ImportantMostRecent, message);
        }));
    }
    private void ShowRecentChanges()
    {
        try
        {
            Version assemblyVersion = typeof(TunerForm).Assembly.GetName().Version ?? new Version(1, 0, 14);
            Version version = new(assemblyVersion.Major, assemblyVersion.Minor, Math.Max(0, assemblyVersion.Build));
            using var dialog = new ReleaseNotesDialog(version, ReleaseNotes.Current(Locale), Locale, _dark.Checked, false);
            dialog.ShowDialog(this);
        }
        catch (InvalidDataException error)
        {
            MessageBox.Show(this, error.Message, T("Nie można pokazać zmian", "Cannot show changes"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    private readonly Button _licenses = new() { AutoSize = true };
    private void ShowAbout()
    {
        MessageBox.Show(this, Tf($"Stroik {DisplayVersion}. Dostępny stroiciel dla osób niewidomych, słabowidzących i widzących. Kamerton, mikrofon i nauka strojenia na słuch. Wykonawca: Proszczok z Codex. Pomysłodawca: Witold.", $"Stroik {DisplayVersion}. Accessible tuner for blind, partially sighted and sighted musicians. Tuning fork, microphone and ear-training practice. Created by Proszczok with Codex. Original idea: Witold."), T("O programie", "About Stroik"), MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    internal static string AboutText(bool english) => english
        ? $"Stroik {DisplayVersion} — an accessible instrument tuner for blind, partially sighted and sighted musicians. It helps tune piano, guitar, ukulele and other instruments using a microphone or a tuning fork tone.\n\nCreated by: Proszczok with Codex.\nOriginal idea: Witold."
        : $"Stroik {DisplayVersion} — dostępny stroiciel instrumentów dla osób niewidomych, słabowidzących i widzących. Pomaga stroić pianino, gitarę, ukulele i inne instrumenty za pomocą mikrofonu lub dźwięku kamertonu.\n\nWykonawca: Proszczok z Codex.\nPomysłodawca: Witold.";
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
    internal async Task CheckUpdatesAsync(bool quiet, Func<Task<ReleaseInfo?>>? findRelease = null)
    {
        if (_closing || IsDisposed || _calibratorBusy || (quiet && !_automaticUpdates.Checked) || !_updateChecks.TryBegin()) return;
        bool prompted = false;
        if (!quiet)
        {
            _updates.Enabled = false;
            SetUpdateStatus("Sprawdzanie aktualizacji…", "Checking for updates…");
        }
        try
        {
            ReleaseInfo? release = await (findRelease ?? Updater.FindNewReleaseAsync)();
            if (_closing || IsDisposed || (quiet && !_automaticUpdates.Checked)) return;
            if (release is null)
            {
                if (!quiet)
                {
                    SetUpdateStatus("Masz najnowszą wersję.", "You have the latest version.");
                    MessageBox.Show(this, T("Masz najnowszą wersję.", "You have the latest version."), T("Aktualizacje", "Updates"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return;
            }
            if (!_updateChecks.ShouldPrompt(release.Version, quiet)) return;
            _updateChecks.MarkPrompted(release.Version);
            prompted = true;
            SetUpdateStatusF($"Dostępna wersja {release.Version}.", $"Version {release.Version} is available.");
            using (var dialog = new ReleaseNotesDialog(release.Version,
                ReleaseNotes.ForLanguage(release.Notes, Locale), Locale, _dark.Checked, true))
            {
                if (dialog.ShowDialog(this) != DialogResult.Yes) return;
            }
            UseWaitCursor = true;
            SetUpdateStatus("Pobieranie i weryfikacja aktualizacji…", "Downloading and verifying the update…");
            string downloaded = await Updater.DownloadVerifiedAsync(release);
            if (_closing || IsDisposed) return;
            UseWaitCursor = false;
            await Updater.LaunchReplacementAsync(downloaded);
            SetUpdateStatus("Instalowanie aktualizacji…", "Installing the update…");
            Close();
        }
        catch (Exception error)
        {
            UpdateInstaller.Log("Update check/install failed", error);
            if (_closing || IsDisposed || (quiet && !prompted)) return;
            UseWaitCursor = false;
            SetUpdateStatusF($"Nie udało się sprawdzić lub zainstalować aktualizacji. Spróbuj ponownie przyciskiem Sprawdź aktualizacje. Szczegóły: {error.Message}",
                $"Could not check or install the update. Retry with Check for updates. Details: {error.Message}");
            if (!quiet || prompted) MessageBox.Show(this, error.Message + "\n\n" + T("Dziennik: ", "Log: ") + UpdateInstaller.LogPath, T("Błąd aktualizacji", "Update error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _updateChecks.End();
            if (!_closing && !IsDisposed) _updates.Enabled = true;
        }
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing) { if (_noteKeyFilter is not null) { Application.RemoveMessageFilter(_noteKeyFilter); _noteKeyFilter = null; } _closing = true; _updateTimer.Dispose(); _speech.Dispose(); _practiceTimer.Dispose(); _audioMonitor?.Dispose(); }
        base.Dispose(disposing);
    }
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr window, int attribute, ref int value, int valueSize);
    private sealed record Choice(int Id, double Value, string Description)
    {
        public override string ToString() => Description;
    }
}
