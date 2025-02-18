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
            return CompareImagesPixelByPixel(files.OriginalFilePath, files.NewFilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in image comparison: {ex}");
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
        var originalImage = Image.Load<Rgba32>(originalFile);
        using var newImage = Image.Load<Rgba32>(newFile);

        // Resize if dimensions are different
        if (originalImage.Width != newImage.Width || originalImage.Height != newImage.Height)
        {
            newImage.Mutate(x => x.Resize(originalImage.Size));
            Console.WriteLine("Image was resized.");
        }

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
        int matchingPixels = 0;
        int totalPixels = originalImage.Width * originalImage.Height;
        int componentPixel = includeAlphaChannel ? 4 : 3;

        originalImage.ProcessPixelRows(newImage, (img1Accessor, img2Accessor) =>
        {
            for (int y = 0; y < img1Accessor.Height; y++)
            {
                var img1Row = MemoryMarshal.Cast<Rgba32, byte>(img1Accessor.GetRowSpan(y));
                var img2Row = MemoryMarshal.Cast<Rgba32, byte>(img2Accessor.GetRowSpan(y));

                int x = 0;
                matchingPixels += CountMatchingPixelsSimd(img1Row, img2Row, componentPixel, ref x);

                // Process remaining pixels (after SIMD)
                for (; x < img1Row.Length; x++)
                {
                    if (img1Row[x] == img2Row[x]) matchingPixels++;
                }
            }
        });

        return ((double)matchingPixels / totalPixels) * 100;
    }

    /// <summary>
    /// Uses SIMD (Single Instruction, Multiple Data) to compare pixel values in chunks for faster processing.
    /// </summary>
    /// <param name="img1Row">The pixel data of the first image row.</param>
    /// <param name="img2Row">The pixel data of the second image row.</param>
    /// <param name="componentPixel">The number of color components per pixel (3 for RGB, 4 for RGBA).</param>
    /// <param name="x">The current pixel position, updated as the function processes pixels.</param>
    /// <returns>The number of matching pixels found using SIMD.</returns>
    private static int CountMatchingPixelsSimd(Span<byte> img1Row, Span<byte> img2Row, int componentPixel, ref int x)
    {
        int matchingPixels = 0;
        int vectorSize = Vector<byte>.Count;

        while (x + vectorSize <= img1Row.Length)
        {
            var vImg1 = new Vector<byte>(img1Row.Slice(x, vectorSize));
            var vImg2 = new Vector<byte>(img2Row.Slice(x, vectorSize));

            var mask = Vector.Equals(vImg1, vImg2);
            int matched = 0;

            for (int i = 0; i < vectorSize; i++)
            {
                if (mask[i] != 0) matched++;
            }

            matchingPixels += matched / componentPixel;
            x += vectorSize;
        }

        return matchingPixels;
    }
}