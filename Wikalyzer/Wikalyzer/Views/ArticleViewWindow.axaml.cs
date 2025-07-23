// Views/ArticleViewWindow.axaml.cs
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Wikalyzer.ViewModels;

namespace Wikalyzer.Views
{
    public partial class ArticleViewWindow : Window
    {
        // 1) Parameterloser Konstruktor für XAML-Instanziierung
        public ArticleViewWindow()
        {
            InitializeComponent();
        }

        // 2) Zusätzlicher Convenience-Konstruktor, der direkt den VM setzt
        public ArticleViewWindow(string title, string articleUrl, string summary)
            : this()
        {
            DataContext = new ArticleViewModel(title, articleUrl, summary);
        }

        private void InitializeComponent()
            => AvaloniaXamlLoader.Load(this);
    }
}