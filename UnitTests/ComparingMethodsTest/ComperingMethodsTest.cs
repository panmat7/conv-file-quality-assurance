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
        Assert.AreEqual(1, diff);
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
        Assert.AreEqual(0, diff);
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
        Assert.AreEqual(new Tuple<int, int>(225, 225), diff);
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
        Assert.AreEqual(new Tuple<int, int>(375, 225), diff);
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
        Assert.AreEqual(new Tuple<int, int>(225, 375), diff);
    }

    [Test]
    public void GetImageResolutionTest_225PNG()
    {
        var diff = ComperingMethods.GetImageResolution(_testFileDirectory + @"Images\225x225.png");
        
        Assert.AreEqual(new Tuple<int, int>(225, 225), diff);
    }

    [Test]
    public void GetImageResolutionTest_600x450JPG()
    {
        var diff = ComperingMethods.GetImageResolution(_testFileDirectory + @"Images\600x450.jpg");
        
        Assert.AreEqual(new Tuple<int, int>(600, 450), diff);
    }
    
    [Test]
    public void GetImageResolution_450x600TIFF()
    {
        var diff = ComperingMethods.GetImageResolution(_testFileDirectory + @"Images\450x600.tiff");
        
        Assert.AreEqual(new Tuple<int, int>(450, 600), diff);
    }

    [Test]
    public void GetPageCountDifferenceTest_3DOCX_3ODT()
    {
        var files = new FilePair
        (
            _testFileDirectory + @"TestDocuments\NoImage3Pages.docx",
            "fmt/413",
            _testFileDirectory + @"TestDocuments\NoImage3Pages.odt",
            "fmt/139"
        );
        
        var diff = ComperingMethods.GetPageCountDifference(files);
        Assert.AreEqual(0, diff);
    }
    
    [Test]
    public void GetPageCountDifferenceTest_8DOCX_3ODT()
    {
        var files = new FilePair
        (
            _testFileDirectory + @"TestDocuments\Image8Pages.docx",
            "fmt/413",
            _testFileDirectory + @"TestDocuments\NoImage3Pages.odt",
            "fmt/139"
        );
        
        var diff = ComperingMethods.GetPageCountDifference(files);
        Assert.AreEqual(5, diff);
    }
    
    [Test]
    public void GetPageCountDifferenceTest_8PDF_3DOCX()
    {
        var files = new FilePair
        (
            _testFileDirectory + @"TestDocuments\Image8Pages.pdf",
            "fmt/276",
            _testFileDirectory + @"TestDocuments\NoImage3Pages.odt",
            "fmt/139"
        );
        
        var diff = ComperingMethods.GetPageCountDifference(files);
        Assert.AreEqual(5, diff);
    }

    [Test]
    public void GetPageCountDifferenceTest_2PPT_2PDF()
    {
        var files = new FilePair
        (
            _testFileDirectory + @"PowerPoint\presentation_without_animations.ppt",
            "fmt/126",
            _testFileDirectory + @"PowerPoint\presentation_without_animations.pdf",
            "fmt/19"
        );
        
        var diff = ComperingMethods.GetPageCountDifference(files);
        Assert.AreEqual(0, diff);
    }
}