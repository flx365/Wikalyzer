// ViewModels/ArticleViewModel.cs
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wikalyzer.Models;
using Wikalyzer.Services;

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
    [RelayCommand]
    private async Task LoadArticleTextAsync()
    {
        ArticleText = await _wiki.GetArticlePlainTextAsync(Title);
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