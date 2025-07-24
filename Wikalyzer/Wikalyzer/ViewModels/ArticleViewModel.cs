// ViewModels/ArticleViewModel.cs
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wikalyzer.Models;
using System.Collections.Generic;
using Avalonia.Media;
using System.Text.RegularExpressions;
using Wikalyzer.Services;
using Avalonia.Controls;
using Avalonia;

namespace Wikalyzer.ViewModels;

public partial class ArticleViewModel : ViewModelBase
{
    private readonly WikiRestClient _wiki = new(WikiLanguage.De);
    private readonly TextAnalyzer   _analyzer = new();

    public string Title { get; }
    public string ArticleUrl { get; }
    public string Summary { get; }

    [ObservableProperty]
    private string _articleText = string.Empty;      // Hier landet der formatierte Text

    [ObservableProperty]
    private TextStats _stats = new();                // Analyse-Ergebnisse

    [ObservableProperty]
    private bool _isSidebarOpen;                     // Sidebar-Visibility

    [ObservableProperty] private List<object> _renderedBlocks = new();

    public ArticleViewModel(string title, string articleUrl, string summary)
    {
        Title      = title;
        ArticleUrl = articleUrl;
        Summary    = summary;
    }

    /// <summary>
    /// Lädt den Artikel-Text per Regex-Parsen und füllt <see cref="ArticleText"/>.
    /// Generiert: <c>LoadArticleTextCommand</c>.
    /// </summary>
    [ObservableProperty]
    private string _articleHtml = string.Empty; // Für WebView


    [RelayCommand]
    private async Task LoadArticleTextAsync()
    {
        var raw = await _wiki.GetArticleMarkdownAsync(Title);
        ArticleText = raw;
        RenderMarkdownBlocks(raw);
    }
    private void RenderMarkdownBlocks(string markdown)
    {
        var blocks = new List<object>();
        var lines = markdown.Split('\n');

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd();

            if (string.IsNullOrWhiteSpace(line))
            {
                blocks.Add(new TextBlock { Text = "", Margin = new Thickness(0, 4) });
                continue;
            }

            // Markdown-Überschriften
            if (line.StartsWith("### "))
            {
                blocks.Add(new TextBlock
                {
                    Text = line[4..].Trim(),
                    FontWeight = FontWeight.Bold,
                    FontSize = 18,
                    Foreground = Brushes.White,
                    Margin = new Thickness(0, 12, 0, 6)
                });
                continue;
            }
            if (line.StartsWith("## "))
            {
                blocks.Add(new TextBlock
                {
                    Text = line[3..].Trim(),
                    FontWeight = FontWeight.Bold,
                    FontSize = 22,
                    Foreground = Brushes.White,
                    Margin = new Thickness(0, 20, 0, 8)
                });
                continue;
            }

            // Bulletpoints
            if (line.StartsWith("- "))
            {
                blocks.Add(new TextBlock
                {
                    Text = "• " + StripMarkdown(line[2..]),
                    Foreground = Brushes.White,
                    Margin = new Thickness(0, 2)
                });
                continue;
            }

            // Absatz (normaler Text)
            blocks.Add(new TextBlock
            {
                Text = StripMarkdown(line),
                Foreground = Brushes.White,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                Margin = new Thickness(0, 2)
            });
        }

        RenderedBlocks = blocks;
    }


    private static string StripMarkdown(string text)
    {
        text = Regex.Replace(text, @"\*\*(.*?)\*\*", "$1");
        text = Regex.Replace(text, @"_(.*?)_", "$1");
        text = Regex.Replace(text, @"\[(.*?)\]\((.*?)\)", "$1");
        return text;
    }


    /// <summary>
    /// Analysiert den aktuell geladenen <see cref="ArticleText"/>.
    /// Generiert: <c>AnalyzeCommand</c>.
    /// </summary>
    [RelayCommand]
    private void Analyze()
    {
        Stats = _analyzer.Analyze(ArticleText);
    }


}