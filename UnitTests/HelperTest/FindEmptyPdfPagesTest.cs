using AvaloniaDraft.Helpers;

namespace UnitTests.HelperTest;

using NUnit.Framework;
using System.IO;

[TestFixture]
public class FindEmptyPdfPagesTest
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
                _testFileDirectory = Path.Combine(curDir, "UnitTests", "ComparingMethodsTest", "TestFiles", "EmptyPageTest");
                return;
            }
            
            curDir = Directory.GetParent(curDir)?.FullName;
        }
        
        throw new Exception("Failed to find project directory \"conv-file-quality-assurance\"");
    }
    
    

    [Test]
    public void CountTrailingEmptyPages_ReturnsCorrectCount()
    {
        string testPdfPath = Path.Combine(_testFileDirectory, "TestEmptyPage.pdf");
        Assert.IsTrue(File.Exists(testPdfPath), "Test PDF file not found.");

        int result = FindEmptyPagesPdf.EmptyPagePdf(testPdfPath);

        Assert.That(result, Is.EqualTo(2)); 
    }
    
    [Test]
    public void CountEmptyPages_ReturnsCorrectCount()
    {
        string testPdfPath = Path.Combine(_testFileDirectory, "Corrupted.pdf");
        Assert.IsTrue(File.Exists(testPdfPath), "Test PDF file not found.");

        int result = FindEmptyPagesPdf.EmptyPagePdf(testPdfPath);

        Assert.That(result, Is.EqualTo(6084)); 
    }

}
