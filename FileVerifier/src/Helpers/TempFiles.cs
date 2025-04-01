using System.IO;

namespace AvaloniaDraft.Helpers;

public static class TempFiles
{
    /// <summary>
    /// Creates a temporary file with a random name, keeping the extension. 
    /// </summary>
    /// <param name="bytes">Byte content of the file.</param>
    /// <param name="folderPath">Path of the folder where the file is to be saved.</param>
    /// <param name="extension">The expected extension of the file. Left out if .temp is fine.</param>
    /// <returns>Path to the file on disk, null if an error occured.</returns>
    public static string? CreateTemporaryFile(byte[] bytes, string folderPath, string? extension = null)
    {
        if (extension == null)
            extension = ".temp";
        else if (!extension.StartsWith('.'))
            extension = "." + extension;

        try
        {
            // Ensure the folder exists
            Directory.CreateDirectory(folderPath);

            //Avoid name collision (very improbable but better to check than not.)
            string tempFilePath;
            do
            {
                tempFilePath = Path.Combine(folderPath, Path.GetRandomFileName() + extension);
            }
            while (File.Exists(tempFilePath)); 

            File.WriteAllBytes(tempFilePath, bytes);
            return tempFilePath;
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