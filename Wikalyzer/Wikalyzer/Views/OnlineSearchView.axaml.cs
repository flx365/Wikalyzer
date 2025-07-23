// Views/OnlineSearchView.axaml.cs
using Avalonia.Controls;
using Avalonia.Interactivity;
using Wikalyzer.Models;
using System;

namespace Wikalyzer.Views;

public partial class OnlineSearchView : UserControl
{
    public OnlineSearchView()
    {
        InitializeComponent();
    }

    private void OnResultDoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (sender is ListBox lb && lb.SelectedItem is ArticleSearchResult article)
        {
            var url = $"https://de.wikipedia.org/wiki/{Uri.EscapeDataString(article.Title)}";
            var window = new ArticleViewWindow(article.Title, url, article.Summary);
            window.Show();
        }
    }
}