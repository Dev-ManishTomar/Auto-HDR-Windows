using System.IO;
using AutoHdrManager.Helpers;
using AutoHdrManager.Models;
using Microsoft.Win32;

namespace AutoHdrManager.Services;

public class RegistryService
{
    private const string D3DKeyPath = @"SOFTWARE\Microsoft\Direct3D";
    private const string BehaviorHdr = "BufferUpgradeOverride=1";
    private const string BehaviorHdr10Bit = "BufferUpgradeOverride=1;BufferUpgradeEnable10Bit=1";

    public void EnsureD3DKeyExists()
    {
        using var key = Registry.CurrentUser.CreateSubKey(D3DKeyPath);
    }

    public List<HdrEntry> ScanAllEntries()
    {
        var entries = new List<HdrEntry>();

        using var d3dKey = Registry.CurrentUser.OpenSubKey(D3DKeyPath);
        if (d3dKey is null)
            return entries;

        foreach (var subKeyName in d3dKey.GetSubKeyNames())
        {
            if (!subKeyName.StartsWith("Application", StringComparison.OrdinalIgnoreCase))
                continue;
            if (subKeyName.Equals("MostRecentApplication", StringComparison.OrdinalIgnoreCase))
                continue;

            using var subKey = d3dKey.OpenSubKey(subKeyName);
            if (subKey is null)
                continue;

            var name = subKey.GetValue("Name") as string;
            if (string.IsNullOrWhiteSpace(name))
                continue;

            var behaviors = subKey.GetValue("D3DBehaviors") as string ?? string.Empty;

            var resolvedPath = name.Contains('\\') || name.Contains('/') ? name : name;
            var fileExists = File.Exists(resolvedPath);

            entries.Add(new HdrEntry
            {
                FullPath = name,
                ExeName = Path.GetFileName(name),
                DisplayName = AppInfoHelper.GetDisplayName(resolvedPath),
                IconSource = AppInfoHelper.GetIconSource(resolvedPath),
                FileExists = fileExists,
                RegistryKeyPath = $@"{D3DKeyPath}\{subKeyName}",
                D3DBehaviors = behaviors,
                IsAutoHdrEnabled = behaviors.Contains(BehaviorHdr, StringComparison.OrdinalIgnoreCase),
                Is10BitEnabled = behaviors.Contains("BufferUpgradeEnable10Bit=1", StringComparison.OrdinalIgnoreCase)
            });
        }

        return entries;
    }

    public HdrEntry? FindEntryByExe(string exePath)
    {
        var fileName = Path.GetFileName(exePath);
        return ScanAllEntries().FirstOrDefault(e =>
            string.Equals(e.FullPath, exePath, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(e.ExeName, fileName, StringComparison.OrdinalIgnoreCase));
    }

    public void ApplyAutoHdr(string exePath, bool enable10Bit)
    {
        EnsureD3DKeyExists();

        var existing = FindEntryByExe(exePath);
        string keyPath;

        if (existing is not null)
        {
            keyPath = existing.RegistryKeyPath;
        }
        else
        {
            var slot = FindFreeApplicationSlot();
            keyPath = $@"{D3DKeyPath}\{slot}";
            using var newKey = Registry.CurrentUser.CreateSubKey(keyPath);
            newKey?.SetValue("Name", exePath, RegistryValueKind.String);
        }

        var value = enable10Bit ? BehaviorHdr10Bit : BehaviorHdr;
        using var regKey = Registry.CurrentUser.CreateSubKey(keyPath);
        regKey?.SetValue("D3DBehaviors", value, RegistryValueKind.String);
    }

    public void RemoveAutoHdr(string registryKeyPath)
    {
        using var key = Registry.CurrentUser.OpenSubKey(registryKeyPath, writable: true);
        key?.DeleteValue("D3DBehaviors", throwOnMissingValue: false);
    }

    public void RemoveEntry(string registryKeyPath)
    {
        var subKeyName = registryKeyPath;
        Registry.CurrentUser.DeleteSubKeyTree(subKeyName, throwOnMissingSubKey: false);
    }

    private string FindFreeApplicationSlot()
    {
        using var d3dKey = Registry.CurrentUser.OpenSubKey(D3DKeyPath);
        var existing = new HashSet<string>(
            d3dKey?.GetSubKeyNames() ?? Array.Empty<string>(),
            StringComparer.OrdinalIgnoreCase);

        for (var i = 0; ; i++)
        {
            var name = $"Application{i}";
            if (!existing.Contains(name))
                return name;
        }
    }
}
