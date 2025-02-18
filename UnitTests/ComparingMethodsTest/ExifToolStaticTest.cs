using AvaloniaDraft.ComparingMethods.ExifTool;

namespace UnitTests.ComparingMethodsTest;

public class ExifToolStaticTest
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

        var path = ExifToolStatic.GetExifPath();
        var result1 = ExifToolStatic.GetExifDataDictionary([filePath1, filePath2], path);
        var result2 = ExifToolStatic.GetExifDataDictionary([filePath1, filePath2], path);

        Assert.That(result1 != null && result1.Count > 0, Is.True);
        Assert.That(result2 != null && result2.Count > 0, Is.True);
    }
}