using System.Drawing;
using AvaloniaDraft.ComparingMethods;

namespace UnitTests.ComparingMethodsTest;

[TestFixture]
public class DocumentSegmentationTest
{
    private string _testFileDirectory;
    
    [SetUp]
    public void Setup()
    {
        var curDir = Directory.GetCurrentDirectory();

        while (!string.IsNullOrEmpty(curDir))
        {
            if (Path.GetFileName(curDir) == "conv-file-quality-assurance")
            {
                _testFileDirectory = curDir + @"\UnitTests\ComparingMethodsTest\TestFiles\";
                return;
            }
            
            curDir = Directory.GetParent(curDir)?.FullName;
        }
        
        throw new Exception("Failed to find project directory \"conv-file-quality-assurance\"");
    }

    [Test]
    public void DocumentSegmentationTests()
    {
        var path1 = _testFileDirectory + @"Images\Documents\seg_test_2.png";
        var path2 = _testFileDirectory + @"Images\Documents\seg_test_6.png";
        var path3 = _testFileDirectory + @"Images\Documents\seg_test_8.png";
        var path4 = _testFileDirectory + @"Images\Documents\seg_test_17.png";
        
        var file2 = File.ReadAllBytes(path2);
        var file3 = File.ReadAllBytes(path3);

        var rects1 = DocumentSegmentation.SegmentDocumentImage(path1);
        var rects2 = DocumentSegmentation.SegmentDocumentImage(file2);
        var rects3 = DocumentSegmentation.SegmentDocumentImage(file3);
        var rects4 = DocumentSegmentation.SegmentDocumentImage(path4);
        var rects5 = DocumentSegmentation.SegmentDocumentImage("Not real");
        var rects6 = DocumentSegmentation.SegmentDocumentImage([]);

        if(rects5 is not null || rects6 is not null) Assert.Fail();
        
        if(rects1 is null || rects2 is null || rects3 is null || rects4 is null) Assert.Fail();
        
        if(rects1!.Count != 2 || rects2!.Count != 6 || rects3!.Count != 8 || rects4!.Count != 17) Assert.Fail();
        
        var segments1 = DocumentSegmentation.GetSegmentPictures(path1, rects1);
        var segments2 = DocumentSegmentation.GetSegmentPictures(file2, rects2);
        var segments3 = DocumentSegmentation.GetSegmentPictures("", rects3);
        var segments4 = DocumentSegmentation.GetSegmentPictures(path4, []);
        
        if(segments3 is not null || segments4 is not null) Assert.Fail();
        
        if(segments1 is null || segments2 is null) Assert.Fail();
        
        if(segments1!.Count != rects1!.Count || segments2!.Count != rects2.Count) Assert.Fail();
        
        Assert.Pass();
    }
}