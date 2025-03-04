using System;
using System.Collections.Generic;
using System.Linq;
using ImageMagick;
using UglyToad.PdfPig;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;
using MimeKit;

namespace AvaloniaDraft.ComparingMethods;

public class ImageExtraction
{
    /// <summary>
    /// Extracts images from a PDF file
    /// </summary>
    /// <param name="filePath"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <returns></returns>
    internal static List<MagickImage> ExtractImagesFromPdf(string filePath)
    {
        var extractedImages = new List<MagickImage>();
    
        // Hash set to store the hash of the image content to avoid duplicates where the same image is used multiple times
        var imageHashes = new HashSet<string>();

        using var pdfDocument = PdfDocument.Open(filePath);
        foreach (var page in pdfDocument.GetPages())
        {
            var images = page.GetImages().ToList();

            // Only extract images that are not already in the hash set
            foreach (var rawBytes in from image in images select image.RawBytes.ToArray() into rawBytes 
                     let hash = Convert.ToBase64String(System.Security.Cryptography.MD5.HashData(rawBytes)) 
                     where imageHashes.Add(hash) select rawBytes)
            {
                using var magickImage = new MagickImage(rawBytes);
                extractedImages.Add((MagickImage)magickImage.Clone());
            }
        }

        return extractedImages;
    }
    
    /// <summary>
    /// Extracts images frm OpenDocument files
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    internal static List<MagickImage> ExtractImagesFromOpenDocuments(string filePath)
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
    /// Extracts images from .eml files
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    internal static List<MagickImage> ExtractImagesFromEml(string filePath)
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
                case MimePart mimeAttachment when mimeAttachment.ContentType.MimeType.StartsWith("image/"):
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
    private static void ExtractImagesFromMultipartRelated(MultipartRelated related, List<MagickImage> images, HashSet<string> imageHashes)
    {
        foreach (var resource in related)
        {
            if (resource is MimePart mimePart && mimePart.ContentType.MimeType.StartsWith("image/"))
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
    private static void ExtractImageFromMimePart(MimePart mimePart, List<MagickImage> images, HashSet<string> imageHashes)
    {
        using var memoryStream = new MemoryStream();
        mimePart.Content.DecodeTo(memoryStream);
        memoryStream.Position = 0;

        var hash = Convert.ToBase64String(System.Security.Cryptography.MD5.HashData(memoryStream.ToArray()));
        if (!imageHashes.Add(hash)) return;

        var magickImage = new MagickImage(memoryStream.ToArray());
        images.Add(magickImage);
    }
    

    /// <summary>
    /// Extracts all images from a xml based PowerPoint format
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    internal static List<MagickImage> ExtractImagesFromXmlBasedPowerPoint(string filePath)
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
    internal static List<MagickImage> ExtractImagesFromDocx(string filePath)
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
    internal static List<MagickImage> ExtractImagesFromXlsx(string filePath)
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

            imageNames.AddRange(from entry in drawingEntries let doc = LoadXDocument(entry) 
                let blipElements = GetBlipElements(doc) from blip in blipElements 
                let embedId = blip
                    .Attribute(XNamespace.Get("http://schemas.openxmlformats.org/officeDocument/2006/relationships") 
                               + "embed")?.Value where !string.IsNullOrEmpty(embedId) 
                let relsEntry = GetRelsEntry(archive, entry) select GetRelationship(relsEntry, embedId) 
                into relationship select (string)relationship?.Attribute("Target")! into target 
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
                (string)rel.Attribute("Type")! == "http://schemas.openxmlformats.org/officeDocument/2006/relationships/image");
    }
}