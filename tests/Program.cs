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
