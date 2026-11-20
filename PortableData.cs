namespace Stroik;

internal static class PortableData
{
    internal static string ProgramDirectory => AppContext.BaseDirectory;
    internal static string LegacyDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
        Environment.SpecialFolderOption.DoNotVerify), "Stroik");
    internal static string DataDirectory(string programDirectory) => Path.Combine(programDirectory, "Stroik-data");
    internal static string UpdatesDirectory(string programDirectory) => Path.Combine(DataDirectory(programDirectory), "updates");

    internal static bool Contains(string directory, string path)
    {
        string root = Path.GetFullPath(directory);
        string candidate = Path.GetFullPath(path);
        return Path.TrimEndingDirectorySeparator(root).Equals(Path.TrimEndingDirectorySeparator(candidate), StringComparison.OrdinalIgnoreCase) ||
            candidate.StartsWith(Path.EndsInDirectorySeparator(root) ? root : root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    internal static void RejectLink(string path)
    {
        if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
            throw new IOException("A filesystem link was found: " + path);
    }

    internal static void EnsureDataDirectory(string directory)
    {
        if (Path.IsPathFullyQualified(LegacyDirectory) && Contains(LegacyDirectory, directory))
            throw new IOException("Move Stroik to a writable folder outside its old AppData directory.");
        Directory.CreateDirectory(directory);
        RejectLink(directory);
    }

    internal static void RejectLinksUnder(string directory, string path)
    {
        directory = Path.TrimEndingDirectorySeparator(Path.GetFullPath(directory));
        path = Path.GetFullPath(path);
        if (!Contains(directory, path)) throw new IOException("A path points outside the data directory.");
        for (string current = path; ; current = Path.GetDirectoryName(current)!)
        {
            RejectLink(current);
            if (current.Equals(directory, StringComparison.OrdinalIgnoreCase)) return;
        }
    }
}
