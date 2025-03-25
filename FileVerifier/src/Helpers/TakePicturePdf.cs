using System;
using System.Collections.Generic;
using System.IO;
using Docnet.Core;
using Docnet.Core.Converters;
using Docnet.Core.Models;
using Docnet.Core.Readers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace AvaloniaDraft.Helpers;

public static class TakePicturePdf
{
    public static void ConvertPdfToImagesToDisk(string path, string output, int? pageStart = null, int? pageEnd = null)
    {
        // Open the PDF document using Docnet
        using (var library = DocLib.Instance)
        using (var docReader = library.GetDocReader(File.ReadAllBytes(path), new PageDimensions(512, 1920)))
        {
            var pageCount = docReader.GetPageCount();

            if (pageEnd > pageCount || pageEnd == null)
            {
                pageEnd = pageCount;
            }

            var outputFiles = Path.GetFileNameWithoutExtension(path);
            
            // Loop through all pages of the PDF
            for (var i = pageStart ?? 0; i < pageEnd; i++)
            {
                using var pageReader = docReader.GetPageReader(i);
                // Get the width and height of the page
                var width = pageReader.GetPageWidth();
                var height = pageReader.GetPageHeight();

                // Get the raw image data of the page (BGRA format)
                var rawImage = pageReader.GetImage(new NaiveTransparencyRemover());

                // Construct the output image file path
                var outputPath = Path.Combine(output, $"{outputFiles}{i + 1}.png");

                // Convert raw image byte data to ImageSharp image
                using (var image = Image.LoadPixelData<Bgra32>(rawImage, width, height))
                {
                    // Save the image to a file (PNG format)
                    image.Save(outputPath);
                }

                Console.WriteLine($"Saved PDF page {i + 1} as {outputPath}");
            }
        }
    }
    
    public static List<byte[]>? ConvertPdfToImagesToBytes(string path, int? pageStart = null, int? pageEnd = null)
    {
        try
        {
            var imgBytes = new List<byte[]>();

            // Open the PDF document using Docnet
            using (var library = DocLib.Instance)
            using (var docReader = library.GetDocReader(File.ReadAllBytes(path), new PageDimensions(512, 1920)))
            {
                var pageCount = docReader.GetPageCount();

                if (pageEnd > pageCount || pageEnd == null)
                {
                    pageEnd = pageCount;
                }

                // Loop through all pages of the PDF
                for (var i = pageStart ?? 0; i < pageEnd; i++)
                {
                    using var pageReader = docReader.GetPageReader(i);
                    // Get the width and height of the page
                    var width = pageReader.GetPageWidth();
                    var height = pageReader.GetPageHeight();

                    // Get the raw image data of the page (BGRA format)
                    var rawImage = pageReader.GetImage(new NaiveTransparencyRemover());

                    // Convert raw image byte data to ImageSharp image
                    using (var image = Image.LoadPixelData<Bgra32>(rawImage, width, height))
                    using (var ms = new MemoryStream())
                    {
                        // Save the image to a file (PNG format)
                        image.Save(ms, new PngEncoder());
                        imgBytes.Add(ms.ToArray());
                    }
                }
            }

            return imgBytes;
        }
        catch
        {
            return null;
        }
    }
}