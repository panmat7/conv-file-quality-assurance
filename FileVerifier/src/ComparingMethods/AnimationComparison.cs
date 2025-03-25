using AvaloniaDraft.FileManager;
using System;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using AvaloniaDraft.Helpers;

namespace AvaloniaDraft.ComparingMethods;

/// <summary>
/// This class is responsible for checking if the original file contains animations
/// </summary>
public static class AnimationComparison
{
    /// <summary>
    /// Checks if the pptx file contains animations
    /// </summary>
    /// <param name="filePath"> File path to file </param>
    /// <returns> Returns whether if animations were found </returns>
    public static bool CheckXmlBasedFormatForAnimation(string filePath)
    {
        using var zip = ZipFile.OpenRead(filePath);
        // Gather all slides
        var slides = zip.Entries.Where(e => 
            e.FullName.StartsWith("ppt/slides/", StringComparison.OrdinalIgnoreCase) && 
            e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)
        );
            
        // Check each slide for animations
        foreach (var slide in slides)
        {
            using var stream = slide.Open();
            var slideXml = XDocument.Load(stream);
                    
            // Check if the slide's xml contents contains animations
            if (slideXml.Descendants().Any(e => e.Name.LocalName is "anim" or "animEffect" or "timing"))
            {
                // Fails the test
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks for animations OpenDocument presentation
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static bool CheckOdpForAnimation(string filePath)
    {
        using var zip = ZipFile.OpenRead(filePath);
        var contentEntry = zip.Entries.FirstOrDefault(e => e.Name.Equals("content.xml", StringComparison.OrdinalIgnoreCase));

        if (contentEntry == null)
            return false; // content.xml not found, assume no animation

        using var stream = contentEntry.Open();
        var xmlDoc = XDocument.Load(stream);

        XNamespace animNs = "urn:oasis:names:tc:opendocument:xmlns:animation:1.0";
        XNamespace smilNs = "urn:oasis:names:tc:opendocument:xmlns:smil-compatible:1.0";

        // Check for animation elements within the correct hierarchy
        var hasAnimation = xmlDoc.Descendants(animNs + "par").Any() ||
                           xmlDoc.Descendants(animNs + "seq").Any() ||
                           xmlDoc.Descendants(animNs + "animate").Any() ||
                           xmlDoc.Descendants(smilNs + "animate").Any();

        return hasAnimation;
    }
}