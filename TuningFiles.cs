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

internal readonly record struct ForkTone([property: JsonRequired] int Harmonics, [property: JsonRequired] int Volume)
{
    internal static ForkTone Default => new(100, 100);
}

internal sealed class ForkCalibration
{
    public string Format { get; set; } = "stroik-fork-calibration";
    public int Version { get; set; } = 1;
    public Dictionary<int, ForkTone> Notes { get; set; } = [];
    internal ForkTone Tone(int midi) => Notes.GetValueOrDefault(midi, ForkTone.Default);

    internal void Validate()
    {
        if (Format != "stroik-fork-calibration" || Version != 1 || Notes is null || Notes.Count > 88 ||
            Notes.Any(note => note.Key is < 21 or > 108 || note.Value.Harmonics is < 0 or > 200 || note.Value.Volume is < 0 or > 150))
            throw new InvalidDataException("Nieprawidłowy plik kalibracji / Invalid calibration file");
    }
}

internal static class TuningFiles
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true
    };

    internal static T Read<T>(string file, string expectedFormat)
    {
        using var stream = File.OpenRead(file);
        if (stream.Length > 100_000) throw new InvalidDataException("JSON exceeds 100 KB");
        using var document = JsonDocument.Parse(stream);
        var root = document.RootElement;
        if (!root.TryGetProperty("format", out var format) || format.GetString() != expectedFormat ||
            !root.TryGetProperty("version", out var version) || version.GetInt32() != 1 ||
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
