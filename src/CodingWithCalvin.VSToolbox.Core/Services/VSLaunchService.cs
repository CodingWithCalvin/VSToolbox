using System.Diagnostics;
using CodingWithCalvin.VSToolbox.Core.Models;

namespace CodingWithCalvin.VSToolbox.Core.Services;

public sealed class VSLaunchService : IVSLaunchService
{
    public void LaunchInstance(VisualStudioInstance instance, string? rootSuffix = null)
    {
        // For Build Tools (no ProductPath), open Explorer to the installation folder
        if (string.IsNullOrEmpty(instance.ProductPath))
        {
            if (Directory.Exists(instance.InstallationPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"\"{instance.InstallationPath}\"",
                    UseShellExecute = true
                });
                return;
            }
            throw new InvalidOperationException($"Installation path not found: {instance.InstallationPath}");
        }

        if (!File.Exists(instance.ProductPath))
        {
            throw new FileNotFoundException($"Visual Studio executable not found: {instance.ProductPath}");
        }

        var arguments = BuildRootSuffixArguments(rootSuffix);

        Process.Start(new ProcessStartInfo
        {
            FileName = instance.ProductPath,
            Arguments = arguments,
            UseShellExecute = true
        });
    }

    public void LaunchInstanceWithSolution(VisualStudioInstance instance, string solutionPath, string? rootSuffix = null)
    {
        if (string.IsNullOrEmpty(instance.ProductPath))
        {
            throw new InvalidOperationException($"Cannot launch {instance.DisplayName} - no executable path (Build Tools cannot be launched directly)");
        }

        if (!File.Exists(instance.ProductPath))
        {
            throw new FileNotFoundException($"Visual Studio executable not found: {instance.ProductPath}");
        }

        if (!File.Exists(solutionPath))
        {
            throw new FileNotFoundException($"Solution file not found: {solutionPath}");
        }

        var rootSuffixArgs = BuildRootSuffixArguments(rootSuffix);
        var arguments = string.IsNullOrEmpty(rootSuffixArgs)
            ? $"\"{solutionPath}\""
            : $"\"{solutionPath}\" {rootSuffixArgs}";

        Process.Start(new ProcessStartInfo
        {
            FileName = instance.ProductPath,
            Arguments = arguments,
            UseShellExecute = true
        });
    }

    private static string BuildRootSuffixArguments(string? rootSuffix)
    {
        if (string.IsNullOrEmpty(rootSuffix))
        {
            return string.Empty;
        }
        return $"/rootSuffix {rootSuffix}";
    }
}
