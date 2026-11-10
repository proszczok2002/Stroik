namespace Stroik;

// Speaker practice alternates playback, listening and a result/speech pause.
internal sealed class PracticeCycle
{
    private DateTime _until;
    private int _phase; // 0 stopped, 1 fork, 2 listening, 3 result
    private readonly object _sync = new();
    internal void Stop() { lock (_sync) _phase = 0; }
    internal void Fork(DateTime now) { lock (_sync) { _phase = 1; _until = now.AddMilliseconds(3400); } }
    internal bool Listening(DateTime now)
    {
        lock (_sync)
        {
        if (_phase == 1 && now >= _until) _phase = 2;
        return _phase == 2;
        }
    }
    internal void Result(DateTime now) { lock (_sync) { _phase = 3; _until = now.AddSeconds(5); } }
    internal bool NeedsFork(DateTime now) { lock (_sync) return _phase == 3 && now >= _until; }
}
