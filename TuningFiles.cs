using System.Text.Json;
using System.Text.Json.Serialization;

namespace Stroik;

internal sealed class InstrumentMap
{
    public string Format { get; set; } = "stroik-instrument-map";
    public int Version { get; set; } = 1;
    public Dictionary<int, double> Notes { get; set; } = [];

    internal double Frequency(int midi, double reference) => Notes.TryGetValue(midi, out double hz)
        ? hz : PitchDetector.NoteFromMidi(midi, reference).Frequency;

    internal void Validate()
    {
        if (Format != "stroik-instrument-map" || Version != 1 || Notes is null || Notes.Count > 88 ||
            Notes.Any(note => note.Key is < 21 or > 108 || !double.IsFinite(note.Value) || note.Value is < 20 or > 10000))
            throw new InvalidDataException("Nieprawidłowy plik mapingu / Invalid instrument map");
    }
}

internal readonly record struct ForkTone([property: JsonRequired] int Harmonics, [property: JsonRequired] int Volume, int? UpperPartials = null)
{
    internal static ForkTone Default => new(100, 100);
}

internal sealed class ForkCalibration
{
    public string Format { get; set; } = "stroik-fork-calibration";
    public int Version { get; set; } = 1;
    public Dictionary<int, ForkTone> Notes { get; set; } = [];
    private static readonly Lazy<ForkCalibration> Factory = new(() =>
    {
        using var stream = typeof(ForkCalibration).Assembly.GetManifestResourceStream("Stroik.fork-calibration.json")
            ?? throw new InvalidOperationException("Missing factory calibration");
        var calibration = JsonSerializer.Deserialize<ForkCalibration>(stream, TuningFiles.Options)!;
        calibration.Validate();
        return calibration;
    });

    internal static ForkTone Effective(ForkCalibration? custom, int midi)
    {
        ForkTone factory = Factory.Value.Tone(midi);
        if (midi is >= 21 and < 48)
            factory = factory with { UpperPartials = factory.UpperPartials ?? Math.Clamp(50 + (midi - 44) * 12, 50, 100) };
        if (custom is not null && custom.Notes.TryGetValue(midi, out ForkTone tone))
            return tone with { UpperPartials = tone.UpperPartials ?? factory.UpperPartials };
        return factory;
    }

    internal ForkTone Tone(int midi) => Notes.GetValueOrDefault(midi, ForkTone.Default);

    internal void Validate()
    {
        if (Format != "stroik-fork-calibration" || Version is not (1 or 2) || Notes is null || Notes.Count > 88 ||
            Notes.Any(note => note.Key is < 21 or > 108 || note.Value.Harmonics is < 0 or > 200 || note.Value.Volume is < 0 or > 150 || note.Value.UpperPartials is < 0 or > 100))
            throw new InvalidDataException("Nieprawidłowy plik kalibracji / Invalid calibration file");
    }
}

internal static class TuningFiles
{
    internal static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };

    internal static T Read<T>(string file, string expectedFormat)
    {
        using var stream = File.OpenRead(file);
        if (stream.Length > 100_000) throw new InvalidDataException("JSON exceeds 100 KB");
        using var document = JsonDocument.Parse(stream);
        var root = document.RootElement;
        if (!root.TryGetProperty("format", out var format) || format.GetString() != expectedFormat ||
            !root.TryGetProperty("version", out var version) || (version.GetInt32() != 1 && (expectedFormat != "stroik-fork-calibration" || version.GetInt32() != 2)) ||
            !root.TryGetProperty("notes", out var notes) || notes.ValueKind != JsonValueKind.Object)
            throw new InvalidDataException("Invalid Stroik JSON format");
        return root.Deserialize<T>(Options) ?? throw new InvalidDataException("Empty JSON");
    }

    internal static void Write<T>(string path, T value)
    {
        string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temporary, JsonSerializer.Serialize(value, Options));
            File.Move(temporary, path, true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
}
