using System.Diagnostics;
using System.Drawing;
using System.IO.Enumeration;
using AvaloniaDraft.ComparingMethods;
using DocumentFormat.OpenXml.Office2013.PowerPoint;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace UnitTests.ComparingMethodsTest;

[TestFixture]
public class DocumentVisualOperationsTest
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

        var rects1 = DocumentVisualOperations.SegmentDocumentImage(path1);
        var rects2 = DocumentVisualOperations.SegmentDocumentImage(file2);
        var rects3 = DocumentVisualOperations.SegmentDocumentImage(file3);
        var rects4 = DocumentVisualOperations.SegmentDocumentImage(path4);
        var rects5 = DocumentVisualOperations.SegmentDocumentImage("Not real");
        var rects6 = DocumentVisualOperations.SegmentDocumentImage(path4);

        if(rects5 is not null || rects6 is not null) Assert.Fail();
        
        if(rects1 is null || rects2 is null || rects3 is null || rects4 is null) Assert.Fail();
        
        if(rects1!.Count != 2 || rects2!.Count != 6 || rects3!.Count != 8 || rects4!.Count != 17) Assert.Fail();
        
        var segments1 = DocumentVisualOperations.GetSegmentPictures(path1, rects1);
        var segments2 = DocumentVisualOperations.GetSegmentPictures(file2, rects2);
        var segments3 = DocumentVisualOperations.GetSegmentPictures("", rects3);
        var segments4 = DocumentVisualOperations.GetSegmentPictures(path4, []);
        
        if(segments3 is not null || segments4 is not null) Assert.Fail();
        
        if(segments1 is null || segments2 is null) Assert.Fail();
        
        if(segments1!.Count != rects1!.Count || segments2!.Count != rects2.Count) Assert.Fail();
        
        Assert.Pass();
    }

    [Test]
    public void DistanceCalculationTests()
    {
        var rects1 = new List<Rectangle>
        {
            new Rectangle(0, 0, 1, 2),
            new Rectangle(2, 1, 2, 2),
            new Rectangle(0, 3, 1, 3),
            new Rectangle(15, 15, 2, 2),
        };

        var rects2 = new List<Rectangle>
        {
            new Rectangle(0, 0, 1, 2),
            new Rectangle(2, 0, 2, 2),
            new Rectangle(0, 4, 1, 3),
            new Rectangle(5, 5, 1, 3),
        };
        
        var (pairs, pairless1, pairless2) = DocumentVisualOperations.PairAndGetOverlapSegments(rects1, rects2);

        Assert.Multiple(() =>
        {
            Assert.That(pairs[0].Item3, Is.EqualTo(1.0));
            Assert.That(pairs[1].Item3, Is.EqualTo(0.3333).Within(0.001));
            Assert.That(pairs[2].Item3, Is.EqualTo(0.5));
            Assert.That(pairless1.Count, Is.EqualTo(1));
            Assert.That(pairless2.Count, Is.EqualTo(1));
        });

        Assert.Pass();
    }
}