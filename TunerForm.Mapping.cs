namespace Stroik;

internal sealed partial class TunerForm
{
    private readonly TabPage _mappingTab = new();
    private readonly ComboBox _mapOctave = new() { DropDownStyle = ComboBoxStyle.DropDownList }, _mapNote = new() { DropDownStyle = ComboBoxStyle.DropDownList }, _mapCount = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label _mapOctaveLabel = MakeLabel(), _mapNoteLabel = MakeLabel(), _mapCountLabel = MakeLabel(), _mapStatus = MakeLabel();
    private readonly Button _mapStart = MakeButton(), _mapStop = MakeButton(), _mapNext = MakeButton(), _mapSave = MakeButton(),
        _mapLoad = MakeButton(), _mapUse = MakeButton(), _mapClear = MakeButton(), _calibrationLoad = MakeButton(), _calibrationClear = MakeButton();
    private readonly CheckBox _mapTap = new() { AutoSize = true, Checked = true };
    private readonly TrackBar _forkVolume = new() { Minimum = 0, Maximum = 100, Value = 100, TickStyle = TickStyle.None, SmallChange = 1, LargeChange = 10 };
    private readonly TrackBar _monitorVolume = new() { Minimum = 0, Maximum = 100, Value = 100, TickStyle = TickStyle.None, SmallChange = 1, LargeChange = 10 };
    private readonly Label _forkVolumeLabel = MakeLabel(), _monitorVolumeLabel = MakeLabel();
    private volatile int _monitorLevel = 100;
    private volatile InstrumentMap? _instrumentMap;
    private ForkCalibration? _forkCalibration;
    private InstrumentMap _mapDraft = new();
    private readonly object _mapLock = new();
    private readonly NoteSession _mapSession = new();
    private readonly TapNavigator _mapNavigator = new();
    private readonly short[] _mapWindow = new short[8192];
    private Microphone? _mapMic;
    private int _mapMidi = 69, _mapFrames, _mapSamples, _mapNeeded = 1;
    private int _mapEpoch;
    private double _mapSum;
    private bool _mapPopulating, _mapTapEnabled = true;
    private bool _mapSpeaking;

    private static Button MakeButton() => new() { Dock = DockStyle.Top, AutoSize = true, MinimumSize = new Size(0, 36) };
    private AudioMonitor CreateMonitor() => new("Stroik — " + T("Odsłuch mikrofonu w słuchawkach", "Monitor microphone in headphones"));

    private void BuildMappingUI(TableLayoutPanel settings)
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, ColumnCount = 1, Padding = new Padding(18) };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _tabs.TabPages.Add(_mappingTab);
        _mappingTab.Controls.Add(panel);
        foreach (Control item in new Control[] { _mapCountLabel, _mapCount, _mapOctaveLabel, _mapOctave, _mapNoteLabel, _mapNote,
            _mapTap, _mapStart, _mapStop, _mapNext, _mapStatus, _mapSave, _mapLoad, _mapUse, _mapClear }) Add(panel, item);
        _mapCount.Items.AddRange(Enumerable.Range(1, 10).Cast<object>().ToArray());
        _mapCount.SelectedIndex = 0;
        _mapOctave.SelectedIndexChanged += (_, _) =>
        {
            if (_mapPopulating || _mapOctave.SelectedIndex < 0) return;
            int midi = Math.Clamp((_mapOctave.SelectedIndex + 1) * 12 + _mapMidi % 12, 21, 108);
            SelectMappingNote(midi);
        };
        _mapNote.SelectedIndexChanged += (_, _) =>
        {
            if (!_mapPopulating && _mapNote.SelectedItem is Choice selected) SelectMappingNote(selected.Id);
        };
        _mapCount.SelectedIndexChanged += (_, _) => { lock (_mapLock) { _mapNeeded = Math.Max(1, _mapCount.SelectedIndex + 1); ResetMappingCapture(); } };
        _mapTap.CheckedChanged += (_, _) => { lock (_mapLock) { _mapTapEnabled = _mapTap.Checked; _mapNavigator.Reset(); } };
        _mapStart.Click += (_, _) => StartMapping();
        _mapStop.Click += (_, _) => StopMapping();
        _mapNext.Click += (_, _) => NextMappingNote();
        _mapSave.Click += (_, _) => SaveMapping();
        _mapLoad.Click += (_, _) => OpenTuningFile(false);
        _mapUse.Click += (_, _) => { _instrumentMap = new InstrumentMap { Notes = new(_mapDraft.Notes) }; ResetResult(); Persist(); MappingStatus(T("Mapa aktywna.", "Instrument map active.")); };
        _mapClear.Click += (_, _) => { _instrumentMap = null; ResetResult(); Persist(); MappingStatus(T("Przywrócono strój standardowy.", "Standard tuning restored.")); };
        _tabs.SelectedIndexChanged += (_, _) =>
        {
            if (_tabs.SelectedTab == _mappingTab) { StopMicrophone(false); _strikeTimer.Stop(); _fork.Stop(); AcceptButton = null; }
            else { StopMapping(false); UpdateStrikeTimer(); AcceptButton = _activeMethod == TuningMethod.TuningFork ? _strike : _start; }
        };

        foreach (Control item in new Control[] { _forkVolumeLabel, _forkVolume, _monitorVolumeLabel, _monitorVolume, _calibrationLoad, _calibrationClear }) Add(settings, item);
        _forkVolume.ValueChanged += (_, _) => { UpdateVolumeLabels(); Persist(); };
        _monitorVolume.ValueChanged += (_, _) => { _monitorLevel = _monitorVolume.Value; UpdateVolumeLabels(); Persist(); };
        _calibrationLoad.Click += (_, _) => OpenTuningFile(true);
        _calibrationClear.Click += (_, _) => { _forkCalibration = null; Persist(); Announce(T("Przywrócono fabryczną barwę kamertonu.", "Default fork calibration restored.")); };
    }

    private void UpdateVolumeLabels()
    {
        _forkVolumeLabel.Text = _forkVolume.AccessibleName = T("Głośność kamertonu, procent", "Fork volume, percent");
        _monitorVolumeLabel.Text = _monitorVolume.AccessibleName = T("Głośność odsłuchu mikrofonu, procent", "Microphone monitoring volume, percent");
    }

    private void LoadAudioPreferences(Preferences settings)
    {
        bool initializing = _initializing;
        _initializing = true;
        _forkVolume.Value = settings.ForkVolume;
        _monitorLevel = _monitorVolume.Value = settings.MonitorVolume;
        _instrumentMap = settings.InstrumentMap;
        _forkCalibration = settings.ForkCalibration;
        if (_instrumentMap is not null) _mapDraft = new InstrumentMap { Notes = new(_instrumentMap.Notes) };
        _initializing = initializing;
    }

    private void ApplyMappingLanguage()
    {
        _mappingTab.Text = T("Mapowanie instrumentu", "Instrument mapping");
        _mapCountLabel.Text = _mapCount.AccessibleName = T("Liczba uderzeń na dźwięk (1–10)", "Strikes per note (1–10)");
        _mapOctaveLabel.Text = _mapOctave.AccessibleName = T("Wybierz oktawę", "Choose octave");
        _mapNoteLabel.Text = _mapNote.AccessibleName = T("Dźwięk docelowy", "Target note");
        _mapTap.Text = T("Po zapisaniu pomiaru dwa stuknięcia wybierają następny dźwięk", "After a measurement is stored, two taps select the next note");
        _mapStart.Text = T("Rozpocznij mapowanie", "Start mapping");
        _mapStop.Text = T("Zakończ mapowanie", "Finish mapping");
        _mapNext.Text = T("Następny dźwięk", "Next note");
        _mapSave.Text = T("Zapisz plik mapingu", "Save mapping file");
        _mapLoad.Text = T("Wczytaj plik mapingu", "Open mapping file");
        _mapUse.Text = T("Użyj mapy do strojenia", "Use map for tuning");
        _mapClear.Text = T("Przywróć strój standardowy", "Restore standard tuning");
        _calibrationLoad.Text = T("Wczytaj kalibrację kamertonu", "Open fork calibration");
        _calibrationClear.Text = T("Przywróć fabryczną barwę kamertonu", "Restore default fork calibration");
        _mapStart.Enabled = _mapMic is null;
        _mapStop.Enabled = _mapMic is not null;
        UpdateVolumeLabels();
        _mapPopulating = true;
        _mapOctave.Items.Clear();
        _mapOctave.Items.AddRange(Enumerable.Range(0, 9).Select(o => NoteCatalog.OctaveLabel(o, Locale)).Cast<object>().ToArray());
        _mapPopulating = false;
        UpdateMappingNotes();
    }

    private void UpdateMappingNotes()
    {
        _mapPopulating = true;
        int octave = NoteCatalog.Octave(_mapMidi);
        _mapOctave.SelectedIndex = octave;
        _mapNote.Items.Clear();
        foreach (int midi in Enumerable.Range(21, 88).Where(n => NoteCatalog.Octave(n) == octave))
        {
            int index = _mapNote.Items.Add(new Choice(midi, octave, NoteCatalog.Label(midi, Locale)));
            if (midi == _mapMidi) _mapNote.SelectedIndex = index;
        }
        _mapPopulating = false;
        _mapStatus.Text = _mapDraft.Notes.TryGetValue(_mapMidi, out double hz)
            ? Tf($"Zapisano {hz:F2} Hz.", $"Stored {hz:F2} Hz.")
            : T("Zagraj wybrany dźwięk i pozwól mu ucichnąć po każdym uderzeniu.", "Play the selected note and let it decay after each strike.");
        _mapUse.Enabled = _mapDraft.Notes.Count > 0;
    }

    private void ResetMappingCapture()
    {
        _mapEpoch++;
        _mapFrames = _mapSamples = 0;
        _mapSum = 0;
        _mapSpeaking = false;
        _mapSession.Reset();
        _mapNavigator.Reset();
    }

    private void SelectMappingNote(int midi)
    {
        if (_mapMidi == midi) return;
        lock (_mapLock) { _mapMidi = midi; ResetMappingCapture(); }
        UpdateMappingNotes();
    }

    private void NextMappingNote()
    {
        if (_mapMidi < 108)
        {
            SelectMappingNote(_mapMidi + 1);
            MappingStatus(NoteCatalog.Label(_mapMidi, Locale));
        }
        else StopMapping();
    }

    private async void MappingStatus(string text)
    {
        _mapStatus.Text = text;
        if (_visualMode.Checked && !_voiceInVisual.Checked)
        {
            lock (_mapLock) _mapSpeaking = false;
            return;
        }
        if (_mapMic is null)
        {
            Announce(text);
            return;
        }
        int epoch;
        lock (_mapLock) { epoch = _mapEpoch; _mapSpeaking = true; }
        try
        {
            if (NvdaReader.Available) await NvdaReader.ReadAsync(text, Locale);
            else if (_speech.Speak(text, Locale))
            {
                var deadline = DateTime.UtcNow.AddSeconds(20);
                while (_speech.IsSpeaking && epoch == _mapEpoch && DateTime.UtcNow < deadline) await Task.Delay(40);
            }
        }
        finally { lock (_mapLock) { if (epoch == _mapEpoch) _mapSpeaking = false; } }
    }

    private void StartMapping()
    {
        if (_mapMic is not null) return;
        StopMicrophone(false);
        var microphone = new Microphone();
        lock (_mapLock) ResetMappingCapture();
        microphone.FrameReady += samples => CaptureMapping(samples, microphone.SampleRate);
        microphone.Failed += error => BeginInvoke(() => { if (ReferenceEquals(_mapMic, microphone)) { StopMapping(false); MappingStatus(error.Message); } });
        try
        {
            microphone.Start();
            _mapMic = microphone;
            _mapStart.Enabled = false;
            _mapStop.Enabled = true;
            MappingStatus(T("Zagraj wybrany dźwięk i pozwól mu ucichnąć po każdym uderzeniu.", "Play the selected note and let it decay after each strike."));
        }
        catch (Exception error) { microphone.Dispose(); MappingStatus(error.Message); }
    }

    private void StopMapping(bool announce = true)
    {
        var mic = _mapMic;
        if (mic is null) return;
        _mapMic = null;
        lock (_mapLock) ResetMappingCapture();
        _speech.Stop();
        mic.Dispose();
        _mapStart.Enabled = true;
        _mapStop.Enabled = false;
        if (announce) MappingStatus(T("Mapowanie zatrzymane.", "Mapping stopped."));
    }

    private void CaptureMapping(short[] samples, int rate)
        => CaptureMappingAt(samples, rate, DateTime.UtcNow);

    private void CaptureMappingAt(short[] samples, int rate, DateTime now)
    {
        lock (_mapLock)
        {
            int epoch = _mapEpoch;
            if (_mapSpeaking) return;
            double rms = PitchDetector.Rms(samples);
            if (_mapSamples >= _mapNeeded)
            {
                if (_mapTapEnabled && _mapNavigator.Observe(TapNavigator.TransientLevel(samples), _minimumRms, now, Volatile.Read(ref _doubleStrikeWindowMilliseconds)))
                    PostMapping(epoch, NextMappingNote);
                return;
            }
            bool sound = rms >= _minimumRms;
            double? frequency = null;
            if (sound)
            {
                int take = Math.Min(samples.Length, _mapWindow.Length);
                int keep = Math.Min(_mapFrames, _mapWindow.Length - take);
                Array.Copy(_mapWindow, _mapFrames - keep, _mapWindow, 0, keep);
                Array.Copy(samples, samples.Length - take, _mapWindow, keep, take);
                _mapFrames = keep + take;
                if (_mapFrames == _mapWindow.Length && PitchDetector.Estimate(_mapWindow, rate) is { Confidence: >= .75 } pitch)
                    frequency = pitch.Frequency * _correctionFactor;
            }
            else _mapFrames = 0;
            if (_mapSession.Observe(sound, frequency, now) is not double hz) return;
            int count = ++_mapSamples, needed = _mapNeeded, midi = _mapMidi;
            _mapSum += hz;
            double average = _mapSum / count;
            _mapSpeaking = true;
            PostMapping(epoch, () =>
            {
                if (count < needed) MappingStatus(Tf($"Pomiar {count} z {needed}.", $"Measurement {count} of {needed}."));
                else
                {
                    _mapDraft.Notes[midi] = average;
                    _mapUse.Enabled = true;
                    MappingStatus(Tf($"Zapisano {average:F2} Hz.", $"Stored {average:F2} Hz."));
                }
            });
        }
    }

    private void PostMapping(int epoch, Action action)
    {
        try { BeginInvoke(() => { if (_mapMic is not null && epoch == _mapEpoch) action(); }); }
        catch (InvalidOperationException) { }
    }

    private void SaveMapping()
    {
        using var dialog = new SaveFileDialog { Title = _mapSave.Text, Filter = "JSON (*.json)|*.json", FileName = "stroik-instrument-map.json",
            InitialDirectory = PortableData.ProgramDirectory, RestoreDirectory = true };
        try { if (dialog.ShowDialog(this) == DialogResult.OK) TuningFiles.Write(dialog.FileName, _mapDraft); }
        catch (Exception error) { MappingStatus(error.Message); }
    }

    private void OpenTuningFile(bool calibration)
    {
        using var dialog = new OpenFileDialog { Filter = "JSON (*.json)|*.json", Title = calibration ? _calibrationLoad.Text : _mapLoad.Text,
            InitialDirectory = PortableData.ProgramDirectory, RestoreDirectory = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            if (calibration)
            {
                var file = TuningFiles.Read<ForkCalibration>(dialog.FileName, "stroik-fork-calibration");
                file.Validate();
                _forkCalibration = file;
                RefreshForkAfterSelection();
                Persist();
                Announce(T("Wczytano kalibrację kamertonu.", "Fork calibration loaded."));
            }
            else
            {
                var file = TuningFiles.Read<InstrumentMap>(dialog.FileName, "stroik-instrument-map");
                file.Validate();
                StopMapping(false);
                _mapDraft = file;
                UpdateMappingNotes();
                MappingStatus(T("Wczytano mapę instrumentu.", "Instrument map loaded."));
            }
        }
        catch (Exception error) { MessageBox.Show(this, error.Message, dialog.Title); }
    }

    private double TargetFrequency(int midi, double reference) => _instrumentMap?.Frequency(midi, reference) ?? PitchDetector.NoteFromMidi(midi, reference).Frequency;
}
