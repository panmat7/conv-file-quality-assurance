using System;
using System.Collections.Generic;
using AvaloniaDraft.ComparisonPipelines;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;

namespace AvaloniaDraft.ComparingMethods.ComparisonPipelines;

public static class DocxPipelines
{
    public static Action<FilePair, int, Action<int>, Action>? GetDocxPipeline(string outputFormat)
    {
        if (FormatCodes.PronomCodesPDF.Contains(outputFormat) || FormatCodes.PronomCodesPDFA.Contains(outputFormat))
        {
            return DocxToPdfPipeline;
        }

        return null;
    }

    private static void DocxToPdfPipeline(FilePair pair, int additionalThreads, Action<int> updateThreadCount,
        Action markDone)
    {
        BasePipeline.ExecutePipeline(() =>
        {
            
        }, additionalThreads, updateThreadCount, markDone);
    }
}