using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoHdrManager.Models;

public partial class HdrEntry : ObservableObject
{
    [ObservableProperty]
    private string _exeName = string.Empty;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _fullPath = string.Empty;

    [ObservableProperty]
    private string _registryKeyPath = string.Empty;

    [ObservableProperty]
    private string _d3DBehaviors = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusSummary))]
    private bool _isAutoHdrEnabled;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusSummary))]
    private bool _is10BitEnabled;

    [ObservableProperty]
    private ImageSource? _iconSource;

    [ObservableProperty]
    private bool _fileExists;

    public string StatusSummary => IsAutoHdrEnabled
        ? Is10BitEnabled ? "Auto HDR + 10-bit" : "Auto HDR"
        : "Inactive";
}
