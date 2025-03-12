using System.IO;
using System.IO.Compression;

namespace AvaloniaDraft.Helpers;

public abstract class ZipHelper
{
    /// <summary>
    /// Extracts files inside a zip archive into a temp directory
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="tempDirectory"></param>
    internal static void ExtractZipFiles(string directory, string tempDirectory)
    {
        var zipFiles = Directory.GetFiles(directory, "*.zip", SearchOption.AllDirectories);
        foreach (var zipFile in zipFiles)
        {
            // Dont work with encrypted zip archives
            if (IsZipEncrypted(zipFile)) continue;
            var extractPath = Path.Combine(tempDirectory, Path.GetFileNameWithoutExtension(zipFile));
            ZipFile.ExtractToDirectory(zipFile, extractPath);
        }
    }

    /// <summary>
    /// Checks if a zip file is encrypted
    /// </summary>
    /// <param name="zipPath"></param>
    /// <returns></returns>
    private static bool IsZipEncrypted(string zipPath)
    {
        try
        {
            using var archive = ZipFile.OpenRead(zipPath);
            foreach (var entry in archive.Entries)
            {
                try
                {
                    // Attempt to read the first byte of each entry
                    using var stream = entry.Open();
                    stream.ReadByte();
                }
                catch (InvalidDataException)
                {
                    // Entry is encrypted or corrupted
                    return true;
                }
            }
            return false;
        }
        catch
        {
            // Fallback for unreadable/corrupted ZIPs
            return true;
        }
    }
}