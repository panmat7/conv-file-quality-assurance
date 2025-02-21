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
}