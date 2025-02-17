using AvaloniaDraft.FileManager;
using AvaloniaDraft.ComparingMethods;
using SixLabors.ImageSharp.Formats.Png;

namespace UnitTests.ComparingMethodsTest;

[TestFixture]
public class ComperingMethodsTest
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
    public void GetFileSizeDifferenceTest_0B_1B()
    {
        var files = new FilePair
        (
            _testFileDirectory + @"TestDocuments\Empty.txt",
            _testFileDirectory + @"TestDocuments\OneLetter.txt"
        );
        
        var diff = ComperingMethods.GetFileSizeDifference(files);
        Assert.That(diff, Is.EqualTo(1));
    }
    
    [Test]
    public void GetFileSizeDifferenceTest_33293B_1B()
    {
        var files = new FilePair
        (
            _testFileDirectory + @"TestDocuments\NoImage3Pages.pdf",
            _testFileDirectory + @"TestDocuments\OneLetter.txt"
        );
        
        var diff = ComperingMethods.GetFileSizeDifference(files);
        Assert.AreEqual(33292, diff);
    }
    
    [Test]
    public void GetFileSizeDifferenceTest_33293B_33293B()
    {
        var files = new FilePair
        (
            _testFileDirectory + @"TestDocuments\NoImage3Pages.pdf",
            _testFileDirectory + @"TestDocuments\NoImage3Pages.pdf"
        );
        
        var diff = ComperingMethods.GetFileSizeDifference(files);
        Assert.That(diff, Is.EqualTo(0));
    }

    [Test]
    public void GetImageResolutionDifferenceTest_225PNG_450PNG()
    {
        var files = new FilePair
        (
            _testFileDirectory + @"Images\225x225.png",
            _testFileDirectory + @"Images\450x450.png"
        );
        
        var diff = ComperingMethods.GetImageResolutionDifference(files);
        Assert.That(diff, Is.EqualTo(new Tuple<int, int>(225, 225)));
    }
    
    [Test]
    public void GetImageResolutionDifferenceTest_225PNG_600x450JPG()
    {
        var files = new FilePair
        (
            _testFileDirectory + @"Images\225x225.png",
            _testFileDirectory + @"Images\600x450.jpg"
        );
        
        var diff = ComperingMethods.GetImageResolutionDifference(files);
        Assert.That(diff, Is.EqualTo(new Tuple<int, int>(375, 225)));
    }
    
    [Test]
    public void GetImageResolutionDifferenceTest_225PNG_450x6000TIFF()
    {
        var files = new FilePair
        (
            _testFileDirectory + @"Images\225x225.png",
            _testFileDirectory + @"Images\450x600.tiff"
        );
        
        var diff = ComperingMethods.GetImageResolutionDifference(files);
        Assert.That(diff, Is.EqualTo(new Tuple<int, int>(225, 375)));
    }

    [Test]
    public void GetImageResolutionTest_225PNG()
    {
        var diff = ComperingMethods.GetImageResolution(_testFileDirectory + @"Images\225x225.png");
        
        Assert.That(diff, Is.EqualTo(new Tuple<int, int>(225, 225)));
    }

    [Test]
    public void GetImageResolutionTest_600x450JPG()
    {
        var diff = ComperingMethods.GetImageResolution(_testFileDirectory + @"Images\600x450.jpg");
        
        Assert.That(diff, Is.EqualTo(new Tuple<int, int>(600, 450)));
    }
    
    [Test]
    public void GetImageResolution_450x600TIFF()
    {
        var diff = ComperingMethods.GetImageResolution(_testFileDirectory + @"Images\450x600.tiff");
        
        Assert.That(diff, Is.EqualTo(new Tuple<int, int>(450, 600)));
    }

    [Test]
    public void GetPageCountDifferenceTest_3DOCX_3ODT()
    {
        var files = new FilePair
        (
            _testFileDirectory + @"TestDocuments\NoImage3Pages.docx",
            "fmt/412",
            _testFileDirectory + @"TestDocuments\NoImage3Pages.odt",
            "fmt/1756"
        );
        
        var diff = ComperingMethods.GetPageCountDifference(files);
        Assert.That(diff, Is.EqualTo(0));
    }
    
    [Test]
    public void GetPageCountDifferenceTest_8DOCX_3ODT()
    {
        var files = new FilePair
        (
            _testFileDirectory + @"TestDocuments\Image8Pages.docx",
            "fmt/412",
            _testFileDirectory + @"TestDocuments\NoImage3Pages.odt",
            "fmt/1756"
        );
        
        var diff = ComperingMethods.GetPageCountDifference(files);
        Assert.That(diff, Is.EqualTo(5));
    }
    
    [Test]
    public void GetPageCountDifferenceTest_8PDF_3DOCX()
    {
        var files = new FilePair
        (
            _testFileDirectory + @"TestDocuments\Image8Pages.pdf",
            "fmt/276",
            _testFileDirectory + @"TestDocuments\NoImage3Pages.odt",
            "fmt/1756"
        );
        
        var diff = ComperingMethods.GetPageCountDifference(files);
        Assert.That(diff, Is.EqualTo(5));
    }

    [Test]
    public void GetPageCountDifferenceTest_2PPT_2PDF()
    {
        var files = new FilePair
        (
            _testFileDirectory + @"PowerPoint\presentation_without_animations.ppt",
            "fmt/126",
            _testFileDirectory + @"PDF\presentation_without_animations.pdf",
            "fmt/19"
        );
        
        var diff = ComperingMethods.GetPageCountDifference(files);
        Assert.That(diff, Is.EqualTo(0));
    }
    
    [Test]
    public void GetPageCountDifferenceExifTest_3DOCX_3ODT()
    {
        var files = new FilePair
        (
            _testFileDirectory + @"TestDocuments\NoImage3Pages.docx",
            "fmt/412",
            _testFileDirectory + @"TestDocuments\NoImage3Pages.odt",
            "fmt/1756"
        );
        
        var diff = ComperingMethods.GetPageCountDifferenceExif(files);
        Assert.That(diff, Is.EqualTo(0));
    }
    
    [Test]
    public void GetPageCountDifferenceExifTest_8DOCX_3ODT()
    {
        var files = new FilePair
        (
            _testFileDirectory + @"TestDocuments\Image8Pages.docx",
            "fmt/412",
            _testFileDirectory + @"TestDocuments\NoImage3Pages.odt",
            "fmt/1756"
        );
        
        var diff = ComperingMethods.GetPageCountDifferenceExif(files);
        Assert.That(diff, Is.EqualTo(5));
    }
    
    [Test]
    public void GetPageCountDifferenceExifTest_8PDF_3DOCX()
    {
        var files = new FilePair
        (
            _testFileDirectory + @"TestDocuments\Image8Pages.pdf",
            "fmt/276",
            _testFileDirectory + @"TestDocuments\NoImage3Pages.odt",
            "fmt/1756"
        );
        
        var diff = ComperingMethods.GetPageCountDifferenceExif(files);
        Assert.That(diff, Is.EqualTo(5));
    }

    [Test]
    public void GetPageCountDifferenceExifTest_2PPT_2PDF()
    {
        var files = new FilePair
        (
            _testFileDirectory + @"PowerPoint\presentation_without_animations.ppt",
            "fmt/126",
            _testFileDirectory + @"PDF\presentation_without_animations.pdf",
            "fmt/19"
        );
        
        var diff = ComperingMethods.GetPageCountDifferenceExif(files);
        Assert.That(diff, Is.EqualTo(0));
    }

    [Test]
    public void GetPageCountTest_8PDF_3ODT()
    {
        var files = new FilePair
        (
            _testFileDirectory + @"TestDocuments\Image8Pages.pdf",
            "fmt/276",
            _testFileDirectory + @"TestDocuments\NoImage3Pages.odt",
            "fmt/1756"
        );

        var count1 = ComperingMethods.GetPageCount(files.OriginalFilePath, files.OriginalFileFormat);
        var count2 = ComperingMethods.GetPageCount(files.NewFilePath, files.NewFileFormat);
        
        Assert.That(count1 is 8 && count2 is 3);
    }

    [Test]
    public void GetMissingOrWrongImageMetadataExifTest_JPG_TIFF()
    {
        var files = new FilePair
        (
            _testFileDirectory + @"Images\600x450.jpg",
            "fmt/43",
            _testFileDirectory + @"Images\450x600.tiff",
            "fmt/353"
        );
        
        var result = ComperingMethods.GetMissingOrWrongImageMetadataExif(files);

        if (result is null || result.Count != 1) Assert.Fail();
        
        Assert.Pass();
    }
    
    [Test]
    public void GetMissingOrWrongImageMetadataExifTest_JPG_PNG()
    {
        var files = new FilePair
        (
            _testFileDirectory + @"Images\600x450.jpg",
            "fmt/43",
            _testFileDirectory + @"Images\T225x225.png",
            "fmt/12"
        );
        
        var result = ComperingMethods.GetMissingOrWrongImageMetadataExif(files);

        if (result is null || result.Count != 4) Assert.Fail();
        
        Assert.Pass();
    }

    [Test]
    public void ContainsTransparencyTest_PNG_NoTransparency()
    {
        var res = ComperingMethods.ContainsTransparency(_testFileDirectory + @"Images\225x225.png", "fmt/11");
        
        Assert.That(res, Is.False);
    }
    
    [Test]
    public void ContainsTransparencyTest_TIFF_NoTransparency()
    {
        var res = ComperingMethods.ContainsTransparency(_testFileDirectory + @"Images\450x600.tiff", "fmt/353");
        
        Assert.That(res, Is.False);
    }
    
    [Test]
    public void ContainsTransparencyTest_JPG()
    {
        var res = ComperingMethods.ContainsTransparency(_testFileDirectory + @"Images\600x450.jpg", "fmt/43");
        
        Assert.That(res, Is.False);
    }
    
    [Test]
    public void ContainsTransparencyTest_PNG_Transparency()
    {
        var res = ComperingMethods.ContainsTransparency(_testFileDirectory + @"Images\T225x225.png", "fmt/11");
        
        Assert.That(res, Is.True);
    }
    
    [Test]
    public void ContainsTransparencyTest_TIFF_Transparency()
    {
        var res = ComperingMethods.ContainsTransparency(_testFileDirectory + @"Images\T450x600.tiff", "fmt/353");
        
        Assert.That(res, Is.True);
    }
}