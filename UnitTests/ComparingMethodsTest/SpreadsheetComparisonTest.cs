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
        
        Assert.That(res, Is.Empty);
    }

    [Test]
    public void PossibleBreakExcelCellTest_True()
    {
        var res = SpreadsheetComparison.PossibleSpreadsheetBreakExcel(_testFileDirectory + @"Spreadsheet\excel_above_break.xlsx");
        
        if(res.Count == 1 && res[0].Name == "Table break") Assert.Pass();
        
        Assert.Fail();
    }
    
    [Test]
    public void PossibleBreakExcelCellTest_Manual_Break()
    {
        var res = SpreadsheetComparison.PossibleSpreadsheetBreakExcel(_testFileDirectory + @"Spreadsheet\excel_manual_break.xlsx");
        
        if(res.Count == 1 && res[0].Name == "Manual page break found") Assert.Pass();
        
        Assert.Fail();
    }

    [Test]
    public void PossibleBreakExcelCellTest_Image()
    {
        var res = SpreadsheetComparison.PossibleSpreadsheetBreakExcel(_testFileDirectory + @"Spreadsheet\excel_with_one_missing_profile_over_cells.xlsx");
        
        if(res.Count == 1 && res[0].Name == "Images found in spreadsheet") Assert.Pass();
        
        Assert.Fail();
    }

    [Test]
    public void PossibleBreakExcelCellTest_Multi_True()
    {
        var res = SpreadsheetComparison.PossibleSpreadsheetBreakExcel(_testFileDirectory +
                                                                      @"Spreadsheet\excel_multi_break.xlsx");
        
        if(res.Count == 1 && res[0].Name == "Table break") Assert.Pass();
        
        Assert.Fail();
    }

    [Test]
    public void PossibleBreakExcelCellTest_Multi_False()
    {
        var res = SpreadsheetComparison.PossibleSpreadsheetBreakExcel(_testFileDirectory +
                                                                      @"Spreadsheet\excel_multi_no_break.xlsx");
        
        Assert.That(res, Is.Empty);
    }

    [Test]
    public void PossibleSpreadsheetBreakOpenDocTest_False()
    {
        var res = SpreadsheetComparison.PossibleSpreadsheetBreakOpenDoc(_testFileDirectory + @"Spreadsheet\opendoc_bellow_break.ods");
        
        if(res is null) Assert.Fail();
        
        Assert.That(res, Is.Empty);
    }

    [Test]
    public void PossibleSpreadsheetBreakOpenDocTest_True()
    {
        var res = SpreadsheetComparison.PossibleSpreadsheetBreakOpenDoc(_testFileDirectory + @"Spreadsheet\opendoc_above_break.ods");
        
        if(res is null) Assert.Fail();
        
        if(res.Count == 1 && res[0].Name == "Table break") Assert.Pass();
        
        Assert.Fail();
    }
    
    [Test]
    public void PossibleSpreadsheetBreakOpenDocTest_Manual_Break()
    {
        var res = SpreadsheetComparison.PossibleSpreadsheetBreakOpenDoc(_testFileDirectory + @"Spreadsheet\opendoc_manual_break.ods");
        
        if(res is null) Assert.Fail();
        
        if(res.Count == 1 && res[0].Name == "Manual page break found") Assert.Pass();
        
        Assert.Fail();
    }

    [Test]
    public void PossibleSpreadsheetBreakOpenDocTest_Image()
    {
        var res = SpreadsheetComparison.PossibleSpreadsheetBreakOpenDoc(_testFileDirectory +
                                                                        @"Spreadsheet\opendoc_with_image.ods");
        
        if(res is null) Assert.Fail();
        
        Assert.That(res, Is.Empty);
    }
    
    [Test]
    public void PossibleSpreadsheetBreakOpenDocTest_Wide_Image()
    {
        var res = SpreadsheetComparison.PossibleSpreadsheetBreakOpenDoc(_testFileDirectory +
                                                                        @"Spreadsheet\opendoc_with_wide_image.ods");
        
        if(res is null) Assert.Fail();
        
        if(res.Count == 1 && res[0].Name == "Object break") Assert.Pass();
        
        Assert.Fail();
    }

    [Test]
    public void PossibleSpreadsheetBreakOpenDocTest_Multi_True()
    {
        var res = SpreadsheetComparison.PossibleSpreadsheetBreakOpenDoc(_testFileDirectory +
                                                                        @"Spreadsheet\opendoc_multi_break.ods");
        
        if(res is null) Assert.Fail();
        
        if(res.Count == 1 && res[0].Name == "Table break") Assert.Pass();
        
        Assert.Fail();
    }

    [Test]
    public void PossibleSpreadsheetBreakOpenDocTest_Multi_False()
    {
        var res = SpreadsheetComparison.PossibleSpreadsheetBreakOpenDoc(_testFileDirectory + @"Spreadsheet\opendoc_multi_no_break.ods");
        
        if(res is null) Assert.Fail();
        
        Assert.That(res, Is.Empty);
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

    [Test]
    public void PossibleSpreadsheetBreakCSV_NoDelimiter()
    {
        var res = SpreadsheetComparison.PossibleLineBreakCsv(_testFileDirectory + @"Spreadsheet\csv_no_delimiter.csv");
        
        if(res is not null) Assert.Fail();
        
        Assert.That(res, Is.Null);
    }
}