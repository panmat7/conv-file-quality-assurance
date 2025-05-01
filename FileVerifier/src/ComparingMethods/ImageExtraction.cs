using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ImageMagick;
using UglyToad.PdfPig;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Xml.Linq;
using AvaloniaDraft.Helpers;
using MimeKit;
using UglyToad.PdfPig.Content;
using RtfDomParser;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;

namespace AvaloniaDraft.ComparingMethods;

public static class ImageExtractionToDisk
{
    private const string ImageMimeTypePrefix = "image/";
    
    /****************************************************PDF IMAGES****************************************************/

    public static void ExtractImagesToDisk(string filePath, string formatCode, string outputPath)
    {
        if (FormatCodes.PronomCodesAllPDF.Contains(formatCode))
            ExtractImagesFromPdfToDisk(filePath, outputPath);
        
        if(FormatCodes.PronomCodesDOCX.Contains(formatCode))
            ExtractImagesFromDocxToDisk(filePath, outputPath);
        
        if(FormatCodes.PronomCodesXMLBasedPowerPoint.Contains(formatCode))
            ExtractImagesFromXmlBasedPowerPointToDisk(filePath, outputPath);
        
        if(FormatCodes.PronomCodesXLSX.Contains(formatCode))
            ExtractImagesFromXlsxToDisk(filePath, outputPath);
        
        if(FormatCodes.PronomCodesXML.Contains(formatCode))
            ExtractImagesFromXlsxToDisk(filePath, outputPath);
        
        if(FormatCodes.PronomCodesODT.Contains(formatCode) || FormatCodes.PronomCodesODP.Contains(formatCode)
           || FormatCodes.PronomCodesODS.Contains(formatCode))
            ExtractImagesFromOpenDocumentsToDisk(filePath, outputPath);
    }
    
    
    /// <summary>
    /// This function extracts all images inside a pdf to disk.
    /// Based on the code from https://stackoverflow.com/questions/26774740/extract-image-from-pdf-using-c-sharp
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="outputDirectory"></param>
    public static void ExtractImagesFromPdfToDisk(string filePath, string outputDirectory)
    {
        var imageHashes = new HashSet<string>();
        using var document = PdfDocument.Open(filePath);
        foreach (var page in document.GetPages())
        {
            foreach (var pdfImage in page.GetImages())
            {
                var bytes = TryGetImage(pdfImage);
                var hash = ComputeHash(bytes);
                if (!imageHashes.Add(hash)) continue;
                using var mem = new MemoryStream(bytes);
                using var img = Image.Load(mem);
                    
                var outputPath = Path.Combine(outputDirectory, $"{Guid.NewGuid()}.png");
                img.Save(outputPath, new PngEncoder());
            }
        }
        byte[] TryGetImage(IPdfImage image)
        {
            if (image.TryGetPng(out var bytes))
                return bytes;
            return image.TryGetBytesAsMemory(out var iroBytes) ? iroBytes.ToArray() : image.RawBytes.ToArray();
        }
    }
    
    /// <summary>
    /// Computes the hash of the image content
    /// </summary>
    /// <param name="rawBytes"></param>
    /// <returns></returns>
    private static string ComputeHash(ReadOnlySpan<byte> rawBytes)
    {
        var hashBytes = MD5.HashData(rawBytes.ToArray());
        return Convert.ToBase64String(hashBytes);
    }
    
    /****************************************************OPEN DOCUMENT IMAGES****************************************************/
    
    /// <summary>
    /// The function extracts images from OpenDocument files to disk
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="outputDirectory"></param>
    public static void ExtractImagesFromOpenDocumentsToDisk(string filePath, string outputDirectory)
    {
        using var zip = ZipFile.OpenRead(filePath);

        var entries = zip.Entries.Where(e =>
            (e.FullName.StartsWith("Pictures/", StringComparison.OrdinalIgnoreCase) ||
             e.FullName.StartsWith("media/", StringComparison.OrdinalIgnoreCase)) &&
            !e.FullName.Contains("/../", StringComparison.OrdinalIgnoreCase));
        
        foreach (var entry in entries)
        {
            var fileName = Path.GetFileName(entry.FullName);
            // Skip filenames that are '..' or contain '..' which could be used for traversal
            if (fileName == ".." || fileName.Contains(".."))
            {
                continue;
            }

            var outputPath = Path.Combine(outputDirectory, fileName);
            var fullOutputPath = Path.GetFullPath(outputPath);
            var fullOutputDir = Path.GetFullPath(outputDirectory.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);

            if (!fullOutputPath.StartsWith(fullOutputDir, StringComparison.OrdinalIgnoreCase))
            {
                throw new IOException("Attempted to extract a file outside the output directory.");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(fullOutputPath)!);
            entry.ExtractToFile(fullOutputPath, true);
        }
    }
    
    /****************************************************RTF IMAGES****************************************************/

    /// <summary>
    /// Extracts images from RTF files to disk
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="outputDirectory"></param>
    public static void ExtractImagesFromRtfToDisk(string filePath, string outputDirectory)
    {
        var imageHashes = new HashSet<string>();

        var doc = new RTFDomDocument();
        doc.Load(filePath);
        
        // Recursively search for images in all elements in the document
        TraverseElementsForSavingToDisk(doc.Elements, outputDirectory, imageHashes);
    }
    
    /// <summary>
    /// Traverses elements in the DOM tree of an RTF document to find images and save them to disk.
    /// </summary>
    /// <param name="docElements"></param>
    /// <param name="outputDirectory"></param>
    /// <param name="imageHashes"></param>
    private static void TraverseElementsForSavingToDisk(RTFDomElementList docElements, string outputDirectory, HashSet<string> imageHashes)
    {
        foreach (var element in docElements)
        {
            switch (element)
            {
                case RTFDomImage image:
                {
                    // Deduplicate images using a hash
                    var hash = Convert.ToBase64String(MD5.HashData(image.Data));
                    if (!imageHashes.Add(hash)) break;

                    var outputPath = Path.Combine(outputDirectory, $"{Guid.NewGuid()}.png");
                    File.WriteAllBytes(outputPath, image.Data);
                    break;
                }
                case RTFDomShapeGroup shapeGroup:
                    // Check if the shape group contains child elements (like images)
                    TraverseElementsForSavingToDisk(shapeGroup.Elements, outputDirectory, imageHashes);
                    break;
                case RTFDomParagraph paragraph:
                    // Paragraphs might contain nested elements (e.g., shapes/images)
                    TraverseElementsForSavingToDisk(paragraph.Elements, outputDirectory, imageHashes);
                    break;
                case RTFDomTableCell cell:
                    // Cells might contain nested elements (e.g., paragraphs)
                    TraverseElementsForSavingToDisk(cell.Elements, outputDirectory, imageHashes);
                    break;
                case RTFDomTableRow row:
                    // Rows might contain nested elements (e.g., cells)
                    TraverseElementsForSavingToDisk(row.Elements, outputDirectory, imageHashes);
                    break;
                case RTFDomTable table:
                    // Tables might contain nested elements (e.g., rows)
                    TraverseElementsForSavingToDisk(table.Elements, outputDirectory, imageHashes);
                    break;
            }
        }
    }
    
    /****************************************************EMAIL IMAGES****************************************************/

    /// <summary>
    /// Extracts images from an eml file to disk
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="outputDirectory"></param>
    public static void ExtractImagesFromEmlToDisk(string filePath, string outputDirectory)
    {
        var imageHashes = new HashSet<string>();

        var message = MimeMessage.Load(filePath);

        if (message.Body is not Multipart multipart) return;

        foreach (var part in multipart)
        {
            switch (part)
            {
                // If the part is a multipart/related, we extract images from it
                case MultipartRelated related:
                    ExtractImagesFromMultipartRelatedToDisk(related, outputDirectory, imageHashes);
                    break;
                // If the part is an image, we extract it
                case MimePart mimeAttachment when mimeAttachment.ContentType.MimeType.StartsWith(ImageMimeTypePrefix):
                    ExtractImageFromMimePartToDisk(mimeAttachment, outputDirectory, imageHashes);
                    break;
            }
        }
    }

    /// <summary>
    /// Extracts an image from a MimePart and saves it to disk
    /// </summary>
    /// <param name="mimeAttachment"></param>
    /// <param name="outputDirectory"></param>
    /// <param name="imageHashes"></param>
    private static void ExtractImageFromMimePartToDisk(MimePart mimeAttachment, string outputDirectory, HashSet<string> imageHashes)
    {
        using var memoryStream = new MemoryStream();
        mimeAttachment.Content.DecodeTo(memoryStream);
        memoryStream.Position = 0;

        var hash = Convert.ToBase64String(MD5.HashData(memoryStream.ToArray()));
        if (!imageHashes.Add(hash)) return;

        var outputPath = Path.Combine(outputDirectory, $"{Guid.NewGuid()}.png");
        File.WriteAllBytes(outputPath, memoryStream.ToArray());
    }

    /// <summary>
    /// Extracts images from a MultipartRelated and saves them to disk
    /// </summary>
    /// <param name="related"></param>
    /// <param name="outputDirectory"></param>
    /// <param name="imageHashes"></param>
    private static void ExtractImagesFromMultipartRelatedToDisk(MultipartRelated related, string outputDirectory, HashSet<string> imageHashes)
    {
        foreach (var resource in related)
        {
            if (resource is MimePart mimePart && mimePart.ContentType.MimeType.StartsWith(ImageMimeTypePrefix))
            {
                ExtractImageFromMimePartToDisk(mimePart, outputDirectory, imageHashes);
            }
        }
    }
    
    /****************************************************XML BASED POWERPOINT IMAGES****************************************************/
    
    /// <summary>
    /// Extracts all images from a xml based PowerPoint format to disk
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="outputDirectory"></param>
    public static void ExtractImagesFromXmlBasedPowerPointToDisk(string filePath, string outputDirectory)
    {
        using var zip = ZipFile.OpenRead(filePath);

        foreach (var entry in zip.Entries.Where(e => 
                     e.FullName.StartsWith("ppt/media/", StringComparison.OrdinalIgnoreCase) &&
                     !e.FullName.Contains("/../", StringComparison.OrdinalIgnoreCase)))
        {
            var fileName = Path.GetFileName(entry.FullName);
            if (fileName == ".." || fileName.Contains(".."))
            {
                continue;
            }

            var outputPath = Path.Combine(outputDirectory, fileName);
            var fullOutputPath = Path.GetFullPath(outputPath);
            var fullOutputDir = Path.GetFullPath(outputDirectory.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);

            if (!fullOutputPath.StartsWith(fullOutputDir, StringComparison.OrdinalIgnoreCase))
            {
                throw new IOException("Attempted to extract PowerPoint image outside output directory");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(fullOutputPath)!);
            entry.ExtractToFile(fullOutputPath, true);
        }
    }
    
    /****************************************************DOCX IMAGES****************************************************/
    
    /// <summary>
    /// Extracts all images from a .docx file to disk
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="outputDirectory"></param>
    public static void ExtractImagesFromDocxToDisk(string filePath, string outputDirectory)
    {
        using var zip = ZipFile.OpenRead(filePath);

        foreach (var entry in zip.Entries.Where(e => 
                     e.FullName.StartsWith("word/media/", StringComparison.OrdinalIgnoreCase) &&
                     !e.FullName.Contains("/../", StringComparison.OrdinalIgnoreCase)))
        {
            var fileName = Path.GetFileName(entry.FullName);
            if (fileName == ".." || fileName.Contains(".."))
            {
                continue;
            }

            var outputPath = Path.Combine(outputDirectory, fileName);
            var fullOutputPath = Path.GetFullPath(outputPath);
            var fullOutputDir = Path.GetFullPath(outputDirectory.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);

            if (!fullOutputPath.StartsWith(fullOutputDir, StringComparison.OrdinalIgnoreCase))
            {
                throw new IOException("Attempted to extract DOCX image outside output directory");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(fullOutputPath)!);
            entry.ExtractToFile(fullOutputPath, true);
        }
    }
    
    /****************************************************XLSX IMAGES****************************************************/
    
    /// <summary>
    /// Extracts all images from a .xlsx file to disk
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="outputDirectory"></param>
    public static void ExtractImagesFromXlsxToDisk(string filePath, string outputDirectory)
    {
        using var zip = ZipFile.OpenRead(filePath);
    
        foreach (var entry in zip.Entries.Where(e => 
                     e.FullName.StartsWith("xl/media/", StringComparison.OrdinalIgnoreCase) &&
                     !e.FullName.Contains("/../", StringComparison.OrdinalIgnoreCase)))
        {
            var fileName = Path.GetFileName(entry.FullName);
            if (fileName == ".." || fileName.Contains(".."))
            {
                continue;
            }

            var outputPath = Path.Combine(outputDirectory, fileName);
            var fullOutputPath = Path.GetFullPath(outputPath);
            var fullOutputDir = Path.GetFullPath(outputDirectory.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);

            if (!fullOutputPath.StartsWith(fullOutputDir, StringComparison.OrdinalIgnoreCase))
            {
                throw new IOException("Attempted to extract XLSX image outside output directory");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(fullOutputPath)!);
            entry.ExtractToFile(fullOutputPath, true);
        }
    }
    
    /// <summary>
    /// Returns the expected PRONOM code for the given magick image format.
    /// </summary>
    /// <param name="oFormat"></param>
    /// <returns></returns>
    public static string? GetExpectedPronomFromImage(MagickFormat oFormat)
    {
        string? expectedPronom; //Note that this might not be the exact code, but it should serve to distinguish format group.
        switch (oFormat)
        {
            case MagickFormat.Png:
                expectedPronom = FormatCodes.PronomCodesPNG.FormatCodes[0];
                break;
            case MagickFormat.Jpeg:
                expectedPronom = FormatCodes.PronomCodesJPEG.FormatCodes[0];
                break;
            case MagickFormat.Gif:
                expectedPronom = FormatCodes.PronomCodesGIF.FormatCodes[0];
                break;
            case MagickFormat.Tiff:
                expectedPronom = FormatCodes.PronomCodesTIFF.FormatCodes[0];
                break;
            case MagickFormat.Bmp:
                expectedPronom = FormatCodes.PronomCodesBMP.FormatCodes[0];
                break;
            default:
                return null;
        }
        return expectedPronom;
    }
    
    /// <summary>
    /// Deletes all saved files/images in the given directory.
    /// </summary>
    /// <param name="directory"></param>
    public static void DeleteSavedFiles(string directory)
    {
        if (!Directory.Exists(directory)) return;
        var files = Directory.GetFiles(directory);
        foreach (var file in files)
        {
            File.Delete(file);
        }
    }
    
    /// <summary>
    /// Checks if two directories have the same number of images.
    /// </summary>
    /// <param name="dir1"></param>
    /// <param name="dir2"></param>
    /// <returns></returns>
    public static bool CheckIfEqualNumberOfImages(string dir1, string dir2)
    {
        var oFiles = Directory.GetFiles(dir1).ToArray();
        var nFiles = Directory.GetFiles(dir2).ToArray();

        return oFiles.Length == nFiles.Length;
    }
    
    /// <summary>
    /// Saves an MagickImage object as an actual image to disk. 
    /// </summary>
    /// <param name="image">The MagickImage object to be saved.</param>
    /// <param name="oFormat">Format of the original, using which the object will be encoded.</param>
    /// <returns>A tuple containing the path to the file and its expected PRONOM code, null if an error occured</returns>
    [Obsolete]
    [ExcludeFromCodeCoverage]
    public static (string, string)? SaveExtractedMagickImageToDisk(MagickImage image, MagickFormat oFormat)
    {
        try
        {
            string ext;
            string?
                expectedPronom; //Note that this might not be the exact code, but it should serve to distinguish format group.
            switch (oFormat)
            {
                case MagickFormat.Png:
                    image.Format = MagickFormat.Png;
                    ext = ".png";
                    expectedPronom = FormatCodes.PronomCodesPNG.FormatCodes[0];
                    break;
                case MagickFormat.Jpeg:
                    image.Format = MagickFormat.Jpeg;
                    ext = ".jpg";
                    expectedPronom = FormatCodes.PronomCodesJPEG.FormatCodes[0];
                    break;
                case MagickFormat.Gif:
                    image.Format = MagickFormat.Gif;
                    ext = ".gif";
                    expectedPronom = FormatCodes.PronomCodesGIF.FormatCodes[0];
                    break;
                case MagickFormat.Tiff:
                    image.Format = MagickFormat.Tiff;
                    ext = ".tiff";
                    expectedPronom = FormatCodes.PronomCodesTIFF.FormatCodes[0];
                    break;
                case MagickFormat.Bmp:
                    image.Format = MagickFormat.Bmp;
                    ext = ".bmp";
                    expectedPronom = FormatCodes.PronomCodesBMP.FormatCodes[0];
                    break;
                default:
                    return null;
            }

            byte[] nImageBytes;
            using (var ms = new MemoryStream())
            {
                image.Write(ms);
                nImageBytes = ms.ToArray();
            }

            var tempDirs = GlobalVariables.FileManager!.GetTempDirectories();
            var tempFilePath = TempFiles.CreateTemporaryFile(nImageBytes, tempDirs.Item2, ext);

            if (tempFilePath == null) return null;

            return (tempFilePath, expectedPronom);
        }
        catch
        {
            return null;
        }
    }
}

[Obsolete("This class is obsolete. Use ImageExtractionToDisk instead.")]
[ExcludeFromCodeCoverage]
public static class ImageExtractionToMemory
{
    private const string ImageMimeTypePrefix = "image/";
    
    /// <summary>
    /// Extracts all non-duplicate images from a pdf in MagickImage format
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static List<MagickImage> ExtractImagesFromPdf(string filePath)
    {
        var pdfImages = GetNonDuplicatePdfImages(filePath);
        return ConvertPdfImagesToMagickImages(pdfImages);
    }

    /// <summary>
    /// Gets all non duplicate Pdf images from a document
    /// </summary>
    /// <param name="filePath"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <returns></returns>
    public static List<IPdfImage> GetNonDuplicatePdfImages(string filePath)
    {
        var extractedImages = new List<IPdfImage>();

        // Hash set to store the hash of the image content to avoid duplicates where the same image is used multiple times
        var imageHashes = new HashSet<string>();

        using var pdfDocument = PdfDocument.Open(filePath);
        foreach (var page in pdfDocument.GetPages())
        {
            var images = page.GetImages().ToList();

            // Only extract images that are not already in the hash set
            extractedImages.AddRange(from image in images
                let hash = ComputeHash(image.RawBytes)
                where imageHashes.Add(hash)
                select image);
        }

        return extractedImages;
    }

    /// <summary>
    /// Converts the images from a pdf to MagickImage which is used for color profile comparison
    /// </summary>
    /// <param name="pdfImages"></param>
    /// <returns></returns>
    public static List<MagickImage> ConvertPdfImagesToMagickImages(List<IPdfImage> pdfImages)
    {
        var magickImages = new List<MagickImage>();

        // Only extract images that are not already in the hash set
        foreach (var image in pdfImages)
        {
            if (image.TryGetPng(out var pngBytes))
            {
                var magickImage = new MagickImage(pngBytes);
                magickImages.Add(magickImage);
            }
            else
            {
                var magickImage = new MagickImage(image.RawBytes);
                magickImages.Add(magickImage);
            }
        }

        return magickImages;
    }

    /// <summary>
    /// Extracts images frm OpenDocument files
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static List<MagickImage> ExtractImagesFromOpenDocuments(string filePath)
    {
        using var zip = ZipFile.OpenRead(filePath);

        var images = zip.Entries
            .Where(e => e.FullName.StartsWith("Pictures/", StringComparison.OrdinalIgnoreCase))
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
    /// Extracts images from .rtf files
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static List<MagickImage> ExtractImagesFromRtf(string filePath)
    {
        var images = new List<MagickImage>();
        var imageHashes = new HashSet<string>();

        var doc = new RTFDomDocument();
        doc.Load(filePath);

        // Recursively search for images in all elements in the document
        TraverseElements(doc.Elements, images, imageHashes);

        return images;
    }

    /// <summary>
    /// Traverses items recursively to find images in RTF documents
    /// </summary>
    /// <param name="elements"></param>
    /// <param name="images"></param>
    /// <param name="imageHashes"></param>
    private static void TraverseElements(
        RTFDomElementList elements,
        List<MagickImage> images,
        HashSet<string> imageHashes)
    {
        foreach (var element in elements)
        {
            switch (element)
            {
                case RTFDomImage image:
                {
                    // Deduplicate images using a hash
                    var hash = Convert.ToBase64String(MD5.HashData(image.Data));
                    if (!imageHashes.Contains(hash))
                    {
                        var magickImage = new MagickImage(image.Data);
                        images.Add(magickImage);
                        imageHashes.Add(hash);
                    }

                    break;
                }
                case RTFDomShapeGroup shapeGroup:
                    // Check if the shape group contains child elements (like images)
                    TraverseElements(shapeGroup.Elements, images, imageHashes);
                    break;
                case RTFDomParagraph paragraph:
                    // Paragraphs might contain nested elements (e.g., shapes/images)
                    TraverseElements(paragraph.Elements, images, imageHashes);
                    break;
                case RTFDomTableCell cell:
                    // Cells might contain nested elements (e.g., paragraphs)
                    TraverseElements(cell.Elements, images, imageHashes);
                    break;
                case RTFDomTableRow row:
                    // Rows might contain nested elements (e.g., cells)
                    TraverseElements(row.Elements, images, imageHashes);
                    break;
                case RTFDomTable table:
                    // Tables might contain nested elements (e.g., rows)
                    TraverseElements(table.Elements, images, imageHashes);
                    break;
            }
        }
    }

    /// <summary>
    /// Extracts images from .eml files
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static List<MagickImage> ExtractImagesFromEml(string filePath)
    {
        var images = new List<MagickImage>();
        var imageHashes = new HashSet<string>();

        var message = MimeMessage.Load(filePath);

        if (message.Body is not Multipart multipart) return images;

        foreach (var part in multipart)
        {
            switch (part)
            {
                // If the part is a multipart/related, we extract images from it
                case MultipartRelated related:
                    ExtractImagesFromMultipartRelated(related, images, imageHashes);
                    break;
                // If the part is an image, we extract it
                case MimePart mimeAttachment when mimeAttachment.ContentType.MimeType.StartsWith(ImageMimeTypePrefix):
                    ExtractImageFromMimePart(mimeAttachment, images, imageHashes);
                    break;
            }
        }

        return images;
    }

    /// <summary>
    /// Extracts images from a MultipartRelated
    /// </summary>
    /// <param name="related"></param>
    /// <param name="images"></param>
    /// <param name="imageHashes"></param>
    private static void ExtractImagesFromMultipartRelated(MultipartRelated related, List<MagickImage> images,
        HashSet<string> imageHashes)
    {
        foreach (var resource in related)
        {
            if (resource is MimePart mimePart && mimePart.ContentType.MimeType.StartsWith(ImageMimeTypePrefix))
            {
                ExtractImageFromMimePart(mimePart, images, imageHashes);
            }
        }
    }

    /// <summary>
    /// Extracts an image from a MimePart
    /// </summary>
    /// <param name="mimePart"></param>
    /// <param name="images"></param>
    /// <param name="imageHashes"></param>
    private static void ExtractImageFromMimePart(MimePart mimePart, List<MagickImage> images,
        HashSet<string> imageHashes)
    {
        using var memoryStream = new MemoryStream();
        mimePart.Content.DecodeTo(memoryStream);
        memoryStream.Position = 0;

        var hash = Convert.ToBase64String(MD5.HashData(memoryStream.ToArray()));
        if (!imageHashes.Add(hash)) return;

        var magickImage = new MagickImage(memoryStream.ToArray());
        images.Add(magickImage);
    }

    /// <summary>
    /// Extracts all images from a xml based PowerPoint format
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static List<MagickImage> ExtractImagesFromXmlBasedPowerPoint(string filePath)
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
    /// Extracts all images from a .docx file
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static List<MagickImage> ExtractImagesFromDocx(string filePath)
    {
        using var zip = ZipFile.OpenRead(filePath);

        var images = zip.Entries
            .Where(e => e.FullName.StartsWith("word/media/", StringComparison.OrdinalIgnoreCase))
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
    /// Extracts all images from a .xlsx file
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static List<MagickImage> ExtractImagesFromXlsx(string filePath)
    {
        using var zip = ZipFile.OpenRead(filePath);

        var images = zip.Entries
            .Where(e => e.FullName.StartsWith("xl/media/", StringComparison.OrdinalIgnoreCase))
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
    /// This function returns a list of all image names used in a xlsx spreadsheet. This is mostly used to determine which
    /// images are drawn inside of cells. Images inside of cells do not store data on color profile and so a way to ignore
    /// these images is necessary. Getting the names of the images inside of cells is not possible so instead names of all
    /// other images are found to filter them out.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static List<string> GetNonAnchoredImagesFromXlsx(string filePath)
    {
        var imageNames = new List<string>();

        using (var archive = ZipFile.Open(filePath, ZipArchiveMode.Read))
        {
            var drawingEntries = archive.Entries
                .Where(entry => entry.FullName.StartsWith("xl/drawings/drawing") && entry.FullName.EndsWith(".xml"));

            imageNames.AddRange(from entry in drawingEntries
                let doc = LoadXDocument(entry)
                let blipElements = GetBlipElements(doc)
                from blip in blipElements
                let embedId = blip
                    .Attribute(XNamespace.Get("http://schemas.openxmlformats.org/officeDocument/2006/relationships")
                               + "embed")?.Value
                where !string.IsNullOrEmpty(embedId)
                let relsEntry = GetRelsEntry(archive, entry)
                select GetRelationship(relsEntry, embedId)
                into relationship
                select (string)relationship?.Attribute("Target")!
                into target
                select Path.GetFileName(target));
        }

        return imageNames.Distinct().ToList(); // Ensure unique names
    }

    /// <summary>
    /// Load an XDocument from a ZipArchiveEntry
    /// </summary>
    /// <param name="entry"></param>
    /// <returns></returns>
    private static XDocument LoadXDocument(ZipArchiveEntry entry)
    {
        using var stream = entry.Open();
        using var reader = new StreamReader(stream);
        return XDocument.Load(reader);
    }

    /// <summary>
    /// Get all blip elements from a drawing XML
    /// </summary>
    /// <param name="doc"></param>
    /// <returns></returns>
    private static IEnumerable<XElement> GetBlipElements(XDocument doc)
    {
        XNamespace a = "http://schemas.openxmlformats.org/drawingml/2006/main";
        return doc.Descendants(a + "blip");
    }

    /// <summary>
    /// Get the _rels entry for a drawing entry
    /// </summary>
    /// <param name="archive"></param>
    /// <param name="entry"></param>
    /// <returns></returns>
    private static ZipArchiveEntry? GetRelsEntry(ZipArchive archive, ZipArchiveEntry entry)
    {
        var drawingDir = Path.GetDirectoryName(entry.FullName)?.Replace('\\', '/');
        var relsPath = $"{drawingDir}/_rels/{Path.GetFileName(entry.FullName)}.rels";
        return archive.GetEntry(relsPath);
    }

    /// <summary>
    /// Get the relationship for an embedId
    /// </summary>
    /// <param name="relsEntry"></param>
    /// <param name="embedId"></param>
    /// <returns></returns>
    private static XElement? GetRelationship(ZipArchiveEntry? relsEntry, string embedId)
    {
        using var relsStream = relsEntry?.Open();
        if (relsStream == null) return null;
        using var relsReader = new StreamReader(relsStream);
        var relsDoc = XDocument.Load(relsReader);
        XNamespace relsNs = "http://schemas.openxmlformats.org/package/2006/relationships";
        return relsDoc.Descendants(relsNs + "Relationship")
            .FirstOrDefault(rel =>
                (string)rel.Attribute("Id")! == embedId &&
                (string)rel.Attribute("Type")! ==
                "http://schemas.openxmlformats.org/officeDocument/2006/relationships/image");
    }

    /// <summary>
    /// Disposes of all magick images after use
    /// </summary>
    /// <param name="images"></param>
    public static void DisposeMagickImages(List<MagickImage> images)
    {
        foreach (var image in images)
        {
            image.Dispose();
        }
    }

    /// <summary>
    /// Deletes all saved files/images in the given directory.
    /// </summary>
    /// <param name="directory"></param>
    public static void DeleteSavedFiles(string directory)
    {
        if (!Directory.Exists(directory)) return;
        var files = Directory.GetFiles(directory);
        foreach (var file in files)
        {
            File.Delete(file);
        }
    }

    /// <summary>
    /// Checks if two directories have the same number of images.
    /// </summary>
    /// <param name="dir1"></param>
    /// <param name="dir2"></param>
    /// <returns></returns>
    public static bool CheckIfEqualNumberOfImages(string dir1, string dir2)
    {
        var oFiles = Directory.GetFiles(dir1).ToArray();
        var nFiles = Directory.GetFiles(dir2).ToArray();

        return oFiles.Length == nFiles.Length;
    }

    /// <summary>
    /// Saves an IPdfImage object to an actual image on disk.
    /// </summary>
    /// <param name="image">The image to be saved.</param>
    /// <returns>A tuple containing the path to the file and its expected PRONOM code, null if an error occured.</returns>
    public static (string, string)? SaveExtractedIPdfImageToDisk(IPdfImage image)
    {
        try
        {
            var format = FormatDeterminer.GetImageFormat(image.RawBytes.ToArray());
            string?
                expectedPronom; //Note that this might not be the exact code, but it should serve to distinguish format group.
            IImageEncoder encoder;

            switch (format)
            {
                case ".jpeg":
                    encoder = new JpegEncoder();
                    expectedPronom = FormatCodes.PronomCodesJPEG.FormatCodes[0];
                    break;
                case ".png":
                    encoder = new PngEncoder();
                    expectedPronom = FormatCodes.PronomCodesPNG.FormatCodes[0];
                    break;
                case ".bmp":
                    encoder = new BmpEncoder();
                    expectedPronom = FormatCodes.PronomCodesBMP.FormatCodes[0];
                    break;
                case ".gif":
                    encoder = new GifEncoder();
                    expectedPronom = FormatCodes.PronomCodesGIF.FormatCodes[0];
                    break;
                case ".tiff":
                    encoder = new TiffEncoder();
                    expectedPronom = FormatCodes.PronomCodesTIFF.FormatCodes[0];
                    break;
                default: return null;
            }

            var tempDirs = GlobalVariables.FileManager!.GetTempDirectories();

            using var ms = new MemoryStream();
            using (var img = Image.Load(image.RawBytes))
            {
                img.Save(ms, encoder);
            }

            var path = TempFiles.CreateTemporaryFile(ms.ToArray(), tempDirs.Item2);


            if (path == null) return null;

            return (path, expectedPronom);
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Computes the hash of the image content
    /// </summary>
    /// <param name="rawBytes"></param>
    /// <returns></returns>
    private static string ComputeHash(ReadOnlySpan<byte> rawBytes)
    {
        var hashBytes = MD5.HashData(rawBytes.ToArray());
        return Convert.ToBase64String(hashBytes);
    }
}