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
using DocumentFormat.OpenXml.Drawing.Diagrams;
using System.Diagnostics;
using Avalonia.VisualTree;
using System.Collections.Generic;
using Org.BouncyCastle.Asn1.Ocsp;
using Avalonia.Input;

namespace AvaloniaDraft.Views;

public partial class ReportView : UserControl
{
    private Logger.Logger Logger;
    private List<Expander> AllResultExpanders;
    private List<Expander> ResultExpanders;


    public ReportView()
    {
        InitializeComponent();

        AllResultExpanders = [];
        ResultExpanders = [];

        if (GlobalVariables.Logger.Finished)
        {
            Logger = GlobalVariables.Logger;
            CreateElements();
            DisplayReport();
        } 
        else
        {
            Logger = new Logger.Logger();
            Logger.Initialize();
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

            Logger.ImportJSON(path);
            CreateElements();
            DisplayReport();
        }
        else
        {
            //TODO: Please select JSON message
        }
    }

    private void CreateElements()
    {
        AllResultExpanders = [];
        ResultExpanders = [];

        ReportSummary.Text = $"{Logger.FileComparisonsFailed}/{Logger.FileComparisonCount} file comparisons failed";
        foreach (var result in Logger.Results)
        {
            var comparisonResultexpander = CreateComparisonResultExpander(result);
            AllResultExpanders.Add(comparisonResultexpander);
            ResultExpanders.Add(comparisonResultexpander);
        }   
    }


    private void DisplayReport()
    {
        ReportStackPanel.Children.Clear();

        foreach (var expander in ResultExpanders)
        {
            ReportStackPanel.Children.Add(expander);
        }
    }


    private void SearchBar_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Search(sender, e);
        }
    }

    private void Search(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var searchString = SearchBar?.Text?.Trim().ToLower();
        if (string.IsNullOrEmpty(searchString))
        {
            ResultExpanders = AllResultExpanders.ToList();
            DisplayReport();
            return;
        }

        ResultExpanders.Clear();

        var keyWords = searchString.Split(' ');
        
        foreach (var expander in AllResultExpanders)
        {
            if (expander.Header is not TextBlock header || header.Text is not string text) continue;

            var match = true;
            foreach (var keyWord in keyWords)
            {
                if (!text.ToLower().Contains(keyWord))
                {
                    match = false;
                    break;
                }
            }

            if (match) ResultExpanders.Add(expander);
        }
        DisplayReport();
    }

    private void ShowFailedFirst(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ResultExpanders = ResultExpanders.OrderByDescending(e => e.Classes.Contains("FAIL")).ToList();
        DisplayReport();
    }


    private void ShowPassedFirst(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ResultExpanders = ResultExpanders.OrderByDescending(e => e.Classes.Contains("PASS")).ToList();
        DisplayReport();
    }


    private Expander CreateComparisonResultExpander(Logger.Logger.ComparisonResult result)
    {
        var expander = new Expander();
        expander.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;

        var stackPanel = new StackPanel();
        var passOrFail = result.Pass ? "PASS" : "FAIL";
        expander.Classes.Add(passOrFail);

        var passText = new TextBlock() { Foreground = Brushes.White };
        stackPanel.Children.Add(passText);

        var testSummary = new TextBlock() { Foreground = Brushes.White };
        stackPanel.Children.Add(testSummary);


        foreach (var test in result.Tests)
        {
            var testExpander = CreateTestResultExpander(test.Value);
            testExpander.Header = new TextBlock { Text = $"{(test.Value.Pass ? "PASS" : "FAIL")} - {test.Key}" };

            stackPanel.Children.Add(testExpander);
        }

        passText.Text = $"Passed: {(result.Pass ? "Yes" : "No")}";

        testSummary.Text = $"Tests ({result.TestsPassed}/{result.TotalTests} passed) :";
        expander.Content = stackPanel;

        var oFile = System.IO.Path.GetFileName(result.FilePair.OriginalFilePath);
        var oFormat = result.FilePair.OriginalFileFormat;
        var nFile = System.IO.Path.GetFileName(result.FilePair.NewFilePath);
        var nFormat = result.FilePair.NewFileFormat;

        var headerText = $"{passOrFail} - {oFile}  -> {nFile} ({oFormat} -> {nFormat})";
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


        // Errors
        if (result.Errors != null && result.Errors.Any())
        {
            var commentsContainer = new Border
            {
                BorderThickness = new Thickness(2),
                BorderBrush = Brushes.Gray,
                Background = Brushes.Black,
                Padding = new Thickness(5),
                Margin = new Thickness(5),
            };

            var commentsStackPanel = new StackPanel();
            commentsStackPanel.Children.Add(new TextBlock
            {
                Text = "Errors:",
                Foreground = Brushes.White
            });
            commentsContainer.Child = commentsStackPanel;
            stackPanel.Children.Add(commentsContainer);

            // Errors
            foreach (var err in result.Errors.OrderByDescending(e => e.Severity))
            {
                (var bgCol, var severityString) = err.Severity switch
                {
                    ErrorSeverity.Unset => (Brushes.Black, "Unset"),
                    ErrorSeverity.Low => (Brushes.DarkGoldenrod, "Low"),
                    ErrorSeverity.Medium => (Brushes.DarkOrange, "Medium"),
                    ErrorSeverity.High => (Brushes.DarkRed, "High"),
                    ErrorSeverity.Internal => (Brushes.Purple, "Internal"),
                    _ => (null, null),
                };
                if (bgCol == null || severityString == null) continue;

                commentsStackPanel.Children.Add(new Border
                {
                    BorderThickness = new Thickness(2),
                    BorderBrush = Brushes.Gray,
                    Background = bgCol,
                    Padding = new Thickness(5),
                    Margin = new Thickness(5),
                    Child = new TextBlock
                    {
                        Text = $"{err.Name}: {err.Description}\nSeverity: {severityString}",
                        Foreground = Brushes.White
                    }
                });
            }
        }


        // Comments
        if (result.Comments != null && result.Comments.Any())
        {
            var commentsContainer = new Border
            {
                BorderThickness = new Thickness(2),
                BorderBrush = Brushes.Gray,
                Background = Brushes.Black,
                Padding = new Thickness(5),
                Margin = new Thickness(5),
            };

            var commentsStackPanel = new StackPanel();
            commentsStackPanel.Children.Add(new TextBlock
            {
                Text = "Comments:",
                Foreground = Brushes.White
            });
            commentsContainer.Child = commentsStackPanel;
            stackPanel.Children.Add(commentsContainer);


            foreach (var comment in result.Comments)
            {

                commentsStackPanel.Children.Add(new Border
                {
                    BorderThickness = new Thickness(2),
                    BorderBrush = Brushes.Gray,
                    Background = Brushes.Black,
                    Padding = new Thickness(5),
                    Margin = new Thickness(5),
                    Child = new TextBlock
                    {
                        Text = comment,
                        Foreground = Brushes.White
                    }
                });
            }
        }


        expander.Content = stackPanel;

        return expander;
    }
}