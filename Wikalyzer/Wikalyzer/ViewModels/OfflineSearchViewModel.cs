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

    // ───────── Properties ─────────

    [ObservableProperty]
    private string? _searchTerm;

    [ObservableProperty]
    private string? _inputText;

    // Nie null → zeigt standardmäßig 0 in allen Feldern
    [ObservableProperty]
    private TextStats _stats = new TextStats();

    [ObservableProperty]
    private bool _isAnalyzing;

    // ───────── Command ────────────

    /// <summary>Führt die Analyse aus und zeigt den Lade­indikator.</summary>
    [RelayCommand(CanExecute = nameof(CanAnalyze))]
    private async Task AnalyzeAsync()
    {
        // Ladeindikator einschalten
        IsAnalyzing = true;

        // Hintergrund-Analyse
        var result = await Task.Run(() => _analyzer.Analyze(InputText ?? string.Empty));

        // Ergebnis setzen (UI-Thread)
        await Dispatcher.UIThread.InvokeAsync(() => Stats = result);

        // Ladeindikator ausschalten
        IsAnalyzing = false;
    }

    /// <summary>Button nur aktiv, wenn Text eingegeben.</summary>
    private bool CanAnalyze() =>
        !string.IsNullOrWhiteSpace(InputText);

    // Re-evaluate Command, sobald sich InputText ändert
    partial void OnInputTextChanged(string? _) =>
        AnalyzeCommand.NotifyCanExecuteChanged();
}