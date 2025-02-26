using AvaloniaDraft.ComparingMethods;

namespace UnitTests.ComparingMethodsTest;

[TestFixture]
public class SpreadsheetComparisonTest
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
    
    
    [Test]
    public void PossibleBreakExcelCellTest_False()
    {
        var res = SpreadsheetComparison.PossibleSpreadsheetBreakExcel(_testFileDirectory + @"Spreadsheet\excel_bellow_break.xlsx");
        
        Assert.That(res, Is.False);
    }

    [Test]
    public void PossibleBreakExcelCellTest_True()
    {
        var res = SpreadsheetComparison.PossibleSpreadsheetBreakExcel(_testFileDirectory + @"Spreadsheet\excel_above_break.xlsx");
        
        Assert.That(res, Is.True);
    }

    [Test]
    public void PossibleBreakExcelCellTest_Image()
    {
        var res = SpreadsheetComparison.PossibleSpreadsheetBreakExcel(_testFileDirectory + @"Spreadsheet\excel_with_one_missing_profile_over_cells.xlsx");
        
        Assert.That(res, Is.True);
    }

    [Test]
    public void PossibleSpreadsheetBreakOpenDocTest_False()
    {
        var res = SpreadsheetComparison.PossibleSpreadsheetBreakOpenDoc(_testFileDirectory + @"Spreadsheet\opendoc_bellow_break.ods");
        
        if(res is null) Assert.Fail();
        
        Assert.That(res, Is.False);
    }

    [Test]
    public void PossibleSpreadsheetBreakOpenDocTest_True()
    {
        var res = SpreadsheetComparison.PossibleSpreadsheetBreakOpenDoc(_testFileDirectory + @"Spreadsheet\opendoc_above_break.ods");
        
        if(res is null) Assert.Fail();
        
        Assert.That(res, Is.True);
    }

    [Test]
    public void PossibleSpreadsheetBreakOpenDocTest_Image()
    {
        var res = SpreadsheetComparison.PossibleSpreadsheetBreakOpenDoc(_testFileDirectory +
                                                                        @"Spreadsheet\opendoc_with_image.ods");
        
        if(res is null) Assert.Fail();
        
        Assert.That(res, Is.True);
    }

    [Test]
    public void PossibleSpreadsheetBreakCSV_False()
    {
        var res = SpreadsheetComparison.PossibleLineBreakCsv(_testFileDirectory + @"Spreadsheet\csv_bellow_break.csv");
        
        if(res is null) Assert.Fail();
        
        Assert.That(res, Is.False);
    }
    
    [Test]
    public void PossibleSpreadsheetBreakCSV_True()
    {
        var res = SpreadsheetComparison.PossibleLineBreakCsv(_testFileDirectory + @"Spreadsheet\csv_above_break.csv");
        
        if(res is null) Assert.Fail();
        
        Assert.That(res, Is.True);
    }
    
    [Test]
    public void PossibleSpreadsheetBreakCSV_False_DifferentDelimiter()
    {
        var res = SpreadsheetComparison.PossibleLineBreakCsv(_testFileDirectory + @"Spreadsheet\csv_bellow_break_different_delimiter.csv");
        
        if(res is null) Assert.Fail();
        
        Assert.That(res, Is.False);
    }
}