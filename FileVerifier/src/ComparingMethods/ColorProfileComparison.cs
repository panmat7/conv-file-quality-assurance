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
        return CompareColorProfiles(files.OriginalFilePath, files.NewFilePath);
    }
    
    public static bool PdfToPdfColorProfileComparison(FilePair files)
    {
        //TODO
        
        // Extract the images for each file
        
        // Gather color profiles if present
        
        // Compare color profiles
        
        // Return result
        
        return true;
    }
    
    public static bool ImageToPdfColorProfileComparison(FilePair files)
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
    public static bool CompareColorProfiles(string oPath, string nPath)
    {
        using var oImage = new MagickImage(oPath);
        using var nImage = new MagickImage(nPath);
        
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

// Check if the files include images
        
// Extract the images for each file
        
// Gather color profiles if present
        
// Compare color profiles
        
// Return result