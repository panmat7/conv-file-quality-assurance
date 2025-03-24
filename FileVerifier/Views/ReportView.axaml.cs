using Avalonia;
using Avalonia.Controls;
using AvaloniaDraft.Helpers;
using AvaloniaDraft.ViewModels;
using AvaloniaDraft.Logger;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Emgu.CV.CvEnum;
using System;
using System.IO;
using System.ComponentModel.DataAnnotations;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Controls.Shapes;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using System.Linq;

namespace AvaloniaDraft.Views;

public partial class ReportView : UserControl
{
    private Logger.Logger logger;
    private int currentRow;

    public ReportView()
    {
        InitializeComponent();
        currentRow = 0;

        if (GlobalVariables.Logger.Finished)
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
        foreach (var result in logger.Results)
        {
            var expander = CreateComparisonResultExpander(result);
            ReportGrid.Children.Add(expander);

            totalComparisons++;
            if (result.Pass) passedComparisons++;
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


        foreach (var test in result.Tests)
        {
            totalTests++;
            if (test.Value.Pass) passedTests++;

            var testExpander = CreateTestResultExpander(test.Value);
            testExpander.Header = new TextBlock { Text = $"{(test.Value.Pass ? "PASS" : "FAIL")} - {test.Key}" };

            stackPanel.Children.Add(testExpander);
        }

        var pass = (passedTests > 0);

        passText.Text = $"Passed: {(pass ? "Yes" : "No")}";

        testSummary.Text = $"Tests ({passedTests}/{totalTests} passed) :";
        expander.Content = stackPanel;

        var oFile = System.IO.Path.GetFileName(result.FilePair.OriginalFilePath);
        var oFormat = result.FilePair.OriginalFileFormat;
        var nFile = System.IO.Path.GetFileName(result.FilePair.NewFilePath);
        var nFormat = result.FilePair.NewFileFormat;

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
            Text = "Passed: " + (result.Pass ? "Yes" : "No"),
            Foreground = Brushes.White,
        });

        if (result.Percentage != null) stackPanel.Children.Add(new TextBlock()
        {
            Text = $"Similarity percentage: {result.Percentage}%",
            Foreground = Brushes.White,
        });

        if (result.Comments != null) foreach (var comment in result.Comments)
        {
            stackPanel.Children.Add(new TextBlock()
            {
                Text = comment,
                Foreground = Brushes.White,
            });
        }

        if (result.Errors.Any()) stackPanel.Children.Add(new TextBlock()
        {
            Text = result.Errors.ToString(),
            Foreground = Brushes.White,
        });


        expander.Content = stackPanel;

        return expander;
    }
}