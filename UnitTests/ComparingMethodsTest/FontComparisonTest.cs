using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (testName.Contains('p')) // 'p' for pass
            {
                expectedResult.pass = true;
            } 
            else if ((testName.Contains('f'))) // 'f' for fail
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
            catch (Exception ex)
            {
                /*Console.WriteLine($"{ex.Message}:");

                Console.WriteLine("Fonts only in original:");
                foreach (var f in comparisonResult.FontsOnlyInOriginal)
                {
                    Console.WriteLine(f);
                }

                Console.WriteLine("Fonts only in converted:");
                foreach (var f in comparisonResult.FontsOnlyInConverted)
                {
                    Console.WriteLine(f);
                }

                Console.WriteLine("Text colors only in original:");
                foreach (var c in comparisonResult.TextColorsOnlyInOriginal)
                {
                    Console.WriteLine(c);
                }

                Console.WriteLine("Background colors only in original:");
                foreach (var c in comparisonResult.BgColorsOnlyInOriginal)
                {
                    Console.WriteLine(c);
                }*/
            }
            
        }
    }
}
