namespace Stroik;

internal static class NoteCatalog
{
    internal const int LowestMidi = 21;  // A0
    internal const int HighestMidi = 108; // C8
    internal const int DefaultMidi = 67; // G4, g razkreślne
    private static readonly string[] PolishPitch = ["c", "cis", "d", "dis", "e", "f", "fis", "g", "gis", "a", "b", "h"];
    private static readonly string[] EnglishPitch = ["C", "C sharp", "D", "D sharp", "E", "F", "F sharp", "G", "G sharp", "A", "B flat", "B"];
    private static readonly string[] PolishOctaves = ["subkontra", "kontra", "wielka", "mała", "razkreślna", "dwukreślna", "trzykreślna", "czterokreślna", "pięciokreślna"];
    private static readonly string[] PolishOctavesIn = ["subkontra", "kontra", "wielkiej", "małej", "razkreślnej", "dwukreślnej", "trzykreślnej", "czterokreślnej", "pięciokreślnej"];
    private static readonly string[] EnglishOctaves = ["sub-contra", "contra", "great", "small", "one-line", "two-line", "three-line", "four-line", "five-line"];

    internal static int Octave(int midi) => midi / 12 - 1;
    internal static bool IsSharp(int midi) => midi % 12 is 1 or 3 or 6 or 8;
    internal static string OctaveLabel(int octave, bool english) =>
        english ? $"{EnglishOctaves[octave]} octave" : $"oktawa {PolishOctaves[octave]}";
    internal static string Label(int midi, bool english)
    {
        int octave = Octave(midi);
        string name = english ? EnglishPitch[midi % 12] : PolishPitch[midi % 12];
        return english ? $"{name}, {EnglishOctaves[octave]} octave" : $"{name} w oktawie {PolishOctavesIn[octave]}";
    }
    internal static int NextSharp(int current, int direction = 1)
    {
        int midi = current;
        for (int i = 0; i < HighestMidi - LowestMidi + 1; i++)
        {
            midi += direction;
            if (midi > HighestMidi) midi = LowestMidi;
            if (midi < LowestMidi) midi = HighestMidi;
            if (IsSharp(midi)) return midi;
        }
        return current;
    }

    internal static string Spoken(int midi, bool english) => english ? Label(midi, true) :
        PolishPitch[midi % 12] + " " + new[] { "subkontra", "kontra", "wielkie", "małe", "razkreślne", "dwukreślne", "trzykreślne", "czterokreślne", "pięciokreślne" }[Octave(midi)];
    internal static string Label(int midi, string language) => language is "pl" or "en" ? Label(midi, language == "en") : Pitch(midi % 12, language) + ", " + OctaveLabel(Octave(midi), language);
    internal static string Spoken(int midi, string language) => language is "pl" or "en" ? Spoken(midi, language == "en") : Label(midi, language);
    internal static string Pitch(int pitch, string language) => language switch
    {
        "de" => new[] { "C", "Cis", "D", "Dis", "E", "F", "Fis", "G", "Gis", "A", "B", "H" }[pitch],
        "ru" => new[] { "до", "до-диез", "ре", "ре-диез", "ми", "фа", "фа-диез", "соль", "соль-диез", "ля", "си-бемоль", "си" }[pitch],
        "fr" => new[] { "do", "do dièse", "ré", "ré dièse", "mi", "fa", "fa dièse", "sol", "sol dièse", "la", "si bémol", "si" }[pitch],
        "es" => new[] { "do", "do sostenido", "re", "re sostenido", "mi", "fa", "fa sostenido", "sol", "sol sostenido", "la", "si bemol", "si" }[pitch],
        _ => language == "pl" ? PolishPitch[pitch] : EnglishPitch[pitch]
    };
    internal static string OctaveLabel(int octave, string language) => language switch
    {
        "de" => new[] { "Subkontraoktave", "Kontraoktave", "große Oktave", "kleine Oktave", "eingestrichene Oktave", "zweigestrichene Oktave", "dreigestrichene Oktave", "viergestrichene Oktave", "fünfgestrichene Oktave" }[octave],
        "ru" => new[] { "субконтроктава", "контроктава", "большая октава", "малая октава", "первая октава", "вторая октава", "третья октава", "четвёртая октава", "пятая октава" }[octave],
        "fr" => "octave " + octave,
        "es" => "octava " + octave,
        _ => OctaveLabel(octave, language == "en")
    };

    internal static int NextBlack(int current, int direction)
    {
        if (direction is not (-1 or 1)) throw new ArgumentOutOfRangeException(nameof(direction));
        for (int midi = current + direction; midi >= LowestMidi && midi <= HighestMidi; midi += direction)
            if (midi % 12 is 1 or 3 or 6 or 8 or 10) return midi;
        return current;
    }

    internal static int NextNatural(int current, int direction)
    {
        if (direction is not (-1 or 1)) throw new ArgumentOutOfRangeException(nameof(direction));
        for (int midi = current + direction; midi >= LowestMidi && midi <= HighestMidi; midi += direction)
            if (midi % 12 is 0 or 2 or 4 or 5 or 7 or 9 or 11) return midi;
        return current;
    }
}
