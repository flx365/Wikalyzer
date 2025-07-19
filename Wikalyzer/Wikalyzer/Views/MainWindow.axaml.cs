using Avalonia.Controls;
using Wikalyzer.ViewModels;

namespace Wikalyzer.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}