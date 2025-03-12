using System;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;

namespace AvaloniaDraft.ComparisonPipelines;

public static class DOCXPipelines
{
    /// <summary>
    /// Function resposible for assigning the correct pipeline for DOCX files
    /// </summary>
    /// <param name="outputFormat">Format of the converted file</param>
    /// <returns>Function with the correct pipeline, null if there were no suitable function.</returns>
    public static Action<FilePair, int, Action<int>, Action>? GetDocxPipeline(string outputFormat)
    {
        if (FormatCodes.PronomCodesPDF.Contains(outputFormat) || FormatCodes.PronomCodesPDFA.Contains(outputFormat))
            return DOCXToPDFPipeline;
        

        return null;
    }
    
    /// <summary>
    /// Pipeline responsible for comparing DOCX to PDF conversions
    /// </summary>
    /// <param name="pair">The pair of files to compare</param>
    /// <param name="additionalThreads">Number of threads available for usage</param>
    /// <param name="updateThreadCount">Callback function used to update current thread count</param>
    /// <param name="markDone">Function marking the FilePair as done</param>
    private static void DOCXToPDFPipeline(FilePair pair, int additionalThreads, Action<int> updateThreadCount,
        Action markDone)
    {
        BasePipeline.ExecutePipeline(() =>
        {
            
        }, additionalThreads, updateThreadCount, markDone);
    }
}