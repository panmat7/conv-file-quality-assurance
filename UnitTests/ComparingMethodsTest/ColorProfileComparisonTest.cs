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
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.pdf");
        
        var files = new FilePair(oFilePath, "fmt-44", nFilePath, "fmt/477");
        var result = ColorProfileComparison.ImageToPdfColorProfileComparison(files);
        Assert.True(result); // Image to PDF with same color profile should pass
    }

    [Fact]
    public void TestBothDifferentProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-AdobeRGB-1.pdf");
        
        var files = new FilePair(oFilePath, "fmt-44", nFilePath, "fmt/477");
        var result = ColorProfileComparison.ImageToPdfColorProfileComparison(files);
        Assert.False(result); // Image to PDF with different color profile should fail
    }
    
    [Fact]
    public void TestOriginalNoProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-without-profile.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.pdf");
        
        var files = new FilePair(oFilePath, "fmt-44", nFilePath, "fmt/477");
        var result = ColorProfileComparison.ImageToPdfColorProfileComparison(files);
        Assert.False(result); // Original file without profile should fail
    }
    
    [Fact]
    public void TestNewNoProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "image-without-profile.pdf");
        
        var files = new FilePair(oFilePath, "fmt-44", nFilePath, "fmt/477");
        var result = ColorProfileComparison.ImageToPdfColorProfileComparison(files);
        Assert.False(result); // New file without profile should fail
    }
}

public class PdfToPdfColorProfileComparisonTest : TestBase
{
    [Fact]
    public void TestBothSameProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "pdf-with-profile-sRGB-1.pdf");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "pdf-with-profile-sRGB-2.pdf");
        
        var files = new FilePair(oFilePath, "fmt/477", nFilePath, "fmt/477");
        var result = ColorProfileComparison.PdfToPdfColorProfileComparison(files);
        Assert.True(result); // PDF to PDF with same color profile should pass
    }

    [Fact]
    public void TestBothDifferentProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "pdf-with-profile-sRGB-1.pdf");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "pdf-with-profile-AdobeRGB-1.pdf");
        
        var files = new FilePair(oFilePath, "fmt/477", nFilePath, "fmt/477");
        var result = ColorProfileComparison.PdfToPdfColorProfileComparison(files);
        Assert.False(result); // PDF to PDF with different color profile should fail
    }
    
    [Fact]
    public void TestOriginalNoProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "pdf-without-profile.pdf");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "pdf-with-profile-sRGB-1.pdf");
        
        var files = new FilePair(oFilePath, "fmt/477", nFilePath, "fmt/477");
        var result = ColorProfileComparison.PdfToPdfColorProfileComparison(files);
        Assert.False(result); // Original file without profile should fail
    }
    
    [Fact]
    public void TestNewNoProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "pdf-with-profile-sRGB-1.pdf");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "pdf-without-profile.pdf");
        
        var files = new FilePair(oFilePath, "fmt/477", nFilePath, "fmt/477");
        var result = ColorProfileComparison.PdfToPdfColorProfileComparison(files);
        Assert.False(result); // New file without profile should fail
    }
}