using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Wikalyzer.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] 
    private bool _isPaneOpen = true;
    
    [ObservableProperty]
    private ViewModelBase _currentPage = new HomePageViewModel();

    [ObservableProperty] 
    private ListItemTemplate? _selectedListItem;

    partial void OnSelectedListItemChanged(ListItemTemplate? value)
    {
        if (value is null) return;
        var instance = Activator.CreateInstance(value.ModelType);
        if (instance is null) return;
        CurrentPage = (ViewModelBase)instance;
    }

    public ObservableCollection<ListItemTemplate> Items { get; } = new()
    {
        new ListItemTemplate(typeof(HomePageViewModel), "HomeRegular"),
        new ListItemTemplate(typeof(OnlineSearchViewModel), "GlobeSearchRegular"),
        new ListItemTemplate(typeof(OfflineSearchViewModel), "DocumentSearchRegular"),
       
    };

    [RelayCommand]
    private void TriggerPane()
    {
        IsPaneOpen = !_isPaneOpen;
    }
}

public class ListItemTemplate
{
    public ListItemTemplate(Type type, string iconKey)
    {
        ModelType = type;
        Label = type.Name.Replace("PageViewModel", "")
            .Replace("ViewModel", "")
            .Replace("Search", "")
            .Replace("Home", "Dashboard")
            .Replace("Online", "Online - Suche")
            .Replace("Offline", "Text analysieren");
        
        Application.Current!.TryFindResource(iconKey, out var res);
        ListItemIcon = (StreamGeometry)res!;
    }
    public string Label { get; }
    public Type ModelType { get; }
    
    public StreamGeometry ListItemIcon { get; }
}