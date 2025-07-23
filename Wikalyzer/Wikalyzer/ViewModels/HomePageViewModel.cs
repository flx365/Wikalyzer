// ViewModels/HomePageViewModel.cs
using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Wikalyzer.ViewModels;

public partial class HomePageViewModel : ViewModelBase
{
    // ─── Kennzahlen (KPIs) ───
    [ObservableProperty]
    private int _totalOfflineAnalyses;

    [ObservableProperty]
    private int _totalOnlineSearches;

    [ObservableProperty]
    private DateTime _lastAnalysisDate;

    // ─── Liste der letzten Analysen ───
    [ObservableProperty]
    private ObservableCollection<string> _recentAnalyses = new();

    public HomePageViewModel()
    {
        // Beispiel‑Initialisierung
        TotalOfflineAnalyses  = 5;
        TotalOnlineSearches   = 3;
        LastAnalysisDate      = DateTime.Now.AddDays(-1);
        RecentAnalyses        = new ObservableCollection<string>
        {
            "Einstein-Analyse",
            "UI-Layout-Test",
            "Regex-Parser-Demo"
        };
    }

    // ─── Schnellzugriffe / Navigation ───
    [RelayCommand]
    private void NavigateOfflineSearch()
    {
        // TODO: über MainWindowViewModel.SelectedListItem navigieren
    }

    [RelayCommand]
    private void NavigateOnlineSearch()
    {
        // TODO: über MainWindowViewModel.SelectedListItem navigieren
    }
}