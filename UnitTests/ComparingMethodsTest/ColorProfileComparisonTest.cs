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
public class ImageToPdfColorProfileComparisonTest : TestBase
{
    [Test]
    public void TestBothSameProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "image-with-profile-sRGB-1.pdf");

        var oImage = new MagickImage(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.ImageToPdfColorProfileComparison(oImage, nImages);
        Assert.That(result, Is.True); // Image to PDF with same color profile should pass
    }

    [Test]
    public void TestBothDifferentProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "image-with-profile-AdobeRGB-1.pdf");
        
        var oImage = new MagickImage(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.ImageToPdfColorProfileComparison(oImage, nImages);
        Assert.That(result, Is.False); // Image to PDF with different color profile should fail
    }
    
    [Test]
    public void TestOriginalNoProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-without-profile.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "image-with-profile-sRGB-1.pdf");
        
        var oImage = new MagickImage(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.ImageToPdfColorProfileComparison(oImage, nImages);
        Assert.That(result, Is.False); // Original file without profile should fail
    }
    
    [Test]
    public void TestNewNoProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "image-without-profile.pdf");
        
        var oImage = new MagickImage(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.ImageToPdfColorProfileComparison(oImage, nImages);
        Assert.That(result, Is.False); // New file without profile should fail
    }
}

[TestFixture]
public class PdfToPdfColorProfileComparisonTest : TestBase
{
    [Test]
    public void TestBothSameProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PDF", "image-with-profile-sRGB-1.pdf");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "image-with-profile-sRGB-2.pdf");
        
        var oImages = ImageExtraction.GetNonDuplicatePdfImages(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.PdfToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True); // PDF to PDF with same color profile should pass
    }

    [Test]
    public void TestBothDifferentProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PDF", "image-with-profile-sRGB-1.pdf");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "image-with-profile-AdobeRGB-1.pdf");
        
        var oImages = ImageExtraction.GetNonDuplicatePdfImages(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.PdfToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.False); // PDF to PDF with different color profile should fail
    }
    
    [Test]
    public void TestBothNoProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PDF", "image-without-profile.pdf");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "image-without-profile.pdf");
        
        var oImages = ImageExtraction.GetNonDuplicatePdfImages(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.PdfToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True); // Two files without profile should pass
    }
    
    [Test]
    public void TestOriginalNoProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PDF", "image-without-profile.pdf");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "image-with-profile-sRGB-1.pdf");
        
        var oImages = ImageExtraction.GetNonDuplicatePdfImages(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.PdfToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.False); // Original file without profile should fail
    }
    
    [Test]
    public void TestNewNoProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PDF", "image-with-profile-sRGB-1.pdf");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "image-without-profile.pdf");
        
        var oImages = ImageExtraction.GetNonDuplicatePdfImages(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.PdfToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.False); // New file without profile should fail
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
    public void TestImagesOfDifferentProfilesDocx()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "word_with_two_images_of_different_profile.docx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "word_with_two_images_of_different_profile.pdf");

        var oImages = ImageExtraction.ExtractImagesFromDocx(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.GeneralDocsToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True); // Two files with same color profiles should pass
    }
    
    [Test]
    public void TestImagesOfDifferentProfilesDocxDisk()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "word_with_two_images_of_different_profile.docx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "word_with_two_images_of_different_profile1.pdf");
    
        ImageExtraction.ExtractImagesFromDocxToDisk(oFilePath, TestExtractionODirectory);
        ImageExtraction.ExtractImagesFromPdfToDisk(nFilePath, TestExtractionNDirectory);
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
            ImageExtraction.DeleteSavedImages(TestExtractionODirectory);
            ImageExtraction.DeleteSavedImages(TestExtractionNDirectory);
            Assert.Multiple(() =>
            {
                Assert.That(Directory.GetFiles(TestExtractionODirectory), Is.Empty);
                Assert.That(Directory.GetFiles(TestExtractionNDirectory), Is.Empty);
            });
        }
    }
    
    [Test]
    public void TestImagesOfSameProfilesDocx()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "word_with_two_images_of_same_profile.docx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "word_with_two_images_of_same_profile.pdf");

        var oImages = ImageExtraction.ExtractImagesFromDocx(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.GeneralDocsToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True); // Two files with same color profiles should pass
    }
    
    [Test]
    public void TestImagesWithOneMissingProfileDocx()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "word_with_two_images_with_one_missing_profile.docx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "word_with_two_images_with_one_missing_profile.pdf");

        var oImages = ImageExtraction.ExtractImagesFromDocx(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.GeneralDocsToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True); // Two files with same color profiles should pass
    }
    
    [Test]
    public void TestImagesWithMissingProfileDocx()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "word_with_images_with_no_profile.docx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "word_with_images_with_no_profile.pdf");

        var oImages = ImageExtraction.ExtractImagesFromDocx(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.GeneralDocsToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True); // Two files with same color profiles should pass
    }
    
    [Test]
    public void TestNoImagesDocx()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "word_with_no_images.docx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "word_with_no_images.pdf");

        var oImages = ImageExtraction.ExtractImagesFromDocx(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.GeneralDocsToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True); // If there are no images it cant fail the test
    }

    [Test]
    public void TestOneProfileSuccessPptx()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PowerPoint", "presentation_with_one_type_color_profile.pptx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "presentation_with_one_type_color_profile.pdf");

        var oImages = ImageExtraction.ExtractImagesFromXmlBasedPowerPoint(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.GeneralDocsToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True); // Two files where color profiles match should pass
    }
    
    [Test]
    public void TestOneProfileSuccessPptxDisk()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PowerPoint", "presentation_with_one_type_color_profile.pptx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "presentation_with_one_type_color_profile.pdf");

        ImageExtraction.ExtractImagesFromXmlBasedPowerPointToDisk(oFilePath, TestExtractionODirectory);
        ImageExtraction.ExtractImagesFromPdfToDisk(nFilePath, TestExtractionNDirectory);
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
            ImageExtraction.DeleteSavedImages(TestExtractionODirectory);
            ImageExtraction.DeleteSavedImages(TestExtractionNDirectory);
            Assert.Multiple(() =>
            {
                Assert.That(Directory.GetFiles(TestExtractionODirectory), Is.Empty);
                Assert.That(Directory.GetFiles(TestExtractionNDirectory), Is.Empty);
            });
        }
    }
    
    [Test]
    public void TestTwoProfileSuccessPptx()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PowerPoint", "presentation_with_two_type_color_profile.pptx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "presentation_with_two_type_color_profile.pdf");

        var oImages = ImageExtraction.ExtractImagesFromXmlBasedPowerPoint(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.GeneralDocsToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True); // Two files where color profiles match should pass
    }
    
    [Test]
    public void TestOneFileNoProfilesPptx()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PowerPoint", "presentation_with_two_type_color_profile.pptx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "presentation_with_two_type_color_profile_wrong.pdf");

        var oImages = ImageExtraction.ExtractImagesFromXmlBasedPowerPoint(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.GeneralDocsToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.False); // New file is missing profiles and so test should fail
    }
    
    [Test]
    public void TestOneProfilePresentOneMissingInEachFilePptx()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PowerPoint", "presentation_with_one_type_color_profile_and_one_missing.pptx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "presentation_with_one_type_color_profile_and_one_missing.pdf");

        var oImages = ImageExtraction.ExtractImagesFromXmlBasedPowerPoint(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.GeneralDocsToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True); // Two files where color profiles match should pass
    }
    
    [Test]
    public void TestBothNoProfilePptx()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PowerPoint", "presentation_with_no_color_profile.pptx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "presentation_with_no_color_profile.pdf");

        var oImages = ImageExtraction.ExtractImagesFromXmlBasedPowerPoint(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.GeneralDocsToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True); // Two files where neither has profiles should pass
    }

    [Test]
    public void TestEmailWithNoImages()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Mail", "email-with-no-images.eml");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "email-with-no-images.pdf");

        var oImages = ImageExtraction.ExtractImagesFromEml(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.GeneralDocsToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True);
    }

    [Test]
    public void TestEmailWithOneMissingProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Mail", "email-with-one-missing-profile.eml");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "email-with-one-missing-profile.pdf");

        var oImages = ImageExtraction.ExtractImagesFromEml(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.GeneralDocsToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void TestEmailWithOneMissingProfileDisk()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Mail", "email-with-one-missing-profile.eml");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "email-with-one-missing-profile.pdf");

        ImageExtraction.ExtractImagesFromEmlToDisk(oFilePath, TestExtractionODirectory);
        ImageExtraction.ExtractImagesFromPdfToDisk(nFilePath, TestExtractionNDirectory);
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
            ImageExtraction.DeleteSavedImages(TestExtractionODirectory);
            ImageExtraction.DeleteSavedImages(TestExtractionNDirectory);
            Assert.Multiple(() =>
            {
                Assert.That(Directory.GetFiles(TestExtractionODirectory), Is.Empty);
                Assert.That(Directory.GetFiles(TestExtractionNDirectory), Is.Empty);
            });
        }
    }

    [Test]
    public void TestEmailWithTwoSameImages()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Mail", "email-with-two-same-images.eml");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "email-with-two-same-images.pdf");

        var oImages = ImageExtraction.ExtractImagesFromEml(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.GeneralDocsToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True);
    }

    [Test]
    public void TestOdtWithNoImages()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "ODT", "odt-with-no-images.odt");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "odt-with-no-images.pdf");

        var oImages = ImageExtraction.ExtractImagesFromOpenDocuments(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.GeneralDocsToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True);
    }

    [Test]
    public void TestOdtWithOneMissingProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "ODT", "odt-with-one-missing-color-profiles.odt");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "odt-with-one-missing-color-profiles.pdf");

        var oImages = ImageExtraction.ExtractImagesFromOpenDocuments(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.GeneralDocsToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void TestOdtWithOneMissingProfileDisk()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "ODT", "odt-with-one-missing-color-profiles.odt");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "odt-with-one-missing-color-profiles.pdf");

        ImageExtraction.ExtractImagesFromOpenDocumentsToDisk(oFilePath, TestExtractionODirectory);
        ImageExtraction.ExtractImagesFromPdfToDisk(nFilePath, TestExtractionNDirectory);
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
            ImageExtraction.DeleteSavedImages(TestExtractionODirectory);
            ImageExtraction.DeleteSavedImages(TestExtractionNDirectory);
            Assert.Multiple(() =>
            {
                Assert.That(Directory.GetFiles(TestExtractionODirectory), Is.Empty);
                Assert.That(Directory.GetFiles(TestExtractionNDirectory), Is.Empty);
            });
        }
    }

    [Test]
    public void TestOdtWithTwoDifferentProfiles()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "ODT", "odt-with-two-color-profiles.odt");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "odt-with-two-color-profiles.pdf");

        var oImages = ImageExtraction.ExtractImagesFromOpenDocuments(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.GeneralDocsToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void TestOdpWithNoImages()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "ODP", "odp-with-no-images.odp");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "odp-with-no-images.pdf");

        var oImages = ImageExtraction.ExtractImagesFromOpenDocuments(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.GeneralDocsToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True);
    }

    [Test]
    public void TestOdpWithOneMissingProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "ODP", "odp-with-one-missing-color-profile.odp");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "odp-with-one-missing-color-profile.pdf");

        var oImages = ImageExtraction.ExtractImagesFromOpenDocuments(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.GeneralDocsToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True);
    }

    [Test]
    public void TestOdpWithTwoDifferentProfiles()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "ODP", "odp-with-two-color-profiles.odp");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "odp-with-two-color-profiles.pdf");

        var oImages = ImageExtraction.ExtractImagesFromOpenDocuments(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.GeneralDocsToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void TestSuccess()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "rtf_with_two_images_of_different_profile.rtf");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "word_with_two_images_of_different_profile.pdf");

        var oImages = ImageExtraction.ExtractImagesFromRtf(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.GeneralDocsToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void TestSuccessDisk()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "rtf_with_two_images_of_different_profile.rtf");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "word_with_two_images_of_different_profile.pdf");

        ImageExtraction.ExtractImagesFromRtfToDisk(oFilePath, TestExtractionODirectory);
        ImageExtraction.ExtractImagesFromPdfToDisk(nFilePath, TestExtractionNDirectory);
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
            ImageExtraction.DeleteSavedImages(TestExtractionODirectory);
            ImageExtraction.DeleteSavedImages(TestExtractionNDirectory);
            Assert.Multiple(() =>
            {
                Assert.That(Directory.GetFiles(TestExtractionODirectory), Is.Empty);
                Assert.That(Directory.GetFiles(TestExtractionNDirectory), Is.Empty);
            });
        }
    }

    [Test]
    public void TestFail()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "rtf_with_two_images_of_different_profile.rtf");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "word_with_two_images_of_same_profile.pdf");

        var oImages = ImageExtraction.ExtractImagesFromRtf(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.GeneralDocsToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.False);
        
        ImageExtraction.DisposeMagickImages(oImages);
        Assert.That(oImages, Has.All.Matches<MagickImage>(img => 
        {
            try
            {
                _ = img.Width;
                return false;
            }
            catch (ObjectDisposedException)
            {
                return true;
            }
        }));
    }
    
    [Test]
    public void TestFailDisk()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "rtf_with_two_images_of_different_profile.rtf");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "word_with_two_images_of_same_profile.pdf");

        ImageExtraction.ExtractImagesFromRtfToDisk(oFilePath, TestExtractionODirectory);
        ImageExtraction.ExtractImagesFromPdfToDisk(nFilePath, TestExtractionNDirectory);
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
            ImageExtraction.DeleteSavedImages(TestExtractionODirectory);
            ImageExtraction.DeleteSavedImages(TestExtractionNDirectory);
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
    public void TestOneProfileMissingInCells()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Spreadsheet", "excel_with_one_missing_profile_in_cells.xlsx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "excel_with_one_missing_profile_in_cells.pdf");

        var oImages = ImageExtraction.ExtractImagesFromXlsx(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        var imagesOverCells = ImageExtraction.GetNonAnchoredImagesFromXlsx(oFilePath);
        
        var result = ColorProfileComparison.XlsxToPdfColorProfileComparison(oImages, nImages, imagesOverCells);
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void TestOneProfileMissingOverAndInCells()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Spreadsheet", "excel_with_one_missing_profile_over_cells.xlsx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "excel_with_one_missing_profile_over_cells.pdf");

        var oImages = ImageExtraction.ExtractImagesFromXlsx(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        var imagesOverCells = ImageExtraction.GetNonAnchoredImagesFromXlsx(oFilePath);
        
        var result = ColorProfileComparison.XlsxToPdfColorProfileComparison(oImages, nImages, imagesOverCells);
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void TestDifferentProfileOverAndInCells()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Spreadsheet", "excel_with_two_images_of_different_profile_over_cells.xlsx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "excel_with_two_images_of_different_profile_over_cells.pdf");

        var oImages = ImageExtraction.ExtractImagesFromXlsx(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        var imagesOverCells = ImageExtraction.GetNonAnchoredImagesFromXlsx(oFilePath);
        
        var result = ColorProfileComparison.XlsxToPdfColorProfileComparison(oImages, nImages, imagesOverCells);
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void TestDifferentProfileOverAndInCellsDisk()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Spreadsheet", "excel_with_two_images_of_different_profile_over_cells.xlsx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "excel_with_two_images_of_different_profile_over_cells.pdf");

        ImageExtraction.ExtractImagesFromXlsxToDisk(oFilePath, TestExtractionODirectory);
        ImageExtraction.ExtractImagesFromPdfToDisk(nFilePath, TestExtractionNDirectory);
        try
        {
            Assert.Multiple(() =>
            {
                Assert.That(Directory.Exists(TestExtractionODirectory), Is.True);
                Assert.That(Directory.Exists(TestExtractionNDirectory), Is.True);
                // Assert.That(Directory.GetFiles(TestExtractionODirectory), Has.Length.EqualTo(2));
                // Assert.That(Directory.GetFiles(TestExtractionNDirectory), Has.Length.EqualTo(2));
            });
    
            var result = ColorProfileComparison.CompareColorProfilesFromDisk(TestExtractionODirectory, TestExtractionNDirectory);
            Assert.That(result, Is.True); // Two files with same color profiles should pass
        }
        finally
        {
            ImageExtraction.DeleteSavedImages(TestExtractionODirectory);
            ImageExtraction.DeleteSavedImages(TestExtractionNDirectory);
            Assert.Multiple(() =>
            {
                Assert.That(Directory.GetFiles(TestExtractionODirectory), Is.Empty);
                Assert.That(Directory.GetFiles(TestExtractionNDirectory), Is.Empty);
            });
        }
    }
    
    [Test]
    public void TestSameProfileInCells()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Spreadsheet", "excel_with_two_images_of_same_profile_in_cells.xlsx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "excel_with_two_images_of_same_profile_in_cells.pdf");

        var oImages = ImageExtraction.ExtractImagesFromXlsx(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        var imagesOverCells = ImageExtraction.GetNonAnchoredImagesFromXlsx(oFilePath);
        
        var result = ColorProfileComparison.XlsxToPdfColorProfileComparison(oImages, nImages, imagesOverCells);
        Assert.That(result, Is.True);
    }
}

[TestFixture]
public class GetNonAnchoredImagesFromXlsxTest : TestBase
{
    [Test]
    public void TestRetrieveImages()
    {
        var filePath = Path.Combine(TestFileDirectory, "Spreadsheet", 
            "excel_with_two_images_of_different_profile_over_cells.xlsx");

        var expected = new List<string> { "image2.jpg" };
        var result = ImageExtraction.GetNonAnchoredImagesFromXlsx(filePath);
        
        Assert.That(result, Is.EqualTo(expected));
    }
}