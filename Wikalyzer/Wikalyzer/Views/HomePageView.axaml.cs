// Views/HomePageView.axaml.cs
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Wikalyzer.Views;

public partial class HomePageView : UserControl
{
    public HomePageView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}