using System.Text.Json;

namespace Stroik;

internal enum TuningMethod { TuningFork, Electronic, HybridSpeakers, HybridHeadphones }
internal enum Consonance { Off, Fifth, Octave, PeriodicFifth }
internal enum ResultUnits { Cents, Hertz, CentsThenHertz, HertzThenCents }
internal enum TunerView { Classic, Beginner }

internal sealed class Preferences
{
    public int ProfileVersion { get; set; } = 1;
    public bool English { get; set; } = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName != "pl";
    public string? Language { get; set; }
    public bool DarkTheme { get; set; }
    public TuningMethod Method { get; set; } = TuningMethod.HybridSpeakers;
    public TunerView View { get; set; } = TunerView.Beginner;
    public InstrumentKind Instrument { get; set; } = InstrumentKind.Piano;
    public UkuleleTuning UkuleleTuning { get; set; } = UkuleleTuning.Standard;
    public PianoOrder PianoOrder { get; set; } = PianoOrder.Ascending;
    public int PlanAnchorMidi { get; set; } = NoteCatalog.LowestMidi;
    public bool DoubleStrikeAdvance { get; set; } = true;
    public int DoubleStrikeWindowMilliseconds { get; set; } = 1200;
    public decimal MicrophoneCorrectionCents { get; set; }
    public bool? VisualMode { get; set; }
    public bool VoiceInVisualMode { get; set; } = true;
    public double ConcertA { get; set; } = 440;
    public int SensitivityIndex { get; set; } = 1;
    public int ToleranceCents { get; set; } = 5;
    public int TargetMidi { get; set; } = NoteCatalog.LowestMidi;
    public int StartOctave { get; set; } = 4;
    public bool ShowOctaveInClassic { get; set; }
    public bool AutoStrike { get; set; }
    public bool AutomaticUpdates { get; set; } = true;
    public bool CheckCalibratorOnStartup { get; set; } = true;
    public bool CalibratorInstallOffered { get; set; }
    public string? CalibratorUpdateOffered { get; set; }
    public int UpdateMinutes { get; set; } = 5;
    public Consonance Consonance { get; set; }
    public bool SimultaneousInterval { get; set; }
    public bool ForkTapAdvance { get; set; }
    public bool MicrophoneMonitor { get; set; }
    public int StrikeIntervalSeconds { get; set; } = 3;
    public int HybridPauseSeconds { get; set; }
    public int ForkVolume { get; set; } = 100;
    public int MonitorVolume { get; set; } = 100;
    public ResultUnits ResultUnits { get; set; } = ResultUnits.Hertz;
    public bool UseInstrumentMap { get; set; } = true;
    public InstrumentMap? InstrumentMap { get; set; }
    public ForkCalibration? ForkCalibration { get; set; }

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    internal static Preferences ReadFile(string path) => Parse(File.ReadAllText(path));

    internal static Preferences Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        var settings = document.RootElement.Deserialize<Preferences>()
            ?? throw new InvalidDataException("The settings file is empty.");
        // Settings from before the new defaults can omit these fields.
        if (!document.RootElement.TryGetProperty(nameof(Method), out _)) settings.Method = TuningMethod.TuningFork;
        if (!document.RootElement.TryGetProperty(nameof(TargetMidi), out _)) settings.TargetMidi = NoteCatalog.DefaultMidi;
        if (!document.RootElement.TryGetProperty(nameof(PlanAnchorMidi), out _)) settings.PlanAnchorMidi = NoteCatalog.DefaultMidi;
        settings.Validate();
        return settings;
    }

    internal void Validate()
    {
        InstrumentMap?.Validate();
        ForkCalibration?.Validate();
        if (ForkVolume is < 0 or > 100 || MonitorVolume is < 0 or > 100) throw new InvalidDataException("Invalid volume");
        if ((!string.IsNullOrEmpty(Language) && !UiLanguages.Codes.Contains(Language)) || ProfileVersion != 1 || !Enum.IsDefined(Method) || !Enum.IsDefined(View) || !Enum.IsDefined(Instrument) || !Enum.IsDefined(UkuleleTuning) || !Enum.IsDefined(PianoOrder) || !Enum.IsDefined(Consonance) || !Enum.IsDefined(ResultUnits) || UpdateMinutes is < 1 or > 1440 ||
            PlanAnchorMidi is < NoteCatalog.LowestMidi or > NoteCatalog.HighestMidi ||
            MicrophoneCorrectionCents is < -100 or > 100 ||
            DoubleStrikeWindowMilliseconds is < 500 or > 2500 ||
            !double.IsFinite(ConcertA) || ConcertA is < 390 or > 466 ||
            SensitivityIndex is < 0 or > 2 || ToleranceCents is < 1 or > 20 ||
            TargetMidi is < -1 or > NoteCatalog.HighestMidi ||
            (TargetMidi >= 0 && TargetMidi < NoteCatalog.LowestMidi) ||
            StartOctave is < 0 or > 8 || StrikeIntervalSeconds is < 1 or > 10 || HybridPauseSeconds is < 0 or > 60)
            throw new InvalidDataException("Invalid Stroik settings file.");
    }

    internal static void SaveToFile(string path, Preferences settings) => File.WriteAllText(path, settings.Serialize());

    internal string Serialize()
    {
        Validate();
        return JsonSerializer.Serialize(this, JsonOptions);
    }
}
