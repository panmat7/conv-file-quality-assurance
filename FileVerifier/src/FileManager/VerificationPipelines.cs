using System;
using AvaloniaDraft.Helpers;

namespace AvaloniaDraft.FileManager;

public static class VerificationPipelines
{
    public static Action<FilePair, int>? GetPNGPipelines(string outputFormat)
    {
        if (FormatCodes.PronomCodesJPEG.Contains(outputFormat))
        {
            return PNGToJPEGPipeline;
        }

        return null;
    }

    private static void PNGToJPEGPipeline(FilePair pair, int additionalThreads)
    {
        //Do all comparison here
    }
}