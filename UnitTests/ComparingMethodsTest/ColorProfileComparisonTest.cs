using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;
using Xunit;
using Assert = Xunit.Assert;
using ImageMagick;

namespace UnitTests.ComparingMethodsTest;

public class CompareColorProfilesTest : TestBase
{
    [Fact]
    public void TestBothProfilesSame()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-2.jpg");
        
        using var oImage = new MagickImage(oFilePath);
        using var nImage = new MagickImage(nFilePath);
        
        var result = ColorProfileComparison.CompareColorProfiles(oImage, nImage);
        Assert.True(result); // Two files with same color profile should pass
    }
    
    [Fact]
    public void TestBothProfilesDifferent()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-AdobeRGB-1.jpg");
        
        using var oImage = new MagickImage(oFilePath);
        using var nImage = new MagickImage(nFilePath);
        
        var result = ColorProfileComparison.CompareColorProfiles(oImage, nImage);
        Assert.False(result); // Two files with different color profile should fail
    }
    
    [Fact]
    public void TestOriginalNoProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-without-profile.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        
        using var oImage = new MagickImage(oFilePath);
        using var nImage = new MagickImage(nFilePath);
        
        var result = ColorProfileComparison.CompareColorProfiles(oImage, nImage);
        Assert.False(result); // Original file without profile should fail
    }
    
    [Fact]
    public void TestNewNoProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "image-without-profile.jpg");
        
        using var oImage = new MagickImage(oFilePath);
        using var nImage = new MagickImage(nFilePath);
        
        var result = ColorProfileComparison.CompareColorProfiles(oImage, nImage);
        Assert.False(result); // New file without profile should fail
    }
    
    [Fact]
    public void TestBothNoProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-without-profile.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "image-without-profile.jpg");
        
        using var oImage = new MagickImage(oFilePath);
        using var nImage = new MagickImage(nFilePath);
        
        var result = ColorProfileComparison.CompareColorProfiles(oImage, nImage);
        Assert.True(result); // Two files without profile should pass
    }
}

public class ImageToPdfColorProfileComparisonTest : TestBase
{
    [Fact]
    public void TestBothSameProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "image-with-profile-sRGB-1.pdf");
        
        var files = new FilePair(oFilePath, "fmt/41", nFilePath, "fmt/477");
        var result = ColorProfileComparison.ImageToPdfColorProfileComparison(files);
        Assert.True(result); // Image to PDF with same color profile should pass
    }

    [Fact]
    public void TestBothDifferentProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "image-with-profile-AdobeRGB-1.pdf");
        
        var files = new FilePair(oFilePath, "fmt/41", nFilePath, "fmt/477");
        var result = ColorProfileComparison.ImageToPdfColorProfileComparison(files);
        Assert.False(result); // Image to PDF with different color profile should fail
    }
    
    [Fact]
    public void TestOriginalNoProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-without-profile.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "image-with-profile-sRGB-1.pdf");
        
        var files = new FilePair(oFilePath, "fmt/41", nFilePath, "fmt/477");
        var result = ColorProfileComparison.ImageToPdfColorProfileComparison(files);
        Assert.False(result); // Original file without profile should fail
    }
    
    [Fact]
    public void TestNewNoProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "image-without-profile.pdf");
        
        var files = new FilePair(oFilePath, "fmt/41", nFilePath, "fmt/477");
        var result = ColorProfileComparison.ImageToPdfColorProfileComparison(files);
        Assert.False(result); // New file without profile should fail
    }
}

public class PdfToPdfColorProfileComparisonTest : TestBase
{
    [Fact]
    public void TestBothSameProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PDF", "image-with-profile-sRGB-1.pdf");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "image-with-profile-sRGB-2.pdf");
        
        var files = new FilePair(oFilePath, "fmt/477", nFilePath, "fmt/477");
        var result = ColorProfileComparison.PdfToPdfColorProfileComparison(files);
        Assert.True(result); // PDF to PDF with same color profile should pass
    }

    [Fact]
    public void TestBothDifferentProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PDF", "image-with-profile-sRGB-1.pdf");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "image-with-profile-AdobeRGB-1.pdf");
        
        var files = new FilePair(oFilePath, "fmt/477", nFilePath, "fmt/477");
        var result = ColorProfileComparison.PdfToPdfColorProfileComparison(files);
        Assert.False(result); // PDF to PDF with different color profile should fail
    }
    
    [Fact]
    public void TestBothNoProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PDF", "image-without-profile.pdf");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "image-without-profile.pdf");
        
        var files = new FilePair(oFilePath, "fmt/477", nFilePath, "fmt/477");
        var result = ColorProfileComparison.PdfToPdfColorProfileComparison(files);
        Assert.True(result); // Two files without profile should pass
    }
    
    [Fact]
    public void TestOriginalNoProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PDF", "image-without-profile.pdf");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "image-with-profile-sRGB-1.pdf");
        
        var files = new FilePair(oFilePath, "fmt/477", nFilePath, "fmt/477");
        var result = ColorProfileComparison.PdfToPdfColorProfileComparison(files);
        Assert.False(result); // Original file without profile should fail
    }
    
    [Fact]
    public void TestNewNoProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PDF", "image-with-profile-sRGB-1.pdf");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "image-without-profile.pdf");
        
        var files = new FilePair(oFilePath, "fmt/477", nFilePath, "fmt/477");
        var result = ColorProfileComparison.PdfToPdfColorProfileComparison(files);
        Assert.False(result); // New file without profile should fail
    }
}

public class ImageToImageColorProfileComparisonTest : TestBase
{
    [Fact]
    public void TestBothProfilesSame()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-2.jpg");
        
        var files = new FilePair(oFilePath, "fmt/41", nFilePath, "fmt/41");
        var result = ColorProfileComparison.ImageToImageColorProfileComparison(files);
        Assert.True(result); // Two files with same color profile should pass
    }
    
    [Fact]
    public void TestBothProfilesDifferent()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-AdobeRGB-1.jpg");
        
        var files = new FilePair(oFilePath, "fmt/41", nFilePath, "fmt/41");
        var result = ColorProfileComparison.ImageToImageColorProfileComparison(files);
        Assert.False(result); // Two files with different color profile should fail
    }
    
    [Fact]
    public void TestOriginalNoProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-without-profile.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        
        var files = new FilePair(oFilePath, "fmt/41", nFilePath, "fmt/41");
        var result = ColorProfileComparison.ImageToImageColorProfileComparison(files);
        Assert.False(result); // Original file without profile should fail
    }
    
    [Fact]
    public void TestNewNoProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "image-without-profile.jpg");
        
        var files = new FilePair(oFilePath, "fmt/41", nFilePath, "fmt/41");
        var result = ColorProfileComparison.ImageToImageColorProfileComparison(files);
        Assert.False(result); // New file without profile should fail
    }

    [Fact]
    public void TestBothNoProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-without-profile.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "image-without-profile.jpg");

        var files = new FilePair(oFilePath, "fmt/41", nFilePath, "fmt/41");
        var result = ColorProfileComparison.ImageToImageColorProfileComparison(files);
        Assert.True(result); // Two files without profile should pass
    }
}

public class DocxToPdfColorProfileComparisonTest : TestBase
{
    [Fact]
    public void TestImagesOfDifferentProfiles()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "word_with_two_images_of_different_profile.docx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "word_with_two_images_of_different_profile.pdf");

        var files = new FilePair(oFilePath, "fmt/412", nFilePath, "fmt/477");
        var result = ColorProfileComparison.DocxToPdfColorProfileComparison(files);
        Assert.True(result); // Two files with same color profiles should pass
    }
    
    [Fact]
    public void TestImagesOfSameProfiles()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "word_with_two_images_of_same_profile.docx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "word_with_two_images_of_same_profile.pdf");

        var files = new FilePair(oFilePath, "fmt/412", nFilePath, "fmt/477");
        var result = ColorProfileComparison.DocxToPdfColorProfileComparison(files);
        Assert.True(result); // Two files with same color profiles should pass
    }
    
    [Fact]
    public void TestImagesWithOneMissingProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "word_with_two_images_with_one_missing_profile.docx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "word_with_two_images_with_one_missing_profile.pdf");

        var files = new FilePair(oFilePath, "fmt/412", nFilePath, "fmt/477");
        var result = ColorProfileComparison.DocxToPdfColorProfileComparison(files);
        Assert.True(result); // Two files with same color profiles should pass
    }
    
    [Fact]
    public void TestImagesWithMissingProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "word_with_images_with_no_profile.docx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "word_with_images_with_no_profile.pdf");

        var files = new FilePair(oFilePath, "fmt/412", nFilePath, "fmt/477");
        var result = ColorProfileComparison.DocxToPdfColorProfileComparison(files);
        Assert.True(result); // Two files with same color profiles should pass
    }
    
    [Fact]
    public void TestNoImages()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "word_with_no_images.docx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "word_with_no_images.pdf");

        var files = new FilePair(oFilePath, "fmt/412", nFilePath, "fmt/477");
        var result = ColorProfileComparison.DocxToPdfColorProfileComparison(files);
        Assert.True(result); // If there are no images it cant fail the test
    }
}

public class PowerPointToPdfColorProfileComparisonTest : TestBase
{
    [Fact]
    public void TestOneProfileSuccess()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PowerPoint", "presentation_with_one_type_color_profile.pptx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "presentation_with_one_type_color_profile.pdf");

        var files = new FilePair(oFilePath, "fmt/215", nFilePath, "fmt/477");
        var result = ColorProfileComparison.XmlBasedPowerPointToPdfColorProfileComparison(files);
        Assert.True(result); // Two files where color profiles match should pass
    }
    
    [Fact]
    public void TestTwoProfileSuccess()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PowerPoint", "presentation_with_two_type_color_profile.pptx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "presentation_with_two_type_color_profile.pdf");

        var files = new FilePair(oFilePath, "fmt/215", nFilePath, "fmt/477");
        var result = ColorProfileComparison.XmlBasedPowerPointToPdfColorProfileComparison(files);
        Assert.True(result); // Two files where color profiles match should pass
    }
    
    [Fact]
    public void TestOneFileNoProfiles()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PowerPoint", "presentation_with_two_type_color_profile.pptx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "presentation_with_two_type_color_profile_wrong.pdf");

        var files = new FilePair(oFilePath, "fmt/215", nFilePath, "fmt/477");
        var result = ColorProfileComparison.XmlBasedPowerPointToPdfColorProfileComparison(files);
        Assert.False(result); // New file is missing profiles and so test should fail
    }
    
    [Fact]
    public void TestOneProfilePresentOneMissingInEachFile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PowerPoint", "presentation_with_one_type_color_profile_and_one_missing.pptx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "presentation_with_one_type_color_profile_and_one_missing.pdf");

        var files = new FilePair(oFilePath, "fmt/215", nFilePath, "fmt/477");
        var result = ColorProfileComparison.XmlBasedPowerPointToPdfColorProfileComparison(files);
        Assert.True(result); // Two files where color profiles match should pass
    }
    
    [Fact]
    public void TestBothNoProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PowerPoint", "presentation_with_no_color_profile.pptx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "presentation_with_no_color_profile.pdf");

        var files = new FilePair(oFilePath, "fmt/215", nFilePath, "fmt/477");
        var result = ColorProfileComparison.XmlBasedPowerPointToPdfColorProfileComparison(files);
        Assert.True(result); // Two files where neither has profiles should pass
    }

}

public class FileColorProfileComparison : TestBase
{
    [Fact]
    public void TestImageToImage()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-2.jpg");
        
        var files = new FilePair(oFilePath, "fmt/41", nFilePath, "fmt/41");
        var result = ColorProfileComparison.FileColorProfileComparison(files);
        Assert.True(result); // Image to Image with same color profile should pass
    }
    
    [Fact]
    public void TestImageToPdf()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "image-with-profile-sRGB-1.pdf");
        
        var files = new FilePair(oFilePath, "fmt/41", nFilePath, "fmt/477");
        var result = ColorProfileComparison.FileColorProfileComparison(files);
        Assert.True(result); // Image to PDF with same color profile should pass
    }
    
    [Fact]
    public void TestPdfToPdf()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PDF", "image-with-profile-sRGB-1.pdf");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "image-with-profile-sRGB-2.pdf");
        
        var files = new FilePair(oFilePath, "fmt/477", nFilePath, "fmt/477");
        var result = ColorProfileComparison.FileColorProfileComparison(files);
        Assert.True(result); // PDF to PDF with same color profile should pass
    }

    [Fact]
    public void TestPowerPointToPdf()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PowerPoint", "presentation_with_two_type_color_profile.pptx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "presentation_with_two_type_color_profile.pdf");

        var files = new FilePair(oFilePath, "fmt/215", nFilePath, "fmt/477");
        var result = ColorProfileComparison.FileColorProfileComparison(files);
        Assert.True(result); // Two files where color profiles match should pass
    }
    
    [Fact]
    public void TestImageToImageDifferentProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-AdobeRGB-1.jpg");
        
        var files = new FilePair(oFilePath, "fmt/41", nFilePath, "fmt/41");
        var result = ColorProfileComparison.FileColorProfileComparison(files);
        Assert.False(result); // Image to Image with different color profile should fail
    }

    [Fact]
    public void TestImageToPdfDifferentProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "image-with-profile-AdobeRGB-1.pdf");

        var files = new FilePair(oFilePath, "fmt/41", nFilePath, "fmt/477");
        var result = ColorProfileComparison.FileColorProfileComparison(files);
        Assert.False(result); // Image to PDF with different color profile should fail
    }

    [Fact]
    public void TestDocxToPdf()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "word_with_two_images_of_different_profile.docx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "word_with_two_images_of_different_profile.pdf");

        var files = new FilePair(oFilePath, "fmt/412", nFilePath, "fmt/477");
        var result = ColorProfileComparison.FileColorProfileComparison(files);
        Assert.True(result); // Two files where color profiles match should pass
    }
}