using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AutoHdrManager.Services;

namespace AutoHdrManager.ViewModels;

public partial class ApplyViewModel : ObservableObject
{
    private readonly RegistryService _registry = new();

    [ObservableProperty]
    private string _exePath = string.Empty;

    [ObservableProperty]
    private bool _enableAutoHdr = true;

    [ObservableProperty]
    private bool _enable10Bit;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _hasStatus;

    [ObservableProperty]
    private bool _isSuccess;

    [ObservableProperty]
    private bool _existingEntryFound;

    [ObservableProperty]
    private string _existingInfo = string.Empty;

    partial void OnExePathChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        CheckExisting();
    }

    [RelayCommand]
    private void Browse()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Executables (*.exe)|*.exe|All Files (*.*)|*.*",
            Title = "Select Game Executable"
        };

        if (dialog.ShowDialog() == true)
        {
            ExePath = dialog.FileName;
        }
    }

    [RelayCommand]
    private void Apply()
    {
        if (string.IsNullOrWhiteSpace(ExePath))
        {
            SetStatus("Please select an executable first.", false);
            return;
        }

        if (!ExePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            SetStatus("Selected file must be an .exe executable.", false);
            return;
        }

        try
        {
            if (EnableAutoHdr)
            {
                _registry.ApplyAutoHdr(ExePath, Enable10Bit);
                var mode = Enable10Bit ? "Auto HDR + 10-bit" : "Auto HDR";
                SetStatus($"{mode} applied to {Path.GetFileName(ExePath)}!", true);
            }
            else
            {
                var existing = _registry.FindEntryByExe(ExePath);
                if (existing is not null)
                {
                    _registry.RemoveAutoHdr(existing.RegistryKeyPath);
                    SetStatus($"Auto HDR removed from {Path.GetFileName(ExePath)}.", true);
                }
                else
                {
                    SetStatus("No existing Auto HDR entry found for this executable.", false);
                }
            }

            CheckExisting();
        }
        catch (Exception ex)
        {
            SetStatus($"Operation failed: {ex.Message}", false);
        }
    }

    [RelayCommand]
    private void RemoveAll()
    {
        if (string.IsNullOrWhiteSpace(ExePath)) return;

        var existing = _registry.FindEntryByExe(ExePath);
        if (existing is null)
        {
            SetStatus("No registry entry found for this executable.", false);
            return;
        }

        try
        {
            _registry.RemoveEntry(existing.RegistryKeyPath);
            SetStatus($"All registry entries removed for {Path.GetFileName(ExePath)}.", true);
            CheckExisting();
        }
        catch (Exception ex)
        {
            SetStatus($"Removal failed: {ex.Message}", false);
        }
    }

    private void CheckExisting()
    {
        var entry = _registry.FindEntryByExe(ExePath);
        ExistingEntryFound = entry is not null;

        if (entry is not null)
        {
            EnableAutoHdr = entry.IsAutoHdrEnabled;
            Enable10Bit = entry.Is10BitEnabled;
            ExistingInfo = entry.IsAutoHdrEnabled
                ? $"Current: {entry.StatusSummary} (key: {Path.GetFileName(entry.RegistryKeyPath)})"
                : $"Entry exists but Auto HDR is not active (key: {Path.GetFileName(entry.RegistryKeyPath)})";
        }
        else
        {
            ExistingInfo = string.Empty;
        }
    }

    private void SetStatus(string message, bool success)
    {
        StatusMessage = message;
        IsSuccess = success;
        HasStatus = true;
    }
}
