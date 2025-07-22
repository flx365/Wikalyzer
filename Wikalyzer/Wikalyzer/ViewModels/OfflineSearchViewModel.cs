using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wikalyzer.Models;
using Wikalyzer.Services;

namespace Wikalyzer.ViewModels;

/// <summary>
/// ViewModel für die Offline-Textanalyse.
/// </summary>
public partial class OfflineSearchViewModel : ViewModelBase
{
    private readonly TextAnalyzer _analyzer = new();

    /// <summary>Suchbegriff (optional).</summary>
    [ObservableProperty]
    private string? _searchTerm;

    /// <summary>Vollständiger Eingabetext.</summary>
    [ObservableProperty]
    private string? _inputText;

    /// <summary>Analyse-Ergebnis (Statistiken).</summary>
    [ObservableProperty]
    private TextStats? _stats;

    /// <summary>Führt die Analyse asynchron aus.</summary>
    [RelayCommand(CanExecute = nameof(CanAnalyze))]
    private async Task AnalyzeAsync()
    {
        if (string.IsNullOrWhiteSpace(InputText))
            return;

        // Analyse im Hintergrund
        var result = await Task.Run(() => _analyzer.Analyze(InputText!));

        // Ergebnis auf UI-Thread setzen
        await Dispatcher.UIThread.InvokeAsync(() => Stats = result);
    }

    /// <summary>Prüft, ob Analyse-Befehl ausgeführt werden darf.</summary>
    private bool CanAnalyze() =>
        !string.IsNullOrWhiteSpace(InputText);

    /// <summary>Wird nach Änderung von InputText ausgeführt.</summary>
    partial void OnInputTextChanged(string? value) =>
        AnalyzeCommand.NotifyCanExecuteChanged();
}