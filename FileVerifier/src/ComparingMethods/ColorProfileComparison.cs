using System;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;
using ImageMagick;

namespace AvaloniaDraft.ComparingMethods;

public class ColorProfileComparison
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
                    => ImageToImageColorProfileComparison(files.OriginalFilePath, files.NewFilePath),
                _ when FormatCodes.PronomCodesImages.Contains(oFormat) && FormatCodes.PronomCodesPDF.Contains(nFormat)
                    => ImageToPdfColorProfileComparison(files.OriginalFilePath, files.NewFilePath),
                _ when FormatCodes.PronomCodesPDF.Contains(oFormat) && FormatCodes.PronomCodesPDF.Contains(nFormat) =>
                    PdfToPdfColorProfileComparison(files.OriginalFilePath, files.NewFilePath),
                _ => false
            };
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error while comparing color profiles: {e.Message}");
            return false; // If checking for color profile fails it automatically fails the test
        }
        
    }
    
    public static bool ImageToImageColorProfileComparison(string originalImagePath, string newImagePath)
    {
        //TODO
        
        // Extract the images for each file
        
        // Gather color profiles if present
        
        // Compare color profiles
        
        // Return result
        
        return true;
    }
    
    public static bool PdfToPdfColorProfileComparison(string originalPdfPath, string newPdfPath)
    {
        //TODO
        
        // Extract the images for each file
        
        // Gather color profiles if present
        
        // Compare color profiles
        
        // Return result
        
        return true;
    }
    
    public static bool ImageToPdfColorProfileComparison(string originalImagePath, string newPdfPath)
    {
        //TODO
        
        // Extract the images for each file
        
        // Gather color profiles if present
        
        // Compare color profiles
        
        // Return result
        
        return true;
    }
    
    /// <summary>
    /// Function checks that embedded color profile for two images are the same
    /// </summary>
    /// <param name="oPath"></param>
    /// <param name="nPath"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static bool CompareColorProfiles(string oPath, string nPath)
    {
        using var oImage = new MagickImage(oPath);
        using var nImage = new MagickImage(nPath);
        
        var oProfile = oImage.GetColorProfile();
        
        if (oProfile == null) throw new Exception("Missing color profile embedded in original image");
        
        var nProfile = nImage.GetColorProfile();
        
        if (nProfile == null) throw new Exception("Missing color profile embedded in new image");
        
        return oProfile.Equals(nProfile);
    }
}

// Check if the files include images
        
// Extract the images for each file
        
// Gather color profiles if present
        
// Compare color profiles
        
// Return result