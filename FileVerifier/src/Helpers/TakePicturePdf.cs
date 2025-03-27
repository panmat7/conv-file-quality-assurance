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
    public static string? ConvertPdfToImagesToDisk(string path, string output, int? pageStart = null, int? pageEnd = null)
    {
        try
        {
            lock (GlobalVariables.ImageExtractionLock)
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

                    var outputFile = Path.GetFileNameWithoutExtension(path);
                    var outputFileExtension = Path.GetExtension(path).TrimStart('.').ToUpper();

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
                        var outputPath = Path.Combine(output, $"{outputFile}_{outputFileExtension}{i + 1}.png");

                        // Convert raw image byte data to ImageSharp image
                        using var image = Image.LoadPixelData<Bgra32>(rawImage, width, height);
                        // Save the image to a file (PNG format)
                        image.Save(outputPath);
                    }
                }
                
                return output;
            }
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Converts pages on from a specified PDF document to images.
    /// </summary>
    /// <param name="path">Absolute to the document.</param>
    /// <param name="pageStart">At which page the page to image conversion is to start. Document start if unspecified or null.</param>
    /// <param name="pageEnd">At which page the page to image conversion is to end. Document start if unspecified or null.</param>
    /// <returns>List of PNG encoded images as bytes, null if an error occured.</returns>
    public static List<byte[]>? ConvertPdfToImagesToBytes(string path, int? pageStart = null, int? pageEnd = null)
    {
        try
        {
            lock (GlobalVariables.ImageExtractionLock)
            {
                var imgBytes = new List<byte[]>();

                // Open the PDF document using Docnet
                using (var library = DocLib.Instance)
                using (var docReader = library.GetDocReader(path, new PageDimensions(512, 1920)))
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
                        
                        if(pageReader == null) continue;
                        
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
        }
        catch
        {
            return null;
        }
    }
}