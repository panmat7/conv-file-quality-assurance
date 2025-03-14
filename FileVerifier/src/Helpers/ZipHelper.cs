using System.IO;
using System.Linq;
using AvaloniaDraft.FileManager;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace AvaloniaDraft.Helpers;

public abstract class ZipHelper
{
    public static readonly string[] CompressedFilesExtensions =
    [
        "*.zip", "*.rar", "*.7z", "*.tar", "*.gz"
    ];

    /// <summary>
    /// Extracts files inside a zip archive into a temp directory
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="tempDirectory"></param>
    /// <param name="fileManager"></param>
    internal static void ExtractCompressedFiles(string directory, string tempDirectory, FileManager.FileManager fileManager)
    {
        var files = CompressedFilesExtensions.SelectMany(ext => Directory.GetFiles(directory, ext, SearchOption.AllDirectories));
        foreach (var file in files)
        {
            if (EncryptedFileHelper.IsCompressedEncrypted(file))
            {
                fileManager.IgnoredFiles.Add(new IgnoredFile(file, ReasonForIgnoring.Encrypted));
            }
            else
            {
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
    }
}