using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Wikalyzer.Views;

public partial class OnlineSearchView : UserControl
{
    public OnlineSearchView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}