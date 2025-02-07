using System;
using System.Collections.Generic;
using System.Linq;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;
using ImageMagick;
using UglyToad.PdfPig;
using System.IO;
using System.IO.Compression;

namespace AvaloniaDraft.ComparingMethods;

// TODO: What if files do not have images

public static class ColorProfileComparison
{
    /// <summary>
    /// Checks if color profile in original and new file are the same
    /// </summary>
    /// <param name="files"> Takes in the two files used during comparison </param>
    /// <returns> Returns whether it passed the test </returns>
    public static bool FileColorProfileComparison(FilePair files)
    {
        var oFormat = files.OriginalFileFormat;
        var nFormat = files.NewFileFormat;

        try
        {
            return oFormat switch
            {
                _ when FormatCodes.PronomCodesImages.Contains(oFormat) && FormatCodes.PronomCodesImages.Contains(nFormat) 
                    => ImageToImageColorProfileComparison(files),
                _ when FormatCodes.PronomCodesImages.Contains(oFormat) && FormatCodes.PronomCodesPDFA.Contains(nFormat)
                    => ImageToPdfColorProfileComparison(files),
                _ when FormatCodes.PronomCodesPDFA.Contains(oFormat) && FormatCodes.PronomCodesPDFA.Contains(nFormat) =>
                    PdfToPdfColorProfileComparison(files),
                _ when FormatCodes.PronomCodesXMLBasedPowerPoint.Contains(oFormat) && FormatCodes.PronomCodesPDFA.Contains(nFormat)
                    => PowerPointToPdfColorProfileComparison(files),
                _ => throw new Exception("Unsupported comparison format.")
            };
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error while comparing color profiles: {e.Message}");
            return false; // If checking for color profile fails it automatically fails the test
        }
    }
    
    /// <summary>
    /// Compares the color profiles of two images
    /// </summary>
    /// <param name="files"></param>
    /// <returns></returns>
    public static bool ImageToImageColorProfileComparison(FilePair files)
    {
        using var oImage = new MagickImage(files.OriginalFilePath);
        using var nImage = new MagickImage(files.NewFilePath);
        return CompareColorProfiles(oImage, nImage);
    }
    
    /// <summary>
    /// Compares the color profiles of two PDF files
    /// </summary>
    /// <param name="files"></param>
    /// <returns></returns>
    public static bool PdfToPdfColorProfileComparison(FilePair files)
    {
        var oImages = ExtractImagesFromPdf(files.OriginalFilePath);
        var nImages = ExtractImagesFromPdf(files.NewFilePath);
        
        // If there are different number of images in the PDF files, it means there is a loss of data and we fail the test
        if (oImages.Count != nImages.Count)
        {
            return false;
        }
        // If there is only one image in each PDF file, we compare the color profiles of the images
        if (oImages.Count == 1 && nImages.Count == 1)
        {
            return CompareColorProfiles(oImages.First(), nImages.First());
        }
        // If there are multiple images in the PDF files, we compare the color profiles of each image
        return oImages.Count == nImages.Count && !oImages.Where((t, i) => !CompareColorProfiles(t, nImages[i])).Any();
    }
    
    /// <summary>
    /// Compares the color profile of an image and a PDF file
    /// </summary>
    /// <param name="files"></param>
    /// <returns></returns>
    public static bool ImageToPdfColorProfileComparison(FilePair files)
    {
        using var oImage = new MagickImage(files.OriginalFilePath);
        var nImages = ExtractImagesFromPdf(files.NewFilePath);
        
        // Check if more than one image is extracted from the PDF file
        return nImages.Count <= 1 && CompareColorProfiles(oImage, nImages.First());
    }
    
    public static bool PowerPointToPdfColorProfileComparison(FilePair files)
    {
        var oImages = ExtractImagesFromPowerPoint(files.OriginalFilePath);
        var nImages = ExtractImagesFromPdf(files.NewFilePath);

        // TODO: What if images are not in same order in original and new?

        return oImages.Count == nImages.Count && !oImages.Where((t, i) => !CompareColorProfiles(t, nImages[i])).Any();
    }

    public static bool WordToPdfColorProfileComparison(FilePair files)
    {
        // TODO: Implement Word to PDF color profile comparison
        return true;
    }

    public static bool ExcelToPdfColorProfileComparison(FilePair files)
    {
        // TODO: Implement Excel to PDF color profile comparison
        return true;
    }

    /// <summary>
    /// Extracts images from a PDF file
    /// </summary>
    /// <param name="filePath"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <returns></returns>
    private static List<MagickImage> ExtractImagesFromPdf(string filePath)
    {
        var extractedImages = new List<MagickImage>();

        using var pdfDocument = PdfDocument.Open(filePath);
        foreach (var page in pdfDocument.GetPages())
        {
            var images = page.GetImages();

            foreach (var image in images)
            {
                // Convert the raw image bytes to a MagickImage
                using var magickImage = new MagickImage(image.RawBytes);
                // Clone the image to avoid disposing it when the using block ends
                extractedImages.Add((MagickImage)magickImage.Clone());
            }
        }
        return extractedImages;
    }

    private static List<MagickImage> ExtractImagesFromPowerPoint(string filePath)
    {
        using var zip = ZipFile.OpenRead(filePath);
    
        var images = zip.Entries
            .Where(e => e.FullName.StartsWith("ppt/media/", StringComparison.OrdinalIgnoreCase))
            .Select(e =>
            {
                using var stream = e.Open();
                using var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                return new MagickImage(memoryStream.ToArray());
            })
            .ToList();
    
        return images;
    }

    /// <summary>
    /// Function checks that embedded color profile for two images are the same
    /// </summary>
    /// <param name="oImage"></param>
    /// <param name="nImage"></param>
    /// <returns></returns>
    public static bool CompareColorProfiles(MagickImage oImage, MagickImage nImage)
    {
        
        var oProfile = oImage.GetColorProfile();
        var nProfile = nImage.GetColorProfile();

        return oProfile switch
        {
            null when nProfile == null => true // If both images do not have color profiles it means no loss of data
            ,
            null => false // If only one image has a color profile it means loss of data
            ,
            _ => nProfile != null && // If only one image has a color profile it means loss of data
                 oProfile.Equals(nProfile)
        };
    }
}