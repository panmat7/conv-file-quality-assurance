using System;
using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;
using AvaloniaDraft.Logger;

namespace AvaloniaDraft.ComparisonPipelines;

public class CSVPipelines
{
    /// <summary>
    /// Function responsible for assigning the correct pipeline for CSV files
    /// </summary>
    /// <param name="outputFormat">Format of the converted file</param>
    /// <returns>Function with the correct pipeline, null if there were no suitable function.</returns>
    public static Action<FilePair, int, Action<int>, Action>? GetCsvPipeline(string? outputFormat)
    {
        if (FormatCodes.PronomCodesPDF.Contains(outputFormat) || FormatCodes.PronomCodesPDFA.Contains(outputFormat))
            return CsvToPdfPipeline;

        return null;
    }
    
    private static void CsvToPdfPipeline(FilePair pair, int additionalThreads, Action<int> updateThreadCount,
        Action markDone)
    {
        BasePipeline.ExecutePipeline(() =>
        {
            var compResult = new ComparisonResult(pair);

            if (GlobalVariables.Options.GetMethod(Methods.TableBreakCheck))
            {
                var res = SpreadsheetComparison.PossibleLineBreakCsv(pair.OriginalFilePath);
                if (res == null)
                    compResult.AddTestResult(Methods.TableBreakCheck, false, errors: [
                        new Error(
                            "Could not perform check for table breaks",
                            "There occured an error when trying to perform check for table breaks.",
                            ErrorSeverity.High,
                            ErrorType.Visual
                        )
                    ]);
                else if (res.Value)
                    compResult.AddTestResult(Methods.TableBreakCheck, false, errors: [
                        new Error(
                            "Table break",
                            "The spreadsheet contains tables that could break during conversion",
                            ErrorSeverity.High,
                            ErrorType.Visual
                        )
                    ]);
                else
                    compResult.AddTestResult(Methods.TableBreakCheck, true, null, [], []);
            }

            GlobalVariables.Logger.AddComparisonResult(compResult);
        }, [pair.OriginalFilePath, pair.NewFilePath], additionalThreads, updateThreadCount, markDone);
    }
}