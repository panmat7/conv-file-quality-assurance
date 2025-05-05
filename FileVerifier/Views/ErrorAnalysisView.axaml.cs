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
using AODL.Document.Content.Text;
using System.Text;
using Avalonia.Interactivity;

namespace AvaloniaDraft.Views;

public partial class ErrorAnalysisView : UserControl
{
    private Logger.Logger Logger;
    private List<Expander> TestExpanders;

    public ErrorAnalysisView()
    {
        InitializeComponent();

        TestExpanders = [];
        ClearButton.IsEnabled = false;

        if (GlobalVariables.Logger.HasFinished())
        {
            Logger = GlobalVariables.Logger;
        }
        else
        {
            LoadFromCurrentReportButton.IsEnabled = false;
            Logger = new Logger.Logger();
            Logger.Initialize();
        }
    }


    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (GlobalVariables.Logger.HasFinished())
        {
            LoadFromCurrentReportButton.IsEnabled = true;
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


    private void LoadFromCurrentReport(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Logger = GlobalVariables.Logger;
        CreateElements();
        DisplayReport();
    }


    private void Clear(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Logger = new Logger.Logger();

        TestExpanders.Clear();

        AnalysisStackPanel.Children.Clear();
        Summary.Text = "";

        ClearButton.IsEnabled = false;
    }


    private void CreateElements()
    {
        TestExpanders.Clear();

        var totalTestsFailed = 0;

        var failedComparisons = Logger.Results.Where(r => !r.Pass).ToList();


        // Seperate expander for internal errors
        if (Logger.InternalErrorFilePairs.Count > 0)
        {
            var stringBuilder = new StringBuilder();
            foreach (var fp in Logger.InternalErrorFilePairs)
            {
                var oFile = System.IO.Path.GetFileName(fp.OriginalFilePath);
                var nFile = System.IO.Path.GetFileName(fp.NewFilePath);
                var filePairName = $"{oFile} -> {nFile}";
                stringBuilder.AppendLine(filePairName);
            }

            var errorExpander = new Expander
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Header = new TextBlock
                {
                    Text = $"Internal error ({Logger.InternalErrorFilePairs.Count})",
                },
                Content = new TextBlock
                {
                    Text = stringBuilder.ToString().TrimEnd(),
                    Foreground = Brushes.White,
                    LineHeight = 30,
                }
            };
            TestExpanders.Add(errorExpander);
        }

        var testExpanders = new Dictionary<string, Expander>();
        var failedComparisonsCount = new Dictionary<string, int>();
        var stringBuilders = new Dictionary<string, StringBuilder>();
        var methods = Methods.GetList();

        foreach (var method in methods.Select(m => m.Name))
        {
            var content = new TextBlock { Foreground = Brushes.White };
            var expander = new Expander
            {
                Content = content,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
            };

            failedComparisonsCount[method] = 0;
            stringBuilders[method] = new StringBuilder();
            testExpanders[method] = expander;
        }


        foreach (var c in failedComparisons)
        {
            var oFile = System.IO.Path.GetFileName(c.FilePair.OriginalFilePath);
            var nFile = System.IO.Path.GetFileName(c.FilePair.NewFilePath);
            var filePairName = $"{oFile} -> {nFile}";

            var failedTests = c.Tests.Where(t => !t.Value.Pass).ToList();
            foreach (var method in failedTests.Select(t => t.Key))
            {
                stringBuilders[method].AppendLine(filePairName);
                failedComparisonsCount[method]++;
                totalTestsFailed++;
            }
        }



        foreach (var e in testExpanders.Where(t => failedComparisonsCount[t.Key] > 0))
        {
            var method = e.Key;
            var expander = e.Value;
            TestExpanders.Add(expander);
            expander.Header = new TextBlock { Text = $"{method} ({failedComparisonsCount[method]})" };
            expander.Content = new TextBlock { 
                Text = stringBuilders[method].ToString().TrimEnd(),
                Foreground = Brushes.White,
                LineHeight = 30,
            };
        }


        Summary.Text = $"{totalTestsFailed} Tests failed";
    }


    private void DisplayReport()
    {
        AnalysisStackPanel.Children.Clear();

        if (TestExpanders.Count == 0) return;
        ClearButton.IsEnabled = true;

        foreach (var expander in TestExpanders)
        {
            AnalysisStackPanel.Children.Add(expander);
        }
    }
}