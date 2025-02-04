using System;
using System.Collections.Generic;
using System.Linq;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;
using ImageMagick;
using UglyToad.PdfPig;

namespace AvaloniaDraft.ComparingMethods;

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
                _ when FormatCodes.PronomCodesImages.Contains(oFormat) && FormatCodes.PronomCodesPDF.Contains(nFormat)
                    => ImageToPdfColorProfileComparison(files),
                _ when FormatCodes.PronomCodesPDF.Contains(oFormat) && FormatCodes.PronomCodesPDF.Contains(nFormat) =>
                    PdfToPdfColorProfileComparison(files),
                _ => false
            };
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error while comparing color profiles: {e.Message}");
            return false; // If checking for color profile fails it automatically fails the test
        }
        
    }
    
    public static bool ImageToImageColorProfileComparison(FilePair files)
    {
        using var oImage = new MagickImage(files.OriginalFilePath);
        using var nImage = new MagickImage(files.NewFilePath);
        return CompareColorProfiles(oImage, nImage);
    }
    
    public static bool PdfToPdfColorProfileComparison(FilePair files)
    {
        var oImages = ExtractImagesFromPdf(files.OriginalFilePath);
        var nImages = ExtractImagesFromPdf(files.NewFilePath);
        
        // TODO
        // Check if there are more than one image in each PDF file
        
        return CompareColorProfiles(oImages.First(), nImages.First());
    }
    
    public static bool ImageToPdfColorProfileComparison(FilePair files)
    {
        using var oImage = new MagickImage(files.OriginalFilePath);
        var nImages = ExtractImagesFromPdf(files.NewFilePath);
        
        // TODO
        // Check if more than one image is extracted from the PDF file
        
        return CompareColorProfiles(oImage, nImages.First());
    }

    /// <summary>
    /// Extracts images from a PDF file
    /// </summary>
    /// <param name="filePath"></param>
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