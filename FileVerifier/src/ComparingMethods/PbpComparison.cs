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
        // TODO JPEG only calculations - Consider adding JPEG-specific handling here.
        try
        {
            // Log the input file paths to help with debugging
            Console.WriteLine($"Comparing images: {files.OriginalFilePath} vs {files.NewFilePath}");

            // Perform pixel-by-pixel comparison of the images
            return CompareImagesPixelByPixel(files.OriginalFilePath, files.NewFilePath);
        }
        catch (Exception ex)
        {
            // Log the exception details to the console
            Console.WriteLine($"Error in image comparison: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");

            // In case there is an inner exception, log that too
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                Console.WriteLine($"Inner Stack Trace: {ex.InnerException.StackTrace}");
            }

            // Return -1 if there was an error
            return -1;
        }
    }

    /// <summary>
    /// Compares two images on a pixel-by-pixel basis, resizing if necessary.
    /// </summary>
    /// <param name="originalFile">The file path of the original image.</param>
    /// <param name="newFile">The file path of the new image.</param>
    /// <param name="includeAlphaChannel">Indicates whether to include the alpha channel in comparison.</param>
    /// <returns>A similarity percentage between 0 and 100, or -1 if an error occurs.</returns>
    private static double CompareImagesPixelByPixel(string originalFile, string newFile, bool includeAlphaChannel = true) 
    {
        // Load the images using ImageSharp
        var originalImage = Image.Load<Rgba32>(originalFile);
        using var newImage = Image.Load<Rgba32>(newFile);

        // Resize the new image if its dimensions differ from the original
        if (originalImage.Width != newImage.Width || originalImage.Height != newImage.Height)
        {
            newImage.Mutate(x => x.Resize(originalImage.Size));  // Resize to match the original image's size
            Console.WriteLine("Image was resized.");
        }

        // Calculate and return the similarity percentage based on pixel comparison
        return CalculatePixelMatchPercentage(originalImage, newImage, includeAlphaChannel);
    }

    /// <summary>
    /// Calculates the percentage of matching pixels between two images.
    /// </summary>
    /// <param name="originalImage">The original image.</param>
    /// <param name="newImage">The new image.</param>
    /// <param name="includeAlphaChannel">Indicates whether to compare the alpha channel.</param>
    /// <returns>A similarity percentage between 0 and 100.</returns>
    private static double CalculatePixelMatchPercentage(Image<Rgba32> originalImage, Image<Rgba32> newImage, bool includeAlphaChannel)
    {
        // Calculate the total number of pixels in the image
        int totalPixels = originalImage.Width * originalImage.Height;
        int componentPixel = includeAlphaChannel ? 4 : 3; // 4 for RGBA, 3 for RGB
        int matchingPixels = 0;

        // Process each row of pixels in parallel (if possible)
        originalImage.ProcessPixelRows(newImage, (img1Accessor, img2Accessor) =>
        {
            for (int y = 0; y < img1Accessor.Height; y++)
            {
                // Convert the row of pixels to byte arrays for comparison
                var img1Row = MemoryMarshal.Cast<Rgba32, byte>(img1Accessor.GetRowSpan(y));
                var img2Row = MemoryMarshal.Cast<Rgba32, byte>(img2Accessor.GetRowSpan(y));

                // Compare the pixels in the row
                matchingPixels += ProcessRowPixels(img1Row, img2Row, componentPixel);
            }
        });

        // Calculate and return the percentage of matching pixels
        return CalculateSimilarityPercentage(matchingPixels, totalPixels);
    }

    /// <summary>
    /// Processes each row of pixels and compares them.
    /// </summary>
    /// <param name="img1Row">The row of pixels from the original image.</param>
    /// <param name="img2Row">The row of pixels from the new image.</param>
    /// <param name="componentPixel">The number of components per pixel (e.g., 3 for RGB, 4 for RGBA).</param>
    /// <returns>The number of matching pixels in the row.</returns>
    private static int ProcessRowPixels(Span<byte> img1Row, Span<byte> img2Row, int componentPixel)
    {
        int matchingPixels = 0;
        int x = 0;

        // Process the row using SIMD for optimal performance
        matchingPixels += CountMatchingPixelsSimd(img1Row, img2Row, componentPixel, ref x);

        // Handle any remaining pixels that were not processed by SIMD
        matchingPixels += CompareRemainingPixels(img1Row, img2Row, componentPixel, ref x);

        return matchingPixels;
    }

    /// <summary>
    /// Compares remaining pixels that were not processed by SIMD.
    /// </summary>
    /// <param name="img1Row">The row of pixels from the original image.</param>
    /// <param name="img2Row">The row of pixels from the new image.</param>
    /// <param name="componentPixel">The number of components per pixel (e.g., 3 for RGB, 4 for RGBA).</param>
    /// <param name="x">The current position in the row of pixels.</param>
    /// <returns>The number of matching pixels in the remaining part of the row.</returns>
    private static int CompareRemainingPixels(Span<byte> img1Row, Span<byte> img2Row, int componentPixel, ref int x)
    {
        int matchingPixels = 0;

        // Compare the remaining pixels one by one
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
    /// <param name="img1Row">The row of pixels from the original image.</param>
    /// <param name="img2Row">The row of pixels from the new image.</param>
    /// <param name="componentPixel">The number of components per pixel (e.g., 3 for RGB, 4 for RGBA).</param>
    /// <param name="x">The position of the pixel in the row.</param>
    /// <returns>True if the pixels match, false otherwise.</returns>
    private static bool ComparePixels(Span<byte> img1Row, Span<byte> img2Row, int componentPixel, int x)
    {
        // Compare each component of the pixel (e.g., R, G, B, A)
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
    /// Calculates the similarity percentage between the images based on matching pixels.
    /// </summary>
    /// <param name="matchingPixels">The number of matching pixels.</param>
    /// <param name="totalPixels">The total number of pixels in the image.</param>
    /// <returns>The similarity percentage between 0 and 100.</returns>
    private static double CalculateSimilarityPercentage(int matchingPixels, int totalPixels)
    {
        // Calculate and return the percentage of matching pixels
        return Math.Min(100, (double)matchingPixels / totalPixels * 100);
    }

    /// <summary>
    /// Counts the number of matching pixels using SIMD (Single Instruction, Multiple Data) for performance.
    /// </summary>
    /// <param name="img1Row">The row of pixels from the original image.</param>
    /// <param name="img2Row">The row of pixels from the new image.</param>
    /// <param name="componentPixel">The number of components per pixel (e.g., 3 for RGB, 4 for RGBA).</param>
    /// <param name="x">The current position in the row of pixels.</param>
    /// <returns>The number of matching pixels in the SIMD-processed portion of the row.</returns>
    private static int CountMatchingPixelsSimd(Span<byte> img1Row, Span<byte> img2Row, int componentPixel, ref int x)
    {
        int matchingPixels = 0;
        int vectorSize = Vector<byte>.Count;

        // Process pixels using SIMD
        while (x + vectorSize <= img1Row.Length && x + vectorSize <= img2Row.Length)
        {
            var vImg1 = new Vector<byte>(img1Row.Slice(x, vectorSize));
            var vImg2 = new Vector<byte>(img2Row.Slice(x, vectorSize));
            
            int matched = 0;

            // Compare the pixels in the vector
            for (int i = 0; i < vectorSize; i += componentPixel)
            {
                bool pixelMatch = true;
                for (int j = 0; j < componentPixel; j++)
                {
                    if (vImg1[i + j] != vImg2[i + j])
                    {
                        pixelMatch = false;
                        break;
                    }
                }

                if (pixelMatch)
                {
                    matched++;
                }
            }

            // Add the number of matching pixels to the total
            matchingPixels += matched;
            x += vectorSize;
        }

        return matchingPixels;
    }
}
