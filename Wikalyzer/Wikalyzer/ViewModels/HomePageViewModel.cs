// ViewModels/HomePageViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OxyPlot;
using OxyPlot.Series;
using Wikalyzer.Services;

namespace Wikalyzer.ViewModels
{
    public partial class HomePageViewModel : ViewModelBase
    {
        private readonly HistoryService _history = HistoryService.Instance;

        // Kennzahlen (KPIs)
        [ObservableProperty]
        private int _totalOfflineAnalyses;

        [ObservableProperty]
        private int _totalOnlineSearches;

        [ObservableProperty]
        private DateTime _lastAnalysisDate;

        // Getrennte Listen für Offline- und Online-History
        [ObservableProperty]
        private ObservableCollection<string> _recentOfflineAnalyses = new();

        [ObservableProperty]
        private ObservableCollection<string> _recentOnlineAnalyses = new();

        // Pie-Chart Model 
        [ObservableProperty]
        private PlotModel _pieChartModel = new PlotModel();

        public HomePageViewModel()
        {
            // 1) KPI‑Werte & Listen initial laden
            UpdateKPIs();
            LoadHistoryEntries();

            // 2) Diagramm mit den geladenen Daten erstellen
            UpdateChart();

            // 3) Auf neue EInträge im Verlauf reagieren
            _history.OfflineAdded += OnOfflineAdded;
            _history.OnlineAdded  += OnOnlineAdded;
        }

        private void UpdateKPIs()
        {
            TotalOfflineAnalyses = _history.OfflineHistory.Count;
            TotalOnlineSearches  = _history.OnlineHistory.Count;
            LastAnalysisDate     = DateTime.Now;
        }

        private void LoadHistoryEntries()
        {
            foreach (var entry in _history.OfflineHistory)
                RecentOfflineAnalyses.Add(entry);

            foreach (var query in _history.OnlineHistory)
                RecentOnlineAnalyses.Add($"Online: {query}");
        }

        private void UpdateChart()
        {
            PieChartModel = BuildPieModel();
        }

        private PlotModel BuildPieModel()
        {
            var model = new PlotModel { Title = "Suche‑Verhältnis" };
            var pie = new PieSeries
            {
                StrokeThickness     = 0.25,
                InsideLabelPosition = 0.8,
                AngleSpan           = 360,
                StartAngle          = 0
            };

            pie.Slices.Add(new PieSlice("Offline", TotalOfflineAnalyses));
            pie.Slices.Add(new PieSlice("Online",  TotalOnlineSearches));
            model.Series.Add(pie);

            return model;
        }

        private void OnOfflineAdded(string summary)
        {
            RecentOfflineAnalyses.Insert(0, summary);
            UpdateKPIs();
            UpdateChart();
        }

        private void OnOnlineAdded(string query)
        {
            RecentOnlineAnalyses.Insert(0, $"Online: {query}");
            UpdateKPIs();
            UpdateChart();
        }

        // Schnellzugriffe / Navigation

        [RelayCommand]
        private void NavigateOfflineSearch()
        {
            if (Application.Current?.ApplicationLifetime
                is IClassicDesktopStyleApplicationLifetime desktop &&
                desktop.MainWindow?.DataContext is MainWindowViewModel mw)
            {
                mw.SelectedListItem = mw.Items
                    .FirstOrDefault(i => i.ModelType == typeof(OfflineSearchViewModel));
            }
        }

        [RelayCommand]
        private void NavigateOnlineSearch()
        {
            if (Application.Current?.ApplicationLifetime
                is IClassicDesktopStyleApplicationLifetime desktop &&
                desktop.MainWindow?.DataContext is MainWindowViewModel mw)
            {
                mw.SelectedListItem = mw.Items
                    .FirstOrDefault(i => i.ModelType == typeof(OnlineSearchViewModel));
            }
        }
    }
}
