using System.Drawing;
using System.Drawing.Imaging;
using CodingWithCalvin.VSToolbox.Core.Models;

namespace CodingWithCalvin.VSToolbox.Services;

public sealed class IconExtractionService
{
    private static readonly string CacheDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "VSToolbox",
        "IconCache");

    public void ExtractAndCacheIcons(IEnumerable<VisualStudioInstance> instances)
    {
        Directory.CreateDirectory(CacheDirectory);

        foreach (var instance in instances)
        {
            instance.IconPath = GetOrExtractIcon(instance);
        }
    }

    private static string? GetOrExtractIcon(VisualStudioInstance instance)
    {
        var cacheFileName = $"{instance.InstanceId}.png";
        var cachePath = Path.Combine(CacheDirectory, cacheFileName);

        if (File.Exists(cachePath))
        {
            return cachePath;
        }

        // Try to extract from ProductPath (devenv.exe)
        if (!string.IsNullOrEmpty(instance.ProductPath) && File.Exists(instance.ProductPath))
        {
            if (TryExtractIcon(instance.ProductPath, cachePath))
            {
                return cachePath;
            }
        }

        // For Build Tools or when ProductPath extraction fails, try common VS executables
        var alternativePaths = new[]
        {
            Path.Combine(instance.InstallationPath, "Common7", "IDE", "devenv.exe"),
            Path.Combine(instance.InstallationPath, "MSBuild", "Current", "Bin", "MSBuild.exe"),
            Path.Combine(instance.InstallationPath, "Common7", "Tools", "VsDevCmd.bat")
        };

        foreach (var altPath in alternativePaths)
        {
            if (File.Exists(altPath) && TryExtractIcon(altPath, cachePath))
            {
                return cachePath;
            }
        }

        return null;
    }

    private static bool TryExtractIcon(string exePath, string outputPath)
    {
        try
        {
            using var icon = Icon.ExtractAssociatedIcon(exePath);
            if (icon is null)
            {
                return false;
            }

            using var bitmap = icon.ToBitmap();
            bitmap.Save(outputPath, ImageFormat.Png);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
