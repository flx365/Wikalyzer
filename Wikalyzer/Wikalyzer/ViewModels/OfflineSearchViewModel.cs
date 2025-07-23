// ViewModels/OfflineSearchViewModel.cs
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wikalyzer.Models;
using Wikalyzer.Services;

namespace Wikalyzer.ViewModels;

public partial class OfflineSearchViewModel : ViewModelBase
{
    private readonly TextAnalyzer _analyzer = new();
    private readonly HistoryService _history = HistoryService.Instance;

    [ObservableProperty] private string? _searchTerm;
    [ObservableProperty] private string? _inputText;
    [ObservableProperty] private TextStats _stats = new TextStats();
    [ObservableProperty] private bool _isAnalyzing;

    // ViewModels/OfflineSearchViewModel.cs
    [RelayCommand(CanExecute = nameof(CanAnalyze))]
    private async Task AnalyzeAsync()
    {
        IsAnalyzing = true;

        // Analyse ausführen
        var result = await Task.Run(() => _analyzer.Analyze(InputText ?? string.Empty));
        await Dispatcher.UIThread.InvokeAsync(() => Stats = result);

        // Mini‑Summary: ersten 50 Zeichen des InputText
        string snippet = InputText?.Trim() ?? "";
        if (snippet.Length > 50)
            snippet = snippet.Substring(0, 50).TrimEnd() + "...";

        // Verlaufseintrag: erst Statistik, dann Newline + Snippet
        var historyText = $"Offline: {Stats.WordCount} Wörter\n{snippet}";
        _history.AddOfflineAnalysis(historyText);

        IsAnalyzing = false;
    }
    
    private bool CanAnalyze() =>
        !string.IsNullOrWhiteSpace(InputText);

    partial void OnInputTextChanged(string? _) =>
        AnalyzeCommand.NotifyCanExecuteChanged();
}