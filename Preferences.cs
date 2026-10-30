using System.Text.Json;

namespace Stroik;

internal sealed record Preferences(bool English, bool DarkTheme)
{
    private static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Stroik", "settings.json");

    internal static Preferences Load()
    {
        try
        {
            if (File.Exists(FilePath))
                return JsonSerializer.Deserialize<Preferences>(File.ReadAllText(FilePath)) ?? Default;
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
        catch (JsonException) { }
        return Default;
    }

    private static Preferences Default => new(
        System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName != "pl", false);

    internal static void Save(bool english, bool darkTheme)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(new Preferences(english, darkTheme)));
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
