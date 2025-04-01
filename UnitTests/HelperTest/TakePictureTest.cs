using System;
using System.IO;
using NUnit.Framework;
using AvaloniaDraft.Helpers;  // Replace with the namespace where TakePicturePdf is located

namespace UnitTests.PdfToImageConversion
{
    [TestFixture]
    public class PdfToImageConversionTests
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
                    _testFileDirectory = curDir + @"\UnitTests\ComparingMethodsTest\TestFiles\";
                    return;
                }
            
                curDir = Directory.GetParent(curDir)?.FullName;
            }
        
            throw new Exception("Failed to find project directory \"conv-file-quality-assurance\"");
        }

        // [Test]
        // public void ConvertPdfToImages_ValidFilePath_ReturnsImages()
        // {
        //     // Arrange: Specify the paths for PDF and the output directory
        //     var pdfFilePath = Path.Combine("/home/snorre/bachelor/test", "sample.pdf"); // Replace with your actual PDF file path
        //     var outputDir = Path.Combine("/home/snorre/bachelor/tmp/", "");
        //
        //     // Act: Call the ConvertPdfToImages method to convert the PDF to images
        //     TakePicturePdf.ConvertPdfToImagesToDisk(pdfFilePath, outputDir);
        //
        //     // Assert: Ensure images were saved in the output directory
        //     var outputFiles = Directory.GetFiles(outputDir, "*.png");
        //     Assert.That(outputFiles.Length, Is.GreaterThan(0), "No images were generated from the PDF.");
        // }

        [Test]
        public void ConvertPdfToImagesToBytesTest()
        {
            var onePage = _testFileDirectory + @"PDF\correct_transparency.pdf";
            var twoPage = _testFileDirectory + @"PDF\presentation_with_one_type_color_profile.pdf";
            var threePage = _testFileDirectory + @"PDF\odp-with-one-missing-color-profile.pdf";

            var res1 = TakePicturePdf.ConvertPdfToImagesToBytes(onePage);
            var res2 = TakePicturePdf.ConvertPdfToImagesToBytes(twoPage);
            var res3 = TakePicturePdf.ConvertPdfToImagesToBytes(threePage);
            var res4 = TakePicturePdf.ConvertPdfToImagesToBytes("fakepath");
            
            if(res1 == null || res2 == null || res3 == null || res4 != null) Assert.Fail();
            
            if(res1.Count != 1 || res2.Count != 2 || res3.Count != 3) Assert.Fail();

            Assert.Pass();
        }
    }
}
