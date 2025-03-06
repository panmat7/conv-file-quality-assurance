using AvaloniaDraft.FileManager;
using System;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using Aspose.Slides;
using AvaloniaDraft.Helpers;

namespace AvaloniaDraft.ComparingMethods;

/// <summary>
/// This class is responsible for checking if the original file contains animations
/// </summary>
public static class AnimationComparison
{
    /// <summary>
    /// Checks if the original file is of PowerPoint format and if it contains animations
    /// </summary>
    /// <param name="files"> Takes in the two files used during comparison </param>
    /// <returns> Returns whether it passed the test </returns>
    public static bool FileAnimationComparison(FilePair files)
    {
        
        // Does not conduct the test if the original file is not of PowerPoint format or
        // if both original and new files are of PowerPoint format
        if (!FormatCodes.PronomCodesPresentationDocuments.Contains(files.OriginalFileFormat) ||
            FormatCodes.PronomCodesPresentationDocuments.Contains(files.OriginalFileFormat) &&
            FormatCodes.PronomCodesPresentationDocuments.Contains(files.NewFileFormat)) return true;
        
        // Check for animations in the PowerPoint file
        try
        {
            // Handle check based on PowerPoint file format
            return files.OriginalFileFormat switch
            {
                _ when FormatCodes.PronomCodesXMLBasedPowerPoint.Contains(files.OriginalFileFormat)
                    => CheckXmlBasedFormatForAnimation(files.OriginalFilePath),
                _ => CheckOtherFormatsForAnimation(files.OriginalFilePath)
            };
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error while checking for animations: {e.Message}");
            return false; // If checking for animations fails it automatically fails the test
        }
    }
    
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
            var slideXml = System.Xml.Linq.XDocument.Load(stream);
                    
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
        // Gather all slides
        var contentEntry = zip.Entries.FirstOrDefault(e => 
            e.Name.Equals("content.xml", StringComparison.OrdinalIgnoreCase));
            
        if (contentEntry == null) return false;
        
        using var stream = contentEntry.Open();
        var contentXml = XDocument.Load(stream);
        
        // Define the animation namespace URI
        XNamespace animNs = "urn:oasis:names:tc:opendocument:xmlns:animation:1.0";
    
        // Check for ANY element in the animation namespace
        return contentXml.Descendants()
            .Any(e => e.Name.Namespace == animNs);
    }
    
    /// <summary>
    ///  Checks if the PowerPoint files other than pptx contain animations
    /// </summary>
    /// <param name="filePath"> File path to file </param>
    /// <returns> Returns whether if animations were found </returns>
    public static bool CheckOtherFormatsForAnimation(string filePath)
    {
        using var file = new Presentation(filePath);
        
        // Check if the ppt file contains animations by checking the timeline of each slide
        return file.Slides.All(slide => slide.Timeline.MainSequence.Count <= 0);
    }
}