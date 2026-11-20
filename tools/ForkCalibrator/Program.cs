namespace Stroik;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new CalibratorForm());
    }
}

internal sealed class CalibratorForm : Form
{
    private readonly TuningFork _fork = new(calibrator: true);
    private readonly ComboBox _language = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ListBox _notes = new() { Height = 150 };
    private readonly TrackBar _harmonics = new() { Maximum = 200, Value = 100, TickFrequency = 25, LargeChange = 10 };
    private readonly TrackBar _volume = new() { Maximum = 150, Value = 100, TickFrequency = 25, LargeChange = 10 };
    private readonly Label _languageLabel = new(), _noteLabel = new(), _harmonicsLabel = new(), _volumeLabel = new(), _help = new();
    private readonly Button _play = new(), _stop = new(), _save = new(), _open = new(), _reset = new();
    private ForkCalibration _calibration = new();
    private string _locale = UiLanguages.Codes.Contains(System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
        ? System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName : "en";
    private bool _loading, _dirty;
    private int Midi => Math.Max(0, _notes.SelectedIndex) + 21;
    private string T(string pl, string en) => UiLanguages.Get(_locale, pl, en);

    internal CalibratorForm()
    {
        Size = new Size(650, 680);
        MinimumSize = new Size(430, 470);
        AutoScaleMode = AutoScaleMode.Dpi;
        StartPosition = FormStartPosition.CenterScreen;
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, ColumnCount = 1, Padding = new Padding(16) };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Controls.Add(panel);
        foreach (Control item in new Control[] { _languageLabel, _language, _help, _noteLabel, _notes, _harmonicsLabel, _harmonics, _volumeLabel, _volume })
        {
            item.Dock = DockStyle.Top;
            if (item is Label label) label.AutoSize = true;
            panel.Controls.Add(item);
        }
        var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Top };
        foreach (Button button in new[] { _play, _stop, _reset, _save, _open }) { button.AutoSize = true; buttons.Controls.Add(button); }
        panel.Controls.Add(buttons);
        _language.Items.AddRange(UiLanguages.Names);
        _language.SelectedIndex = Array.IndexOf(UiLanguages.Codes, _locale);
        _language.SelectedIndexChanged += (_, _) => { _locale = UiLanguages.Codes[_language.SelectedIndex]; Translate(); };
        _notes.SelectedIndexChanged += (_, _) => { if (!_loading) { _fork.Stop(); LoadTone(); } };
        _harmonics.ValueChanged += (_, _) => StoreTone();
        _volume.ValueChanged += (_, _) => StoreTone();
        _play.Click += (_, _) => Play();
        _stop.Click += (_, _) => _fork.Stop();
        _save.Click += (_, _) => Save();
        _open.Click += (_, _) => Open();
        _reset.Click += (_, _) => { _calibration.Notes.Remove(Midi); _dirty = true; LoadTone(); };
        FormClosing += (_, eventArgs) => { if (!CanDiscard()) eventArgs.Cancel = true; else _fork.Dispose(); };
        Translate();
        _notes.SelectedIndex = NoteCatalog.DefaultMidi - 21;
    }

    private void Translate()
    {
        string? version = typeof(CalibratorForm).Assembly.GetName().Version?.ToString(3);
        Text = T("Stroik — kalibrator kamertonu", "Stroik — fork calibrator") + " " + version;
        _languageLabel.Text = _language.AccessibleName = T("Język", "Language");
        _noteLabel.Text = _notes.AccessibleName = T("Dźwięk docelowy", "Target note");
        _harmonicsLabel.Text = _harmonics.AccessibleName = T("Harmoniczne wybranej nuty, procent", "Selected note harmonics, percent");
        _volumeLabel.Text = _volume.AccessibleName = T("Głośność wybranej nuty, procent", "Selected note volume, percent");
        _help.Text = T("Wybierz nutę. Zmień suwaki strzałkami i naciśnij Alt+U, aby odsłuchać. 100 to barwa i poziom Stroika. Ctrl+S zapisuje całą kalibrację.",
            "Select a note. Adjust the sliders with the arrows and press Alt+U to listen. 100 is the Stroik baseline for timbre and volume. Ctrl+S saves the entire calibration.");
        _play.Text = T("Uderz kamerton", "Strike tuning fork");
        _stop.Text = T("Zatrzymaj kamerton", "Stop tuning fork");
        _reset.Text = T("Przywróć wartości tej nuty", "Reset this note");
        _save.Text = T("Zapisz kalibrację kamertonu", "Save fork calibration");
        _open.Text = T("Wczytaj kalibrację kamertonu", "Open fork calibration");
        int selected = Midi - 21;
        _loading = true;
        _notes.Items.Clear();
        _notes.Items.AddRange(Enumerable.Range(21, 88).Select(n => NoteCatalog.Label(n, _locale)).Cast<object>().ToArray());
        _notes.SelectedIndex = selected;
        LoadTone();
    }

    private void LoadTone()
    {
        _loading = true;
        ForkTone tone = _calibration.Tone(Midi);
        _harmonics.Value = tone.Harmonics;
        _volume.Value = tone.Volume;
        _loading = false;
    }

    private void StoreTone()
    {
        if (_loading) return;
        _calibration.Notes[Midi] = new ForkTone(_harmonics.Value, _volume.Value);
        _dirty = true;
    }

    private void Play()
    {
        try { _fork.Strike(PitchDetector.NoteFromMidi(Midi, 440).Frequency, lowerTone: _calibration.Tone(Midi), label: Text); }
        catch (Exception error) { MessageBox.Show(this, error.Message, Text); }
    }

    private bool Save()
    {
        using var dialog = new SaveFileDialog { Title = _save.Text, Filter = "JSON (*.json)|*.json", FileName = "stroik-fork-calibration.json" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return false;
        try { TuningFiles.Write(dialog.FileName, _calibration); _dirty = false; return true; }
        catch (Exception error) { MessageBox.Show(this, error.Message, Text); return false; }
    }

    private void Open()
    {
        if (!CanDiscard()) return;
        using var dialog = new OpenFileDialog { Title = _open.Text, Filter = "JSON (*.json)|*.json" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            var file = TuningFiles.Read<ForkCalibration>(dialog.FileName, "stroik-fork-calibration");
            file.Validate();
            _calibration = file;
            _dirty = false;
            LoadTone();
        }
        catch (Exception error) { MessageBox.Show(this, error.Message, Text); }
    }

    private bool CanDiscard()
    {
        if (!_dirty) return true;
        var answer = MessageBox.Show(this, T("Zapisać zmiany kalibracji?", "Save calibration changes?"), Text,
            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
        return answer == DialogResult.No || answer == DialogResult.Yes && Save();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == (Keys.Control | Keys.S)) { Save(); return true; }
        if (keyData == (Keys.Control | Keys.O)) { Open(); return true; }
        if (keyData == (Keys.Alt | Keys.U)) { Play(); return true; }
        if (keyData == Keys.Escape) { _fork.Stop(); return true; }
        return base.ProcessCmdKey(ref msg, keyData);
    }
}
