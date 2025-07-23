// Views/OnlineSearchView.xaml.cs
using Avalonia.Controls;
using Avalonia.Interactivity;
using Wikalyzer.Models;
using Wikalyzer.ViewModels;

namespace Wikalyzer.Views;

public partial class OnlineSearchView : UserControl
{
    public OnlineSearchView()
    {
        InitializeComponent();
    }

    private async void OnResultDoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not OnlineSearchViewModel vm)
            return;

        // Wenn wir im Datei/Bild-Modus sind, abbrechen (nicht öffnen)
        if (vm.SelectedFilter.Id == 6)
            return;

        // Sonst normalen Artikel öffnen
        if ((sender as ListBox)?.SelectedItem is not ArticleSearchResult item)
            return;

        var owner = this.VisualRoot as Window;

        var artVm = new ArticleViewModel(item.Title,
            item.PageUrl,
            item.Summary);
        var artWin = new ArticleViewWindow { DataContext = artVm };
        await artWin.ShowDialog(owner!);
    }
}