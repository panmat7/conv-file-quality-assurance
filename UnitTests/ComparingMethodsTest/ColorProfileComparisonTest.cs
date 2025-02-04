using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;
using Xunit;
using Assert = Xunit.Assert;

namespace UnitTests.ComparingMethodsTest;

public class CompareColorProfilesTest : TestBase
{
    [Fact]
    public void TestBothProfilesSame()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-2.jpg");
        
        var result = ColorProfileComparison.CompareColorProfiles(oFilePath, nFilePath);
        Assert.True(result); // Two files with same color profile should pass
    }
    
    [Fact]
    public void TestBothProfilesDifferent()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-AdobeRGB-1.jpg");
        
        var result = ColorProfileComparison.CompareColorProfiles(oFilePath, nFilePath);
        Assert.False(result); // Two files with different color profile should fail
    }
    
    [Fact]
    public void TestOriginalNoProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-without-profile.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        
        var result = ColorProfileComparison.CompareColorProfiles(oFilePath, nFilePath);
        Assert.False(result); // Original file without profile should fail
    }
    
    [Fact]
    public void TestNewNoProfile()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "image-with-profile-sRGB-1.jpg");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "image-without-profile.jpg");
        
        var result = ColorProfileComparison.CompareColorProfiles(oFilePath, nFilePath);
        Assert.False(result); // New file without profile should fail
    }
}