using System;
using System.IO;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Aspose.Slides;
using Aspose.Words;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using UglyToad.PdfPig;


namespace AvaloniaDraft.ComparingMethods;

public static class ComperingMethods
{
    /// <summary>
    /// Returns the difference size between two files 
    /// </summary>
    /// <param name="files">The two files to be compared</param>
    /// <returns>The size difference in bytes</returns>
    public static long GetFileSizeDifference(FilePair files)
    {
        var originalSize = new FileInfo(files.OriginalFilePath).Length;
        var newSize = new FileInfo(files.NewFilePath).Length;
        
        return long.Abs(originalSize - newSize);
    }
    
    /// <summary>
    /// Returns the difference of resolution between two images
    /// </summary>
    /// <param name="files">The two image files to be compared</param>
    /// <returns> </returns>
    public static Tuple<int, int>? GetImageResolutionDifference(FilePair files)
    {
        try
        {
            using Image image1 = Image.Load(files.OriginalFilePath), image2 = Image.Load(files.NewFilePath);
            var difWidth = image1.Width - image2.Width;
            var difHeight = image1.Height - image2.Height;
            return new Tuple<int, int>(int.Abs(difWidth), int.Abs(difHeight));
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Returns the resolution of an image
    /// </summary>
    /// <param name="path">Absolute path to the image</param>
    /// <returns>Tuple containing the image's width and height</returns>
    public static Tuple<int, int>? GetImageResolution(string path)
    {
        try
        {
            using var image = Image.Load(path);
            return new Tuple<int, int>(image.Width, image.Height);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Returns the difference in pages between two documents. 
    /// </summary>
    /// <param name="files">Files to be compared</param>
    /// <returns>Either a positive integer with the page difference, -1 meaning error while getting pages or null meaning not supported file type</returns>
    public static int? GetPageCountDifference(FilePair files)
    {
        var originalPageCount = 0;
        var newPageCount = 0;

        try
        {
            var originalPages = GetPageCount(files.OriginalFilePath, files.OriginalFileFormat);
            var newPages = GetPageCount(files.NewFilePath, files.NewFileFormat);
            
            if(originalPages == null || newPages == null) return null;
            if(originalPages == -1 || newPages == -1) return -1;
            
            return int.Abs((int)(originalPages - newPages));
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Returns the number of pages in a document
    /// </summary>
    /// <param name="path">Absolute path to the document</param>
    /// <param name="format">PRONOM code of the file type</param>
    /// <returns>Either a positive integer with page count, -1 meaning error while getting pages or null meaning not supported file type</returns>
    public static int? GetPageCount(string path, string format)
    {
        try
        {
            //Text documents
            if (FormatCodes.PronomCodesTextDocuments.Contains(format))
            {
                var doc = new Document(path);
                return doc.PageCount;
            }

            //For presentations - return number of slides
            if (FormatCodes.PronomCodesPresentationDocuments.Contains(format))
            {
                var presentation = new Presentation(path);
                return presentation.Slides.Count;
            }
            
            //For PDFs
            if (FormatCodes.PronomCodesPDF.Contains(format) || FormatCodes.PronomCodesPDFA.Contains(format))
            {
                using var doc = PdfDocument.Open(path);
                return doc.NumberOfPages;
            }
        }
        catch
        {
            return null;
        }

        return -1;
    }
    
    /// <summary>
    /// Compares two images by calculating the pixel-by-pixel similarity between them.
    /// </summary>
    /// <param name="files">A pair of file paths containing the original and the new image for comparison.</param>
    /// <returns>
    /// A double value representing the percentage similarity between the two images, where 100 means identical images and 0 means no similarity.
    /// A value of -1 indicates that an error occurred during the comparison.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when any of the input file paths are null or empty.</exception>
    /// <exception cref="IOException">Thrown when there's an issue reading the image files (e.g., unsupported format or file corruption).</exception>
    /// <exception cref="Exception">Thrown for any unexpected error that may occur during the comparison process.</exception>
    public static double PbpImageComparing(FilePair files) // thread allowed to start parameter
    {
        var path1 = files.OriginalFilePath;
        var path2 = files.NewFilePath;
        const bool IncludeAlphaChannel = true;

        try
        {
            using var img1 = Image.Load<Rgba32>(path1);
            using var img2 = Image.Load<Rgba32>(path2);

            // Resize the second image to match the first if the sizes differ
            if (img1.Width != img2.Width || img1.Height != img2.Height)
            {
                img2.Mutate(x => x.Resize(img1.Size));
                Console.WriteLine("Image was resized.");
            }

            int matchingPixels = 0;
            int pixelCount = img1.Width * img1.Height;
            int componentPixel = IncludeAlphaChannel ? 4 : 3;

            // Process pixel rows with SIMD optimization
            img1.ProcessPixelRows<Rgba32>(img2, (img1Accessor, img2Accessor) =>
            {
                for (int y = 0; y < img1Accessor.Height; y++)
                {
                    var img1Row = MemoryMarshal.Cast<Rgba32, byte>(img1Accessor.GetRowSpan(y));
                    var img2Row = MemoryMarshal.Cast<Rgba32, byte>(img2Accessor.GetRowSpan(y));

                    int x = 0;

                    // SIMD-based comparison for chunks of vector size
                    while (x + Vector<byte>.Count <= img1Row.Length)
                    {
                        var vImg1 = new Vector<byte>(img1Row.Slice(x, Vector<byte>.Count));
                        var vImg2 = new Vector<byte>(img2Row.Slice(x, Vector<byte>.Count));
                        
                        // Compare the vectors and create a mask where bytes match
                        var mask = Vector.Equals(vImg1, vImg2);

                        // Count the number of matching components in the mask
                        int matched = 0;
                        for (int i = 0; i < Vector<byte>.Count; i++)
                        {
                            // Check if the mask byte is non-zero (meaning a match occurred)
                            if (mask[i] != 0) matched++;  // Check if the byte is non-zero (indicating a match)
                        }


                        // Update matching pixel count, dividing by components per pixel (RGBA = 4 components)
                        matchingPixels += matched / componentPixel;
                        x += Vector<byte>.Count;
                    }

                    // Handle remaining pixels manually
                    for (; x < img1Row.Length; x++)
                    {
                        if (img1Row[x] == img2Row[x])
                        {
                            matchingPixels++;
                        }
                    }
                }
            });

            // Calculate similarity percentage
            double similarity = ((double)matchingPixels / pixelCount) * 100;
            return similarity;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex}");
            return -1; // Return 0 similarity in case of error
        }
    }

}

            
        






