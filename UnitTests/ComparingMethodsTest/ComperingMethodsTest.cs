using AvaloniaDraft.FileManager;
using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.Helpers;
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
    public void CheckFileSizeDifferenceTest_0B_1B()
    {
        var files = new FilePair
        (
            _testFileDirectory + @"TestDocuments\Empty.txt",
            _testFileDirectory + @"TestDocuments\OneLetter.txt"
        );
        
        var diff = ComperingMethods.CheckFileSizeDifference(files, 0.5);
        
        if(diff is null) Assert.Fail();
       
        Assert.That(diff, Is.True);
    }
    
    [Test]
    public void CheckFileSizeDifferenceTest_33293B_1B()
    {
        var files = new FilePair
        (
            _testFileDirectory + @"TestDocuments\NoImage3Pages.pdf",
            _testFileDirectory + @"TestDocuments\OneLetter.txt"
        );
        
        var diff = ComperingMethods.CheckFileSizeDifference(files, 0.5);
        
        if(diff is null) Assert.Fail();
        
        Assert.That(diff, Is.True);
    }
    
    [Test]
    public void CheckFileSizeDifferenceTest_33293B_33293B()
    {
        var files = new FilePair
        (
            _testFileDirectory + @"TestDocuments\NoImage3Pages.pdf",
            _testFileDirectory + @"TestDocuments\NoImage3Pages.pdf"
        );
        
        var diff = ComperingMethods.CheckFileSizeDifference(files, 0.5);
        
        if(diff is null) Assert.Fail();
        
        Assert.That(diff, Is.False);
    }

    [Test]
    public void CheckFileSizeDifferenceTest_Invalid()
    {
        var files = new FilePair
        (
            "Not",
            "Real"
        );
        
        var diff = ComperingMethods.CheckFileSizeDifference(files, 0.5);
        Assert.That(diff, Is.Null);
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
    public void GetImageResolutionDifferenceTest_Invalid()
    {
        var files = new FilePair
        (
            "Not",
            "Real"
        );
        
        var diff = ComperingMethods.GetImageResolutionDifference(files);
        Assert.That(diff, Is.Null);
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
    public void GetImageResolution_Invalid()
    {
        var diff = ComperingMethods.GetImageResolution("Not real");
        
        Assert.That(diff, Is.Null);
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
    public void GetPageCountDifferenceExifTest_2PPT_2ODP()
    {
        var files = new FilePair
        (
            _testFileDirectory + @"PowerPoint\presentation_without_animations.ppt",
            "fmt/126",
            _testFileDirectory + @"ODP\odp-with-animations.odp",
            "fmt/1754"
        );
        
        var diff = ComperingMethods.GetPageCountDifferenceExif(files);
        Assert.That(diff, Is.EqualTo(0));
    }

    [Test]
    public void GetPageCountDifferenceExifTest_2RTF_2PDF()
    {
        var files = new FilePair
        (
            _testFileDirectory + @"TestDocuments\rtf_with_two_images_of_different_profile.rtf",
            "fmt/355",
            _testFileDirectory + @"PDF\presentation_without_animations.pdf",
            "fmt/19"
        );
        
        var diff = ComperingMethods.GetPageCountDifferenceExif(files);
        Assert.That(diff, Is.EqualTo(0));
    }

    [Test]
    public void GetPageCountDifferenceExifTest_Invalid()
    {
        var files = new FilePair
        (
            _testFileDirectory + @"Images\225x225.png",
            _testFileDirectory + @"Images\450x600.tiff"
        );
        
        var diff = ComperingMethods.GetPageCountDifferenceExif(files);
        Assert.That(diff, Is.Null);
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
    public void GetMissingOrWrongImageMetadataExifTest_Invalid()
    {
        var files = new FilePair
        (
            _testFileDirectory + "Not",
            "fmt/312312313",
            _testFileDirectory + "Real",
            "fmt/313132313"
        );
        
        var result = ComperingMethods.GetMissingOrWrongImageMetadataExif(files);
        
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetMissingOrWrongImageMetadataExifTest_NotImageWrongPronom()
    {
        var files = new FilePair
        (
            _testFileDirectory + @"ODS\ods-with-no-images.ods",
            "fmt/43",
            _testFileDirectory + @"PDF\correct_transparency.pdf",
            "fmt/12"
        );
        
        var res = ComperingMethods.GetMissingOrWrongImageMetadataExif(files);
        
        //Should not fail, as the pronom codes match
        if(res is null) Assert.Fail();
        
        //Everything should be missing, as the comparison commenced but none of the values are present in non-images
        Assert.Multiple(() =>
        {
            Assert.That(res.Any(e => e.Name == "Image resolution missing in original file"), Is.True);
            Assert.That(res.Any(e => e.Name == "Image resolution missing in new file"), Is.True);
            Assert.That(res.Any(e => e.Name == "Bit-depth missing in original file"), Is.True);
            Assert.That(res.Any(e => e.Name == "Bit-depth missing in new file"), Is.True);
            Assert.That(res.Any(e => e.Name == "Color type missing in original file"), Is.True);
            Assert.That(res.Any(e => e.Name == "Color type missing in new file"), Is.True);
        });
    }

    [Test]
    public void ContainsTransparencyTest_PNG_NoTransparency()
    {
        var res = ComperingMethods.ContainsTransparency(_testFileDirectory + @"Images\225x225.png");
        
        Assert.That(res, Is.False);
    }
    
    [Test]
    public void ContainsTransparencyTest_TIFF_NoTransparency()
    {
        var res = ComperingMethods.ContainsTransparency(_testFileDirectory + @"Images\450x600.tiff");
        
        Assert.That(res, Is.False);
    }
    
    [Test]
    public void ContainsTransparencyTest_JPG()
    {
        var res = ComperingMethods.ContainsTransparency(_testFileDirectory + @"Images\600x450.jpg");
        
        Assert.That(res, Is.False);
    }
    
    [Test]
    public void ContainsTransparencyTest_PNG_Transparency()
    {
        var res = ComperingMethods.ContainsTransparency(_testFileDirectory + @"Images\T225x225.png");
        
        Assert.That(res, Is.True);
    }
    
    [Test]
    public void ContainsTransparencyTest_TIFF_Transparency()
    {
        var res = ComperingMethods.ContainsTransparency(_testFileDirectory + @"Images\T450x600.tiff");
        
        Assert.That(res, Is.True);
    }

    [Test]
    public void VisualDocumentComparisonTest_SameFile()
    {
        var pair = new FilePair(
            _testFileDirectory + @"TestDocuments\Image8Pages.pdf",
            "fmt/12",
            _testFileDirectory + @"TestDocuments\Image8Pages.pdf",
            "fmt/12"
        );
        
        var res = ComperingMethods.VisualDocumentComparison(pair);
        
        if(res is null || res.Count > 0) Assert.Fail();
        
        Assert.Pass();
    }
    
    [Test]
    public void VisualDocumentComparisonTest_SameFileSelectedPages()
    {
        var pair = new FilePair(
            _testFileDirectory + @"TestDocuments\Image8Pages.pdf",
            "fmt/12",
            _testFileDirectory + @"TestDocuments\Image8Pages.pdf",
            "fmt/12"
        );
        
        var res = ComperingMethods.VisualDocumentComparison(pair, 2, 5);
        
        if(res is null || res.Count > 0) Assert.Fail();
        
        Assert.Pass();
    }
    
    [Test]
    public void VisualDocumentComparisonTest_DifferentPageCount()
    {
        var pair = new FilePair(
            _testFileDirectory + @"TestDocuments\Image8Pages.pdf",
            "fmt/12",
            _testFileDirectory + @"TestDocuments\NoImage3Pages.pdf",
            "fmt/12"
        );
        
        var res = ComperingMethods.VisualDocumentComparison(pair);
        
        if(res is null || res.Count != 1) Assert.Fail();
        
        Assert.That(res.Any(e => e.Name == "Could not preform visual comparison due to mismatched page count"), Is.True);
    }
    
    [Test]
    public void VisualDocumentComparisonTest_ChangedFile()
    {
        var pair = new FilePair(
            _testFileDirectory + @"TestDocuments\Image8Pages.pdf",
            "fmt/12",
            _testFileDirectory + @"TestDocuments\Image8PagesChanged.pdf",
            "fmt/12"
        );
        
        var res = ComperingMethods.VisualDocumentComparison(pair);
        
        if(res is null || res.Count == 0) Assert.Fail();
        
        Assert.Pass();
    }

    [Test]
    public void VisualDocumentComparisonTest_Invalid()
    {
        var pair = new FilePair(
            _testFileDirectory + @"TestDocuments\Image8Pages.docx",
            "fmt/12",
            _testFileDirectory + @"TestDocuments\notarealfile",
            "fmt/12"
        );
        
        var res = ComperingMethods.VisualDocumentComparison(pair);
        
        if(res is not null) Assert.Fail();
        
        Assert.Pass();
    }
}