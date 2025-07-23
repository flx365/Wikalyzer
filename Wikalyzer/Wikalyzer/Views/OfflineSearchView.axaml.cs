using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Wikalyzer.ViewModels;

namespace Wikalyzer.Views;

public partial class OfflineSearchView : UserControl
{
    public OfflineSearchView()
    {
        InitializeComponent();
        DataContext = new OfflineSearchViewModel();
    }
}