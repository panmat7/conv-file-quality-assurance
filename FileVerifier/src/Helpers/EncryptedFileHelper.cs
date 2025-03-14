using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using AvaloniaDraft.FileManager;
using SharpCompress.Archives;
using UglyToad.PdfPig;

namespace AvaloniaDraft.Helpers;

internal static class EncryptedFileHelper
{

    /// <summary>
    /// Checks a file for encryption or corruption
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static ReasonForIgnoring CheckFileEncryptionOrCorruption(string filePath)
    {
        return Path.GetExtension(filePath).ToLower() switch
        {
            ".pdf" => IsPdfEncrypted(filePath),
            ".docx" or ".xlsx" or ".pptx" => IsOfficeFileEncrypted(filePath),
            ".odt" or ".ods" or ".odp" => IsOpenDocumentEncrypted(filePath),
            _ => ReasonForIgnoring.None
        };
    }
    
    /// <summary>
    /// Checks whether an opendocument file like docx is encrypted/corrupted
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private static ReasonForIgnoring IsOpenDocumentEncrypted(string filePath)
    {
        try
        {
            using var zip = ZipFile.OpenRead(filePath);
            
            var manifest = zip.GetEntry("META-INF/manifest.xml");
            if (manifest == null) return ReasonForIgnoring.None;

            using var stream = manifest.Open();
            var xDoc = XDocument.Load(stream);
            var ns = XNamespace.Get("urn:oasis:names:tc:opendocument:xmlns:manifest:1.0");

            return xDoc.Descendants(ns + "encryption-data").Any() ? ReasonForIgnoring.Encrypted : ReasonForIgnoring.None;
        }
        catch (Exception)
        {
            return ReasonForIgnoring.Corrupted;
        }
    }

    /// <summary>
    /// Checks whether a pdf file like docx is encrypted/corrupted
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private static ReasonForIgnoring IsPdfEncrypted(string filePath)
    {
        try
        {
            using var pdf = PdfDocument.Open(filePath);
            return pdf.IsEncrypted ? ReasonForIgnoring.Encrypted : ReasonForIgnoring.None;
        }
        // Likely a corrupted file
        catch (Exception)
        {
            return ReasonForIgnoring.Corrupted;
        }
    }

    /// <summary>
    /// Checks whether an office file like docx is encrypted/corrupted
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private static ReasonForIgnoring IsOfficeFileEncrypted(string filePath)
    {
        try
        {
            using var zip = ZipFile.OpenRead(filePath);
            
            if (zip.Entries.Any(e => e.FullName.Equals("EncryptedPackage", StringComparison.OrdinalIgnoreCase)))
                return ReasonForIgnoring.Encrypted;
            
            var contentTypes = zip.GetEntry("[Content_Types].xml");
            if (contentTypes == null) return ReasonForIgnoring.None;
            using var stream = contentTypes.Open();
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd().Contains("EncryptedPackage") ? ReasonForIgnoring.Encrypted : ReasonForIgnoring.None;
        }
        catch (Exception)
        {
            return ReasonForIgnoring.Corrupted;
        }
    }
    
    /// <summary>
    /// Checks if a zip file is encrypted
    /// </summary>
    /// <param name="zipPath"></param>
    /// <returns></returns>
    public static bool IsCompressedEncrypted(string zipPath)
    {
        try
        {
            using var archive = ArchiveFactory.Open(zipPath);

            var firstEntry = archive.Entries.FirstOrDefault();
            if (firstEntry == null) return false;
            using var stream = firstEntry.OpenEntryStream();
            stream.ReadByte();

            return false;
        }
        catch
        {
            // Archive is encrypted or corrupted
            return true;
        }
    }
}