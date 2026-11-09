namespace Stroik;

// Used on the UI thread by startup, timer and manual checks.
internal sealed class UpdateCheckState
{
    internal const int IntervalMilliseconds = 5 * 60 * 1000;
    private bool _running;
    private Version? _lastPrompted;

    internal bool TryBegin()
    {
        if (_running) return false;
        _running = true;
        return true;
    }

    internal void End() => _running = false;

    internal bool ShouldPrompt(Version version, bool quiet) => !quiet || _lastPrompted != version;
    internal void MarkPrompted(Version version) => _lastPrompted = version;
}
