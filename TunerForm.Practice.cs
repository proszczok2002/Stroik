using System.Globalization;

namespace Stroik;

internal sealed partial class TunerForm
{
    private void ConfigureUpdateTimer()
    {
        _updateTimer.Stop();
        _updateTimer.Interval = (int)_updateMinutes.Value * 60000;
        _updateMinutes.Enabled = _automaticUpdates.Checked;
        if (!_initializing && Visible && !_closing && _automaticUpdates.Checked) _updateTimer.Start();
    }
    private void UpdateIntervalHelp()
    {
        _forkHelp.AccessibleDescription = T("W trybie hybrydowym głośnikowym po wzorcu czekaj na jego wybrzmienie. Odsłuch mikrofonu włączaj tylko w słuchawkach. Przy nasłuchiwaniu dwóch stuków automatyczny kamerton ma co najmniej 6,5 sekundy odstępu. Stuki mogą też wykryć dwa przypadkowe hałasy.", "In speaker hybrid mode, wait for the reference to decay. Enable microphone monitoring only with headphones. When listening for two taps, automatic fork strikes are at least 6.5 seconds apart. Two accidental noises can also trigger navigation.");
        if (_consonanceKind == Consonance.Off) { _consonanceHelp.Text = ""; return; }
        int lower = _targetMidi < 0 ? InstrumentCatalog.DefaultMidi(_instrumentKind, _ukuleleKind) : _targetMidi;
        int upper = lower + IntervalAnalysis.Semitones(_consonanceKind);
        _consonanceHelp.Text = upper > NoteCatalog.HighestMidi
            ? T("Górny dźwięk poza zakresem — wybierz niższy dźwięk docelowy.", "Upper note is out of range — choose a lower target.")
            : T($"Od dołu: {NoteCatalog.Label(lower, false)} i {NoteCatalog.Label(upper, false)}. ", $"Bottom up: {NoteCatalog.Label(lower, true)} and {NoteCatalog.Label(upper, true)}. ") +
              (_together ? T("Zagraj razem przez około 2 sekundy. Wynik eksperymentalny: harmoniczna może udawać drugi dźwięk. Sprawdź także osobno.", "Play together for about 2 seconds. Experimental: a harmonic can mimic a second note. Also check separately.")
              : T("Zagraj dolny dźwięk, wycisz, potem górny i wycisz. Dudnienia są szacowane dla idealnych harmonicznych; pianino może wymagać rozciągnięcia oktaw.", "Play lower, mute, then upper and mute. Beats are estimated for ideal harmonics; a piano may need stretched octaves."));
        if (_activeMethod == TuningMethod.TuningFork && upper <= NoteCatalog.HighestMidi)
            _consonanceHelp.Text = T($"Kamerton zagra razem: {NoteCatalog.Label(lower, false)} i {NoteCatalog.Label(upper, false)}. Odstęp równomiernie temperowany.", $"The fork plays together: {NoteCatalog.Label(lower, true)} and {NoteCatalog.Label(upper, true)}. Equal-tempered interval.");
    }
    private void ReceiveIntervalNote(double frequency)
    {
        int lower = _targetMidi < 0 ? InstrumentCatalog.DefaultMidi(_instrumentKind, _ukuleleKind) : _targetMidi;
        int wanted = lower + (_lowerIntervalFrequency is null ? 0 : IntervalAnalysis.Semitones(_consonanceKind));
        if (wanted > NoteCatalog.HighestMidi) { Announce(T("Wybierz niższy cel.", "Choose a lower target.")); return; }
        double expected = PitchDetector.NoteFromMidi(wanted, _reference).Frequency;
        if (Math.Abs(1200 * Math.Log2(frequency / expected)) > 90)
        {
            Announce(T($"Nie ten dźwięk. Zagraj {NoteCatalog.Label(wanted, false)}.", $"Wrong note. Play {NoteCatalog.Label(wanted, true)}."));
            _ignoreUntil = DateTime.UtcNow.AddMilliseconds(1500); return;
        }
        if (_lowerIntervalFrequency is null)
        {
            _lowerIntervalFrequency = frequency;
            _ignoreUntil = DateTime.UtcNow.AddMilliseconds(1500);
            _result.Text = T("Dolny zapisany. Zagraj górny dźwięk i wycisz.", "Lower note stored. Play the upper note and mute.");
            Announce(_result.Text);
        }
        else
        {
            ShowInterval(IntervalAnalysis.Compare(_lowerIntervalFrequency.Value, frequency, _consonanceKind), false);
            _lowerIntervalFrequency = null;
        }
    }
    private void ProcessIntervalTogether(short[] samples, DateTime now, int generation)
    {
        if (PitchDetector.Rms(samples) < _minimumRms) { _pairCount = 0; return; }
        int count = Math.Min(samples.Length, _pairWindow.Length - _pairCount);
        Array.Copy(samples, 0, _pairWindow, _pairCount, count); _pairCount += count;
        if (_pairCount < _pairWindow.Length) return;
        _pairCount = 0;
        if (now - _lastPairAt < TimeSpan.FromSeconds(2)) return;
        _lastPairAt = now;
        int lower = _targetMidi < 0 ? InstrumentCatalog.DefaultMidi(_instrumentKind, _ukuleleKind) : _targetMidi;
        var reading = lower + IntervalAnalysis.Semitones(_consonanceKind) <= NoteCatalog.HighestMidi
            ? IntervalAnalysis.Together(_pairWindow, _microphone?.SampleRate ?? 48000, PitchDetector.NoteFromMidi(lower, _reference).Frequency / _correctionFactor, _consonanceKind) : null;
        try
        {
            BeginInvoke((Action)(() =>
            {
                if (generation != Volatile.Read(ref _measurementGeneration) || _closing) return;
                if (reading is IntervalReading pair) ShowInterval(IntervalAnalysis.Compare(pair.Lower * _correctionFactor, pair.Upper * _correctionFactor, _consonanceKind), true);
                else { _result.Text = T("Brak dwóch wyraźnych składowych. Spróbuj pomiaru osobno.", "Two clear components not found. Try separate measurement."); Announce(_result.Text); }
            }));
        }
        catch (InvalidOperationException) { }
    }
    private void ShowInterval(IntervalReading reading, bool experimental)
    {
        string prefix = experimental ? T("Eksperymentalny pomiar składowych. ", "Experimental component measurement. ") : "";
        var culture = CultureInfo.GetCultureInfo(_english ? "en-US" : "pl-PL");
        string cents = reading.Cents.ToString("+0.0;-0.0;0.0", culture), beats = reading.Beats.ToString("0.00", culture);
        _result.Text = prefix + T($"Odchyłka od stroju równomiernego: {cents} centów. Szacowane dudnienia: {beats} na sekundę.", $"Deviation from equal temperament: {cents} cents. Estimated beats: {beats} per second.");
        _details.Text = $"{reading.Lower.ToString("0.00", culture)} Hz / {reading.Upper.ToString("0.00", culture)} Hz";
        _lastAnnouncement = _result.Text; _repeat.Enabled = true; Announce(_result.Text);
        _ignoreUntil = DateTime.UtcNow.AddSeconds(4);
        if (_activeMethod == TuningMethod.HybridSpeakers && _microphone is not null) { _acceptAudio = false; _practice.Result(DateTime.UtcNow); }
    }
}
