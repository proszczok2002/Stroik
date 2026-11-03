using System.Reflection;
using System.Text.RegularExpressions;

namespace Stroik;

internal static class ReleaseNotes
{
    private const string ResourceName = "Stroik.RELEASE_NOTES.md";

    internal static string Current(bool english)
    {
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName)
            ?? throw new InvalidDataException("The embedded release notes are missing.");
        using var reader = new StreamReader(stream);
        return ForLanguage(reader.ReadToEnd(), english);
    }

    internal static string ForLanguage(string? markdown, bool english)
    {
        string fallback = english ? "No release notes are available for this version."
            : "Opis zmian tej wersji nie jest dostępny.";
        if (string.IsNullOrWhiteSpace(markdown)) return fallback;

        string[] lines = markdown.Replace("\r\n", "\n").Split('\n');
        string heading = english ? "## English" : "## Polski";
        int start = Array.FindIndex(lines, line => line.Trim().Equals(heading, StringComparison.OrdinalIgnoreCase));
        if (start < 0) return fallback;

        var selected = new List<string>();
        for (int i = start + 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.StartsWith("## ", StringComparison.Ordinal)) break;
            line = Regex.Replace(line, @"\[([^\]]+)\]\([^)]+\)", "$1");
            line = line.Replace("**", "").Replace("`", "");
            if (line.StartsWith("- ", StringComparison.Ordinal)) line = "• " + line[2..];
            selected.Add(line);
        }
        string result = string.Join(Environment.NewLine, selected).Trim();
        if (result.Length > 6000) result = result[..6000] + "…";
        return string.IsNullOrWhiteSpace(result) ? fallback : result;
    }
}
