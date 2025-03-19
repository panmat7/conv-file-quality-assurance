using System.IO.Abstractions.TestingHelpers;
using AvaloniaDraft.FileManager;
using UnitTests.ComparingMethodsTest;

namespace UnitTests.FileManagerTest;

[TestFixture]
public class FileManagerTest : TestBase
{
    [Test]
    public void FileManagerCreationTest()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"C:\testOriginal\test1.txt", new MockFileData("Test data") },
                { @"C:\testOriginal\test2.txt", new MockFileData("Test data") },
                { @"C:\testOriginal\test3.txt", new MockFileData("Test data") },
                { @"C:\testOriginal\test3.docx", new MockFileData("Test data") },
                { @"C:\testOriginal\pairless1.txt", new MockFileData("Test data") },
                
                { @"C:\testNew\test1.pdf", new MockFileData("Test data") },
                { @"C:\testNew\test2.pdf", new MockFileData("Test data") },
                { @"C:\testNew\test3_TXT.pdf", new MockFileData("Test data") },
                { @"C:\testNew\test3_DOCX.pdf", new MockFileData("Test data") },
                { @"C:\testNew\test3_ODT.pdf", new MockFileData("Test data") },
                { @"C:\testNew\pairless2.txt", new MockFileData("Test data") },
            }
        );

        var f = new FileManager(@"C:\testOriginal\", @"C:\testNew\", fileSystem);
        
        if(f == null) Assert.Fail();
        
        var pairs = f.GetFilePairs();
        var pairless = f.GetPairlessFiles();

        var checkPairs = new List<FilePair>
        {
            new(@"C:\testOriginal\test1.txt", @"C:\testNew\test1.pdf"),
            new(@"C:\testOriginal\test2.txt", @"C:\testNew\test2.pdf"),
            new(@"C:\testOriginal\test3.txt", @"C:\testNew\test3_TXT.pdf"),
            new(@"C:\testOriginal\test3.docx", @"C:\testNew\test3_DOCX.pdf"),
            
        };
        var checkPairless = new List<string>
        {
            @"C:\testOriginal\pairless1.txt",
            @"C:\testNew\pairless2.txt",
            @"C:\testNew\test3_ODT.pdf"
        };
        
        Assert.Multiple(() =>
        {
            CollectionAssert.AreEquivalent(checkPairs, pairs);
            CollectionAssert.AreEquivalent(checkPairless, pairless);
        });
        
        Assert.Pass();
    }
}