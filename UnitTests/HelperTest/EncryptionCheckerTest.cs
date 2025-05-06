using UnitTests.ComparingMethodsTest;
using AvaloniaDraft.Helpers;
using AvaloniaDraft.ProgramManager;

namespace UnitTests.HelperTest;

[TestFixture]
public class IsCompressedEncryptedTest : TestBase
{
    [Test]
    public void TestEncryptedZipFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "encrypted.zip");
        var result = EncryptionChecker.IsCompressedEncrypted(filePath);
        Assert.That(result, Is.True);
    }

    [Test]
    public void TestNonEncryptedZipFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "nonencrypted.zip");
        var result = EncryptionChecker.IsCompressedEncrypted(filePath);
        Assert.That(result, Is.False);
    }
    
    [Test]
    public void TestEncryptedRarFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "encrypted.rar");
        var result = EncryptionChecker.IsCompressedEncrypted(filePath);
        Assert.That(result, Is.True);
    }

    [Test]
    public void TestNonEncryptedRarFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "nonencrypted.rar");
        var result = EncryptionChecker.IsCompressedEncrypted(filePath);
        Assert.That(result, Is.False);
    }
}

[TestFixture]
public class CheckFileEncryptionOrCorruptionTest : TestBase
{
    [Test]
    public void TestEncryptedPdfFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "encrypted.pdf");
        var result = EncryptionChecker.CheckForEncryption(filePath);
        Assert.That(result, Is.EqualTo(ReasonForIgnoring.Encrypted));
    }

    [Test]
    public void TestNonEncryptedPdfFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "nonencrypted.pdf");
        var result = EncryptionChecker.CheckForEncryption(filePath);
        Assert.That(result, Is.EqualTo(ReasonForIgnoring.None));
    }
    
    [Test]
    public void TestEncryptedOdtFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "encrypted.odt");
        var result = EncryptionChecker.CheckForEncryption(filePath);
        Assert.That(result, Is.EqualTo(ReasonForIgnoring.Encrypted));
    }

    [Test]
    public void TestNonEncryptedOdtFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "nonencrypted.odt");
        var result = EncryptionChecker.CheckForEncryption(filePath);
        Assert.That(result, Is.EqualTo(ReasonForIgnoring.None));
    }
    
    [Test]
    public void TestEncryptedDocxFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "encrypted.docx");
        var result = EncryptionChecker.CheckForEncryption(filePath);
        Assert.That(result, Is.EqualTo(ReasonForIgnoring.Encrypted));
    }

    [Test]
    public void TestNonEncryptedDocxFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "nonencrypted.docx");
        var result = EncryptionChecker.CheckForEncryption(filePath);
        Assert.That(result, Is.EqualTo(ReasonForIgnoring.None));
    }
    
    [Test]
    public void TestEncryptedPptxFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "encrypted.pptx");
        var result = EncryptionChecker.CheckForEncryption(filePath);
        Assert.That(result, Is.EqualTo(ReasonForIgnoring.Encrypted));
    }
    
    [Test]
    public void TestEncryptedOdpFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "encrypted.odp");
        var result = EncryptionChecker.CheckForEncryption(filePath);
        Assert.That(result, Is.EqualTo(ReasonForIgnoring.Encrypted));
    }
    
    [Test]
    public void TestCorruptedOdtFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "corrupted.odt");
        var result = EncryptionChecker.CheckForEncryption(filePath);
        Assert.That(result, Is.EqualTo(ReasonForIgnoring.None));
    }
    
    [Test]
    public void TestCorruptedPdfFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "corrupted.pdf");
        var result = EncryptionChecker.CheckForEncryption(filePath);
        Assert.That(result, Is.EqualTo(ReasonForIgnoring.None));
    }
    
    [Test]
    public void TestCorruptedDocxFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "corrupted.docx");
        var result = EncryptionChecker.CheckForEncryption(filePath);
        Assert.That(result, Is.EqualTo(ReasonForIgnoring.None));
    }

    [Test]
    public void TestUnsupportedFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "225x225.png");
        var result = EncryptionChecker.CheckForEncryption(filePath);
        Assert.That(result, Is.EqualTo(ReasonForIgnoring.None));
    }
}