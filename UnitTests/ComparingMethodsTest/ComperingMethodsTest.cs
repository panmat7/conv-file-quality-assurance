using AvaloniaDraft.FileManager;
using AvaloniaDraft.ComparingMethods;
using SixLabors.ImageSharp.Formats.Png;

namespace UnitTests.ComparingMethodsTest;

[TestFixture]
public class ComperingMethodsTest
{
    private string TestFileDirectory = "";
    
    [SetUp]
    public void Setup()
    {
        var curDir = Directory.GetCurrentDirectory();

        while (!string.IsNullOrEmpty(curDir))
        {
            if (Path.GetFileName(curDir) == "conv-file-quality-assurance")
            {
                TestFileDirectory = curDir + @"\UnitTests\ComparingMethodsTest\TestFiles\";
                return;
            }
            
            curDir = Directory.GetParent(curDir)?.FullName;
        }
        
        throw new Exception("Failed to find project directory \"conv-file-quality-assurance\"");
    }

    [Test]
    public void GetFileSizeDifferenceTest()
    {
        var files = new FilePair
        (
            TestFileDirectory + @"TestDocuments\Empty.txt",
            TestFileDirectory + @"TestDocuments\OneLetter.txt"
        );
        
        var diff = ComperingMethods.GetFileSizeDifference(files);
        Assert.AreEqual(1, diff);
    }

    [Test]
    public void GetImageResolutionDifference_225PNG_450PNG()
    {
        var files = new FilePair
        (
            TestFileDirectory + @"Images\225x225.png",
            TestFileDirectory + @"Images\450x450.png"
        );
        
        var diff = ComperingMethods.GetImageResolutionDifference(files);
        Assert.AreEqual(new Tuple<int, int>(225, 225), diff);
    }
    
    [Test]
    public void GetImageResolutionDifference_225PNG_600x450JPG()
    {
        var files = new FilePair
        (
            TestFileDirectory + @"Images\225x225.png",
            TestFileDirectory + @"Images\600x450.jpg"
        );
        
        var diff = ComperingMethods.GetImageResolutionDifference(files);
        Assert.AreEqual(new Tuple<int, int>(375, 225), diff);
    }
    
    [Test]
    public void GetImageResolutionDifference_225PNG_600x450TIFF()
    {
        var files = new FilePair
        (
            TestFileDirectory + @"Images\225x225.png",
            TestFileDirectory + @"Images\450x600.tiff"
        );
        
        var diff = ComperingMethods.GetImageResolutionDifference(files);
        Assert.AreEqual(new Tuple<int, int>(225, 375), diff);
    }

    [Test]
    public void GetPageCountDifferenceTest_3DOCX_3ODT()
    {
        var files = new FilePair
        (
            TestFileDirectory + @"TestDocuments\NoImage3Pages.docx",
            "fmt/413",
            TestFileDirectory + @"TestDocuments\NoImage3Pages.odt",
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
            TestFileDirectory + @"TestDocuments\Image8Pages.docx",
            "fmt/413",
            TestFileDirectory + @"TestDocuments\NoImage3Pages.odt",
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
            TestFileDirectory + @"TestDocuments\Image8Pages.pdf",
            "fmt/276",
            TestFileDirectory + @"TestDocuments\NoImage3Pages.odt",
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
            TestFileDirectory + @"PowerPoint\presentation_without_animations.ppt",
            "fmt/126",
            TestFileDirectory + @"PowerPoint\presentation_without_animations.pdf",
            "fmt/19"
        );
        
        var diff = ComperingMethods.GetPageCountDifference(files);
        Assert.AreEqual(0, diff);
    }
}