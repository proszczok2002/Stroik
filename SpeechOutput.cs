using System.Runtime.InteropServices;

namespace Stroik;

// SAPI stays on the UI's STA. Speak is asynchronous; completion is polled without blocking it.
internal sealed class SpeechOutput : IDisposable
{
    private dynamic? _voice;
    internal string? LastError { get; private set; }
    internal bool IsSpeaking
    {
        get { try { return _voice is not null && !(bool)_voice.WaitUntilDone(0); } catch { return false; } }
    }
    internal bool Speak(string text, bool english)
    {
        try
        {
            if (_voice is null)
            {
                Type type = Type.GetTypeFromProgID("SAPI.SpVoice") ?? throw new InvalidOperationException("SAPI voice unavailable");
                _voice = Activator.CreateInstance(type)!;
            }
            dynamic choices = _voice.GetVoices("Language=" + (english ? "409" : "415"), "");
            try { if (choices.Count > 0) _voice.Voice = choices.Item(0); }
            finally { Marshal.ReleaseComObject(choices); }
            _voice.Speak(text, 1 | 2 | 16); // asynchronous, purge previous, plain text
            LastError = null;
            return true;
        }
        catch (Exception error) { LastError = error.Message; return false; }
    }
    internal void Stop() { try { _voice?.Speak("", 1 | 2 | 16); } catch { } }
    public void Dispose()
    {
        Stop();
        if (_voice is not null) { Marshal.ReleaseComObject(_voice); _voice = null; }
    }
}
