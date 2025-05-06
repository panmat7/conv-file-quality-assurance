using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvaloniaDraft.ProgramManager;

namespace UnitTests.ComparingMethodsTest;

public class FontComparisonTest
{
    private List<FilePair> filePairs;

    [SetUp]
    public void Setup()
    {
        filePairs = [];
        var curDir = Directory.GetCurrentDirectory();

        while (!string.IsNullOrEmpty(curDir))
        {
            if (Path.GetFileName(curDir) == "conv-file-quality-assurance")
            {
                var testDir = curDir + "/UnitTests/ComparingMethodsTest/TestFiles/FontComparison";
                var originalFilesDir = testDir + "/Original";
                var convertedFilesDir = testDir + "/Converted";

                var originalFilePaths = Directory.GetFiles(originalFilesDir);
                var convertedFilePaths = Directory.GetFiles(convertedFilesDir);

                foreach (var oFile in originalFilePaths)
                {
                    var nFile = convertedFilePaths.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f) == 
                                                                    Path.GetFileNameWithoutExtension(oFile));
                    if (nFile == null) throw new Exception("Failed to pair files");


                    var oFmt = GetFormatCode(oFile);
                    var nFmt = GetFormatCode(nFile);
                    if (oFmt == null || nFmt == null) throw new Exception("Failed to assign format codes");
                    filePairs.Add(new FilePair(oFile, oFmt, nFile, nFmt));
                }
                

                return;
            }

            curDir = Directory.GetParent(curDir)?.FullName;
        }

        throw new Exception("Failed to find project directory \"conv-file-quality-assurance\"");
    }

    private string? GetFormatCode(string src)
    {
        var ext = Path.GetExtension(src).ToUpper();
            return ext switch
            {
                ".PDF" => FormatCodes.PronomCodesPDF.PronomCodes[0],
                ".DOCX" => FormatCodes.PronomCodesDOCX.PronomCodes[0],
                ".PPTX" => FormatCodes.PronomCodesPPTX.PronomCodes[0],
                ".XLSX"=> FormatCodes.PronomCodesXLSX.PronomCodes[0],
                ".ODT" => FormatCodes.PronomCodesODT.PronomCodes[0],
                ".ODS" => FormatCodes.PronomCodesODS.PronomCodes[0],
                ".ODP" => FormatCodes.PronomCodesODP.PronomCodes[0],
                ".RTF" => FormatCodes.PronomCodesRTF.PronomCodes[0],
                ".EML" => FormatCodes.PronomCodesEML.PronomCodes[0],
                ".HTML" => FormatCodes.PronomCodesHTML.PronomCodes[0],
                _ => null,
            };
    }


    [Test]
    public void Test()
    {
        // Compare every test file pair
        foreach (var fp in filePairs)
        {
            var testName = Path.GetFileNameWithoutExtension(fp.OriginalFilePath);

            (bool pass, bool foreignChars) expectedResult;
            if (testName.Contains("_p")) // 'p' for pass
            {
                expectedResult.pass = true;
            } 
            else if ((testName.Contains("_f"))) // 'f' for fail
            {
                expectedResult.pass = false;
            } 
            else
            {
                throw new Exception($"Test {testName} not formatted correctly");
            }
            expectedResult.foreignChars = testName.Contains("fc");

            var comparisonResult = FontComparison.CompareFiles(fp);
            (bool pass, bool foreignChars) result = (comparisonResult.Pass, comparisonResult.ContainsForeignCharacters);

            try
            {
                Assert.That(result, Is.EqualTo(expectedResult), $"Test failed: ({testName})");
            }
            catch
            {
                Console.WriteLine($"{testName}:");

                if (comparisonResult.Errors.Count > 0)
                {
                    Console.WriteLine("Errors:", comparisonResult.Errors);
                    foreach (var e in comparisonResult.Errors)
                    {
                        Console.WriteLine(e.Description);
                    }
                }

                PrintList("Fonts only in original:", comparisonResult.FontsOnlyInOriginal);
                PrintList("Fonts only in converted:", comparisonResult.FontsOnlyInConverted);
                PrintList("Text colors only in original:", comparisonResult.TextColorsOnlyInOriginal);
                PrintList("Background colors only in original:", comparisonResult.BgColorsOnlyInOriginal);
                Console.WriteLine();
            }
            
        }
    }


    private static void PrintList(string title, List<string> list)
    {
        if (list.Count == 0) return;

        Console.WriteLine(title);
        foreach (var item in list)
        {
            Console.WriteLine(item);
        }
        Console.WriteLine();
    }
}
