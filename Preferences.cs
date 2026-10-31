using System.Text.Json;

namespace Stroik;

internal enum TuningMethod { TuningFork, Electronic }
internal enum TunerView { Classic, Beginner }

internal sealed class Preferences
{
    public int ProfileVersion { get; set; } = 1;
    public bool English { get; set; } = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName != "pl";
    public bool DarkTheme { get; set; }
    public TuningMethod Method { get; set; } = TuningMethod.TuningFork;
    public TunerView View { get; set; } = TunerView.Beginner;
    public double ConcertA { get; set; } = 440;
    public int SensitivityIndex { get; set; } = 1;
    public int ToleranceCents { get; set; } = 5;
    public int TargetMidi { get; set; } = NoteCatalog.DefaultMidi;
    public int StartOctave { get; set; } = 4;
    public bool ShowOctaveInClassic { get; set; }
    public bool AutoStrike { get; set; }
    public int StrikeIntervalSeconds { get; set; } = 3;

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Stroik", "settings.json");

    internal static Preferences Load()
    {
        try { return File.Exists(FilePath) ? ReadFile(FilePath) : new Preferences(); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException or InvalidDataException)
        { return new Preferences(); }
    }

    internal static Preferences ReadFile(string path)
    {
        var settings = JsonSerializer.Deserialize<Preferences>(File.ReadAllText(path))
            ?? throw new InvalidDataException("The settings file is empty.");
        settings.Validate();
        return settings;
    }

    internal void Validate()
    {
        if (ProfileVersion != 1 || !Enum.IsDefined(Method) || !Enum.IsDefined(View) ||
            !double.IsFinite(ConcertA) || ConcertA is < 390 or > 466 ||
            SensitivityIndex is < 0 or > 2 || ToleranceCents is < 1 or > 20 ||
            TargetMidi is < -1 or > NoteCatalog.HighestMidi ||
            (TargetMidi >= 0 && TargetMidi < NoteCatalog.LowestMidi) ||
            StartOctave is < 0 or > 8 || StrikeIntervalSeconds is < 1 or > 10)
            throw new InvalidDataException("Invalid Stroik settings file.");
    }

    internal static void Save(Preferences settings)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            SaveToFile(FilePath, settings);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException) { }
    }

    internal static void SaveToFile(string path, Preferences settings)
    {
        settings.Validate();
        File.WriteAllText(path, JsonSerializer.Serialize(settings, JsonOptions));
    }
}
