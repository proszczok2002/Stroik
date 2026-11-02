using Stroik;
using System.Net;

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

if (TunerForm.DecideForkShortcut(false, false) != ForkShortcutAction.SingleStrike ||
    TunerForm.DecideForkShortcut(false, true) != ForkShortcutAction.SingleStrike ||
    TunerForm.DecideForkShortcut(true, true) != ForkShortcutAction.Pause ||
    TunerForm.DecideForkShortcut(true, false) != ForkShortcutAction.Resume)
    throw new Exception("Alt+U ma rozróżniać ręczne uderzenie od pauzy i wznowienia automatu.");
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

using (var handler = new SequenceHandler(HttpStatusCode.ServiceUnavailable, HttpStatusCode.OK))
using (var testClient = new HttpClient(handler))
using (var recovered = await Updater.GetWithRetryAsync(testClient, new Uri("https://example.com/stroik")))
{
    if (recovered.StatusCode != HttpStatusCode.OK || handler.Calls != 2)
        throw new Exception("Aktualizator nie ponowił chwilowego błędu sieciowego.");
}
using (var handler = new SequenceHandler(HttpStatusCode.NotFound, HttpStatusCode.OK))
using (var testClient = new HttpClient(handler))
{
    try
    {
        using var unexpected = await Updater.GetWithRetryAsync(testClient, new Uri("https://example.com/stroik"));
        throw new Exception("Aktualizator zaakceptował brakujący plik.");
    }
    catch (HttpRequestException) when (handler.Calls == 1) { }
}
string currentExe = Path.Combine(Path.GetTempPath(), "Stroik.exe");
string ownedBackup = currentExe + ".old-" + Guid.NewGuid().ToString("N");
if (!Updater.IsOwnedBackup(currentExe, ownedBackup) ||
    Updater.IsOwnedBackup(currentExe, Path.Combine(Path.GetTempPath(), "Other.exe.old-" + Guid.NewGuid().ToString("N"))))
    throw new Exception("Błędna walidacja starej kopii EXE.");
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
        TargetMidi = 69, Instrument = InstrumentKind.Ukulele, UkuleleTuning = UkuleleTuning.SwappedAG,
        PianoOrder = PianoOrder.AlternateFiveSemitones, PlanAnchorMidi = 67,
        DoubleStrikeAdvance = false, DoubleStrikeWindowMilliseconds = 1900,
        MicrophoneCorrectionCents = 12.5M, VisualMode = false,
        View = TunerView.Classic, DarkTheme = true, AutoStrike = true };
    Preferences.SaveToFile(profile, original);
    var loaded = Preferences.ReadFile(profile);
    if (loaded.Method != original.Method || loaded.ConcertA != 435 ||
        loaded.View != original.View || loaded.Instrument != InstrumentKind.Ukulele ||
        loaded.UkuleleTuning != UkuleleTuning.SwappedAG || loaded.PianoOrder != PianoOrder.AlternateFiveSemitones ||
        loaded.PlanAnchorMidi != 67 || loaded.DoubleStrikeAdvance || loaded.DoubleStrikeWindowMilliseconds != 1900 ||
        loaded.MicrophoneCorrectionCents != 12.5M ||
        loaded.VisualMode != false || !loaded.DarkTheme || !loaded.AutoStrike)
        throw new Exception("Profil nie zachował ustawień.");
    File.WriteAllText(profile, "{\"ConcertA\":999}");
    try { Preferences.ReadFile(profile); throw new Exception("Zaakceptowano niepoprawny profil."); }
    catch (InvalidDataException) { }
}
finally { File.Delete(profile); }
foreach ((InstrumentKind kind, int[] expected) in new (InstrumentKind, int[])[]
{
    (InstrumentKind.Guitar, [40, 45, 50, 55, 59, 64]),
    (InstrumentKind.Ukulele, [67, 60, 64, 69]),
    (InstrumentKind.Violin, [55, 62, 69, 76]),
    (InstrumentKind.Bass, [28, 33, 38, 43])
})
{
    int[] actual = InstrumentCatalog.Strings(kind).Select(s => s.Midi).ToArray();
    if (!actual.SequenceEqual(expected) || InstrumentCatalog.DefaultMidi(kind) != expected[0])
        throw new Exception($"Niepoprawna kolejność lub oktawy strun: {kind}.");
    foreach (int midi in expected)
    {
        double frequency = PitchDetector.NoteFromMidi(midi, 440).Frequency;
        if (InstrumentCatalog.NearestOpenString(kind, frequency, 440) != midi)
            throw new Exception($"Tryb automatyczny nie rozpoznał struny {kind}: {midi}.");
    }
}
if (InstrumentCatalog.HasStrings(InstrumentKind.Piano) || InstrumentCatalog.HasStrings(InstrumentKind.Chromatic))
    throw new Exception("Tryby pełnozakresowe nie powinny ograniczać dźwięków.");
var oldProfile = System.Text.Json.JsonSerializer.Deserialize<Preferences>("{\"ProfileVersion\":1,\"TargetMidi\":67}");
if (oldProfile?.Instrument != InstrumentKind.Piano || oldProfile.DoubleStrikeWindowMilliseconds != 1200)
    throw new Exception("Stary profil bez wyboru instrumentu nie zachował pianina.");
try
{
    new Preferences { Instrument = (InstrumentKind)99 }.Validate();
    throw new Exception("Zaakceptowano nieznany instrument.");
}
catch (InvalidDataException) { }

if (!InstrumentCatalog.Strings(InstrumentKind.Ukulele, UkuleleTuning.SwappedAG)
        .Select(s => s.Midi).SequenceEqual(new[] { 69, 60, 64, 67 }) ||
    InstrumentCatalog.NearestOpenString(InstrumentKind.Ukulele, 392, 440, UkuleleTuning.SwappedAG) != 67)
    throw new Exception("Zamiana G/A w ukulele ma błędne położenie lub wysokość.");
int[] radial = TuningPlan.Build(67, PianoOrder.AlternateFiveSemitones).Take(11).ToArray();
if (!radial.SequenceEqual(new[] { 67, 68, 69, 70, 71, 72, 66, 65, 64, 63, 62 }) ||
    TuningPlan.Next(67, 72, PianoOrder.AlternateFiveSemitones) != 66 ||
    !TuningPlan.Build(67, PianoOrder.AlternateFiveWholeTones).Take(6)
        .SequenceEqual(new[] { 67, 69, 71, 73, 75, 77 }))
    throw new Exception("Plan strojenia nie przechodzi po pięciu dźwiękach na każdą stronę.");
if (!KeyboardGuide.Describe(67, false).Contains("między pierwszym a drugim czarnym", StringComparison.Ordinal) ||
    !KeyboardGuide.Describe(67, true).Contains("between the first and second black", StringComparison.Ordinal))
    throw new Exception("Opis położenia G na klawiaturze jest błędny.");
var navigator = new DoubleStrikeNavigator();
DateTime strikes = DateTime.UtcNow;
navigator.Arm(69, 440, strikes);
if (navigator.Observe(0.02, 0.008, 440, strikes.AddMilliseconds(100)) is not null ||
    navigator.Observe(0, 0.008, null, strikes.AddMilliseconds(220)) is not null ||
    navigator.Observe(0.025, 0.008, 440, strikes.AddMilliseconds(420)) != 69)
    throw new Exception("Dwa szybkie uderzenia nie przeszły do następnego dźwięku.");
navigator.Arm(69, 440, strikes);
navigator.Observe(0.02, 0.008, 466, strikes.AddMilliseconds(100));
navigator.Observe(0, 0.008, null, strikes.AddMilliseconds(220));
if (navigator.Observe(0.025, 0.008, 466, strikes.AddMilliseconds(420)) is not null)
    throw new Exception("Dwa uderzenia o złej wysokości przesunęły cel.");
navigator.Arm(69, 440, strikes);
navigator.Observe(0.025, 0.008, 440, strikes.AddMilliseconds(100), 1200);
navigator.Observe(0, 0.008, null, strikes.AddMilliseconds(300), 1200);
if (navigator.Observe(0.009, 0.008, 440, strikes.AddMilliseconds(1100), 1200) != 69)
    throw new Exception("Cichsze drugie uderzenie nie zostało rozpoznane w ustawionym czasie.");
navigator.Arm(69, 440, strikes);
navigator.Observe(0.025, 0.008, 440, strikes.AddMilliseconds(100), 500);
navigator.Observe(0, 0.008, null, strikes.AddMilliseconds(300), 500);
if (navigator.Observe(0.02, 0.008, 440, strikes.AddMilliseconds(1100), 500) is not null)
    throw new Exception("Zbyt odległe uderzenia zostały uznane za parę.");
navigator.Arm(69, 440, strikes);
navigator.Observe(0.025, 0.008, 440, strikes.AddMilliseconds(100), 1800);
navigator.Observe(0, 0.008, null, strikes.AddMilliseconds(300), 1800);
if (navigator.Observe(0.02, 0.008, 440, strikes.AddMilliseconds(1600), 1800) != 69)
    throw new Exception("Dłuższy ustawiony czas nie pozwala na spokojniejsze dwa uderzenia.");
navigator.Arm(69, 440, strikes);
navigator.Observe(0.025, 0.008, 440, strikes.AddMilliseconds(100), 1800);
if (navigator.Observe(0.018, 0.008, 440, strikes.AddMilliseconds(400), 1800) is not null ||
    navigator.Observe(0.015, 0.008, 440, strikes.AddMilliseconds(700), 1800) is not null)
    throw new Exception("Jedno wybrzmiewające uderzenie przesunęło dźwięk.");
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
        var instrument = (System.Windows.Forms.ComboBox)typeof(TunerForm).GetField("_instrument", flags)!.GetValue(form)!;
        var ukuleleTuning = (System.Windows.Forms.ComboBox)typeof(TunerForm).GetField("_ukuleleTuning", flags)!.GetValue(form)!;
        var pianoOrder = (System.Windows.Forms.ComboBox)typeof(TunerForm).GetField("_pianoOrder", flags)!.GetValue(form)!;
        var visualMode = (System.Windows.Forms.CheckBox)typeof(TunerForm).GetField("_visualMode", flags)!.GetValue(form)!;
        var calibration = (System.Windows.Forms.NumericUpDown)typeof(TunerForm).GetField("_calibration", flags)!.GetValue(form)!;
        var doubleStrikeWindow = (System.Windows.Forms.NumericUpDown)typeof(TunerForm).GetField("_doubleStrikeWindow", flags)!.GetValue(form)!;
        var target = (System.Windows.Forms.ComboBox)typeof(TunerForm).GetField("_target", flags)!.GetValue(form)!;
        var octave = (System.Windows.Forms.ComboBox)typeof(TunerForm).GetField("_octave", flags)!.GetValue(form)!;
        var view = (System.Windows.Forms.ComboBox)typeof(TunerForm).GetField("_view", flags)!.GetValue(form)!;
        var tabs = (System.Windows.Forms.TabControl)typeof(TunerForm).GetField("_tabs", flags)!.GetValue(form)!;
        var settingsTab = (System.Windows.Forms.TabPage)typeof(TunerForm).GetField("_settingsTab", flags)!.GetValue(form)!;
        var language = (System.Windows.Forms.ComboBox)typeof(TunerForm).GetField("_language", flags)!.GetValue(form)!;
        var dark = (System.Windows.Forms.CheckBox)typeof(TunerForm).GetField("_dark", flags)!.GetValue(form)!;
        var updates = (System.Windows.Forms.Button)typeof(TunerForm).GetField("_updates", flags)!.GetValue(form)!;
        if (method.Items.Count != 2 || instrument.Items.Count != 6 ||
            ukuleleTuning.Items.Count != 2 || pianoOrder.Items.Count != 3 ||
            visualMode.Parent != language.Parent || calibration.Parent != language.Parent ||
            doubleStrikeWindow.Minimum != 0.5M || doubleStrikeWindow.Maximum != 2.5M ||
            doubleStrikeWindow.Value < 0.5M || doubleStrikeWindow.Value > 2.5M ||
            string.IsNullOrWhiteSpace(doubleStrikeWindow.AccessibleName) ||
            target.Items.Count != (InstrumentCatalog.HasStrings((InstrumentKind)instrument.SelectedIndex)
                ? InstrumentCatalog.Strings((InstrumentKind)instrument.SelectedIndex).Count + 1
                : NoteCatalog.HighestMidi - NoteCatalog.LowestMidi + 2) ||
            octave.Items.Count != 9 || view.Items.Count != 2 || string.IsNullOrWhiteSpace(target.AccessibleName))
            throw new Exception("Interfejs nie udostępnia wszystkich metod, oktaw lub opisów dostępności.");
        if (tabs.TabPages.Count != 2 || tabs.SelectedIndex != 0 ||
            language.Parent is null || language.Parent.Parent != settingsTab ||
            dark.Parent != language.Parent || updates.Parent != language.Parent ||
            tabs.DrawMode != System.Windows.Forms.TabDrawMode.OwnerDrawFixed)
            throw new Exception($"Zakładki: count={tabs.TabPages.Count}, selected={tabs.SelectedIndex}, parent={language.Parent?.GetType().Name}, tabParent={language.Parent?.Parent?.GetType().Name}, lang={language.Parent?.Parent == settingsTab}, dark={dark.Parent == language.Parent}, updates={updates.Parent == language.Parent}, draw={tabs.DrawMode}");
        var autoStrike = (System.Windows.Forms.CheckBox)typeof(TunerForm).GetField("_autoStrike", flags)!.GetValue(form)!;
        var automaticStatus = (System.Windows.Forms.Label)typeof(TunerForm).GetField("_automaticStatus", flags)!.GetValue(form)!;
        var strikeTimer = (System.Windows.Forms.Timer)typeof(TunerForm).GetField("_strikeTimer", flags)!.GetValue(form)!;
        var processCmdKey = typeof(TunerForm).GetMethod("ProcessCmdKey", flags)!;
        int originalMethod = method.SelectedIndex;
        bool originalAutoStrike = autoStrike.Checked;
        method.SelectedIndex = 0;
        autoStrike.Checked = true;
        if (!strikeTimer.Enabled) throw new Exception("Zaznaczenie automatu nie uruchomiło timera.");
        var shortcutMessage = System.Windows.Forms.Message.Create(IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero);
        object?[] shortcutArgs = [shortcutMessage, System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.U];
        if ((bool)processCmdKey.Invoke(form, shortcutArgs)! != true || !autoStrike.Checked || strikeTimer.Enabled)
            throw new Exception("Alt+U nie wstrzymał automatu bez odznaczenia pola.");
        shortcutArgs = [shortcutMessage, System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.U];
        if ((bool)processCmdKey.Invoke(form, shortcutArgs)! != true || !autoStrike.Checked || !strikeTimer.Enabled)
            throw new Exception("Alt+U nie wznowił automatu.");
        autoStrike.Checked = false;
        if (strikeTimer.Enabled || !automaticStatus.Text.Contains("Alt+U", StringComparison.Ordinal))
            throw new Exception("Tryb ręczny nie zatrzymał automatu lub nie opisuje skrótu Alt+U.");
        autoStrike.Checked = originalAutoStrike;
        method.SelectedIndex = originalMethod;
        var englishField = typeof(TunerForm).GetField("_english", flags)!;
        var applyLanguage = typeof(TunerForm).GetMethod("ApplyLanguage", flags)!;
        var populateChoices = typeof(TunerForm).GetMethod("PopulateChoices", flags)!;
        var readSettings = typeof(TunerForm).GetMethod("ReadSettings", flags)!;
        Preferences originalSettings = (Preferences)readSettings.Invoke(form, null)!;
        foreach (InstrumentKind kind in Enum.GetValues<InstrumentKind>())
        {
            var fixture = new Preferences { Instrument = kind, TargetMidi = InstrumentCatalog.DefaultMidi(kind),
                English = originalSettings.English };
            populateChoices.Invoke(form, [fixture]);
            applyLanguage.Invoke(form, null);
            int expectedCount = InstrumentCatalog.HasStrings(kind) ? InstrumentCatalog.Strings(kind).Count + 1
                : NoteCatalog.HighestMidi - NoteCatalog.LowestMidi + 2;
            if (instrument.SelectedIndex != (int)kind || target.Items.Count != expectedCount ||
                target.SelectedIndex != (InstrumentCatalog.HasStrings(kind) ? 1 : NoteCatalog.DefaultMidi - NoteCatalog.LowestMidi + 1))
                throw new Exception($"Lista dźwięków nie odpowiada instrumentowi {kind}.");
            if (kind == InstrumentKind.Guitar &&
                !target.Items[1]!.ToString()!.Contains("6", StringComparison.Ordinal))
                throw new Exception("Lista gitary nie wskazuje numeru struny.");
        }
        populateChoices.Invoke(form, [new Preferences { Instrument = InstrumentKind.Ukulele,
            UkuleleTuning = UkuleleTuning.SwappedAG, TargetMidi = 69 }]);
        if (ukuleleTuning.SelectedIndex != 1 || target.SelectedIndex != 1 ||
            !target.Items[1]!.ToString()!.Contains("69", StringComparison.Ordinal) &&
            !target.Items[1]!.ToString()!.Contains("a w oktawie", StringComparison.Ordinal) &&
            !target.Items[1]!.ToString()!.Contains("A, one-line", StringComparison.Ordinal))
            throw new Exception("Alternatywny układ ukulele nie pojawia się w interfejsie.");
        populateChoices.Invoke(form, [originalSettings]);
        applyLanguage.Invoke(form, null);
        bool originalEnglish = (bool)englishField.GetValue(form)!;
        foreach (bool english in new[] { false, true })
        {
            englishField.SetValue(form, english);
            populateChoices.Invoke(form, [new Preferences { Instrument = InstrumentKind.Guitar, TargetMidi = 40, English = english }]);
            applyLanguage.Invoke(form, null);
            if (instrument.Items[1]!.ToString() != (english ? "Guitar" : "Gitara") ||
                !target.Items[1]!.ToString()!.StartsWith(english ? "String 6" : "Struna 6", StringComparison.Ordinal) ||
                tabs.TabPages[0].Text != (english ? "Tuner" : "Stroik") ||
                settingsTab.Text != (english ? "Settings" : "Ustawienia") ||
                settingsTab.AccessibleName != settingsTab.Text ||
                string.IsNullOrWhiteSpace(language.AccessibleName) ||
                string.IsNullOrWhiteSpace(instrument.AccessibleName) ||
                string.IsNullOrWhiteSpace(updates.AccessibleName) ||
                automaticStatus.AccessibleName != (english ? "Tuning fork status" : "Stan kamertonu") ||
                !automaticStatus.Text.Contains("Alt+U", StringComparison.Ordinal))
                throw new Exception("Zakładki lub ich etykiety dostępności nie przełączają się PL/EN.");
        }
        englishField.SetValue(form, originalEnglish);
        populateChoices.Invoke(form, [originalSettings]);
        applyLanguage.Invoke(form, null);
    }
    catch (Exception error) { uiError = error; }
});
uiThread.SetApartmentState(ApartmentState.STA);
uiThread.Start();
uiThread.Join();
if (uiError is not null) throw new Exception("Kontrola interfejsu nie powiodła się.", uiError);
if (args.Contains("--github-probe", StringComparer.Ordinal))
{
    ReleaseInfo? release = await Updater.FindNewReleaseAsync();
    if (release is null) throw new Exception("Publiczne API GitHub nie wskazało nowszego wydania.");
    using var checksumResponse = await Updater.GetWithRetryAsync(new HttpClient(), release.Checksum);
    string publicHash = Updater.ParseChecksum(await checksumResponse.Content.ReadAsStringAsync());
    Console.WriteLine($"GitHub updater probe: {release.Version}; EXE SHA-256 {publicHash}");
    if (args.Contains("--download-probe", StringComparer.Ordinal))
    {
        string downloaded = await Updater.DownloadVerifiedAsync(release);
        string verifyDirectory = Path.Combine(Environment.CurrentDirectory, "dist", "verify-updater");
        Directory.CreateDirectory(verifyDirectory);
        string copy = Path.Combine(verifyDirectory, $"Stroik-{release.Version}.exe");
        File.Copy(downloaded, copy, true);
        Directory.Delete(Path.GetDirectoryName(downloaded)!, true);
        using var copiedFile = File.OpenRead(copy);
        Console.WriteLine($"Verified download: {new FileInfo(copy).Length} bytes; {Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(copiedFile))}");
    }
}Console.WriteLine("Wszystkie kontrole pomiaru zakończone poprawnie.");

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

sealed class SequenceHandler(params HttpStatusCode[] statuses) : HttpMessageHandler
{
    internal int Calls { get; private set; }
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        int index = Math.Min(Calls++, statuses.Length - 1);
        return Task.FromResult(new HttpResponseMessage(statuses[index])
        {
            Content = new StringContent("{}"),
            RequestMessage = request
        });
    }
}
