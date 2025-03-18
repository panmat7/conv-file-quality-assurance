using System.IO;

namespace AvaloniaDraft.Helpers;

public static class TempFiles
{
    /// <summary>
    /// Creates a temporary file with a random name, keeping the extension. 
    /// </summary>
    /// <param name="bytes">Byte content of the file.</param>
    /// <param name="extension">The expected extension of the file. Left out if .temp is fine.</param>
    /// <returns></returns>
    public static string? CreateTemporaryFile(byte[] bytes, string? extension = null)
    {
        if (extension == null)
            extension = ".temp";
        else if (!extension.StartsWith('.'))
            extension = "." + extension;
        
        //Should be thread safe 
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + extension);

        try
        {
            File.WriteAllBytes(path, bytes);
            return path;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Deletes a temporary (or really any) file.
    /// </summary>
    /// <param name="path">Path to the file.</param>
    public static void DeleteTemporaryFile(string path)
    {
        if (File.Exists(path))
        {
            try
            {
                File.Delete(path);
            }
            catch
            {
                //What do we do here?
            }
        }
    }
}