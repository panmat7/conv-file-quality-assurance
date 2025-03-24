using System;
using System.IO;
using Docnet.Core;
using Docnet.Core.Converters;
using Docnet.Core.Models;
using Docnet.Core.Readers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace AvaloniaDraft.Helpers;

public static class TakePicturePdf
{
    
    public static void ConvertPdfToImages(string path, string output, int pageStart = 0, int pageEnd = 0)
    {
        // Open the PDF document using Docnet
        using (var library = DocLib.Instance)
        using (var docReader = library.GetDocReader(File.ReadAllBytes(path), new PageDimensions(512, 1920)))
        {
            int pageCount = docReader.GetPageCount();

            if (pageEnd > pageCount || pageEnd == 0)
            {
                pageEnd = pageCount;
            }

            var outputFiles = Path.GetFileNameWithoutExtension(path);
            
            // Loop through all pages of the PDF
            for (int i = pageStart; i < pageEnd; i++)
            {
                using (var pageReader = docReader.GetPageReader(i))
                {
                    
                    // Get the width and height of the page
                    int width = pageReader.GetPageWidth();
                    int height = pageReader.GetPageHeight();

                    // Get the raw image data of the page (BGRA format)
                    byte[] rawImage = pageReader.GetImage(new NaiveTransparencyRemover());

                    // Construct the output image file path
                    string outputPath = Path.Combine(output, $"{outputFiles}{i + 1}.png");

                    // Convert raw image byte data to ImageSharp image
                    using (Image<Bgra32> image = Image.LoadPixelData<Bgra32>(rawImage, width, height))
                    {
                        // Save the image to a file (PNG format)
                        image.Save(outputPath);
                    }

                    Console.WriteLine($"Saved PDF page {i + 1} as {outputPath}");
                }
            }
        }
    }
}