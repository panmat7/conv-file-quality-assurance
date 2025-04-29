using AvaloniaDraft.Helpers;

namespace UnitTests.HelperTest;

[TestFixture]
public class FormatDeterminerTest
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
    public void FormatDeterminerTest_All()
    {
        var pathToFormat = new Dictionary<string, string?>
        {
            {
                _testFileDirectory + @"Images\225x225.png",
                ".png"
            },
            {
                _testFileDirectory + @"Images\600x450.jpg",
                ".jpeg"
            },
            {
                _testFileDirectory + @"Images\450x600.tiff",
                ".tiff"
            },
            {
                _testFileDirectory + @"Images\600x450.bmp",
                ".bmp"
            },
            {
                _testFileDirectory + @"Images\gif-animated.gif",
                ".gif"
            },
            {
                _testFileDirectory + @"ODT\odt-with-no-images.odt",
                null
            }
        };

        foreach (var test in pathToFormat)
        {
            var bytes = File.ReadAllBytes(test.Key);
            var res = FormatDeterminer.GetImageFormat(bytes);
            Assert.That(res, Is.EqualTo(test.Value));
        }
        
        Assert.Pass();
    }
}