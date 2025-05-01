using AvaloniaDraft.ComparingMethods;
using ImageMagick;

namespace UnitTests.ComparingMethodsTest;

[TestFixture]
public class CompareColorProfilesTest : TestBase
{
    [Test]
    public void TestBothProfilesSame()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-2.jpg");
        
        using var oImage = new MagickImage(oFilePath);
        using var nImage = new MagickImage(nFilePath);
        
        var result = ColorProfileComparison.CompareColorProfiles(oImage, nImage);
        Assert.That(result, Is.True); // Two files with same color profile should pass
    }
    
    [Test]
    public void TestBothProfilesDifferent()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-AdobeRGB-1.jpg");
        
        using var oImage = new MagickImage(oFilePath);
        using var nImage = new MagickImage(nFilePath);
        
        var result = ColorProfileComparison.CompareColorProfiles(oImage, nImage);
        Assert.That(result, Is.False); // Two files with different color profile should fail
    }
    
    [Test]
    public void TestOriginalNoProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-without-profile.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        
        using var oImage = new MagickImage(oFilePath);
        using var nImage = new MagickImage(nFilePath);
        
        var result = ColorProfileComparison.CompareColorProfiles(oImage, nImage);
        Assert.That(result, Is.False); // Original file without profile should fail
    }
    
    [Test]
    public void TestNewNoProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "image-without-profile.jpg");
        
        using var oImage = new MagickImage(oFilePath);
        using var nImage = new MagickImage(nFilePath);
        
        var result = ColorProfileComparison.CompareColorProfiles(oImage, nImage);
        Assert.That(result, Is.False); // New file without profile should fail
    }
    
    [Test]
    public void TestBothNoProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-without-profile.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "image-without-profile.jpg");
        
        using var oImage = new MagickImage(oFilePath);
        using var nImage = new MagickImage(nFilePath);
        
        var result = ColorProfileComparison.CompareColorProfiles(oImage, nImage);
        Assert.That(result, Is.True); // Two files without profile should pass
    }
}

[TestFixture]
public class ImageToImageColorProfileComparisonTest : TestBase
{
    [Test]
    public void TestBothProfilesSame()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-2.jpg");
        
        using var oImage = new MagickImage(oFilePath);
        using var nImage = new MagickImage(nFilePath);
        
        var result = ColorProfileComparison.ImageToImageColorProfileComparison(oImage, nImage);
        Assert.That(result, Is.True); // Two files with same color profile should pass
    }
    
    [Test]
    public void TestBothProfilesDifferent()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-AdobeRGB-1.jpg");
        
        using var oImage = new MagickImage(oFilePath);
        using var nImage = new MagickImage(nFilePath);
        
        var result = ColorProfileComparison.ImageToImageColorProfileComparison(oImage, nImage);
        Assert.That(result, Is.False); // Two files with different color profile should fail
    }
    
    [Test]
    public void TestOriginalNoProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-without-profile.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        
        using var oImage = new MagickImage(oFilePath);
        using var nImage = new MagickImage(nFilePath);
        
        var result = ColorProfileComparison.ImageToImageColorProfileComparison(oImage, nImage);
        Assert.That(result, Is.False); // Original file without profile should fail
    }
    
    [Test]
    public void TestNewNoProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "image-without-profile.jpg");
        
        using var oImage = new MagickImage(oFilePath);
        using var nImage = new MagickImage(nFilePath);
        
        var result = ColorProfileComparison.ImageToImageColorProfileComparison(oImage, nImage);
        Assert.That(result, Is.False); // New file without profile should fail
    }

    [Test]
    public void TestBothNoProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-without-profile.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "image-without-profile.jpg");

        using var oImage = new MagickImage(oFilePath);
        using var nImage = new MagickImage(nFilePath);
        
        var result = ColorProfileComparison.ImageToImageColorProfileComparison(oImage, nImage);
        Assert.That(result, Is.True); // Two files without profile should pass
    }
}

[TestFixture]
public class GeneralDocsToPdfColorProfileComparisonTest : TestBase
{
    [Test]
    public void TestImagesOfDifferentProfilesDocxDisk()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "word_with_two_images_of_different_profile.docx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "word_with_two_images_of_different_profile1.pdf");
    
        ImageExtractionToDisk.ExtractImagesFromDocxToDisk(oFilePath, TestExtractionODirectory);
        ImageExtractionToDisk.ExtractImagesFromPdfToDisk(nFilePath, TestExtractionNDirectory);
        try
        {
            Assert.Multiple(() =>
            {
                Assert.That(Directory.Exists(TestExtractionODirectory), Is.True);
                Assert.That(Directory.Exists(TestExtractionNDirectory), Is.True);
                Assert.That(Directory.GetFiles(TestExtractionODirectory), Has.Length.EqualTo(2));
                Assert.That(Directory.GetFiles(TestExtractionNDirectory), Has.Length.EqualTo(2));
            });
    
            var result = ColorProfileComparison.CompareColorProfilesFromDisk(TestExtractionODirectory, TestExtractionNDirectory);
            Assert.That(result, Is.True); // Two files with same color profiles should pass
        }
        finally
        {
            ImageExtractionToDisk.DeleteSavedFiles(TestExtractionODirectory);
            ImageExtractionToDisk.DeleteSavedFiles(TestExtractionNDirectory);
            Assert.Multiple(() =>
            {
                Assert.That(Directory.GetFiles(TestExtractionODirectory), Is.Empty);
                Assert.That(Directory.GetFiles(TestExtractionNDirectory), Is.Empty);
            });
        }
    }
    
    [Test]
    public void TestOneProfileSuccessPptxDisk()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PowerPoint", "presentation_with_one_type_color_profile.pptx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "presentation_with_one_type_color_profile.pdf");

        ImageExtractionToDisk.ExtractImagesFromXmlBasedPowerPointToDisk(oFilePath, TestExtractionODirectory);
        ImageExtractionToDisk.ExtractImagesFromPdfToDisk(nFilePath, TestExtractionNDirectory);
        try
        {
            Assert.Multiple(() =>
            {
                Assert.That(Directory.Exists(TestExtractionODirectory), Is.True);
                Assert.That(Directory.Exists(TestExtractionNDirectory), Is.True);
                Assert.That(Directory.GetFiles(TestExtractionODirectory), Has.Length.EqualTo(2));
                Assert.That(Directory.GetFiles(TestExtractionNDirectory), Has.Length.EqualTo(2));
            });
    
            var result = ColorProfileComparison.CompareColorProfilesFromDisk(TestExtractionODirectory, TestExtractionNDirectory);
            Assert.That(result, Is.True); // Two files with same color profiles should pass
        }
        finally
        {
            ImageExtractionToDisk.DeleteSavedFiles(TestExtractionODirectory);
            ImageExtractionToDisk.DeleteSavedFiles(TestExtractionNDirectory);
            Assert.Multiple(() =>
            {
                Assert.That(Directory.GetFiles(TestExtractionODirectory), Is.Empty);
                Assert.That(Directory.GetFiles(TestExtractionNDirectory), Is.Empty);
            });
        }
    }
    
    [Test]
    public void TestEmailWithOneMissingProfileDisk()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Mail", "email-with-one-missing-profile.eml");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "email-with-one-missing-profile.pdf");

        ImageExtractionToDisk.ExtractImagesFromEmlToDisk(oFilePath, TestExtractionODirectory);
        ImageExtractionToDisk.ExtractImagesFromPdfToDisk(nFilePath, TestExtractionNDirectory);
        try
        {
            Assert.Multiple(() =>
            {
                Assert.That(Directory.Exists(TestExtractionODirectory), Is.True);
                Assert.That(Directory.Exists(TestExtractionNDirectory), Is.True);
                Assert.That(Directory.GetFiles(TestExtractionODirectory), Has.Length.EqualTo(2));
                Assert.That(Directory.GetFiles(TestExtractionNDirectory), Has.Length.EqualTo(2));
            });
    
            var result = ColorProfileComparison.CompareColorProfilesFromDisk(TestExtractionODirectory, TestExtractionNDirectory);
            Assert.That(result, Is.True); // Two files with same color profiles should pass
        }
        finally
        {
            ImageExtractionToDisk.DeleteSavedFiles(TestExtractionODirectory);
            ImageExtractionToDisk.DeleteSavedFiles(TestExtractionNDirectory);
            Assert.Multiple(() =>
            {
                Assert.That(Directory.GetFiles(TestExtractionODirectory), Is.Empty);
                Assert.That(Directory.GetFiles(TestExtractionNDirectory), Is.Empty);
            });
        }
    }
    
    [Test]
    public void TestOdtWithOneMissingProfileDisk()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "ODT", "odt-with-one-missing-color-profiles.odt");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "odt-with-one-missing-color-profiles.pdf");

        ImageExtractionToDisk.ExtractImagesFromOpenDocumentsToDisk(oFilePath, TestExtractionODirectory);
        ImageExtractionToDisk.ExtractImagesFromPdfToDisk(nFilePath, TestExtractionNDirectory);
        try
        {
            Assert.Multiple(() =>
            {
                Assert.That(Directory.Exists(TestExtractionODirectory), Is.True);
                Assert.That(Directory.Exists(TestExtractionNDirectory), Is.True);
                Assert.That(Directory.GetFiles(TestExtractionODirectory), Has.Length.EqualTo(2));
                Assert.That(Directory.GetFiles(TestExtractionNDirectory), Has.Length.EqualTo(2));
            });
    
            var result = ColorProfileComparison.CompareColorProfilesFromDisk(TestExtractionODirectory, TestExtractionNDirectory);
            Assert.That(result, Is.True); // Two files with same color profiles should pass
        }
        finally
        {
            ImageExtractionToDisk.DeleteSavedFiles(TestExtractionODirectory);
            ImageExtractionToDisk.DeleteSavedFiles(TestExtractionNDirectory);
            Assert.Multiple(() =>
            {
                Assert.That(Directory.GetFiles(TestExtractionODirectory), Is.Empty);
                Assert.That(Directory.GetFiles(TestExtractionNDirectory), Is.Empty);
            });
        }
    }
    
    [Test]
    public void TestSuccessDisk()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "rtf_with_two_images_of_different_profile.rtf");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "word_with_two_images_of_different_profile.pdf");

        ImageExtractionToDisk.ExtractImagesFromRtfToDisk(oFilePath, TestExtractionODirectory);
        ImageExtractionToDisk.ExtractImagesFromPdfToDisk(nFilePath, TestExtractionNDirectory);
        try
        {
            Assert.Multiple(() =>
            {
                Assert.That(Directory.Exists(TestExtractionODirectory), Is.True);
                Assert.That(Directory.Exists(TestExtractionNDirectory), Is.True);
                Assert.That(Directory.GetFiles(TestExtractionODirectory), Has.Length.EqualTo(2));
                Assert.That(Directory.GetFiles(TestExtractionNDirectory), Has.Length.EqualTo(2));
            });
    
            var result = ColorProfileComparison.CompareColorProfilesFromDisk(TestExtractionODirectory, TestExtractionNDirectory);
            Assert.That(result, Is.True); // Two files with same color profiles should pass
        }
        finally
        {
            ImageExtractionToDisk.DeleteSavedFiles(TestExtractionODirectory);
            ImageExtractionToDisk.DeleteSavedFiles(TestExtractionNDirectory);
            Assert.Multiple(() =>
            {
                Assert.That(Directory.GetFiles(TestExtractionODirectory), Is.Empty);
                Assert.That(Directory.GetFiles(TestExtractionNDirectory), Is.Empty);
            });
        }
    }
    
    [Test]
    public void TestFailDisk()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "rtf_with_two_images_of_different_profile.rtf");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "word_with_two_images_of_same_profile.pdf");

        ImageExtractionToDisk.ExtractImagesFromRtfToDisk(oFilePath, TestExtractionODirectory);
        ImageExtractionToDisk.ExtractImagesFromPdfToDisk(nFilePath, TestExtractionNDirectory);
        try
        {
            Assert.Multiple(() =>
            {
                Assert.That(Directory.Exists(TestExtractionODirectory), Is.True);
                Assert.That(Directory.Exists(TestExtractionNDirectory), Is.True);
                Assert.That(Directory.GetFiles(TestExtractionODirectory), Has.Length.EqualTo(2));
                Assert.That(Directory.GetFiles(TestExtractionNDirectory), Has.Length.EqualTo(2));
            });
    
            var result = ColorProfileComparison.CompareColorProfilesFromDisk(TestExtractionODirectory, TestExtractionNDirectory);
            Assert.That(result, Is.False); // Two files with same color profiles should pass
        }
        finally
        {
            ImageExtractionToDisk.DeleteSavedFiles(TestExtractionODirectory);
            ImageExtractionToDisk.DeleteSavedFiles(TestExtractionNDirectory);
            Assert.Multiple(() =>
            {
                Assert.That(Directory.GetFiles(TestExtractionODirectory), Is.Empty);
                Assert.That(Directory.GetFiles(TestExtractionNDirectory), Is.Empty);
            });
        }
    }
}

[TestFixture]
public class XlsxToPdfColorProfileComparison : TestBase
{
    [Test]
    public void TestDifferentProfileOverAndInCellsDisk()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Spreadsheet", "excel_with_two_images_of_different_profile_over_cells.xlsx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "excel_with_two_images_of_different_profile_over_cells.pdf");

        ImageExtractionToDisk.ExtractImagesFromXlsxToDisk(oFilePath, TestExtractionODirectory);
        ImageExtractionToDisk.ExtractImagesFromPdfToDisk(nFilePath, TestExtractionNDirectory);
        try
        {
            Assert.Multiple(() =>
            {
                Assert.That(Directory.Exists(TestExtractionODirectory), Is.True);
                Assert.That(Directory.Exists(TestExtractionNDirectory), Is.True);
                Assert.That(ImageExtractionToDisk.CheckIfEqualNumberOfImages(TestExtractionODirectory, TestExtractionNDirectory), Is.True);
            });
    
            var result = ColorProfileComparison.CompareColorProfilesFromDisk(TestExtractionODirectory, TestExtractionNDirectory);
            Assert.That(result, Is.False); // Will fail because color profile is not retained after conversion
        }
        finally
        {
            ImageExtractionToDisk.DeleteSavedFiles(TestExtractionODirectory);
            ImageExtractionToDisk.DeleteSavedFiles(TestExtractionNDirectory);
            Assert.Multiple(() =>
            {
                Assert.That(Directory.GetFiles(TestExtractionODirectory), Is.Empty);
                Assert.That(Directory.GetFiles(TestExtractionNDirectory), Is.Empty);
            });
        }
    }
}