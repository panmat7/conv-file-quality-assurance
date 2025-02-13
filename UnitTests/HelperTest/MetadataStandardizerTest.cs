using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.ComparingMethods.ExifTool;
using AvaloniaDraft.Helpers;

namespace UnitTests.HelperTest;

public class MetadataStandardizerTest
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
    public void StandardizeImageMetadataTest()
    {
        var pngPath = _testFileDirectory + @"Images\225x225.png";
        var jpgPath = _testFileDirectory + @"Images\600x450.jpg";
        var tifPath = _testFileDirectory + @"Images\450x600.tiff";
        
        var pngData = ExifToolStatic.GetExifDataImageMetadata([pngPath], GlobalVariables.ExifPath);
        var jpgData = ExifToolStatic.GetExifDataImageMetadata([jpgPath], GlobalVariables.ExifPath);
        var tifData = ExifToolStatic.GetExifDataImageMetadata([tifPath], GlobalVariables.ExifPath);
        
        var pngStan = MetadataStandardizer.StandardizeImageMetadata(pngData![0], "fmt/13");
        var jpgStan = MetadataStandardizer.StandardizeImageMetadata(jpgData![0], "fmt/44");
        var tifStan = MetadataStandardizer.StandardizeImageMetadata(tifData![0], "fmt/353");

        if (pngStan.ImgWidth != 225 || pngStan.ImgHeight != 225 || pngStan.ColorType != ColorType.Index || pngStan.BitDepth != 8)
            Assert.Fail();
        
        if(jpgStan.ImgWidth != 600 || jpgStan.ImgHeight != 450 || jpgStan.ColorType != ColorType.RGB || jpgStan.BitDepth != 8)
            Assert.Fail();
        
        if(tifStan.ImgWidth != 450 || tifStan.ImgHeight != 600 || tifStan.ColorType != ColorType.RGB || tifStan.BitDepth != 8)
            Assert.Fail();
        
        Assert.Pass();
    }
}