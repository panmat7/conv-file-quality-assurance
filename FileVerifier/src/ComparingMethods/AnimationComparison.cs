using AvaloniaDraft.FileManager;
using System;
using System.IO.Compression;
using System.Linq;

namespace AvaloniaDraft.ComparingMethods;

public static class AnimationComparison
{
    /// <summary>
    /// Checks if the original file is of PowerPoint format and if it contains animations
    /// </summary>
    /// <param name="files"> Takes in the two files used during comparison </param>
    /// <returns> Returns whether it passed the test </returns>
    public static bool FileAnimationComparison(FilePair files)
    {
        
        // Check if the file format is of PowerPoint format
        if (files.OriginalFileFormat != "fmt/126" && files.OriginalFileFormat != "fmt/215") return true;
        
        // TODO: Check if the new file is also of some type of PowerPoint format. Animations would be the same in both files.
        
        // Check for animations in the PowerPoint file
        try
        {
            // Handle different PowerPoint file formats
            return files.OriginalFileFormat switch
            {
                "fmt/126" => CheckPptxAnimation(files.OriginalFilePath),
                "fmt/215" => CheckPptAnimation(files.OriginalFilePath),
                _ => true
            };
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error while checking for animations: {e.Message}");
            return false; // If checking for animations fails it automatically fails the test
        }
    }

    /// <summary>
    ///  Checks if the ppt file contains animations
    /// </summary>
    /// <param name="filePath"> File path to file </param>
    /// <returns> Returns whether if animations were found </returns>
    private static bool CheckPptAnimation(string filePath)
    {
        throw new Exception("Not implemented check for ppt animations");
    }

    /// <summary>
    /// Checks if the pptx file contains animations
    /// </summary>
    /// <param name="filePath"> File path to file </param>
    /// <returns> Returns whether if animations were found </returns>
    private static bool CheckPptxAnimation(string filePath)
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
                return false;
            }
        }

        return true;
    }
}