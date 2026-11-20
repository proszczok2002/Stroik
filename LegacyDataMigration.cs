using System.Security.Cryptography;

namespace Stroik;

internal static class LegacyDataMigration
{
    private sealed record Entry(string RelativePath, bool Directory);

    internal static bool TryMove(string legacyRoot, string programDirectory)
    {
        if (!Path.IsPathFullyQualified(legacyRoot) || PortableData.Contains(legacyRoot, programDirectory)) return false;
        if (!Directory.Exists(legacyRoot)) return !File.Exists(legacyRoot);

        string data = PortableData.DataDirectory(programDirectory);
        string staging = Path.Combine(data, ".migration-" + Guid.NewGuid().ToString("N"));
        try
        {
            PortableData.RejectLink(legacyRoot);
            PortableData.EnsureDataDirectory(data);
            using var operation = new FileStream(Path.Combine(data, ".migration.lock"), FileMode.OpenOrCreate,
                FileAccess.ReadWrite, FileShare.None, 1, FileOptions.DeleteOnClose);
            var entries = ReadTree(legacyRoot);
            if (entries.Count == 0)
            {
                Directory.Delete(legacyRoot);
                return true;
            }

            Directory.CreateDirectory(staging);
            foreach (var entry in entries)
            {
                string copy = Path.Combine(staging, entry.RelativePath);
                if (entry.Directory) Directory.CreateDirectory(copy);
                else
                {
                    string original = Path.Combine(legacyRoot, entry.RelativePath);
                    PortableData.RejectLinksUnder(legacyRoot, original);
                    CopyVerified(original, copy);
                }
            }
            if (!entries.SequenceEqual(ReadTree(legacyRoot))) return false;

            string snapshot = Path.Combine(data, "migracja-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N"));
            Directory.Move(staging, snapshot);
            // Commit the complete copy before removing anything from the old folder. Reopen each
            // file with writes denied and remove it only if its current bytes are in the snapshot.
            foreach (var entry in entries.Where(entry => !entry.Directory))
            {
                string original = Path.Combine(legacyRoot, entry.RelativePath);
                PortableData.RejectLinksUnder(legacyRoot, original);
                using var source = new FileStream(original, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
                using var copy = File.OpenRead(Path.Combine(snapshot, entry.RelativePath));
                if (source.Length != copy.Length || !SHA256.HashData(source).AsSpan().SequenceEqual(SHA256.HashData(copy))) return false;
                File.Delete(original);
            }
            foreach (var entry in entries.Where(entry => entry.Directory).Reverse())
            {
                string original = Path.Combine(legacyRoot, entry.RelativePath);
                PortableData.RejectLinksUnder(legacyRoot, original);
                Directory.Delete(original);
            }
            PortableData.RejectLink(legacyRoot);
            Directory.Delete(legacyRoot);
            return true;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException) { return false; }
        finally
        {
            // Only this attempt's private staging directory is disposable. Committed copies remain.
            try { if (Directory.Exists(staging)) Directory.Delete(staging, recursive: true); }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException) { }
        }
    }

    private static List<Entry> ReadTree(string root)
    {
        var entries = new List<Entry>();
        var pending = new Stack<string>();
        pending.Push(root);
        while (pending.TryPop(out string? directory))
        {
            PortableData.RejectLink(directory);
            foreach (var path in Directory.EnumerateFileSystemEntries(directory))
            {
                PortableData.RejectLink(path);
                bool isDirectory = (File.GetAttributes(path) & FileAttributes.Directory) != 0;
                entries.Add(new(Path.GetRelativePath(root, path), isDirectory));
                if (isDirectory) pending.Push(path);
            }
        }
        // Relative paths sort parent directories first for creation and last for removal.
        return entries.OrderBy(entry => entry.RelativePath, StringComparer.Ordinal).ToList();
    }

    private static void CopyVerified(string original, string copy)
    {
        using var source = new FileStream(original, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var target = new FileStream(copy, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
        source.CopyTo(target);
        target.Flush(flushToDisk: true);
        source.Position = target.Position = 0;
        if (source.Length != target.Length || !SHA256.HashData(source).AsSpan().SequenceEqual(SHA256.HashData(target)))
            throw new IOException("The copied data does not match the original: " + original);
    }
}
