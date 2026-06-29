using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AutoHdrManager.Helpers;

public static class AppInfoHelper
{
    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SHGetFileInfo(
        string pszPath, uint dwFileAttributes,
        ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    }

    private const uint SHGFI_ICON = 0x000000100;
    private const uint SHGFI_SMALLICON = 0x000000001;
    private const uint SHGFI_LARGEICON = 0x000000000;

    public static string GetDisplayName(string exePath)
    {
        if (!File.Exists(exePath))
            return CleanExeName(Path.GetFileName(exePath));

        try
        {
            var info = FileVersionInfo.GetVersionInfo(exePath);

            if (!string.IsNullOrWhiteSpace(info.FileDescription))
                return info.FileDescription;

            if (!string.IsNullOrWhiteSpace(info.ProductName))
                return info.ProductName;
        }
        catch
        {
            // Fall through to filename-based name
        }

        return CleanExeName(Path.GetFileName(exePath));
    }

    public static ImageSource? GetIconSource(string exePath)
    {
        if (!File.Exists(exePath))
            return null;

        try
        {
            var shinfo = new SHFILEINFO();
            var result = SHGetFileInfo(
                exePath, 0, ref shinfo,
                (uint)Marshal.SizeOf(typeof(SHFILEINFO)),
                SHGFI_ICON | SHGFI_LARGEICON);

            if (result == IntPtr.Zero || shinfo.hIcon == IntPtr.Zero)
                return null;

            var source = Imaging.CreateBitmapSourceFromHIcon(
                shinfo.hIcon,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            source.Freeze();
            DestroyIcon(shinfo.hIcon);
            return source;
        }
        catch
        {
            return null;
        }
    }

    private static string CleanExeName(string fileName)
    {
        var name = Path.GetFileNameWithoutExtension(fileName);
        if (string.IsNullOrEmpty(name))
            return fileName;

        // "chrome" -> "Chrome", "my_game" -> "My Game", "MyGame" -> "My Game"
        var result = new System.Text.StringBuilder();
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (c == '_' || c == '-')
            {
                result.Append(' ');
                continue;
            }

            if (i > 0 && char.IsUpper(c) && char.IsLower(name[i - 1]))
                result.Append(' ');

            result.Append(i == 0 || (i > 0 && result[^1] == ' ') ? char.ToUpper(c) : c);
        }

        return result.ToString();
    }
}
