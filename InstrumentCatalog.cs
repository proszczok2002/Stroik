namespace Stroik;

internal enum InstrumentKind { Piano, Guitar, Ukulele, Violin, Bass, Chromatic }

internal enum UkuleleTuning { Standard, SwappedAG }

internal readonly record struct OpenString(int Number, int Midi);

internal static class InstrumentCatalog
{
    private static readonly OpenString[] Guitar =
        [new(6, 40), new(5, 45), new(4, 50), new(3, 55), new(2, 59), new(1, 64)];
    private static readonly OpenString[] Ukulele =
        [new(4, 67), new(3, 60), new(2, 64), new(1, 69)];
    private static readonly OpenString[] UkuleleSwappedAG =
        [new(4, 69), new(3, 60), new(2, 64), new(1, 67)];
    private static readonly OpenString[] Violin =
        [new(4, 55), new(3, 62), new(2, 69), new(1, 76)];
    private static readonly OpenString[] Bass =
        [new(4, 28), new(3, 33), new(2, 38), new(1, 43)];

    internal static bool HasStrings(InstrumentKind kind) => kind is not (InstrumentKind.Piano or InstrumentKind.Chromatic);

    internal static IReadOnlyList<OpenString> Strings(InstrumentKind kind, UkuleleTuning ukuleleTuning = UkuleleTuning.Standard) => kind switch
    {
        InstrumentKind.Guitar => Guitar,
        InstrumentKind.Ukulele => ukuleleTuning == UkuleleTuning.SwappedAG ? UkuleleSwappedAG : Ukulele,
        InstrumentKind.Violin => Violin,
        InstrumentKind.Bass => Bass,
        _ => []
    };

    internal static int DefaultMidi(InstrumentKind kind, UkuleleTuning ukuleleTuning = UkuleleTuning.Standard) =>
        HasStrings(kind) ? Strings(kind, ukuleleTuning)[0].Midi : NoteCatalog.DefaultMidi;

    internal static int NearestOpenString(InstrumentKind kind, double frequency, double reference,
        UkuleleTuning ukuleleTuning = UkuleleTuning.Standard)
    {
        IReadOnlyList<OpenString> strings = Strings(kind, ukuleleTuning);
        if (strings.Count == 0) throw new ArgumentException("The instrument has no open strings.", nameof(kind));
        return strings.MinBy(s => Math.Abs(Math.Log2(frequency / PitchDetector.NoteFromMidi(s.Midi, reference).Frequency))).Midi;
    }
}