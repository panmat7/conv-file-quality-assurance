using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;


namespace UnitTests.ComparingMethodsTest
{
    [TestFixture]
    public class PbpMagickTest
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
                    _testFileDirectory = Path.Combine(curDir, "UnitTests", "ComparingMethodsTest", "TestFiles", "Images");
                    return;
                }

                curDir = Directory.GetParent(curDir)?.FullName;
            }

            throw new Exception("Failed to find project directory \"conv-file-quality-assurance\"");
        }

        private static readonly string[] FileNames = new[]
        {
            "BMP.bmp",
            "JPEG.jpeg",
            "PNG.png",
            "GIF.gif",
            "TIFF.tiff"
        };

        // This will provide the test cases for different image pairs
        public static IEnumerable<object[]> ImagePairs
        {
            get
            {
                var curDir = Directory.GetCurrentDirectory();
                string basePath = "";

                while (!string.IsNullOrEmpty(curDir))
                {
                    if (Path.GetFileName(curDir) == "conv-file-quality-assurance")
                    {
                        basePath = Path.Combine(curDir, "UnitTests", "ComparingMethodsTest", "TestFiles", "Images", "Pbp");
                        break;
                    }

                    curDir = Directory.GetParent(curDir)?.FullName;
                }

                if (basePath == null)
                    throw new Exception("Failed to locate base image path.");

                foreach (var file1 in FileNames)
                {
                    foreach (var file2 in FileNames)
                    {
                        var path1 = Path.Combine(basePath, file1);
                        var path2 = Path.Combine(basePath, file2);

                        yield return new object[] { path1, path2 };
                    }
                }
            }
        }

        // Test to verify that images are compared correctly
        [Test, TestCaseSource(nameof(ImagePairs))]
        public void CalculateImageSimilarity_ValidImageFormats_ShouldNotReturnMinusOne(string path1, string path2)
        {
            var files = new FilePair(path1, path2);
            double similarity;

            try
            {
                similarity = PbpComparisonMagick.CalculateImageSimilarity(files);
                Console.WriteLine(similarity);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Exception during image comparison: {ex.Message}");
                return;
            }

            Assert.That(similarity, Is.Not.EqualTo(-1), 
                $"Comparison failed for {path1} vs {path2}");
        }

        // Identical images comparison (expecting similarity close to 0)
        [Test]
        public void CalculateImageSimilarity_IdenticalImages_ReturnsZeroDistance()
        {
            var filePath = Path.Combine(_testFileDirectory, "225x225.png");
            var files = new FilePair(filePath, filePath);

            var similarity = PbpComparisonMagick.CalculateImageSimilarity(files);

            Assert.That(similarity, Is.EqualTo(0).Within(0.01), "Similarity between identical images should be 0.");
        }

        // Different images comparison (expecting similarity > 0)
        [Test]
        public void CalculateImageSimilarity_DifferentImages_ReturnsNonZeroDistance()
        {
            var original = Path.Combine(_testFileDirectory, "black.png");
            var modified = Path.Combine(_testFileDirectory, "white.png");
            var files = new FilePair(original, modified);

            var similarity = PbpComparisonMagick.CalculateImageSimilarity(files);

            Assert.That(similarity, Is.GreaterThan(0), "Different images should have a non-zero distance.");
        }

        // Resized image comparison (expecting some distance)
        [Test]
        public void CalculateImageSimilarity_ResizedImage_ReturnsDistance()
        {
            var original = Path.Combine(_testFileDirectory, "225x225.png");
            var resized = Path.Combine(_testFileDirectory, "450x450.png");
            var files = new FilePair(original, resized);

            var similarity = PbpComparisonMagick.CalculateImageSimilarity(files);

            Assert.That(similarity, Is.GreaterThanOrEqualTo(0), "Function should return a value even for resized images.");
        }

        // Invalid file paths (should return -1)
        [Test]
        public void CalculateImageSimilarity_InvalidFilePaths_ReturnsMinusOne()
        {
            var files = new FilePair("nonexistent1.png", "nonexistent2.png");

            var similarity = PbpComparisonMagick.CalculateImageSimilarity(files);

            Assert.That(similarity, Is.EqualTo(-1), "Invalid file paths should return -1.");
        }
        
        [Test]
        public void CalculateImageSimilarityByte_Returns0()
        {
            var original = Path.Combine(_testFileDirectory, "225x225.png");
            var resized = Path.Combine(_testFileDirectory, "225x225.png");
            var files = new FilePair(original, resized);
            
            var originalFile = File.ReadAllBytes(files.OriginalFilePath);
            var newFile = File.ReadAllBytes(files.NewFilePath);

            var similarity = PbpComparisonMagick.CalculateImageSimilarity(originalFile, newFile);

            Assert.That(similarity, Is.EqualTo(0), "Should return 0");
        }
    }
}
