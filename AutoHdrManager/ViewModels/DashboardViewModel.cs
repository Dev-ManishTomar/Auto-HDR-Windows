using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AutoHdrManager.Models;
using AutoHdrManager.Services;

namespace AutoHdrManager.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly RegistryService _registry = new();

    [ObservableProperty]
    private ObservableCollection<HdrEntry> _entries = new();

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private string _statusMessage = "Click Refresh to scan the registry.";

    [ObservableProperty]
    private bool _isEmpty = true;

    [RelayCommand]
    private async Task ScanAsync()
    {
        IsScanning = true;
        StatusMessage = "Scanning registry...";

        try
        {
            var scanned = await Task.Run(() => _registry.ScanAllEntries());

            Application.Current.Dispatcher.Invoke(() =>
            {
                Entries.Clear();
                foreach (var entry in scanned)
                    Entries.Add(entry);

                IsEmpty = Entries.Count == 0;
                StatusMessage = Entries.Count == 0
                    ? "No Auto HDR entries found in the registry."
                    : $"Found {Entries.Count} application(s) with Direct3D overrides.";
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Scan failed: {ex.Message}";
        }
        finally
        {
            IsScanning = false;
        }
    }

    [RelayCommand]
    private void ToggleHdr(HdrEntry? entry)
    {
        if (entry is null) return;

        try
        {
            if (entry.IsAutoHdrEnabled)
            {
                _registry.RemoveAutoHdr(entry.RegistryKeyPath);
                entry.IsAutoHdrEnabled = false;
                entry.Is10BitEnabled = false;
                entry.D3DBehaviors = string.Empty;
                StatusMessage = $"Auto HDR disabled for {entry.ExeName}.";
            }
            else
            {
                _registry.ApplyAutoHdr(entry.FullPath, false);
                entry.IsAutoHdrEnabled = true;
                entry.Is10BitEnabled = false;
                entry.D3DBehaviors = "BufferUpgradeOverride=1";
                StatusMessage = $"Auto HDR enabled for {entry.ExeName}.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to update {entry.ExeName}: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Toggle10Bit(HdrEntry? entry)
    {
        if (entry is null || !entry.IsAutoHdrEnabled) return;

        try
        {
            var new10Bit = !entry.Is10BitEnabled;
            _registry.ApplyAutoHdr(entry.FullPath, new10Bit);
            entry.Is10BitEnabled = new10Bit;
            entry.D3DBehaviors = new10Bit
                ? "BufferUpgradeOverride=1;BufferUpgradeEnable10Bit=1"
                : "BufferUpgradeOverride=1";
            StatusMessage = new10Bit
                ? $"10-bit enabled for {entry.ExeName}."
                : $"10-bit disabled for {entry.ExeName}.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to update {entry.ExeName}: {ex.Message}";
        }
    }

    [RelayCommand]
    private void OpenFileLocation(HdrEntry? entry)
    {
        if (entry is null) return;

        try
        {
            if (File.Exists(entry.FullPath))
            {
                Process.Start("explorer.exe", $"/select,\"{entry.FullPath}\"");
            }
            else
            {
                StatusMessage = $"File not found: {entry.FullPath}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not open location: {ex.Message}";
        }
    }

    [RelayCommand]
    private void RemoveEntry(HdrEntry? entry)
    {
        if (entry is null) return;

        var result = MessageBox.Show(
            $"Remove all registry entries for {entry.ExeName}?\n\nThis will delete the Application subkey entirely.",
            "Confirm Removal",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            _registry.RemoveEntry(entry.RegistryKeyPath);
            Entries.Remove(entry);
            IsEmpty = Entries.Count == 0;
            StatusMessage = $"Removed {entry.ExeName} from registry.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to remove {entry.ExeName}: {ex.Message}";
        }
    }
}
