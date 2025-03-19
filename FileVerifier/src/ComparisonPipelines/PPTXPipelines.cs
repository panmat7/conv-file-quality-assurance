using System;
using System.Collections.Generic;
using System.IO;
using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;

namespace AvaloniaDraft.ComparisonPipelines;

public static class PptxPipelines
{
    /// <summary>
    /// Pipeline responsible for comparing Pptx to other PDF conversions
    /// </summary>
    /// <param name="pair">The pair of files to compare</param>
    /// <param name="additionalThreads">Number of threads available for usage</param>
    /// <param name="updateThreadCount">Callback function used to update current thread count</param>
    /// <param name="markDone">Function marking the FilePair as done</param>
    private static void PptxToPdfPipeline(FilePair pair, int additionalThreads, Action<int> updateThreadCount,
        Action markDone)
    {
        BasePipeline.ExecutePipeline(() =>
        {
            List<Error> e = [];
            
            if (true) //Check options for file size check later
            {
                var res = ComperingMethods.GetFileSizeDifference(pair);
                
                var f = new FileInfo(pair.OriginalFilePath);
                
                if (res > f.Length * 0.5) //Adjust for accuracy later
                {
                    //For now only printing to console
                    e.Add(new Error(
                        "File Size Difference",
                        "The difference in size for the two files exceeds expected values.",
                        ErrorSeverity.High,
                        ErrorType.FileError
                    ));
                }
            }
            
            if (true) //Check options for metadata check later
            {
                var res = ComperingMethods.GetMissingOrWrongImageMetadataExif(pair);

                if (res is null)
                {
                    e.Add(new Error(
                        "Error getting image metadata",
                        "There occured an error while trying to get metadata from one of the files.",
                        ErrorSeverity.High,
                        ErrorType.Metadata
                    ));
                } else if (res.Count > 0)
                {
                    e.AddRange(res);
                }
            }

            if (true) // Check for animation
            {
                var res = false;
                var exceptionOccurred = false;

                try
                {
                    res = AnimationComparison.CheckXmlBasedFormatForAnimation(pair.OriginalFileFormat);
                }
                catch (Exception)
                {
                    exceptionOccurred = true;
                    e.Add(new Error(
                        "Error while checking for animation usage in original pptx file.",
                        "There occurred an error while trying to find animation usage of the pptx file",
                        ErrorSeverity.High,
                        ErrorType.Metadata
                    ));
                }
                if (!exceptionOccurred && !res)
                {
                    e.Add(new Error(
                        "The original pptx file contains animations",
                        "Context from original pptx file may be lost due to use of animations.",
                        ErrorSeverity.Medium,
                        ErrorType.Visual
                    ));
                }
            }
            
            if (true) // Check for color profile later
            {
                var res = false;
                var exceptionOccurred = false;

                try
                {
                    res = ColorProfileComparison.XmlBasedPowerPointToPdfColorProfileComparison(pair);
                }
                catch (Exception)
                {
                    exceptionOccurred = true;
                    e.Add(new Error(
                        "Error comparing color profiles in pptx and pdf contained images",
                        "There occurred an error while extracting and comparing " +
                        "color profiles of the images contained in the pptx and pdf.",
                        ErrorSeverity.High,
                        ErrorType.Metadata
                    ));
                }

                if (!exceptionOccurred && !res)
                {
                    e.Add(new Error(
                        "Difference in images contained in the pptx and pdf color profile",
                        "The images contained in the pptx and pdf did not pass Color Profile comparison.",
                        ErrorSeverity.Medium,
                        ErrorType.Metadata
                    ));
                }
            }
            
            if (true) // Check for transparency later
            {
                var res = false;
                var exceptionOccurred = false;

                try
                {
                    res = TransparencyComparison.XmlBasedPowerPointToPdfTransparencyComparison(pair);
                }
                catch (Exception)
                {
                    exceptionOccurred = true;
                    e.Add(new Error(
                        "Error comparing transparency in pptx contained images",
                        "There occurred an error while comparing transparency" +
                        " of the images contained in the pptx and pdf.",
                        ErrorSeverity.High,
                        ErrorType.Metadata
                    ));
                }

                if (!exceptionOccurred && !res)
                {
                    e.Add(new Error(
                        "Difference in images contained in the pptx and pdf in transparency",
                        "The images contained in the pptx and pdf did not pass Transparency comparison.",
                        ErrorSeverity.Medium,
                        ErrorType.Visual
                    ));
                }
            }
            
            ConsoleService.Instance.WriteToConsole(
                $"Result for {Path.GetFileName(pair.OriginalFilePath)}-{Path.GetFileName(pair.NewFilePath)} Comparison: \n" +
                e.GenerateErrorString() + "\n\n");
            
        }, additionalThreads, updateThreadCount, markDone);
    }
    
    
}