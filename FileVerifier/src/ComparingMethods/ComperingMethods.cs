using System;
using System.Collections.Generic;
using System.IO;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Text.Json;
using Aspose.Slides;
using AvaloniaDraft.ComparingMethods.ExifTool;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.PixelFormats;
using UglyToad.PdfPig;
using Document = Aspose.Words.Document;
using ImageMetadata = AvaloniaDraft.ComparingMethods.ExifTool.ImageMetadata;


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
        var result = ExifToolStatic.GetExifDataDictionary([path], GlobalVariables.ExifPath, false);
        
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
        var metaOriginal = ExifToolStatic.GetExifDataImageMetadata([files.OriginalFilePath], GlobalVariables.ExifPath)?[0];
        var metaNew = ExifToolStatic.GetExifDataImageMetadata([files.NewFilePath], GlobalVariables.ExifPath)?[0];

        if (metaOriginal == null || metaNew == null) return null;
        
        //Standardize
        var originalStandardized = MetadataStandardizer.StandardizeImageMetadata(metaOriginal, files.OriginalFileFormat);
        var newStandardized = MetadataStandardizer.StandardizeImageMetadata(metaNew, files.NewFileFormat);
        
        var errors = new List<Error>();
        
        //Check properties, note errors and mismatches
        if (!originalStandardized.VerifyResolution())
        {
            errors.Add(new Error(
                "ImageResolutionOriginal",
                "Error trying to get original image resolution",
                ErrorSeverity.High,
                ErrorType.Metadata
            ));
        }
        
        if(!newStandardized.VerifyResolution())
        {
            errors.Add(new Error(
                "ImageResolutionNew",
                "Error trying to get new image resolution",
                ErrorSeverity.High,
                ErrorType.Metadata
            ));
        }
        
        if (!originalStandardized.CompareResolution(newStandardized))
        {
            errors.Add(new Error(
                "ImageResolution",
                "Mismatched resolution between images",
                ErrorSeverity.High,
                ErrorType.Metadata
            ));
        }

        if (!originalStandardized.VerifyBitDepth())
        {
            errors.Add(new Error(
                "BitDepthOriginal",
                "Error trying to get original image bit-depth",
                ErrorSeverity.Medium,
                ErrorType.Metadata
            ));
        }

        if (!newStandardized.VerifyBitDepth())
        {
            errors.Add(new Error(
                "BitDepthNew",
                "Error trying to get new image bit-depth",
                ErrorSeverity.Medium,
                ErrorType.Metadata
            ));
        }

        if (!originalStandardized.CompareBitDepth(newStandardized))
        {
            errors.Add(new Error(
                "BitDepth",
                "Mismatched resolution between images",
                ErrorSeverity.Medium,
                ErrorType.Metadata
            ));
        }
        
        if (!originalStandardized.VerifyColorType())
        {
            errors.Add(new Error(
                "ColorTypeOriginal",
                "Error trying to get original image color type",
                ErrorSeverity.Medium,
                ErrorType.Metadata
            ));
        }

        if (!newStandardized.VerifyColorType())
        {
            errors.Add(new Error(
                "ColorTypeNew",
                "Error trying to get new image color type",
                ErrorSeverity.Medium,
                ErrorType.Metadata
            ));
        }

        if (!originalStandardized.CompareColorType(newStandardized))
        {
            errors.Add(new Error(
                "ColorType",
                "Mismatched color type between images",
                ErrorSeverity.Medium,
                ErrorType.Metadata
            ));
        }

        if(!originalStandardized.VerifyPhysicalUnits() || !newStandardized.VerifyPhysicalUnits())
        {
            errors.Add(new Error(
                "PhysicalUnitsMissing",
                "Error trying to get original physical units",
                ErrorSeverity.Medium,
                ErrorType.Metadata
            ));
        }
        else if(!originalStandardized.ComparePhysicalUnits(newStandardized))
        {
            errors.Add(new Error(
                "PhysicalUnits",
                "Mismatched physical units between images",
                ErrorSeverity.Medium,
                ErrorType.Metadata
            ));
        }
        
        errors.AddRange(originalStandardized.GetMissingAdditionalValues(newStandardized));
        
        return errors;
    }
    
    /// <summary>
    /// Checks if an image contains transparency by checking if any of the pixels has an A value bellow 255
    /// </summary>
    /// <param name="filePath">Absolute path to the image</param>
    /// <param name="format">PRONOM code of the file type</param>
    /// <returns>True or false</returns>
    public static bool ContainsTransparency(string filePath, string format)
    {
        //Only PNG and TIFF support transparency
        if (!FormatCodes.PronomCodesPNG.Contains(format) && !FormatCodes.PronomCodesTIFF.Contains(format)) return false;

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