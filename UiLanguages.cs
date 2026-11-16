using System.Globalization;
using System.Text.Json;

namespace Stroik;

internal static class UiLanguages
{
    internal static readonly string[] Codes = ["pl", "en", "de", "ru", "fr", "es"];
    internal static readonly string[] Names = ["Polski", "English", "Deutsch", "Русский", "Français", "Español"];
    private static readonly Dictionary<string, Dictionary<string, string>> Tables = new();
    internal static CultureInfo Culture(string code) => CultureInfo.GetCultureInfo(code switch { "pl" => "pl-PL", "en" => "en-US", "de" => "de-DE", "ru" => "ru-RU", "fr" => "fr-FR", _ => "es-ES" });
    internal static string Get(string code, string pl, string en)
    {
        if (code == "pl") return pl;
        if (code == "en") return en;
        return Table(code).TryGetValue(en, out string? translated) ? translated : en;
    }
    internal static IReadOnlyDictionary<string, string> Table(string code)
    {
        lock (Tables)
        {
            if (!Tables.TryGetValue(code, out var table))
            {
                using var source = typeof(UiLanguages).Assembly.GetManifestResourceStream("Stroik.locale." + code + ".json");
                table = source is null ? new() : JsonSerializer.Deserialize<Dictionary<string, string>>(source)!;
                Tables[code] = table;
            }
            return table;
        }
    }
    internal static string Format(string code, FormattableString pl, FormattableString en)
    {
        var source = code == "pl" ? pl : en;
        return string.Format(Culture(code), Get(code, pl.Format, en.Format), source.GetArguments());
    }
}
