using System.Windows.Controls;
using AutoHdrManager.ViewModels;

namespace AutoHdrManager.Views;

public partial class DashboardPage : Page
{
    private readonly DashboardViewModel _viewModel = new();

    public DashboardPage()
    {
        DataContext = _viewModel;
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (_viewModel.Entries.Count == 0)
            await _viewModel.ScanCommand.ExecuteAsync(null);
    }
}
