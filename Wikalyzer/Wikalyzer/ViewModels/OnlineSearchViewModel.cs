// ViewModels/OnlineSearchViewModel.cs
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wikalyzer.Models;
using Wikalyzer.Services;

namespace Wikalyzer.ViewModels;

public partial class OnlineSearchViewModel : ViewModelBase
{
    private readonly WikiRestClient _wiki;

    public OnlineSearchViewModel()
    {
        _wiki = new WikiRestClient(WikiLanguage.De);
        Filters = new ObservableCollection<NamespaceFilter>
        {
            new() { Name = "Artikel",    Id = 0,  Description = "Nur Wikipedia-Artikel durchsuchen" },
            new() { Name = "Benutzer",   Id = 2,  Description = "Benutzerseiten auf Wikipedia durchsuchen" },
            new() { Name = "Datei/Bild", Id = 6,  Description = "Dateien und Bilder aus Wikimedia Commons durchsuchen" },
            new() { Name = "Alle",       Id = -1, Description = "Alle verfügbaren Namensräume einbeziehen" }
        };

        SelectedFilter = Filters.First();
        
    }

    [ObservableProperty] private string? _searchTerm;
    [ObservableProperty] private ObservableCollection<ArticleSearchResult> _searchResults 
        = new();
    [ObservableProperty] private bool _isSearching;
    public ObservableCollection<NamespaceFilter> Filters { get; }
    [ObservableProperty] private NamespaceFilter _selectedFilter;

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm)) return;

        IsSearching     = true;
        SearchResults.Clear();

        var items = await _wiki.SearchWithThumbnailsAsync(
            SearchTerm!, SelectedFilter.Id, limit:10, thumbSize:150);
        foreach (var it in items)
            SearchResults.Add(it);

        IsSearching = false;
    }
    
}
