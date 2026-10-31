using Stroik;

foreach (double expected in new[] { 65.406, 110.0, 261.626, 440.0, 523.251, 880.0, 1174.659 })
{
    short[] samples = Tone(expected, 48000, 8192);
    var estimate = PitchDetector.Estimate(samples, 48000);
    if (estimate is null || Math.Abs(estimate.Value.Frequency - expected) > 0.6 || estimate.Value.Confidence < 0.75)
        throw new Exception($"Zły pomiar {expected} Hz: {estimate}");
    Console.WriteLine($"{expected:F3} Hz -> {estimate.Value.Frequency:F3} Hz; pewność {estimate.Value.Confidence:F3}");
}

if (PitchDetector.Estimate(new short[8192], 48000) is not null)
    throw new Exception("Cisza nie może mieć wysokości dźwięku.");

Note a = PitchDetector.Nearest(440, 440);
if (a.Name != "A4" || Math.Abs(a.Frequency - 440) > 0.001)
    throw new Exception("Błędne przypisanie nuty A4.");

var session = new NoteSession();
DateTime start = DateTime.UtcNow;
session.Observe(true, 439.5, start);
session.Observe(true, 439.6, start.AddMilliseconds(100));
session.Observe(true, 439.4, start.AddMilliseconds(200));
if (session.Observe(false, null, start.AddMilliseconds(500)) is not null)
    throw new Exception("Wynik zgłoszono przed 450 ms ciszy.");
double? completed = session.Observe(false, null, start.AddMilliseconds(700));
if (completed is null || Math.Abs(completed.Value - 439.5) > 0.01)
    throw new Exception("Brak końcowego wyniku nuty.");
if (session.Observe(false, null, start.AddMilliseconds(800)) is not null)
    throw new Exception("Wynik tej samej nuty ogłoszono drugi raz.");

string hash = new string('a', 64);
if (Updater.ParseChecksum($"{hash}  Stroik.exe\n") != hash)
    throw new Exception("Błędny odczyt sumy SHA-256.");
try
{
    Updater.ParseChecksum($"{hash}  Other.exe\n");
    throw new Exception("Zaakceptowano sumę dla innego pliku.");
}
catch (InvalidDataException) { }

string releaseJson = System.Text.Json.JsonSerializer.Serialize(new
{
    tag_name = "v1.1.0",
    draft = false,
    prerelease = false,
    assets = new[]
    {
        new { name = "Stroik.exe", browser_download_url = "https://github.com/proszczok2002/Stroik/releases/download/v1.1.0/Stroik.exe" },
        new { name = "Stroik.exe.sha256", browser_download_url = "https://github.com/proszczok2002/Stroik/releases/download/v1.1.0/Stroik.exe.sha256" }
    }
});
if (Updater.ParseRelease(releaseJson, new Version(1, 0, 0, 0))?.Version != new Version(1, 1, 0))
    throw new Exception("Nie wykryto nowszego wydania.");
if (Updater.ParseRelease(releaseJson, new Version(1, 1, 0, 0)) is not null)
    throw new Exception("Zaproponowano tę samą wersję jako aktualizację.");
try
{
    Updater.ParseRelease(releaseJson.Replace("https://github.com/", "https://example.com/"), new Version(1, 0, 0, 0));
    throw new Exception("Zaakceptowano pliki poza GitHubem projektu.");
}
catch (InvalidDataException) { }

foreach (double expected in new[] { 27.5, 41.203, 2093.005, 4186.009 })
{
    var estimate = PitchDetector.Estimate(Tone(expected, 48000, 8192), 48000);
    if (estimate is null || Math.Abs(estimate.Value.Frequency - expected) > Math.Max(0.7, expected * 0.006) ||
        estimate.Value.Confidence < 0.75)
        throw new Exception($"Błędny pomiar skrajnej oktawy {expected} Hz: {estimate}");
}
if (NoteCatalog.Label(67, false) != "g w oktawie razkreślnej" ||
    NoteCatalog.Label(67, true) != "G, one-line octave" ||
    NoteCatalog.Octave(71) != 4 || NoteCatalog.Octave(72) != 5 ||
    NoteCatalog.NextSharp(60) != 61 || NoteCatalog.NextSharp(61) != 63)
    throw new Exception("Błędna nawigacja lub tradycyjna nazwa dźwięku.");
string profile = Path.Combine(Path.GetTempPath(), "stroik-test-" + Guid.NewGuid().ToString("N") + ".json");
try
{
    var original = new Preferences { Method = TuningMethod.Electronic, ConcertA = 435,
        TargetMidi = 67, View = TunerView.Classic, DarkTheme = true, AutoStrike = true };
    Preferences.SaveToFile(profile, original);
    var loaded = Preferences.ReadFile(profile);
    if (loaded.Method != original.Method || loaded.ConcertA != 435 ||
        loaded.View != original.View || !loaded.DarkTheme || !loaded.AutoStrike)
        throw new Exception("Profil nie zachował ustawień.");
    File.WriteAllText(profile, "{\"ConcertA\":999}");
    try { Preferences.ReadFile(profile); throw new Exception("Zaakceptowano niepoprawny profil."); }
    catch (InvalidDataException) { }
}
finally { File.Delete(profile); }
using (var wave = TuningFork.BuildWave(440))
{
    if (wave.Length != 44 + 44100 * 3 * 2)
        throw new Exception("Kamerton wygenerował niepoprawny plik WAV.");
    byte[] header = new byte[4];
    wave.ReadExactly(header);
    if (System.Text.Encoding.ASCII.GetString(header) != "RIFF")
        throw new Exception("Kamerton nie wygenerował pliku RIFF.");
}
Exception? uiError = null;
var uiThread = new Thread(() =>
{
    try
    {
        using var form = new TunerForm();
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var method = (System.Windows.Forms.ComboBox)typeof(TunerForm).GetField("_method", flags)!.GetValue(form)!;
        var target = (System.Windows.Forms.ComboBox)typeof(TunerForm).GetField("_target", flags)!.GetValue(form)!;
        var octave = (System.Windows.Forms.ComboBox)typeof(TunerForm).GetField("_octave", flags)!.GetValue(form)!;
        var view = (System.Windows.Forms.ComboBox)typeof(TunerForm).GetField("_view", flags)!.GetValue(form)!;
        var tabs = (System.Windows.Forms.TabControl)typeof(TunerForm).GetField("_tabs", flags)!.GetValue(form)!;
        var settingsTab = (System.Windows.Forms.TabPage)typeof(TunerForm).GetField("_settingsTab", flags)!.GetValue(form)!;
        var language = (System.Windows.Forms.ComboBox)typeof(TunerForm).GetField("_language", flags)!.GetValue(form)!;
        var dark = (System.Windows.Forms.CheckBox)typeof(TunerForm).GetField("_dark", flags)!.GetValue(form)!;
        var updates = (System.Windows.Forms.Button)typeof(TunerForm).GetField("_updates", flags)!.GetValue(form)!;
        if (method.Items.Count != 2 || target.Items.Count != NoteCatalog.HighestMidi - NoteCatalog.LowestMidi + 2 ||
            octave.Items.Count != 9 || view.Items.Count != 2 || string.IsNullOrWhiteSpace(target.AccessibleName))
            throw new Exception("Interfejs nie udostępnia wszystkich metod, oktaw lub opisów dostępności.");
        if (tabs.TabPages.Count != 2 || tabs.SelectedIndex != 0 ||
            language.Parent is null || language.Parent.Parent != settingsTab ||
            dark.Parent != language.Parent || updates.Parent != language.Parent ||
            tabs.DrawMode != System.Windows.Forms.TabDrawMode.OwnerDrawFixed)
            throw new Exception($"Zakładki: count={tabs.TabPages.Count}, selected={tabs.SelectedIndex}, parent={language.Parent?.GetType().Name}, tabParent={language.Parent?.Parent?.GetType().Name}, lang={language.Parent?.Parent == settingsTab}, dark={dark.Parent == language.Parent}, updates={updates.Parent == language.Parent}, draw={tabs.DrawMode}");
        var englishField = typeof(TunerForm).GetField("_english", flags)!;
        var applyLanguage = typeof(TunerForm).GetMethod("ApplyLanguage", flags)!;
        bool originalEnglish = (bool)englishField.GetValue(form)!;
        foreach (bool english in new[] { false, true })
        {
            englishField.SetValue(form, english);
            applyLanguage.Invoke(form, null);
            if (tabs.TabPages[0].Text != (english ? "Tuner" : "Stroik") ||
                settingsTab.Text != (english ? "Settings" : "Ustawienia") ||
                settingsTab.AccessibleName != settingsTab.Text ||
                string.IsNullOrWhiteSpace(language.AccessibleName) ||
                string.IsNullOrWhiteSpace(updates.AccessibleName))
                throw new Exception("Zakładki lub ich etykiety dostępności nie przełączają się PL/EN.");
        }
        englishField.SetValue(form, originalEnglish);
        applyLanguage.Invoke(form, null);
    }
    catch (Exception error) { uiError = error; }
});
uiThread.SetApartmentState(ApartmentState.STA);
uiThread.Start();
uiThread.Join();
if (uiError is not null) throw new Exception("Kontrola interfejsu nie powiodła się.", uiError);
Console.WriteLine("Wszystkie kontrole pomiaru zakończone poprawnie.");

static short[] Tone(double frequency, int sampleRate, int length)
{
    var samples = new short[length];
    for (int i = 0; i < length; i++)
    {
        double t = (double)i / sampleRate;
        double value = 0.45 * Math.Sin(2 * Math.PI * frequency * t)
                     + 0.15 * Math.Sin(4 * Math.PI * frequency * t);
        samples[i] = (short)Math.Round(value * short.MaxValue);
    }
    return samples;
}
