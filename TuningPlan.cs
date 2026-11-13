namespace Stroik;

internal enum PianoOrder { Ascending, AlternateFiveSemitones, AlternateFiveWholeTones }

internal static class TuningPlan
{
    internal static IReadOnlyList<int> Build(int anchor, PianoOrder order)
    {
        if (anchor is < NoteCatalog.LowestMidi or > NoteCatalog.HighestMidi)
            throw new ArgumentOutOfRangeException(nameof(anchor));
        if (order == PianoOrder.Ascending)
            return Enumerable.Range(anchor, NoteCatalog.HighestMidi - anchor + 1).ToArray();

        int step = order == PianoOrder.AlternateFiveSemitones ? 1 :
            order == PianoOrder.AlternateFiveWholeTones ? 2 :
            throw new ArgumentOutOfRangeException(nameof(order));
        var notes = new List<int> { anchor };
        int maxOffset = Math.Max(NoteCatalog.HighestMidi - anchor, anchor - NoteCatalog.LowestMidi) / step + 1;
        for (int first = 1; first <= maxOffset; first += 5)
        {
            for (int direction = 1; direction >= -1; direction -= 2)
            {
                for (int offset = first; offset < first + 5; offset++)
                {
                    int midi = anchor + direction * offset * step;
                    if (midi is >= NoteCatalog.LowestMidi and <= NoteCatalog.HighestMidi) notes.Add(midi);
                }
            }
        }
        return notes;
    }

    internal static int? Next(int anchor, int current, PianoOrder order, int highest = NoteCatalog.HighestMidi)
    {
        IReadOnlyList<int> notes = Build(anchor, order);
        int index = -1;
        for (int i = 0; i < notes.Count; i++) if (notes[i] == current) { index = i; break; }
        if (index >= 0)
            for (int next = index + 1; next < notes.Count; next++)
                if (notes[next] <= highest) return notes[next];
        return null;
    }
}

internal static class KeyboardGuide
{
    private static readonly string[] Polish =
    [
        "biały klawisz bezpośrednio przed grupą dwóch czarnych",
        "pierwszy czarny klawisz w grupie dwóch",
        "biały klawisz między dwoma czarnymi w grupie dwóch",
        "drugi czarny klawisz w grupie dwóch",
        "biały klawisz bezpośrednio za grupą dwóch czarnych",
        "biały klawisz bezpośrednio przed grupą trzech czarnych",
        "pierwszy czarny klawisz w grupie trzech",
        "biały klawisz między pierwszym a drugim czarnym klawiszem w grupie trzech",
        "drugi czarny klawisz w grupie trzech",
        "biały klawisz między drugim a trzecim czarnym klawiszem w grupie trzech",
        "trzeci czarny klawisz w grupie trzech",
        "biały klawisz bezpośrednio za grupą trzech czarnych"
    ];
    private static readonly string[] English =
    [
        "white key immediately before a group of two black keys",
        "first black key in a group of two",
        "white key between the two black keys in a group of two",
        "second black key in a group of two",
        "white key immediately after a group of two black keys",
        "white key immediately before a group of three black keys",
        "first black key in a group of three",
        "white key between the first and second black keys in a group of three",
        "second black key in a group of three",
        "white key between the second and third black keys in a group of three",
        "third black key in a group of three",
        "white key immediately after a group of three black keys"
    ];
    internal static string Describe(int midi, bool english) =>
        english ? $"{NoteCatalog.Label(midi, true)}: {English[midi % 12]}." :
                  $"{NoteCatalog.Label(midi, false)}: {Polish[midi % 12]}.";
}
