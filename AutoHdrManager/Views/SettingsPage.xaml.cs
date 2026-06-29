using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace AutoHdrManager.Views;

public partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();

        var current = ApplicationThemeManager.GetAppTheme();
        ThemeToggle.IsChecked = current == ApplicationTheme.Dark;
    }

    private void ThemeToggle_OnChecked(object sender, RoutedEventArgs e)
    {
        ApplicationThemeManager.Apply(ApplicationTheme.Dark, WindowBackdropType.Mica, updateAccent: true);
    }

    private void ThemeToggle_OnUnchecked(object sender, RoutedEventArgs e)
    {
        ApplicationThemeManager.Apply(ApplicationTheme.Light, WindowBackdropType.Mica, updateAccent: true);
    }
}
