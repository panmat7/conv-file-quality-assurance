using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using AvaloniaDraft.ComparingMethods.ExifTool;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ColorType = AvaloniaDraft.Helpers.ColorType;


namespace AvaloniaDraft.ComparingMethods;

public static class ComperingMethods
{
    /// <summary>
    /// Returns whether the size difference between two files exceeds expectations. 
    /// </summary>
    /// <param name="files">The two files to be compared</param>
    /// <param name="toleranceValue">The values </param>
    /// <returns>True/false whether the difference is too large. Null means that the size could not have been gotten.</returns>
    public static bool? CheckFileSizeDifference(FilePair files, double toleranceValue)
    {
        try
        {
            var originalSize = new FileInfo(files.OriginalFilePath).Length;
            var newSize = new FileInfo(files.NewFilePath).Length;
        
            return (long.Abs(originalSize - newSize) > originalSize * toleranceValue);
        }
        catch { return null; }
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
    /// Returns the page count using ExifTool to extract it from metadata
    /// </summary>
    /// <param name="files">Pair of files which are to be compared</param>
    /// <returns>Either a positive integer with page count difference, -1 meaning error while getting pages or null meaning not supported file type</returns>
    public static int? GetPageCountDifferenceExif(FilePair files)
    {
        try
        {
            var originalPages = GetPageCountExif(files.OriginalFilePath, files.OriginalFileFormat);
            var newPages = GetPageCountExif(files.NewFilePath, files.NewFileFormat);
            
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
    /// Returns the number of pages for a specific document
    /// </summary>
    /// <param name="path">Absolute path to the document</param>
    /// <param name="format">PRONOM code of the file type</param>
    /// <returns>Either a positive integer with page count, -1 meaning error while getting pages or null meaning not supported file type</returns>
    public static int? GetPageCountExif(string path, string format)
    {
        var result = GlobalVariables.ExifTool.GetExifDataDictionary([path], false);
        
        if(result == null || result.Count == 0) return null;

        try
        {
            var propertyName = "";
            
            if (FormatCodes.PronomCodesDOCX.Contains(format))
            {
                //Unboxing the value
                propertyName = "Pages";
            }

            if (FormatCodes.PronomCodesODT.Contains(format))
            {
                propertyName = "Document-statisticPage-count";
            }
            
            if (FormatCodes.PronomCodesPDF.Contains(format) || FormatCodes.PronomCodesPDFA.Contains(format))
            {
                propertyName = "PageCount";
            }
        
            //Does not work for OpenDocument Presentations
            if (FormatCodes.PronomCodesPresentationDocuments.Contains(format) && !FormatCodes.PronomCodesODP.Contains(format))
            {
                propertyName = "Slides";
            }

            if (propertyName == "")
                return null;
            
            var unboxed = result[0].TryGetValue(propertyName, out var pages) && pages is JsonElement jpages && jpages.TryGetInt32(out int ipages)
                ? ipages
                : 0;
            return unboxed;
        }
        catch { return -1; }
    }
    
    /// <summary>
    /// Returns a list of all metadata missing or wrongly written in the files.
    /// </summary>
    /// <param name="files">Files to be checked</param>
    /// <returns>Dictionary of error-type to error-description. Null meaning error while getting data. Empty list meaning no errors.</returns>
    public static List<Error>? GetMissingOrWrongImageMetadataExif(FilePair files)
    {
        //Get metadata
        var metaOriginal = GlobalVariables.ExifTool.GetExifDataImageMetadata([files.OriginalFilePath])?[0];
        var metaNew = GlobalVariables.ExifTool.GetExifDataImageMetadata([files.NewFilePath])?[0];

        if (metaOriginal == null || metaNew == null) return null;
        
        //Standardize
        var originalStandardized = MetadataStandardizer.StandardizeImageMetadata(metaOriginal, files.OriginalFileFormat);
        var newStandardized = MetadataStandardizer.StandardizeImageMetadata(metaNew, files.NewFileFormat);
        
        var errors = new List<Error>();
        
        //Check properties, note errors and mismatches
        if (!originalStandardized.VerifyResolution())
        {
            errors.Add(new Error(
                "Image resolution missing in original file",
                "Error trying to get original image resolution.",
                ErrorSeverity.High,
                ErrorType.Metadata
            ));
        }
        
        if(!newStandardized.VerifyResolution())
        {
            errors.Add(new Error(
                "Image resolution missing in new file",
                "Error trying to get new image resolution.",
                ErrorSeverity.High,
                ErrorType.Metadata
            ));
        }
        
        if (!originalStandardized.CompareResolution(newStandardized))
        {
            errors.Add(new Error(
                "Image resolution difference (metadata)",
                "Mismatched resolution between images in metadata.",
                ErrorSeverity.High,
                ErrorType.Metadata
            ));
        }

        if (!originalStandardized.VerifyBitDepth())
        {
            errors.Add(new Error(
                "Bit-depth missing in original file",
                "Error trying to get original image bit-depth.",
                ErrorSeverity.Medium,
                ErrorType.Metadata
            ));
        }

        if (!newStandardized.VerifyBitDepth())
        {
            errors.Add(new Error(
                "Bit-depth missing in new file",
                "Error trying to get new image bit-depth",
                ErrorSeverity.Medium,
                ErrorType.Metadata
            ));
        }

        if (!originalStandardized.CompareBitDepth(newStandardized))
        {
            errors.Add(new Error(
                "Bit-depth mismatch",
                "Mismatched bit-depth between images.",
                ErrorSeverity.Medium,
                ErrorType.Metadata
            ));
        }
        
        if (!originalStandardized.VerifyColorType())
        {
            errors.Add(new Error(
                "Color type missing in original file",
                "Error trying to get original image color type.",
                ErrorSeverity.Medium,
                ErrorType.Metadata
            ));
        }

        if (!newStandardized.VerifyColorType())
        {
            errors.Add(new Error(
                "Color type missing in new file",
                "Error trying to get new image color type",
                ErrorSeverity.Medium,
                ErrorType.Metadata
            ));
        }

        if(VerifyColorType(originalStandardized, newStandardized) is { } error)
            errors.Add(error);

        if(!originalStandardized.VerifyPhysicalUnits() ^ !newStandardized.VerifyPhysicalUnits()) //XOR
        {
            errors.Add(new Error(
                "Physical units missing",
                "Only one file contains physical units.",
                ErrorSeverity.Medium,
                ErrorType.Metadata
            ));
        }
        else if(!originalStandardized.ComparePhysicalUnits(newStandardized))
        {
            errors.Add(new Error(
                "Physical units mismatch",
                "Mismatched physical units between images",
                ErrorSeverity.Medium,
                ErrorType.Metadata
            ));
        }
        
        errors.AddRange(originalStandardized.GetMissingAdditionalValues(newStandardized));
        
        return errors;
    }
    
    /// <summary>
    /// Verifies color type of two image metadata, providing additional warning in case of transparency loss
    /// </summary>
    /// <param name="orgMeta">Metadata of the original file</param>
    /// <param name="newMeta">Metadata of the new file</param>
    /// <returns>Null if no difference was found, otherwise an Error object.</returns>
    private static Error? VerifyColorType(StandardizedImageMetadata orgMeta, StandardizedImageMetadata newMeta)
    {
        if (orgMeta.ColorType == newMeta.ColorType) return null;
        
        if ((orgMeta.Format == "png" || orgMeta.Format == "tiff") && (orgMeta.ColorType == ColorType.RGBA))
        {
            if(ContainsTransparency(orgMeta.Path) && newMeta.ColorType != ColorType.RGBA)
            {
                return new Error(
                    "Color type mismatch",
                    "Mismatched color type between images. Transparency loss",
                    ErrorSeverity.High,
                    ErrorType.Metadata
                );
            }
        }
        
        
        return new Error(
            "Color type mismatch",
            "Mismatched color type between images",
            ErrorSeverity.Medium,
            ErrorType.Metadata
        );
    }
    
    /// <summary>
    /// Checks if an image contains transparency by checking if any of the pixels has an A value bellow 255
    /// </summary>
    /// <param name="filePath">Absolute path to the image</param>
    /// <returns>True or false</returns>
    public static bool ContainsTransparency(string filePath)
    {
        using var image = Image.Load(filePath);
        
        //Trying to ensure rgba32 format
        if (image is not Image<Rgba32> rgbaImage)
        {
            rgbaImage = image.CloneAs<Rgba32>();
        }
            
        var harTransparency = false;
        
        rgbaImage.ProcessPixelRows(accessor => {
                for (int i = 0; i < accessor.Height; i++)
                {
                    var row = accessor.GetRowSpan(i);

                    for (int j = 0; j < row.Length; j++)
                    {
                        if (row[j].A == 255) continue;
                        harTransparency = true;
                        return;
                    }
                }
            }
        );
            
        return harTransparency;
    }
}