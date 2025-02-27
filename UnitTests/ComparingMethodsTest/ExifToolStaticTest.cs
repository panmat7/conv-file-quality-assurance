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
    public void GetExifDataTest_Valid()
    {
        var filePath1 = _testFileDirectory + @"Images\225x225.png";
        var filePath2 = _testFileDirectory + @"Images\450x450.png";

        var path = ExifToolStatic.GetExifPath();
        var result1 = ExifToolStatic.GetExifDataDictionary([filePath1, filePath2], path);
        var result2 = ExifToolStatic.GetExifDataDictionary([filePath1, filePath2], path);

        Assert.Multiple(() =>
        {
            Assert.That(result1 is { Count: > 0 }, Is.True);
            Assert.That(result2 is { Count: > 0 }, Is.True);
        });
    }

    [Test]
    public void GetExifDataTest_Empty()
    {
        var filePath1 = "";
        var filePath2 = "";

        var path = ExifToolStatic.GetExifPath();
        var result = ExifToolStatic.GetExifDataDictionary([filePath1, filePath2], path);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetExifDataTest_Invalid()
    {
        var filePath1 = "Not";
        var filePath2 = "Real";
        
        var path = ExifToolStatic.GetExifPath();
        var result = ExifToolStatic.GetExifDataDictionary([filePath1, filePath2], path);
        
        Assert.That(result, Is.Null);
    }
}