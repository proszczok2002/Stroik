namespace Stroik;

// Recognizes two separate, pitch-confirmed attacks after a completed in-tune note.
// The audio thread owns this state; the UI only receives a completed navigation event.
internal sealed class DoubleStrikeNavigator
{
    private int _targetMidi = -1;
    private double _targetFrequency, _envelope;
    private DateTime _armedAt, _lastAttack, _firstConfirmed, _quietSince, _candidateAt;
    private bool _sound, _candidate;
    private int _confirmed;

    internal void Reset()
    {
        _targetMidi = -1;
        _targetFrequency = 0;
        _envelope = 0;
        _sound = false;
        _candidate = false;
        _confirmed = 0;
        _quietSince = DateTime.MinValue;
    }

    internal void Arm(int midi, double frequency, DateTime now)
    {
        Reset();
        _targetMidi = midi;
        _targetFrequency = frequency;
        _armedAt = now;
        _quietSince = now;
    }

    internal bool IsCounting => _confirmed > 0;
    internal bool IsAwaitingPitch => _candidate;

    internal int? Observe(double rms, double threshold, double? frequency, DateTime now)
    {
        if (_targetMidi < 0) return null;
        if (now - _armedAt > TimeSpan.FromSeconds(20)) { Reset(); return null; }

        bool sound = rms >= threshold;
        bool onset = sound && rms >= threshold * 1.5 &&
            ((!_sound && now - _quietSince >= TimeSpan.FromMilliseconds(50)) ||
             (_sound && _envelope > 0 && rms > _envelope * 1.8 &&
              now - _lastAttack >= TimeSpan.FromMilliseconds(130)));
        if (onset)
        {
            _lastAttack = now;
            _candidateAt = now;
            _candidate = true;
        }
        if (!sound && _sound) _quietSince = now;
        _sound = sound;
        _envelope = _envelope == 0 ? rms : rms > _envelope
            ? _envelope * 0.75 + rms * 0.25
            : _envelope * 0.96 + rms * 0.04;

        if (_candidate && now - _candidateAt > TimeSpan.FromMilliseconds(420)) _candidate = false;
        if (_confirmed == 1 && now - _firstConfirmed > TimeSpan.FromMilliseconds(900)) _confirmed = 0;
        if (!_candidate || frequency is not > 0) return null;
        double cents = 1200 * Math.Log2(frequency.Value / _targetFrequency);
        if (Math.Abs(cents) > 35) return null;
        _candidate = false;
        if (_confirmed == 0)
        {
            _confirmed = 1;
            _firstConfirmed = now;
            return null;
        }
        if (now - _firstConfirmed < TimeSpan.FromMilliseconds(130)) return null;
        int midi = _targetMidi;
        Reset();
        return midi;
    }
}