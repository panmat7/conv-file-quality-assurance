using System;
using System.IO.Abstractions;
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
        if (FormatCodes.PronomCodesImages.Contains(pair.OriginalFileFormat))
            return ImagePipelines.GetImagePipelines(pair.NewFileFormat);
        
        if (FormatCodes.PronomCodesDOCX.Contains(pair.OriginalFileFormat))
            return DocxPipelines.GetDocxPipeline(pair.NewFileFormat);
        
        if(FormatCodes.PronomCodesPPTX.Contains(pair.OriginalFileFormat))
            return PptxPipelines.GetPptxPipeline(pair.NewFileFormat);
        
        if(FormatCodes.PronomCodesPDF.Contains(pair.OriginalFileFormat))
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

        if (FormatCodes.PronomCodesCSV.Contains(pair.OriginalFileFormat))
            return CSVPipelines.GetCsvPipeline(pair.NewFileFormat);

        if (FormatCodes.PronomCodesRTF.Contains(pair.OriginalFileFormat))
            return RtfPipelines.GetRtfPipeline(pair.NewFileFormat);
        
        return null;
    }

    public static (string, string) CreateTempFoldersForImages()
    {
        var fileSystem = GlobalVariables.FileManager?.GetFilesystem();
    
        var tempODirectory = fileSystem?.Path.Combine(fileSystem.Path.GetTempPath(), fileSystem.Path.GetRandomFileName());
        var tempNDirectory = fileSystem?.Path.Combine(fileSystem.Path.GetTempPath(), fileSystem.Path.GetRandomFileName());

        if (tempODirectory == null) return (string.Empty, string.Empty);
        fileSystem?.Directory.CreateDirectory(tempODirectory);
        if (tempNDirectory == null) return (string.Empty, string.Empty);
        fileSystem?.Directory.CreateDirectory(tempNDirectory);

        return (tempODirectory, tempNDirectory);
    }
    
    public static void DeleteTempFolders(string tempODirectory, string tempNDirectory)
    {
        var fileSystem = GlobalVariables.FileManager?.GetFilesystem();
        if (fileSystem == null) return;
        
        if (fileSystem.Directory.Exists(tempODirectory))
            fileSystem.Directory.Delete(tempODirectory, true);
        if (fileSystem.Directory.Exists(tempNDirectory))
            fileSystem.Directory.Delete(tempNDirectory, true);
    }
}