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
public class DocxToPdfColorProfileComparisonTest : TestBase
{
    [Test]
    public void TestImagesOfDifferentProfiles()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "word_with_two_images_of_different_profile.docx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "word_with_two_images_of_different_profile.pdf");

        var oImages = ImageExtraction.ExtractImagesFromDocx(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.DocxToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True); // Two files with same color profiles should pass
    }
    
    [Test]
    public void TestImagesOfSameProfiles()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "word_with_two_images_of_same_profile.docx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "word_with_two_images_of_same_profile.pdf");

        var oImages = ImageExtraction.ExtractImagesFromDocx(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.DocxToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True); // Two files with same color profiles should pass
    }
    
    [Test]
    public void TestImagesWithOneMissingProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "word_with_two_images_with_one_missing_profile.docx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "word_with_two_images_with_one_missing_profile.pdf");

        var oImages = ImageExtraction.ExtractImagesFromDocx(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.DocxToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True); // Two files with same color profiles should pass
    }
    
    [Test]
    public void TestImagesWithMissingProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "word_with_images_with_no_profile.docx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "word_with_images_with_no_profile.pdf");

        var oImages = ImageExtraction.ExtractImagesFromDocx(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.DocxToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True); // Two files with same color profiles should pass
    }
    
    [Test]
    public void TestNoImages()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "word_with_no_images.docx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "word_with_no_images.pdf");

        var oImages = ImageExtraction.ExtractImagesFromDocx(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.DocxToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True); // If there are no images it cant fail the test
    }
}

[TestFixture]
public class PowerPointToPdfColorProfileComparisonTest : TestBase
{
    [Test]
    public void TestOneProfileSuccess()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PowerPoint", "presentation_with_one_type_color_profile.pptx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "presentation_with_one_type_color_profile.pdf");

        var oImages = ImageExtraction.ExtractImagesFromXmlBasedPowerPoint(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.XmlBasedPowerPointToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True); // Two files where color profiles match should pass
    }
    
    [Test]
    public void TestTwoProfileSuccess()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PowerPoint", "presentation_with_two_type_color_profile.pptx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "presentation_with_two_type_color_profile.pdf");

        var oImages = ImageExtraction.ExtractImagesFromXmlBasedPowerPoint(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.XmlBasedPowerPointToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True); // Two files where color profiles match should pass
    }
    
    [Test]
    public void TestOneFileNoProfiles()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PowerPoint", "presentation_with_two_type_color_profile.pptx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "presentation_with_two_type_color_profile_wrong.pdf");

        var oImages = ImageExtraction.ExtractImagesFromXmlBasedPowerPoint(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.XmlBasedPowerPointToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.False); // New file is missing profiles and so test should fail
    }
    
    [Test]
    public void TestOneProfilePresentOneMissingInEachFile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PowerPoint", "presentation_with_one_type_color_profile_and_one_missing.pptx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "presentation_with_one_type_color_profile_and_one_missing.pdf");

        var oImages = ImageExtraction.ExtractImagesFromXmlBasedPowerPoint(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.XmlBasedPowerPointToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True); // Two files where color profiles match should pass
    }
    
    [Test]
    public void TestBothNoProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PowerPoint", "presentation_with_no_color_profile.pptx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "presentation_with_no_color_profile.pdf");

        var oImages = ImageExtraction.ExtractImagesFromXmlBasedPowerPoint(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.XmlBasedPowerPointToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True); // Two files where neither has profiles should pass
    }

}

[TestFixture]
public class EmlToPdfColorProfileComparison : TestBase
{
    [Test]
    public void TestEmailWithNoImages()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Mail", "email-with-no-images.eml");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "email-with-no-images.pdf");

        var oImages = ImageExtraction.ExtractImagesFromEml(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.EmlToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True);
    }

    [Test]
    public void TestEmailWithOneMissingProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Mail", "email-with-one-missing-profile.eml");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "email-with-one-missing-profile.pdf");

        var oImages = ImageExtraction.ExtractImagesFromEml(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.EmlToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True);
    }

    [Test]
    public void TestEmailWithTwoSameImages()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Mail", "email-with-two-same-images.eml");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "email-with-two-same-images.pdf");

        var oImages = ImageExtraction.ExtractImagesFromEml(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.EmlToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True);
    }
}

[TestFixture]
public class OdtAndOdpToPdfColorProfileComparison : TestBase
{
    [Test]
    public void TestOdtWithNoImages()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "ODT", "odt-with-no-images.odt");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "odt-with-no-images.pdf");

        var oImages = ImageExtraction.ExtractImagesFromOpenDocuments(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.OpenDocumentToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True);
    }

    [Test]
    public void TestOdtWithOneMissingProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "ODT", "odt-with-one-missing-color-profiles.odt");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "odt-with-one-missing-color-profiles.pdf");

        var oImages = ImageExtraction.ExtractImagesFromOpenDocuments(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.OpenDocumentToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True);
    }

    [Test]
    public void TestOdtWithTwoDifferentProfiles()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "ODT", "odt-with-two-color-profiles.odt");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "odt-with-two-color-profiles.pdf");

        var oImages = ImageExtraction.ExtractImagesFromOpenDocuments(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.OpenDocumentToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void TestOdpWithNoImages()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "ODP", "odp-with-no-images.odp");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "odp-with-no-images.pdf");

        var oImages = ImageExtraction.ExtractImagesFromOpenDocuments(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.OpenDocumentToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True);
    }

    [Test]
    public void TestOdpWithOneMissingProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "ODP", "odp-with-one-missing-color-profile.odp");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "odp-with-one-missing-color-profile.pdf");

        var oImages = ImageExtraction.ExtractImagesFromOpenDocuments(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.OpenDocumentToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True);
    }

    [Test]
    public void TestOdpWithTwoDifferentProfiles()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "ODP", "odp-with-two-color-profiles.odp");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "odp-with-two-color-profiles.pdf");

        var oImages = ImageExtraction.ExtractImagesFromOpenDocuments(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.OpenDocumentToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True);
    }
}

[TestFixture]
public class RtfToPdfColorProfileComparison : TestBase
{
    [Test]
    public void TestSuccess()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "rtf_with_two_images_of_different_profile.rtf");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "word_with_two_images_of_different_profile.pdf");

        var oImages = ImageExtraction.ExtractImagesFromRtf(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.RtfToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.True);
    }

    [Test]
    public void TestFail()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "rtf_with_two_images_of_different_profile.rtf");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "word_with_two_images_of_same_profile.pdf");

        var oImages = ImageExtraction.ExtractImagesFromRtf(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = ColorProfileComparison.RtfToPdfColorProfileComparison(oImages, nImages);
        Assert.That(result, Is.False);
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

        var oImages = ImageExtraction.ExtractImagesFromOpenDocuments(oFilePath);
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

        var oImages = ImageExtraction.ExtractImagesFromOpenDocuments(oFilePath);
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

        var oImages = ImageExtraction.ExtractImagesFromOpenDocuments(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        var imagesOverCells = ImageExtraction.GetNonAnchoredImagesFromXlsx(oFilePath);
        
        var result = ColorProfileComparison.XlsxToPdfColorProfileComparison(oImages, nImages, imagesOverCells);
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void TestSameProfileInCells()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Spreadsheet", "excel_with_two_images_of_same_profile_in_cells.xlsx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "excel_with_two_images_of_same_profile_in_cells.pdf");

        var oImages = ImageExtraction.ExtractImagesFromOpenDocuments(oFilePath);
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