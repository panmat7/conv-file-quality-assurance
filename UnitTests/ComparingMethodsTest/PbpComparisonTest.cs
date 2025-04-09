using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;

namespace UnitTests.ComparingMethodsTest;

[TestFixture]
public class PbpComparisonTests
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
                _testFileDirectory = curDir + "/UnitTests/ComparingMethodsTest/TestFiles";
                return;
            }
            
            curDir = Directory.GetParent(curDir)?.FullName;
        }
        
        throw new Exception("Failed to find project directory \"conv-file-quality-assurance\"");
    }

    [Test]
    public void CalculateImageSimilarity_ValidFilePair_ReturnsSimilarity100()
    {
        // Arrange: Specify file paths for images to compare
        var originalFilePath = Path.Combine(_testFileDirectory, "Images/225x225.png");
        var newFilePath = Path.Combine(_testFileDirectory, "Images/225x225.png");
        
        var files = new FilePair(originalFilePath, newFilePath);

        // Act: Call the CalculateImageSimilarity method to compare images
        var result = PbpComparison.CalculateImageSimilarity(files);
        Console.WriteLine(result);
        // Assert: Check if result is 100
        Assert.That(result, Is.InRange(100, 100));
    }
    
    [Test]
    public void CalculateImageSimilarity_ValidFilePair_ReturnsSimilarity_WithResizing()
    {
        // Arrange: Specify file paths for images to compare
        var originalFilePath = Path.Combine(_testFileDirectory, "Images/225x225.png");
        var newFilePath = Path.Combine(_testFileDirectory, "Images/450x450.png");
        
        var files = new FilePair(originalFilePath, newFilePath);

        // Act: Call the CalculateImageSimilarity method to compare images
        var result = PbpComparison.CalculateImageSimilarity(files);
        Console.WriteLine(result);
        // Assert: Check that the result is high percentage
        Assert.That(result, Is.InRange(50, 100));
    }
    
    [Test]
    public void CalculateImageSimilarity_ValidFilePair_ReturnsSimilarity0()
    {
        // Arrange: Specify file paths for images to compare
        var originalFilePath = Path.Combine(_testFileDirectory, "Images/black.png");
        var newFilePath = Path.Combine(_testFileDirectory, "Images/white.png");
        
        var files = new FilePair(originalFilePath, newFilePath);

        // Act: Call the CalculateImageSimilarity method to compare images
        var result = PbpComparison.CalculateImageSimilarity(files);
        Console.WriteLine(result);
        // Assert: Check that the result is 0
        Assert.That(result, Is.InRange(0, 0));
    }

    [Test]
    public void CalculateImageSimilarity_InvalidFilePath_ReturnsError()
    {
        // Arrange: Invalid file paths
        var files = new FilePair("Images/invalid_path1.png", "Images/invalid_path2.png");

        // Act: Call the method with invalid paths
        var result = PbpComparison.CalculateImageSimilarity(files);

        // Assert: Ensure the result is -1 for invalid paths
        Assert.That(result, Is.EqualTo(-1));
    }
}
