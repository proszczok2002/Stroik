namespace Stroik;

// Explicitly enabled acoustic gesture: two separate attacks, independent of pitch.
internal sealed class TapNavigator
{
    internal static double TransientLevel(ReadOnlySpan<short> samples)
    {
        double level = 0;
        for (int i = 0; i < samples.Length; i += 192)
            level = Math.Max(level, PitchDetector.Rms(samples.Slice(i, Math.Min(192, samples.Length - i))));
        return level;
    }

    private double _envelope;
    private DateTime _lastAttack, _first;
    private bool _sound, _ready;
    private DateTime _quietSince;
    private int _quietBlocks, _attackBlocks;
    private bool _impulse;
    private DateTime _firstImpulse;

    internal void Reset()
    {
        _envelope = 0;
        _first = _lastAttack = _quietSince = _firstImpulse = DateTime.MinValue;
        _sound = _ready = _impulse = false;
        _quietBlocks = _attackBlocks = 0;
    }

    // Confirm a short transient followed by silence before counting it as a tap.
    internal bool ObserveImpulses(ReadOnlySpan<short> samples, int rate, double threshold, DateTime end, int gap)
    {
        int block = rate / 250;
        for (int i = 0; i + block <= samples.Length; i += block)
        {
            var now = end.AddSeconds((i + block - samples.Length) / (double)rate);
            bool active = PitchDetector.Rms(samples.Slice(i, block)) >= threshold;
            if (active)
            {
                if (_quietBlocks >= 5)
                {
                    _impulse = _quietBlocks >= 18;
                    _attackBlocks = 0;
                    if (!_impulse) _firstImpulse = DateTime.MinValue;
                }
                _quietBlocks = 0;
                if (++_attackBlocks > 9)
                {
                    _impulse = false;
                    _firstImpulse = DateTime.MinValue;
                }
            }
            else if (++_quietBlocks == 5 && _impulse)
            {
                _impulse = false;
                double elapsed = (now - _firstImpulse).TotalMilliseconds;
                if (elapsed >= 180 && elapsed <= gap) { Reset(); return true; }
                if (elapsed >= 180) _firstImpulse = now;
            }
        }
        return false;
    }
    internal bool Observe(double rms, double threshold, DateTime now, int gap)
    {
        if (!_ready)
        {
            if (rms < threshold * .8)
            {
                if (_quietSince == DateTime.MinValue) _quietSince = now;
                if (now - _quietSince >= TimeSpan.FromMilliseconds(150)) _ready = true;
            }
            else _quietSince = DateTime.MinValue;
            return false;
        }
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
