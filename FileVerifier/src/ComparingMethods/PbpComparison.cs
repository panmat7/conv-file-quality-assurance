using System;
using System.Numerics;
using System.Runtime.InteropServices;
using AvaloniaDraft.FileManager;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace AvaloniaDraft.ComparingMethods;

public static class PbpComparison
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
        using var originalImage = Image.Load<Rgba32>(originalFile);
        using var newImage = Image.Load<Rgba32>(newFile);

        // Resize the new image if necessary
        if (originalImage.Width != newImage.Width || originalImage.Height != newImage.Height)
        {
            newImage.Mutate(x => x.Resize(originalImage.Size));  
            Console.WriteLine("Warning: The new image was resized to match the original. This may affect accuracy.");
        }

        return CalculatePixelMatchPercentage(originalImage, newImage);
    }

    /// <summary>
    /// Calculates the percentage of matching pixels between two images.
    /// </summary>
    private static double CalculatePixelMatchPercentage(Image<Rgba32> originalImage, Image<Rgba32> newImage)
    {
        int totalPixels = originalImage.Width * originalImage.Height;
        int matchingPixels = 0;

        originalImage.ProcessPixelRows(newImage, (img1Accessor, img2Accessor) =>
        {
            for (int y = 0; y < img1Accessor.Height; y++)
            {
                var img1Row = MemoryMarshal.Cast<Rgba32, byte>(img1Accessor.GetRowSpan(y));
                var img2Row = MemoryMarshal.Cast<Rgba32, byte>(img2Accessor.GetRowSpan(y));
                matchingPixels += ProcessRowPixels(img1Row, img2Row, 4); // RGB has 3 components
            }
        });

        return Math.Min(100, (double)matchingPixels / totalPixels * 100);
    }

    /// <summary>
    /// Processes each row of pixels and compares them.
    /// </summary>
    private static int ProcessRowPixels(Span<byte> img1Row, Span<byte> img2Row, int componentPixel)
    {
        int matchingPixels = 0;
        int x = 0;

        matchingPixels += CountMatchingPixels(img1Row, img2Row, componentPixel, ref x);
        matchingPixels += CompareRemainingPixels(img1Row, img2Row, componentPixel, ref x);

        return matchingPixels;
    }

    /// <summary>
    /// Compares remaining pixels that were not processed by SIMD.
    /// </summary>
    private static int CompareRemainingPixels(Span<byte> img1Row, Span<byte> img2Row, int componentPixel, ref int x)
    {
        int matchingPixels = 0;

        for (; x < img1Row.Length; x += componentPixel)
        {
            if (ComparePixels(img1Row, img2Row, componentPixel, x))
            {
                matchingPixels++;
            }
        }

        return matchingPixels;
    }

    /// <summary>
    /// Compares two pixels at the specified position.
    /// </summary>
    private static bool ComparePixels(Span<byte> img1Row, Span<byte> img2Row, int componentPixel, int x)
    {
        for (int i = 0; i < componentPixel; i++)
        {
            if (img1Row[x + i] != img2Row[x + i])
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Counts the number of matching pixels using SIMD (Single Instruction, Multiple Data) for performance.
    /// </summary>
    private static int CountMatchingPixels(Span<byte> img1Row, Span<byte> img2Row, int componentPixel, ref int x)
    {
        int matchingPixels = 0;

        // Iterate through pixels one-by-one
        for (; x < img1Row.Length && x < img2Row.Length; x += componentPixel)
        {
            bool pixelMatch = true;
            
            // Compare each component (R, G, B) of the pixel
            for (int j = 0; j < componentPixel; j++)
            {
                if (Math.Abs(img1Row[x + j] - img2Row[j]) >= 3)
                {
                    pixelMatch = false;
                    break;
                }
            }

            if (pixelMatch)
            {
                matchingPixels++;
            }
        }

        return matchingPixels;
    }


}
