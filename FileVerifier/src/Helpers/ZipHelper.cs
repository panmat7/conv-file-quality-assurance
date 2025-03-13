using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace AvaloniaDraft.Helpers;

public abstract class ZipHelper
{
    public static readonly string[] CompressedFilesExtensions = ["*.zip", "*.rar", "*.7z", "*.tar", "*.gz"];
    
    /// <summary>
    /// Extracts files inside a zip archive into a temp directory
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="tempDirectory"></param>
    internal static void ExtractCompressedFiles(string directory, string tempDirectory)
    {
        var files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories)
            .Where(f => CompressedFilesExtensions.Contains(Path.GetExtension(f).ToLower()));

        foreach (var file in files)
        {
            Console.WriteLine($"File {file} in compressed state {directory}");
            var extractPath = Path.Combine(tempDirectory, Path.GetFileNameWithoutExtension(file));

            Directory.CreateDirectory(extractPath);

            using var archive = ArchiveFactory.Open(file);
            foreach (var entry in archive.Entries)
            {
                if (!entry.IsDirectory)
                {
                    entry.WriteToDirectory(extractPath, new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                }
            }
        }
    }

    /// <summary>
    /// Checks if a zip file is encrypted
    /// </summary>
    /// <param name="zipPath"></param>
    /// <returns></returns>
    internal static bool IsCompressedEncrypted(string zipPath)
    {
        try
        {
            using var archive = ArchiveFactory.Open(zipPath);

            var firstEntry = archive.Entries.FirstOrDefault();
            if (firstEntry == null) return false;
            using var stream = firstEntry.OpenEntryStream();
            stream.ReadByte();

            return false;
        }
        catch
        {
            // Archive is encrypted or corrupted
            return true;
        }
    }
}