using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.Helpers;
using AvaloniaDraft.Logger;
using AvaloniaDraft.ProgramManager;

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
        catch(Exception er)
        {
            Console.WriteLine(er);
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


            GlobalVariables.Logger.AddInternalErrorFilePair(files[0], files[1]);
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

    /// <summary>
    /// This function creates a single temporary folder where extracted images can be stored.
    /// </summary>
    /// <returns></returns>
    public static string CreateTempFolderForImages()
    {
        var fileSystem = GlobalVariables.ProgramManager?.GetFilesystem();
        var tempDirectory = fileSystem?.Path.Combine(fileSystem.Path.GetTempPath(), fileSystem.Path.GetRandomFileName());
        if (tempDirectory == null) return string.Empty;
        fileSystem?.Directory.CreateDirectory(tempDirectory);
        return tempDirectory;
    }
    
    /// <summary>
    /// This function creates both temporary folders where extracted images can be stored.
    /// </summary>
    /// <returns></returns>
    public static (string, string) CreateTempFoldersForImages()
    {
        var tempFolder1 = CreateTempFolderForImages();
        var tempFolder2 = CreateTempFolderForImages();
        return (tempFolder1, tempFolder2);
    }

    /// <summary>
    /// This function deletes the temporary folder where images were stored.
    /// </summary>
    /// <param name="tempDirectory"></param>
    private static void DeleteTempFolder(string tempDirectory)
    {
        var fileSystem = GlobalVariables.ProgramManager?.GetFilesystem();
        if (fileSystem == null) return;
        
        if (fileSystem.Directory.Exists(tempDirectory))
            fileSystem.Directory.Delete(tempDirectory, true);
    }
    
    /// <summary>
    /// This function deletes both temporary folders where images were stored.
    /// </summary>
    /// <param name="tempODirectory"></param>
    /// <param name="tempNDirectory"></param>
    public static void DeleteTempFolders(string tempODirectory, string tempNDirectory)
    {
        DeleteTempFolder(tempODirectory);
        DeleteTempFolder(tempNDirectory);
    }

    [Obsolete("Transparency check is no longer its own method, but part of metadata check.")]
    public static void CheckTransparency(string tempFolder, string tempFolder2, FilePair pair, ref ComparisonResult compResult)
    {
        var res = false;
        var exceptionOccurred = false;
        Error error;
            
        try
        {
            res = TransparencyComparison.CompareTransparencyInImagesOnDisk(tempFolder, tempFolder2);
        }
        catch (Exception er)
        {
            Console.WriteLine(er);
            exceptionOccurred = true;
            error = new Error(
                "Error comparing transparency across images",
                "There occurred an error while comparing transparency" +
                " of the images.",
                ErrorSeverity.Medium,
                ErrorType.Metadata
            );
            compResult.AddTestResult(Methods.Transparency, false, errors: [error]);
        }
            
        switch (exceptionOccurred)
        {
            case false when !res:
                error = new Error(
                    "Difference of transparency detected in images contained in the docx",
                    "The images contained in the docx and pdf files did not pass Transparency comparison.",
                    ErrorSeverity.Medium,
                    ErrorType.Visual
                );
                compResult.AddTestResult(Methods.Transparency, false, errors: [error]);
                break;
            case false when res:
                compResult.AddTestResult(Methods.Transparency, true);
                break;
        }
    }

    /// <summary>
    /// This function compares the color profiles between images stored in two temporary folders.
    /// </summary>
    /// <param name="tempFolder"></param>
    /// <param name="tempFolder2"></param>
    /// <param name="pair"></param>
    public static void CheckColorProfiles(string tempFolder, string tempFolder2, FilePair pair, ref ComparisonResult compResult)
    {
        var res = false;
        var exceptionOccurred = false;
        Error error;

        try
        {
            res = ColorProfileComparison.CompareColorProfilesFromDisk(tempFolder, tempFolder2);
        }
        catch (Exception er)
        {
            Console.WriteLine(er);
            exceptionOccurred = true;
            error = new Error(
                "Error comparing color profiles across images",
                "There occurred an error while extracting and comparing " +
                "color profiles in the images.",
                ErrorSeverity.High,
                ErrorType.Metadata
            );
            compResult.AddTestResult(Methods.ColorProfile, false, errors: [error]);
        }

        switch (exceptionOccurred)
        {
            case false when !res:
                error = new Error(
                    "Mismatching color profile",
                    "The color profile in the new file does not match the original in at least one image.",
                    ErrorSeverity.Medium,
                    ErrorType.Metadata
                );
                compResult.AddTestResult(Methods.ColorProfile, false, errors: [error]);
                break;
            case false when res:
                compResult.AddTestResult(Methods.ColorProfile, true);
                break;
        }
    }
}