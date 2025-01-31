using System.IO.Abstractions.TestingHelpers;
using AvaloniaDraft.FileManager;

namespace UnitTests;

public class FileManagerTest
{
    [SetUp]
    public void Setup()
    {
        
    }

    [Test]
    public void FileManager_NormalCase_Success()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"C:\testOriginal\test1.txt", new MockFileData("Test data") },
                { @"C:\testOriginal\test2.txt", new MockFileData("Test data") },
                { @"C:\testOriginal\pairless1.txt", new MockFileData("Test data") },
                
                { @"C:\testNew\test1.pdf", new MockFileData("Test data") },
                { @"C:\testNew\test2.pdf", new MockFileData("Test data") },
                { @"C:\testNew\pairless2.txt", new MockFileData("Test data") },
            }
        );

        var f = new FileManager(@"C:\testOriginal\", @"C:\testNew\", fileSystem);
        var pairs = f.GetFilePairs();
        var pairless = f.GetPairlessFiles();

        var checkPairs = new List<FilePair>
        {
            new(@"C:\testOriginal\test1.txt", @"C:\testNew\test1.pdf"),
            new(@"C:\testOriginal\test2.txt", @"C:\testNew\test2.pdf"),
        };
        var checkPairless = new List<string>
        {
            @"C:\testOriginal\pairless1.txt",
            @"C:\testNew\pairless2.txt"
        };
        
        CollectionAssert.AreEquivalent(checkPairs, pairs);
        CollectionAssert.AreEquivalent(checkPairless, pairless);
        
        Assert.Pass();
    }
}