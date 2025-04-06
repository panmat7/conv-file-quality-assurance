using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;

namespace UnitTests.ComparingMethodsTest;

[TestFixture]
public class ExtractedImageMetadataTest
{
    private string _testFileDirectory = "";

    [SetUp]
    public void Setup()
    {
        var curDir = Directory.GetCurrentDirectory();

        while (!string.IsNullOrEmpty(curDir))
        {
            if (Path.GetFileName(curDir) == "conv-file-quality-assurance")
            {
                _testFileDirectory = curDir + "/UnitTests/ComparingMethodsTest/TestFiles";
                return;
            }
            
            curDir = Directory.GetParent(curDir)?.FullName;
        }
        
        throw new Exception("Failed to find project directory \"conv-file-quality-assurance\"");
    }
}