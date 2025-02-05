using AvaloniaDraft.ComparingMethods.ExifTool;

namespace UnitTests.ComparingMethodsTest;

public class ExifToolTest
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
                _testFileDirectory = curDir + @"\UnitTests\ComparingMethodsTest\TestFiles\";
                return;
            }
            
            curDir = Directory.GetParent(curDir)?.FullName;
        }
        
        throw new Exception("Failed to find project directory \"conv-file-quality-assurance\"");
    }
    
    [Test]
    public void GetExifDataTest()
    {
        var filePath1 = _testFileDirectory + @"Images\225x225.png";
        var filePath2 = _testFileDirectory + @"Images\450x450.png";
        
        var result = ExifTool.GetExifData([filePath1, filePath2]);

        Assert.That(result != null && result.Count > 0, Is.True);
    }
}