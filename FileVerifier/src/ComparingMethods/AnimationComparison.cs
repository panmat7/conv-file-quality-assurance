using AvaloniaDraft.FileManager;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using Aspose.Slides;

namespace AvaloniaDraft.ComparingMethods;

/// <summary>
/// This class is responsible for checking if the original file contains animations
/// </summary>
public static class AnimationComparison
{
    
    /// <summary>
    /// Contains all the PowerPoint formats
    /// </summary>
    private static readonly HashSet<string> PowerPointFormats =
    [
        "fmt/215", "fmt/126", "fmt/125", "fmt/124", "x-fmt/88", "fmt/1748", "fmt/1747", "fmt/1867", "fmt/1866",
        "fmt/179", "x-fmt/87", "fmt/181", "fmt/180", "fmt/182", "x-fmt/216", "x-sfw/40", "x-sfw/278", "fmt/629",
        "fmt/630", "fmt/631", "fmt/632", "fmt/633", "fmt/636", "fmt/487"
    ];
    
    /// <summary>
    /// Checks if the original file is of PowerPoint format and if it contains animations
    /// </summary>
    /// <param name="files"> Takes in the two files used during comparison </param>
    /// <returns> Returns whether it passed the test </returns>
    public static bool FileAnimationComparison(FilePair files)
    {
        
        // Does not conduct the test if the original file is not of PowerPoint format or
        // if both original and new files are of PowerPoint format
        if (!IsPowerPointFile(files.OriginalFileFormat) || 
            IsPowerPointFile(files.OriginalFileFormat) && IsPowerPointFile(files.NewFileFormat)) return true;
        
        // Check for animations in the PowerPoint file
        try
        {
            // Handle check based on PowerPoint file format
            return files.OriginalFileFormat switch
            {
                "fmt/126" => CheckPptxFilesForAnimation(files.OriginalFilePath),
                _ => CheckGeneralFilesForAnimation(files.OriginalFilePath)
            };
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error while checking for animations: {e.Message}");
            return false; // If checking for animations fails it automatically fails the test
        }
    }

    /// <summary>
    /// Checks if the file format is of one of the PowerPoint formats
    /// </summary>
    /// <param name="fileFormat"> The id of the file format being checked  </param>
    /// <returns> Returns whether if it is of PowerPoint format </returns>
    private static bool IsPowerPointFile(string fileFormat) => PowerPointFormats.Contains(fileFormat);
    
    /// <summary>
    ///  Checks if the PowerPoint files other than pptx contain animations
    /// </summary>
    /// <param name="filePath"> File path to file </param>
    /// <returns> Returns whether if animations were found </returns>
    private static bool CheckGeneralFilesForAnimation(string filePath)
    {
        using var file = new Presentation(filePath);
        
        // Check if the ppt file contains animations by checking the timeline of each slide
        return file.Slides.All(slide => slide.Timeline.MainSequence.Count <= 0);
    }

    /// <summary>
    /// Checks if the pptx file contains animations
    /// </summary>
    /// <param name="filePath"> File path to file </param>
    /// <returns> Returns whether if animations were found </returns>
    private static bool CheckPptxFilesForAnimation(string filePath)
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
}