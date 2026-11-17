namespace Stroik;

internal sealed partial class TunerForm
{
    private volatile bool _waitingForResultSpeech;
    private int _resultSpeechEpoch;
    private bool ConfirmedNavigationEnabled => _consonanceKind == Consonance.Off &&
        (_activeMethod == TuningMethod.Electronic ? _doubleStrikeEnabled :
         _activeMethod is TuningMethod.HybridSpeakers or TuningMethod.HybridHeadphones && _forkTapEnabled);

    // Arm navigation only once the user has heard the result. The microphone
    // must not interpret the synthesizer or the reference as navigation strikes.
    private async void ReadResult(string text, int inTuneMidi = -1, double frequency = 0, double tolerance = 5)
    {
        int epoch = ++_resultSpeechEpoch;
        int generation = Volatile.Read(ref _measurementGeneration);
        _waitingForResultSpeech = true;
        bool speakers = _activeMethod == TuningMethod.HybridSpeakers;
        bool hybrid = speakers || _activeMethod == TuningMethod.HybridHeadphones;
        if (hybrid) _fork.Stop();
        if (speakers) { _acceptAudio = false; _practice.Stop(); }
        if (!_visualMode.Checked || _voiceInVisual.Checked)
        {
            if (NvdaReader.Available) await NvdaReader.ReadAsync(text, Locale);
            else if (_speech.Speak(text, Locale))
            {
                var deadline = DateTime.UtcNow.AddSeconds(120);
                while (_speech.IsSpeaking && DateTime.UtcNow < deadline && epoch == _resultSpeechEpoch && !_closing)
                    await Task.Delay(30);
            }
        }
        if (epoch != _resultSpeechEpoch || generation != Volatile.Read(ref _measurementGeneration) || _closing || IsDisposed) return;
        lock (_navigationLock)
        {
            if (inTuneMidi >= 0 && ConfirmedNavigationEnabled)
                _strikeNavigator.Arm(inTuneMidi, frequency, DateTime.UtcNow, tolerance);
            else _strikeNavigator.Reset();
        }
        _ignoreUntil = DateTime.UtcNow;
        _waitingForResultSpeech = false;
        if (speakers && _microphone is not null)
        {
            _practice.Result(DateTime.UtcNow, (int)_hybridPause.Value);
            if (_hybridPause.Value == 0) Strike();
        }
        else if (hybrid && _microphone is not null) Strike();
    }

    private void ShowLicenses()
    {
        string body = "";
        foreach (string name in new[] { "THIRD_PARTY_NOTICES.md", "NVDA-LGPL-2.1.txt" })
        {
            using var stream = typeof(TunerForm).Assembly.GetManifestResourceStream("Stroik." + name)!;
            using var reader = new StreamReader(stream); body += reader.ReadToEnd() + "\n\n";
        }
        using var dialog = new Form { Text = T("Licencje składników", "Third-party licenses"), Size = new Size(680, 540), StartPosition = FormStartPosition.CenterParent };
        var text = new TextBox { Text = body, ReadOnly = true, Multiline = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill, AccessibleName = dialog.Text };
        var close = new Button { Text = T("Zamknij", "Close"), DialogResult = DialogResult.Cancel, Dock = DockStyle.Bottom, Height = 44 };
        dialog.Controls.Add(text); dialog.Controls.Add(close); dialog.CancelButton = close; dialog.ShowDialog(this);
    }

    private void UpdateMethodHelp()
    {
        _methodHelp.Text = _activeMethod switch
        {
            TuningMethod.TuningFork => T("Kamerton: wzorzec do strojenia na słuch. W trybie ręcznym Alt+U powtarza dźwięk, a automat odtwarza go cyklicznie. Pole dwóch stuknięć samodzielnie włącza i wyłącza nasłuch mikrofonu.", "Tuning fork: a reference for tuning by ear. In manual mode Alt+U repeats the note; automatic mode repeats it at intervals. The double-strike checkbox starts and stops microphone listening."),
            TuningMethod.Electronic => T("Stroik elektroniczny: pomiar mikrofonem bez wzorca. Graj jedną nutę i pozwól jej ucichnąć; wynik podaje kierunek i odchyłkę. Przydatny do samodzielnego strojenia i kontroli wysokości.", "Electronic tuner: microphone measurement without a reference tone. Play one note and let it stop; the result gives direction and deviation. Useful for independent tuning and pitch checks."),
            TuningMethod.HybridSpeakers => T("Hybryda z głośnikami: nauka strojenia na słuch bez słuchawek. Kamerton i mikrofon działają na przemian. Po wyniku program czeka na zakończenie mowy i wybraną przerwę. Najpierw wysłuchaj wskazówek.", "Speaker hybrid: learn tuning by ear without headphones. The reference and microphone alternate. After a result, the app waits for speech and the chosen pause. Listen to guidance first."),
            _ => T("Hybryda ze słuchawkami: słyszysz wzorzec, a mikrofon jednocześnie mierzy instrument. Opcjonalny odsłuch pomaga słyszeć instrument w słuchawkach. Załóż słuchawki przed jego włączeniem, aby uniknąć sprzężenia.", "Headphone hybrid: hear the reference while the microphone measures the instrument. Optional monitoring helps you hear the instrument in headphones. Put headphones on before enabling it to prevent feedback.")
        };
    }

    private async void ReadGuidanceBeforeFork()
    {
        int epoch = ++_guidanceEpoch;
        string text = _methodHelp.Text + "\n\n" + _beginnerHint.Text;
        // Native focus events reach the reader after the button handler returns.
        // Let them settle before submitting guidance, then wait for actual completion.
        await Task.Delay(300);
        if (epoch != _guidanceEpoch || _closing || IsDisposed || _microphone is null) return;
        bool finished = false;
        if (NvdaReader.Available) finished = await NvdaReader.ReadAsync(text, Locale);
        else if (!ScreenReaderDetector.IsActive() && _speech.Speak(text, Locale))
        {
            var deadline = DateTime.UtcNow.AddSeconds(120);
            while (_speech.IsSpeaking && DateTime.UtcNow < deadline && epoch == _guidanceEpoch && !_closing) await Task.Delay(100);
            finished = !_speech.IsSpeaking;
        }
        if (epoch != _guidanceEpoch || _closing || IsDisposed || _microphone is null) return;
        if (!finished)
        {
            // Readers without a completion API keep the reference silent until the user is ready.
            using var dialog = new Form { Text = T("Wskazówki przed strojeniem", "Guidance before tuning"), Size = new Size(660, 440), StartPosition = FormStartPosition.CenterParent };
            var body = new TextBox { ReadOnly = true, Multiline = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill, Text = text };
            var start = new Button { Text = T("Rozpocznij po wysłuchaniu wskazówek", "Start after listening to guidance"), Dock = DockStyle.Bottom, Height = 60, DialogResult = DialogResult.OK };
            dialog.Controls.Add(body); dialog.Controls.Add(start); dialog.AcceptButton = start;
            if (dialog.ShowDialog(this) != DialogResult.OK) { StopMicrophone(false); return; }
        }
        if (epoch != _guidanceEpoch || _microphone is null) return;
        _waitingForGuidance = false; Strike();
    }
}
