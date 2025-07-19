using ReactiveUI;
using Avalonia.ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using Wikalyzer.Services;
using Wikalyzer.Models;

namespace Wikalyzer.ViewModels;

public partial class MainWindowViewModel : ReactiveObject
{
    private readonly TextAnalyzer _analyzer = new();
    private string? _searchTerm;

    public string? SearchTerm
    {
        get => _searchTerm;
        set => this.RaiseAndSetIfChanged(ref _searchTerm, value);
    }
    
    private string? _inputText;
    public string? InputText
    {
        get => _inputText;
        set => this.RaiseAndSetIfChanged(ref _inputText, value);
    }
    
    private TextStats? _stats;

    public TextStats? Stats
    {
        get => _stats;
        set => this.RaiseAndSetIfChanged(ref _stats, value);
    }
    
    public ReactiveCommand<Unit, Unit> AnalyzeCommand { get; }

    public MainWindowViewModel()
    {
        var canExecute = this.WhenAnyValue(x => x.InputText, input => !string.IsNullOrWhiteSpace(input))
                             .ObserveOn(RxApp.MainThreadScheduler);

        AnalyzeCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var result = await Task.Run(() => _analyzer.Analyze(InputText!));
            await Dispatcher.UIThread.InvokeAsync(() => Stats = result);
        }, canExecute);
    }
}