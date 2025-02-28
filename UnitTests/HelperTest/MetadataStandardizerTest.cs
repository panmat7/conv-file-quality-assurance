using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.ComparingMethods.ExifTool;
using AvaloniaDraft.Helpers;
using UnitTests.ComparingMethodsTest;

namespace UnitTests.HelperTest;

[SetUpFixture]
public class ExifToolSetupTeardown
{
    [OneTimeTearDown]
    public void AfterTests()
    {
        GlobalVariables.ExifTool.Dispose();
    }
}

[TestFixture]
public class MetadataStandardizerTest : TestBase
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
        var bmpPath = _testFileDirectory + @"Images\600x450.bmp";
        var gifPath = _testFileDirectory + @"Images\gif-animated.gif";
        
        var pngData = GlobalVariables.ExifTool.GetExifDataImageMetadata([pngPath]);
        var jpgData = GlobalVariables.ExifTool.GetExifDataImageMetadata([jpgPath]);
        var tifData = GlobalVariables.ExifTool.GetExifDataImageMetadata([tifPath]);
        var bmpData = GlobalVariables.ExifTool.GetExifDataImageMetadata([bmpPath]);
        var gifData = GlobalVariables.ExifTool.GetExifDataImageMetadata([gifPath]);
        
        var pngStan = MetadataStandardizer.StandardizeImageMetadata(pngData![0], "fmt/13");
        var jpgStan = MetadataStandardizer.StandardizeImageMetadata(jpgData![0], "fmt/44");
        var tifStan = MetadataStandardizer.StandardizeImageMetadata(tifData![0], "fmt/353");
        var bmpStan = MetadataStandardizer.StandardizeImageMetadata(bmpData![0], "fmt/116");
        var gifStan = MetadataStandardizer.StandardizeImageMetadata(gifData![0], "fmt/4");

        if (pngStan.ImgWidth != 225 || pngStan.ImgHeight != 225 || pngStan.ColorType != ColorType.Index || pngStan.BitDepth != 8)
            Assert.Fail();
        
        if(jpgStan.ImgWidth != 600 || jpgStan.ImgHeight != 450 || jpgStan.ColorType != ColorType.RGB || jpgStan.BitDepth != 8)
            Assert.Fail();
        
        if(tifStan.ImgWidth != 450 || tifStan.ImgHeight != 600 || tifStan.ColorType != ColorType.RGB || tifStan.BitDepth != 8)
            Assert.Fail();
        
        if(bmpStan.ImgWidth != 600 || bmpStan.ImgHeight != 450 || bmpStan.ColorType != ColorType.RGB || bmpStan.BitDepth != 8)
            Assert.Fail();
        
        if(gifStan.ImgWidth != 225 || gifStan.ImgHeight != 225 || gifStan.FrameCount != 4 || gifStan.BitDepth != 7)
            Assert.Fail();
        
        Assert.Pass();
    }

    [Test]
    public void StandardizedImageMetadataObjTest()
    {
        var obj1 = new StandardizedImageMetadata();
        var obj2 = new StandardizedImageMetadata();

        obj2.Path = "/test/path";
        obj2.Name = "test name";
        obj2.ImgHeight = 225;
        obj2.ImgWidth = 600;
        obj2.ColorType = ColorType.Index;
        obj2.BitDepth = 8;
        obj2.PPUnitX = 25;
        obj2.PPUnitY = 25;
        obj2.PUnit = "inches";
        obj2.AdditionalValues.Add("TEST1", new object());
        
        obj1.AdditionalValues.Add("TEST2", new object());
        
        if(obj1.VerifyResolution() || !obj2.VerifyResolution()) Assert.Fail();
        if(obj1.VerifyBitDepth() || !obj2.VerifyBitDepth()) Assert.Fail();
        if(obj1.VerifyColorType() || !obj2.VerifyColorType()) Assert.Fail();
        if(obj1.VerifyPhysicalUnits() || !obj2.VerifyPhysicalUnits()) Assert.Fail();
        
        if(obj2.CompareResolution(obj1)) Assert.Fail();
        if(obj2.CompareBitDepth(obj1)) Assert.Fail();
        if(obj2.CompareColorType(obj1)) Assert.Fail();
        if(obj2.ComparePhysicalUnits(obj1)) Assert.Fail();
        if(obj2.ComparePhysicalUnitsFlexible(obj1)) Assert.Fail();

        if(obj1.GetMissingAdditionalValues(obj2).Count != 2) Assert.Fail();
        
        Assert.Pass();
    }
}