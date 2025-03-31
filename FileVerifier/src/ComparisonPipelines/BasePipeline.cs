using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Avalonia;
using Avalonia.Threading;
using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;
using AvaloniaDraft.Views;

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
    public static void ExecutePipeline(Action pipeline, string[] files, int additionalThreads, Action<int> updateThreadCount,
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

            UiControlService.Instance.AppendToConsole(
                $"Verification for {files[0]}-{files[1]} failed:\n" +
                e.FormatErrorMessage() +
                "\n\n"
            );
        }
        finally
        {
            UiControlService.Instance.MarkProgress();
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
        
        if (FormatCodes.PronomCodesDOCX.Contains(pair.OriginalFileFormat))
            return DocxPipelines.GetDocxPipeline(pair.NewFileFormat);

        if (FormatCodes.PronomCodesJPEG.Contains(pair.OriginalFileFormat))
            return JpgPipelines.GetJPEGPipelines(pair.NewFileFormat);
        
        if (FormatCodes.PronomCodesPPTX.Contains(pair.OriginalFileFormat))
            return PptxPipelines.GetPptxPipeline(pair.NewFileFormat);
        
        if (FormatCodes.PronomCodesPDF.Contains(pair.OriginalFileFormat))
            return PdfPipelines.GetPdfPipelines(pair.NewFileFormat);

        if (FormatCodes.PronomCodesODP.Contains(pair.OriginalFileFormat))
            return OdpPipelines.GetOdpPipeline(pair.NewFileFormat);

        if (FormatCodes.PronomCodesEML.Contains(pair.OriginalFileFormat))
            return EmlPipelines.GetEmlPipeline(pair.NewFileFormat);

        if (FormatCodes.PronomCodesODT.Contains(pair.OriginalFileFormat))
            return ODTPipelines.GetOdtPipeline(pair.NewFileFormat);

        if (FormatCodes.PronomCodesODS.Contains(pair.OriginalFileFormat))
            return ODSPipelines.GetOdsPipeline(pair.NewFileFormat);

        if (FormatCodes.PronomCodesXLSX.Contains(pair.OriginalFileFormat))
            return XLSXPipelines.GetXlsxPipeline(pair.NewFileFormat);
        
        return null;
    }


    /// <summary>
    /// Compare the fonts of two files
    /// </summary>
    /// <param name="fp">The file pair</param>
    public static List<Error> CompareFonts(FilePair fp)
    {
        if (!GlobalVariables.Options.GetMethod(Methods.Fonts)) return [];

        var comments = new List<string>();
        var errors = new List<Error>();

        var result = FontComparison.CompareFiles(fp);

        if (result.Errors.Count > 0)
        {
            foreach (var e in result.Errors) errors.Add(e);
        }

        if (result.ContainsForeignCharacters) comments.Add("Contains foreign characters");


        if (result.FontsOnlyInOriginal.Count > 0 || result.FontsOnlyInConverted.Count > 0)
        {
            errors.Add(new Error(
                "Font difference", 
                "Different fonts were detected in the two files.",
                ErrorSeverity.Medium,
                ErrorType.Visual)
            );

            if (result.FontsOnlyInOriginal.Count > 0)
            {
                StringBuilder bld = new StringBuilder();
                bld.Append("Fonts only in original:");
                foreach (var f in result.FontsOnlyInOriginal) bld.Append($"\n{f}");
                comments.Add(bld.ToString());
            }

            if (result.FontsOnlyInConverted.Count > 0)
            {
                StringBuilder bld = new StringBuilder();
                bld.Append("Fonts only in converted:");
                foreach (var f in result.FontsOnlyInConverted) bld.Append($"\n{f}");
                comments.Add(bld.ToString());
            }
        }


        if (result.BgColorsNotConverted)
        {
            errors.Add(new Error(
                "Background color difference",
                "Different background colors were detected in the two files.",
                ErrorSeverity.Medium,
                ErrorType.Visual
            ));
        }


        if (result.TextColorsNotConverted)
        {
            errors.Add(new Error(
                "Text color difference",
                "Different text colors were detected in the two files.",
                ErrorSeverity.Medium,
                ErrorType.Visual
            ));
        }

        GlobalVariables.Logger.AddTestResult(fp, Methods.Fonts.Name, result.Pass, null, comments, errors);

        return errors;
    }
    
}