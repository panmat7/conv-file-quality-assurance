using AvaloniaDraft.FileManager;
using System;
using System.IO.Compression;
using System.Linq;
using Aspose.Slides;

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
        if (!IsPowerPointFile(files.OriginalFilePath)) return true;
        
        // If the original file was converted to a different PowerPoint format we do not need to check for animations
        if (IsPowerPointFile(files.NewFileFormat)) return true;
        
        // Check for animations in the PowerPoint file
        try
        {
            // Handle different PowerPoint file formats
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
    private static bool IsPowerPointFile(string fileFormat)
    {
        return fileFormat is "fmt/215" or "fmt/126" or "fmt/125" or "fmt/124" or "x-fmt/88" 
            or "fmt/1748" or "fmt/1747" or "fmt/1867" or "fmt/1866" or "fmt/179" or "x-fmt/87" or "fmt/181" 
            or "fmt/180" or "fmt/182" or "x-fmt/216" or "x-sfw/40" or "x-sfw/278" or "fmt/629" 
            or "fmt/630" or "fmt/631" or "fmt/632" or "fmt/633" or "fmt/636" or "fmt/487";
    }
    
    /// <summary>
    ///  Checks if the PowerPoint files other than pptx contain animations
    /// </summary>
    /// <param name="filePath"> File path to file </param>
    /// <returns> Returns whether if animations were found </returns>
    private static bool CheckGeneralFilesForAnimation(string filePath)
    {
        var file = new Presentation(filePath);
        
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
                return false;
            }
        }

        return true;
    }
}