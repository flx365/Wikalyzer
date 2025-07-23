// ViewModels/HomePageViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wikalyzer.Services;

namespace Wikalyzer.ViewModels
{
    public partial class HomePageViewModel : ViewModelBase
    {
        // Singleton-Service
        private readonly HistoryService _history = HistoryService.Instance;

        // ─── Kennzahlen (KPIs) ───
        [ObservableProperty]
        private int _totalOfflineAnalyses;

        [ObservableProperty]
        private int _totalOnlineSearches;

        [ObservableProperty]
        private DateTime _lastAnalysisDate;

        // ─── Getrennte Listen für Offline- und Online-History ───
        [ObservableProperty]
        private ObservableCollection<string> _recentOfflineAnalyses = new();

        [ObservableProperty]
        private ObservableCollection<string> _recentOnlineAnalyses = new();

        public HomePageViewModel()
        {
            // Initial­befüllung aus dem HistoryService
            TotalOfflineAnalyses  = _history.OfflineHistory.Count;
            TotalOnlineSearches   = _history.OnlineHistory.Count;
            LastAnalysisDate      = DateTime.Now;

            foreach (var off in _history.OfflineHistory)
                RecentOfflineAnalyses.Add(off);

            foreach (var on in _history.OnlineHistory)
                RecentOnlineAnalyses.Add($"Online: {on}");

            // Live‑Updates abonnieren
            _history.OfflineAdded += summary =>
            {
                TotalOfflineAnalyses = _history.OfflineHistory.Count;
                LastAnalysisDate     = DateTime.Now;
                RecentOfflineAnalyses.Insert(0, summary);
            };
            _history.OnlineAdded += query =>
            {
                TotalOnlineSearches  = _history.OnlineHistory.Count;
                RecentOnlineAnalyses.Insert(0, $"Online: {query}");
            };
        }

        // ─── Schnellzugriffe / Navigation ───

        [RelayCommand]
        private void NavigateOfflineSearch()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
                desktop.MainWindow?.DataContext is MainWindowViewModel mw)
            {
                mw.SelectedListItem = mw.Items
                    .FirstOrDefault(i => i.ModelType == typeof(OfflineSearchViewModel));
            }
        }

        [RelayCommand]
        private void NavigateOnlineSearch()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
                desktop.MainWindow?.DataContext is MainWindowViewModel mw)
            {
                mw.SelectedListItem = mw.Items
                    .FirstOrDefault(i => i.ModelType == typeof(OnlineSearchViewModel));
            }
        }
    }
}
