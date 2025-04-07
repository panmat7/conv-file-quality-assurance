using System;
using AvaloniaDraft.FileManager;
using ImageMagick;

namespace AvaloniaDraft.ComparingMethods;

public static class PbpComparisonMagick
{
    /// <summary>
    /// Calculates the similarity between two images by comparing their pixels.
    /// </summary>
    /// <param name="files">A pair of file paths containing the original and new images.</param>
    /// <param name="threadsCount">The number of threads to use for processing (not currently implemented).</param>
    /// <returns>A similarity percentage between 0 and 100, or -1 if an error occurs.</returns>
    public static double CalculateImageSimilarity(FilePair files, int threadsCount = 1)
    { 
        try
        {
            Console.WriteLine($"Comparing images: {files.OriginalFilePath} vs {files.NewFilePath}");

            return CompareImagesPixelByPixel(files.OriginalFilePath, files.NewFilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in image comparison: {ex.Message}\nStack Trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}\nInner Stack Trace: {ex.InnerException.StackTrace}");
            }
            return -1;
        }
    }

    /// <summary>
    /// Compares two images on a pixel-by-pixel basis, resizing if necessary.
    /// </summary>
    private static double CompareImagesPixelByPixel(string originalFile, string newFile) 
    {
        using var originalImage = new MagickImage(originalFile);
        using var newImage = new MagickImage(newFile);
        
        //Convert to Lab color space 
        originalImage.ColorSpace = ColorSpace.Lab;
        newImage.ColorSpace = ColorSpace.Lab;

        // Resize the new image if necessary
        if (originalImage.Width != newImage.Width || originalImage.Height != newImage.Height)
        {
            newImage.Resize(originalImage.Width, originalImage.Height);  
            Console.WriteLine("Warning: The new image was resized to match the original. This may affect accuracy.");
        }

        return CheckDistance(originalImage, newImage);
    }

    /// <summary>
    /// Calculates the percentage of matching pixels between two images.
    /// </summary>
    private static double CheckDistance(MagickImage originalImage, MagickImage newImage)
    {
        // Ensure both images are in Lab color space
        originalImage.ColorSpace = ColorSpace.Lab;
        newImage.ColorSpace = ColorSpace.Lab;

        var oImage = originalImage.GetPixels();
        var nImage = newImage.GetPixels();

        double totalDistance = 0;
        uint totalPixels = originalImage.Height * originalImage.Width;

        for (var y = 0; y < originalImage.Height; y++)
        {
            for (var x = 0; x < originalImage.Width; x++)
            {
                var pixel1 = oImage.GetPixel(x, y).ToColor();
                var pixel2 = nImage.GetPixel(x, y).ToColor();

                if (pixel1 != null)
                {
                    double l1 = pixel1.R;
                    double a1 = pixel1.G;
                    double b1 = pixel1.B;

                    if (pixel2 != null)
                    {
                        double l2 = pixel2.R;
                        double a2 = pixel2.G;
                        double b2 = pixel2.B;

                        // Euclidean distance formula
                        double distance = Math.Sqrt(
                            Math.Pow(l1 - l2, 2) +
                            Math.Pow(a1 - a2, 2) +
                            Math.Pow(b1 - b2, 2)
                        );

                        totalDistance += distance;
                    }
                }
            }
        }

        // Return the average Lab distance
        return totalDistance / totalPixels;
    }
}
