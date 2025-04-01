using System;
using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;

namespace AvaloniaDraft.ComparisonPipelines;

public class CSVPipelines
{
    private static void CsvToPdfPipeline(FilePair pair, int additionalThreads, Action<int> updateThreadCount,
        Action markDone)
    {
        BasePipeline.ExecutePipeline(() =>
        {
            if (GlobalVariables.Options.GetMethod(Methods.TableBreakCheck.Name))
            {
                var res = SpreadsheetComparison.PossibleLineBreakCsv(pair.OriginalFilePath);
                if (res == null)
                    GlobalVariables.Logger.AddTestResult(pair, Methods.TableBreakCheck.Name, false, errors: [
                        new Error(
                            "Could not preform check for table breaks",
                            "There occured an error when trying to preform check for table breaks.",
                            ErrorSeverity.High,
                            ErrorType.Visual
                        )
                    ]);
                else if (res.Value)
                    GlobalVariables.Logger.AddTestResult(pair, Methods.TableBreakCheck.Name, false, errors: [
                        new Error(
                            "Table break",
                            "The spreadsheet contains tables that could break during conversion",
                            ErrorSeverity.High,
                            ErrorType.Visual
                        )
                    ]);
                else
                    GlobalVariables.Logger.AddTestResult(pair, Methods.TableBreakCheck.Name, true);
            }
        }, [pair.OriginalFilePath, pair.NewFilePath], additionalThreads, updateThreadCount, markDone);
    }
}