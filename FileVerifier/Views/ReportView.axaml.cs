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
using System.Diagnostics;
using Avalonia.VisualTree;
using System.Collections.Generic;
using Org.BouncyCastle.Asn1.Ocsp;
using Avalonia.Input;
using Avalonia.Media.Immutable;
using Avalonia.Interactivity;
using System.Text;
using AvaloniaDraft.FileManager;

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

        ClearButton.IsEnabled = false;

        if (GlobalVariables.Logger.HasFinished())
        {
            Logger = GlobalVariables.Logger;
        }
        else
        {
            LoadCurrentReportButton.IsEnabled = false;
            Logger = new Logger.Logger();
            Logger.Initialize();
        }
    }


    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (GlobalVariables.Logger.HasFinished())
        {
            LoadCurrentReportButton.IsEnabled = true;
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
            FileTypeFilter =
            [
                new FilePickerFileType("JSON file")
                {
                    Patterns = ["*.json"]
                }
            ]
        });

        if (result != null && result.Count > 0)
        {
            var json = result[0];
            var path = json.Path.AbsolutePath;

            var tempLogger = new Logger.Logger();
            tempLogger.Initialize();
            tempLogger.ImportJSON(path);

            Logger = tempLogger;

            CreateElements();
            DisplayReport();
        }
        else
        {
            //TODO: Please select JSON message
        }
    }


    private void LoadCurrentReport(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Logger = GlobalVariables.Logger;
        CreateElements();
        DisplayReport();
    }


    private void Clear(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Logger = new Logger.Logger();

        AllResultExpanders.Clear();
        ResultExpanders.Clear();

        ReportStackPanel.Children.Clear();
        IgnoredFilesStackPanel.Children.Clear();
        ReportSummary.Text = "";

        ClearButton.IsEnabled = false;
    }

    private void CreateElements()
    {
        AllResultExpanders.Clear();
        ResultExpanders.Clear();
        IgnoredFilesStackPanel.Children.Clear();

        ReportSummary.Text = $"{Logger.FileComparisonsFailed}/{Logger.FileComparisonCount} file comparisons failed " +
            $"| Completed in {Logger.Stopwatch.Elapsed.ToString("hh\\:mm\\:ss")}";

        CreateIgnoredFilesExpander();

        foreach (var result in Logger.Results)
        {
            var comparisonResultexpander = CreateComparisonResultExpander(result);

            AllResultExpanders.Add(comparisonResultexpander);
            ResultExpanders.Add(comparisonResultexpander);
        }
    }


    private void CreateIgnoredFilesExpander()
    {
        if (Logger.IgnoredFiles.Count > 0)
        {
            var ignoredFilesExpanderStackPanel = new StackPanel();
            var ignoredFilesExpander = new Expander
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Header = new TextBlock
                {
                    Text = $"Ignored files ({Logger.IgnoredFiles.Count})",
                },
                Content = ignoredFilesExpanderStackPanel,
            };

            var reasons = new List<(FileManager.ReasonForIgnoring enm, string str)> {
                (FileManager.ReasonForIgnoring.UnsupportedFormat, "Unsupported file format"),
                (FileManager.ReasonForIgnoring.Encrypted, "Encrypted"),
                (FileManager.ReasonForIgnoring.Corrupted, "Corrupted"),
                (FileManager.ReasonForIgnoring.EncryptedOrCorrupted, "Encrypted or corrupted"),
                (FileManager.ReasonForIgnoring.Unknown, "Unknown reason")
            };


            foreach (var reason in reasons)
            {
                var files = GlobalVariables.Logger.IgnoredFiles.Where(f => f.Reason == reason.enm);
                if (!files.Any()) continue;

                var stringBuilder = new StringBuilder();
                var fileCount = 0;
                foreach (var file in files)
                {
                    fileCount++;
                    var path = file.FilePath;
                    var dir = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(path));
                    var fileName = System.IO.Path.GetFileName(path);
                    stringBuilder.AppendLine($"{dir}/{fileName}");
                }

                var expander = new Expander
                {
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                    Header = new TextBlock
                    {
                        Text = $"{reason.str} ({fileCount})",
                    },
                    Content = new TextBlock
                    {
                        Foreground = Brushes.White,
                        LineHeight = 30,
                        Text = stringBuilder.ToString().Trim(),
                    }
                };

                ignoredFilesExpanderStackPanel.Children.Add(expander);
            }

            IgnoredFilesStackPanel.Children.Add(ignoredFilesExpander);
        }
        else
        {
            IgnoredFilesStackPanel.Children.Add(new TextBlock
            {
                Text = "No ignored files.",
                Foreground = Brushes.White,
            });
        }
    }


    private void DisplayReport()
    {
        ReportStackPanel.Children.Clear();

        if (ResultExpanders.Count == 0) return;

        ClearButton.IsEnabled = true;
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
                if (!text.Contains(keyWord, StringComparison.CurrentCultureIgnoreCase))
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


    private Expander CreateComparisonResultExpander(Logger.ComparisonResult result)
    {
        var expander = new Expander
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
        };

        var passOrFail = result.Pass ? "PASS" : "FAIL";
        expander.Classes.Add(passOrFail);

        var oFile = System.IO.Path.GetFileName(result.FilePair.OriginalFilePath);
        var oFormat = result.FilePair.OriginalFileFormat;
        var nFile = System.IO.Path.GetFileName(result.FilePair.NewFilePath);
        var nFormat = result.FilePair.NewFileFormat;

        var headerText = $"{passOrFail} - {oFile}  -> {nFile} ({oFormat} -> {nFormat})";
        expander.Header = new TextBlock { Text = headerText };


        expander.Expanded += (s, e) =>
        {
            if (expander.Content == null)
            {
                expander.Content = CreateResultContainer(result);
            }
        };

        return expander;
    }



    private StackPanel CreateResultContainer(ComparisonResult result)
    {
        var stackPanel = new StackPanel();

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

        var totalTests = result.Tests.Count;
        var testsPassed = result.Tests.Count(t => t.Value.Pass);
        testSummary.Text = $"Tests ({testsPassed}/{totalTests} passed)";

        return stackPanel;
    }


    private Expander CreateTestResultExpander(Logger.TestResult result)
    {
        var maxTextboxWidth = 500;
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
            var errorsContainer = new Border
            {
                BorderThickness = new Thickness(2),
                BorderBrush = Brushes.Gray,
                Background = Brushes.Black,
                Padding = new Thickness(5),
                Margin = new Thickness(5),
            };

            var errorsStackPanel = new StackPanel();
            errorsStackPanel.Children.Add(new TextBlock
            {
                Text = "Errors:",
                Foreground = Brushes.White
            });
            errorsContainer.Child = errorsStackPanel;
            stackPanel.Children.Add(errorsContainer);

            // List errors in order of severity
            foreach (var err in result.Errors.OrderByDescending(e => e.Severity))
            {
                var errTypeString = err.ErrorType switch
                {
                    ErrorType.Unset => "Unset",
                    ErrorType.Metadata => "Metadata",
                    ErrorType.Visual => "Visual",
                    ErrorType.KnownErrorSource => "Known error source",
                    ErrorType.FileError => "File error",
                    _ => null,
                };
                if (errTypeString == null) continue;

                (var errCol, var errSeverityString) = err.Severity switch
                {
                    ErrorSeverity.Unset => (Brushes.Gray, "Unset"),
                    ErrorSeverity.Low => (Brushes.Yellow, "Low"),
                    ErrorSeverity.Medium => (Brushes.Orange, "Medium"),
                    ErrorSeverity.High => (Brushes.Red, "High"),
                    ErrorSeverity.Internal => (Brushes.Black, "Internal"),
                    _ => (null, null),
                };
                if (errCol == null || errSeverityString == null) continue;


                var errStackPanel = new StackPanel();
                errStackPanel.Orientation = Avalonia.Layout.Orientation.Horizontal;
                errorsStackPanel.Children.Add(errStackPanel);

                errStackPanel.Children.Add(new Ellipse
                {
                    Width = 30,
                    Height = 30,
                    Fill = errCol,
                    Stroke = Brushes.White,
                    StrokeThickness = 1,
                });
                errStackPanel.Children.Add(new Border
                {
                    BorderThickness = new Thickness(2),
                    BorderBrush = Brushes.Gray,
                    Padding = new Thickness(5),
                    Margin = new Thickness(5),
                    Child = new TextBlock
                    {
                        Text = $"{err.Name}: {err.Description}\nType: {errTypeString}\nSeverity: {errSeverityString}",
                        MaxWidth = maxTextboxWidth - 44,
                        Width = maxTextboxWidth - 44,
                        TextWrapping = TextWrapping.Wrap,
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

            var commentsStackPanel = new StackPanel
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            };
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
                    MaxWidth = maxTextboxWidth,
                    Width = maxTextboxWidth,
                    Child = new TextBlock
                    {
                        Text = comment,
                        MaxWidth = maxTextboxWidth,
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = Brushes.White
                    }
                });
            }
        }

        expander.Content = stackPanel;

        return expander;
    }
}