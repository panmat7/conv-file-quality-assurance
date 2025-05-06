using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using AvaloniaDraft.ProgramManager;
using SharpCompress.Archives;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Exceptions;

namespace AvaloniaDraft.Helpers;

public static class EncryptionChecker
{

    /// <summary>
    /// Checks a file for encryption or corruption
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static ReasonForIgnoring CheckForEncryption(string filePath)
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
        // File is corrupted
        catch (Exception)
        {
            return ReasonForIgnoring.None;
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
        // File is encrypted
        catch (PdfDocumentEncryptedException)
        {
            return ReasonForIgnoring.Encrypted;
        }
        // File is likely corrupted
        catch (Exception)
        {
            return ReasonForIgnoring.None;
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
            // Read first 8 bytes of the file
            var header = new byte[8];
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fs.ReadExactly(header, 0, header.Length);
            }

            // Check if it's a standard ZIP archive
            if (header[0] == 0x50 && header[1] == 0x4B)
            {
                // It's a normal Office file (not encrypted)
                return ReasonForIgnoring.None;
            }

            // Check if it matches the CBF (Compound File Binary Format) signature
            if (header[0] == 0xD0 && header[1] == 0xCF && header[2] == 0x11 && header[3] == 0xE0 &&
                header[4] == 0xA1 && header[5] == 0xB1 && header[6] == 0x1A && header[7] == 0xE1)
            {
                return ReasonForIgnoring.Encrypted;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking encryption status: {ex.Message}");
            return ReasonForIgnoring.None;
        }

        return ReasonForIgnoring.None;
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