using Wpf.Ui.Controls;
using AutoHdrManager.Services;

namespace AutoHdrManager;

public partial class MainWindow : FluentWindow
{
    public MainWindow()
    {
        InitializeComponent();

        RootNavigation.SetPageProviderService(new PageService());
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        RootNavigation.Navigate(typeof(Views.DashboardPage));
    }
}
