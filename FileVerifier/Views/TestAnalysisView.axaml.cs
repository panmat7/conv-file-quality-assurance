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

namespace AvaloniaDraft.Views;

public partial class TestAnalysisView : UserControl
{
    private Logger.Logger Logger;
    private List<Expander> TestExpanders;

    public TestAnalysisView()
    {
        InitializeComponent();

        TestExpanders = [];

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

    private void CreateElements()
    {
        TestExpanders.Clear();

        var totalTestsFailed = 0;

        var failedComparisons = Logger.Results.Where(r => !r.Pass).ToList();

        var testExpanders = new Dictionary<string, Expander>();
        var failedComparisonsCount = new Dictionary<string, int>();
        var stringBuilders = new Dictionary<string, StringBuilder>();
        var methods = Methods.GetList();
        foreach (var method in methods)
        {
            var content = new TextBlock { Foreground = Brushes.White };
            var expander = new Expander
            {
                Content = content,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
            };

            failedComparisonsCount[method.Name] = 0;
            stringBuilders[method.Name] = new StringBuilder();
            testExpanders[method.Name] = expander;
        }


        foreach (var c in failedComparisons)
        {
            var oFile = System.IO.Path.GetFileName(c.FilePair.OriginalFilePath);
            var nFile = System.IO.Path.GetFileName(c.FilePair.NewFilePath);
            var filePairName = oFile + " -> " + nFile;

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
            expander.Header = new TextBlock { Text = method + " - " + failedComparisonsCount[method] };
            expander.Content = new TextBlock { 
                Text = stringBuilders[method].ToString().TrimEnd(),
                Foreground = Brushes.White, 
                LineHeight = 30,
            };
        }


        Summary.Text = $"Tests failed (Total {totalTestsFailed})";
    }


    private void DisplayReport()
    {
        AnalysisStackPanel.Children.Clear();

        foreach (var expander in TestExpanders)
        {
            AnalysisStackPanel.Children.Add(expander);
        }
    }
}