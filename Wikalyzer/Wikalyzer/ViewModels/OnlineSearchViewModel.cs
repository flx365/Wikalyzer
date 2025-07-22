using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Wikalyzer.ViewModels;

/// <summary>
/// ViewModel für die Online-Suchseite.
/// Enthält Suchlogik und Datenbindung für die Online-Suche.
/// </summary>
public partial class OnlineSearchViewModel : ViewModelBase
{
    // ───────── Eingabe für Suchbegriff ─────────

    [ObservableProperty]
    private string? _searchTerm; // Bindet sich an die Eingabe in der UI

    // ───────── Suchergebnisse (z. B. ListBox) ─────────

    [ObservableProperty]
    private ObservableCollection<string> _searchResults = new();

    // ───────── Statusanzeige für Ladeanimation ─────────

    [ObservableProperty]
    private bool _isSearching; // Wird true während der Suche

    /// <summary>
    /// Simuliert eine Online-Suche. Ersetzt später API-Call.
    /// </summary>
    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
            return;

        IsSearching = true;
        SearchResults.Clear();

        await Task.Delay(1000); // Simuliertes Warten auf API-Ergebnis

        // Platzhalter-Ergebnisse
        SearchResults.Add($"Ergebnis 1 für '{SearchTerm}'");
        SearchResults.Add($"Ergebnis 2 für '{SearchTerm}'");
        SearchResults.Add($"Ergebnis 3 für '{SearchTerm}'");

        IsSearching = false;
    }
}