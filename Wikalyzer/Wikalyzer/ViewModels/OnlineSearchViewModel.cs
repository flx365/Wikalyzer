// ViewModels/OnlineSearchViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wikalyzer.Models;
using Wikalyzer.Services;

namespace Wikalyzer.ViewModels;

/// <summary>
/// ViewModel für die Onlinesuche mit Mini‑Summary im Verlauf.
/// </summary>
public partial class OnlineSearchViewModel : ViewModelBase
{
    private readonly WikiRestClient _wiki;
    private readonly HistoryService _history = HistoryService.Instance;

    public OnlineSearchViewModel()
    {
        _wiki = new WikiRestClient(WikiLanguage.De);

        Filters = new ObservableCollection<NamespaceFilter>
        {
            new() { Name = "Artikel",    Id = 0 },
            new() { Name = "Benutzer",   Id = 2 },
            new() { Name = "Datei/Bild", Id = 6 },
            new() { Name = "Alle",       Id = -1 }
        };
        SelectedFilter = Filters.First();
    }

    [ObservableProperty] private string? _searchTerm;
    [ObservableProperty] private ObservableCollection<ArticleSearchResult> _searchResults = new();
    [ObservableProperty] private bool _isSearching;

    public ObservableCollection<NamespaceFilter> Filters { get; }

    [ObservableProperty] private NamespaceFilter _selectedFilter;

    // ViewModels/OnlineSearchViewModel.cs
    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
            return;

        IsSearching = true;
        SearchResults.Clear();

        var items = await _wiki.SearchWithThumbnailsAsync(SearchTerm!, SelectedFilter.Id, limit:10, thumbSize:150);
        foreach (var it in items)
            SearchResults.Add(it);
        IsSearching = false;

        // Mini‑Summary: erstes Summary‑Snippet
        var snippet = items.FirstOrDefault()?.Summary ?? "";
        if (snippet.Length > 50)
            snippet = snippet.Substring(0, 50).TrimEnd() + "...";

        // Verlaufseintrag: erst Suchbegriff, dann Newline + Snippet
        var historyText = $"Online: {SearchTerm}\n{snippet}";
        _history.AddOnlineSearch(historyText);
    }

}
