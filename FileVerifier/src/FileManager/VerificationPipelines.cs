using System;
using System.IO;
using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.Helpers;

namespace AvaloniaDraft.FileManager;

public static class VerificationPipelines
{
    /// <summary>
    /// Function responsible for assigning the correct pipeline for PNG files
    /// </summary>
    /// <param name="outputFormat">Format of the converted file</param>
    /// <returns>Function with the correct pipeline, null if none for were</returns>
    public static Action<FilePair, int, Action<int>, Action>? GetPNGPipelines(string outputFormat)
    {
        if (FormatCodes.PronomCodesJPEG.Contains(outputFormat))
        {
            return PNGToJPEGPipeline;
        }

        return null;
    }

    /// <summary>
    /// Pipeline responsible for compering PNG to JPEG conversions
    /// </summary>
    /// <param name="pair">The pair os files to compare</param>
    /// <param name="additionalThreads">Number of threads available for usage</param>
    /// <param name="updateThreadCount">Callback function used to update current thread count</param>
    /// <param name="markDone">Function marking the FilePair as done</param>
    private static void PNGToJPEGPipeline(FilePair pair, int additionalThreads, Action<int> updateThreadCount, Action markDone)
    {
        try
        {
            if (true) //Check options for file size check later
            {
                var res = ComperingMethods.GetFileSizeDifference(pair);
                
                var f = new FileInfo(pair.OriginalFileFormat);
                
                if (res > f.Length * 1.5 || res < f.Length * 0.5) //Adjust for accuracy later
                {
                    //Log failed
                }
            }

            if (true) //Check options for metadata check later
            {
                var res = ComperingMethods.GetMissingOrWrongImageMetadataExif(pair);

                if (res is null)
                {
                    //Log error while getting data
                } else if (res.Count > 0)
                {
                    //Log difference
                }
            }

            if (true) //Check for point by point later
            {
                //pbp here
            }
            
            
        }
        catch
        {
            //Here handle and log errors
        }
        finally
        {
            updateThreadCount(-(1 + additionalThreads)); //Ensuring that this happens even if something fails
            markDone();
        }
    }
}