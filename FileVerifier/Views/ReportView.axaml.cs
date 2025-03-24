using Avalonia.Controls;
using AvaloniaDraft.Helpers;
using Avalonia.Platform.Storage;
using Avalonia.Media;

namespace AvaloniaDraft.Views;

public partial class ReportView : UserControl
{
    private Logger.Logger logger;
    private int currentRow;

    public ReportView()
    {
        InitializeComponent();
        currentRow = 0;

        if (GlobalVariables.Logger.finished)
        {
            logger = GlobalVariables.Logger;
            DisplayReport();
        } 
        else
        {
            logger = new Logger.Logger();
            logger.Initialize();
        }
    }


    private async void LoadJson(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var result = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select JSON report",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("JSON file")
                {
                    Patterns = new[] { "*.json" }
                }
            }
        });

        if (result != null && result.Count > 0)
        {
            var json = result[0];
            var path = json.Path.AbsolutePath;

            logger.ImportJSON(path);
            DisplayReport();
        }
        else
        {
            //TODO: Please select JSON message
        }
    }


    private void DisplayReport()
    {
        InitializeComponent();
        currentRow = 0;


        var totalComparisons = 0;
        var passedComparisons = 0;
        foreach (var result in logger.results)
        {
            var expander = CreateComparisonResultExpander(result);
            ReportGrid.Children.Add(expander);

            totalComparisons++;
            if (result.pass) passedComparisons++;
        }
        ReportSummary.Text = $"{passedComparisons}/{totalComparisons} comparisons successful.";
    }


    private Expander CreateComparisonResultExpander(Logger.Logger.ComparisonResult result)
    {
        var expander = new Expander();
        expander.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;

        Grid.SetColumn(expander, 2);
        Grid.SetRow(expander, currentRow);
        currentRow++;


        var stackPanel = new StackPanel();

        int totalTests = 0;
        int passedTests = 0;

        var passText = new TextBlock() { Foreground = Brushes.White };
        stackPanel.Children.Add(passText);

        var testSummary = new TextBlock() { Foreground = Brushes.White };
        stackPanel.Children.Add(testSummary);


        foreach (var test in result.tests)
        {
            totalTests++;
            if (test.Value.pass) passedTests++;

            var testExpander = CreateTestResultExpander(test.Value);
            testExpander.Header = new TextBlock { Text = $"{(test.Value.pass ? "PASS" : "FAIL")} - {test.Key}" };

            stackPanel.Children.Add(testExpander);
        }

        var pass = (passedTests > 0);

        passText.Text = $"Passed: {(pass ? "Yes" : "No")}";

        testSummary.Text = $"Tests ({passedTests}/{totalTests} passed) :";
        expander.Content = stackPanel;

        var oFile = System.IO.Path.GetFileName(result.filePair.OriginalFilePath);
        var oFormat = result.filePair.OriginalFileFormat;
        var nFile = System.IO.Path.GetFileName(result.filePair.NewFilePath);
        var nFormat = result.filePair.NewFileFormat;

        var headerText = $"{(pass ? "PASS" : "FAIL")} - {oFile} ({oFormat}) -> {nFile} ({nFormat})";
        expander.Header = new TextBlock { Text = headerText };

        return expander;
    }


    private Expander CreateTestResultExpander(Logger.Logger.TestResult result)
    {
        var expander = new Expander();
        expander.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;

        var stackPanel = new StackPanel();

        stackPanel.Children.Add(new TextBlock()
        {
            Text = "Passed: " + (result.pass ? "Yes" : "No"),
            Foreground = Brushes.White,
        });

        if (result.percentage != null) stackPanel.Children.Add(new TextBlock()
        {
            Text = $"Similarity percentage: {result.percentage}%",
            Foreground = Brushes.White,
        });

        if (result.comments != null) foreach (var comment in result.comments)
        {
            stackPanel.Children.Add(new TextBlock()
            {
                Text = comment,
                Foreground = Brushes.White,
            });
        }

        if (result.error != null) stackPanel.Children.Add(new TextBlock()
        {
            Text = result.error.ToString(),
            Foreground = Brushes.White,
        });


        expander.Content = stackPanel;

        return expander;
    }
}