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
using ColorType = AvaloniaDraft.Helpers.ColorType;
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
    public static double PbpImageComparing(FilePair files) // TODO thread allowed to start parameter
    // TODO overload slik at det er mulig å sende inn to bilder istedenfor et filepair
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
                "ImageResolutionOriginalMissing",
                "Error trying to get original image resolution",
                ErrorSeverity.High,
                ErrorType.Metadata
            ));
        }
        
        if(!newStandardized.VerifyResolution())
        {
            errors.Add(new Error(
                "ImageResolutionNewMissing",
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
                "BitDepthOriginalMissing",
                "Error trying to get original image bit-depth",
                ErrorSeverity.Medium,
                ErrorType.Metadata
            ));
        }

        if (!newStandardized.VerifyBitDepth())
        {
            errors.Add(new Error(
                "BitDepthNewMissing",
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
                "ColorTypeOriginalMissing",
                "Error trying to get original image color type",
                ErrorSeverity.Medium,
                ErrorType.Metadata
            ));
        }

        if (!newStandardized.VerifyColorType())
        {
            errors.Add(new Error(
                "ColorTypeNewMissing",
                "Error trying to get new image color type",
                ErrorSeverity.Medium,
                ErrorType.Metadata
            ));
        }

        if(VerifyColorType(originalStandardized, newStandardized) is { } error)
            errors.Add(error);

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
    /// Verifies color type of two image metadata, providing additional warning in case of transparency loss
    /// </summary>
    /// <param name="orgMeta"></param>
    /// <param name="newMeta"></param>
    /// <returns></returns>
    private static Error? VerifyColorType(StandardizedImageMetadata orgMeta, StandardizedImageMetadata newMeta)
    {
        if (orgMeta.ColorType == newMeta.ColorType) return null;
        
        if ((orgMeta.Format == "png" || orgMeta.Format == "tiff") && (orgMeta.ColorType == ColorType.RGBA))
        {
            if(ContainsTransparency(orgMeta.Path) && newMeta.ColorType != ColorType.RGBA)
            {
                return new Error(
                    "ColorType",
                    "Mismatched color type between images. Transparency loss",
                    ErrorSeverity.High,
                    ErrorType.Metadata
                );
            }
        }
        
        
        return new Error(
            "ColorType",
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