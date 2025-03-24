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
            var curDir = "/home/snorre/bachelor/";

        }

        [Test]
        public void ConvertPdfToImages_ValidFilePath_ReturnsImages()
        {
            // Arrange: Specify the paths for PDF and the output directory
            var pdfFilePath = Path.Combine("/home/snorre/bachelor/test", "sample.pdf"); // Replace with your actual PDF file path
            var outputDir = Path.Combine("/home/snorre/bachelor/tmp/", "");

            // Act: Call the ConvertPdfToImages method to convert the PDF to images
            TakePicturePdf.ConvertPdfToImages(pdfFilePath, outputDir);

            // Assert: Ensure images were saved in the output directory
            var outputFiles = Directory.GetFiles(outputDir, "*.png");
            Assert.That(outputFiles.Length, Is.GreaterThan(0), "No images were generated from the PDF.");
        }

        

        

        
    }
}
