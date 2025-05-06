using System.IO.Abstractions.TestingHelpers;
using AvaloniaDraft.ProgramManager;

namespace UnitTests.ProgramManagerTest;

[TestFixture]
public class SingleFileManagerTest
{
    [Test]
    public void SingleFileManagerCreationTest()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"C:\testOriginal\test1.txt", new MockFileData("Test data") },
                { @"C:\testOriginal\test2.txt", new MockFileData("Test data") },
                { @"C:\testOriginal\test3.txt", new MockFileData("Test data") },
                { @"C:\testOriginal\test3.docx", new MockFileData("Test data") },
                { @"C:\testOriginal\pairless1.txt", new MockFileData("Test data") },
            }
        );
        
        var sfm = new SingleFileManager(@"C:\testOriginal\", fileSystem);
        
        if(sfm is null) Assert.Fail();
        
        Assert.Pass();
    }
}