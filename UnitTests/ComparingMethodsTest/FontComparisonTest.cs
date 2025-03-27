using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.ComparingMethodsTest;

public class FontComparisonTest
{
    private string[] originalFilePaths;
    private string[] convertedFilePaths;

    [SetUp]
    public void Setup()
    {
        var curDir = Directory.GetCurrentDirectory();

        while (!string.IsNullOrEmpty(curDir))
        {
            if (Path.GetFileName(curDir) == "conv-file-quality-assurance")
            {
                var testDir = curDir + "/UnitTests/ComparingMethodsTest/TestFiles/FontComparison";
                var originalFilesDir = testDir + "/Original";
                var convertedFilesDir = testDir + "/Converted";

                originalFilePaths = Directory.GetFiles(originalFilesDir);
                convertedFilePaths = Directory.GetFiles(convertedFilesDir);

                return;
            }

            curDir = Directory.GetParent(curDir)?.FullName;
        }

        throw new Exception("Failed to find project directory \"conv-file-quality-assurance\"");
    }


    [Test]
    public void Test()
    {
        // Compare every test file pair
        foreach (var originalFile in originalFilePaths)
        {
            var testName = Path.GetFileNameWithoutExtension(originalFile);

            var convertedFile = convertedFilePaths.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f) == testName);
            if (convertedFile == null) throw new Exception("Could not find a matching file pair for " + testName);


            bool expectedResult;
            if (testName.Contains('s')) // 's' for success (pass)
            {
                expectedResult = true;
            } 
            else if ((testName.Contains('f'))) // 'f' for fail
            {
                expectedResult = false;
            } 
            else
            {
                throw new Exception($"Test {testName} not formatted correctly");
            }

            var fp = new FilePair(originalFile, convertedFile);

            var result = FontComparison.CompareFiles(fp);

            try
            {
                Assert.That(result.Pass, Is.EqualTo(expectedResult), $"Test failed: (Original: {testName})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Assertion failed: {ex.Message}");
            }
            
        }
    }
}
