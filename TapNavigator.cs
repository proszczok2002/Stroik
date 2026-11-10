namespace Stroik;

// Explicitly enabled acoustic gesture: two separate attacks, independent of pitch.
internal sealed class TapNavigator
{
    private double _envelope;
    private DateTime _lastAttack, _first;
    private bool _sound;
    internal void Reset() { _envelope = 0; _first = _lastAttack = DateTime.MinValue; _sound = false; }
    internal bool Observe(double rms, double threshold, DateTime now, int gap)
    {
        bool sound = rms >= threshold * 1.1;
        bool attack = sound && (!_sound || (_envelope > 0 && rms > _envelope * 1.7)) && now - _lastAttack >= TimeSpan.FromMilliseconds(180);
        _envelope = _envelope == 0 ? rms : _envelope * 0.8 + rms * 0.2;
        _sound = sound;
        if (!attack) return false;
        _lastAttack = now;
        if (_first != DateTime.MinValue && (now - _first).TotalMilliseconds <= gap) { Reset(); return true; }
        _first = now; return false;
    }
}
