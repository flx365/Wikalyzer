// Views/ArticleViewWindow.axaml.cs
using Avalonia.Controls;
using Wikalyzer.ViewModels;


namespace Wikalyzer.Views;

public partial class ArticleViewWindow : Window
{
    public ArticleViewWindow(string title, string url, string summary)
    {
        InitializeComponent();
        DataContext = new ArticleViewModel(title, url, summary);
    }
} 