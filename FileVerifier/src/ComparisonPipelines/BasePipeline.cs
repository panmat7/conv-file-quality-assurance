using System;
using AvaloniaDraft.ComparingMethods.ComparisonPipelines;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;

namespace AvaloniaDraft.ComparisonPipelines;

public static class BasePipeline
{
    /// <summary>
    /// The base for every pipeline. This function ensures correct handling of potential errors and the correct handling
    /// of finishing actions. 
    /// </summary>
    /// <param name="pipeline">The actual content of the pipeline. This is here the checks should be executed.</param>
    /// <param name="additionalThreads">The number of threads delegated to the pipeline.</param>
    /// <param name="updateThreadCount">Callback function used to update current thread count</param>
    /// <param name="markDone">Function marking the FilePair as done</param>
    public static void ExecutePipeline(Action pipeline, int additionalThreads, Action<int> updateThreadCount,
        Action markDone)
    {
        try
        {
            pipeline();
        }
        catch
        {
            var e = new Error(
                "Error during file processing.",
                "An internal error occurred while processing the files.",
                ErrorSeverity.Internal
            );

            ConsoleService.Instance.WriteToConsole(e.FormatErrorMessage());
        }
        finally
        {
            updateThreadCount(-(1 + additionalThreads));
            markDone();
        }
    }

    /// <summary>
    /// <c>SelectPipeline</c> selects the pipeline to be used for file comparison. 
    /// </summary>
    /// <param name="pair">The two files to be compared, based on them a function will be chosen.</param>
    /// <returns>The selected pipeline function, or null if none fitting the formats were found.</returns>
    public static Action<FilePair, int, Action<int>, Action>? SelectPipeline(FilePair pair)
    {
        if (FormatCodes.PronomCodesPNG.Contains(pair.OriginalFileFormat))
            return PngPipelines.GetPNGPipelines(pair.NewFileFormat);
        
        if(FormatCodes.PronomCodesDOCX.Contains(pair.OriginalFileFormat))
            return DOCXPipelines.GetDocxPipeline(pair.NewFileFormat);
        
        return null;
    }
}