using System.Diagnostics;
using System.Text.Json;
using CodingWithCalvin.VSToolbox.Core.Models;

namespace CodingWithCalvin.VSToolbox.Core.Services;

public sealed class VSCodeDetectionService : IVSCodeDetectionService
{
    private static readonly string[] VSCodePaths =
    [
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Microsoft VS Code", "Code.exe"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft VS Code", "Code.exe")
    ];

    private static readonly string[] VSCodeInsidersPaths =
    [
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Microsoft VS Code Insiders", "Code - Insiders.exe"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft VS Code Insiders", "Code - Insiders.exe")
    ];

    public Task<IReadOnlyList<VisualStudioInstance>> GetInstalledInstancesAsync(CancellationToken cancellationToken = default)
    {
        var instances = new List<VisualStudioInstance>();

        var vsCodePath = VSCodePaths.FirstOrDefault(File.Exists);
        if (vsCodePath is not null)
        {
            var version = GetFileVersion(vsCodePath);
            var extensions = GetInstalledExtensions(isInsiders: false);
            instances.Add(CreateVSCodeInstance(vsCodePath, version, extensions, isInsiders: false));
        }

        var vsCodeInsidersPath = VSCodeInsidersPaths.FirstOrDefault(File.Exists);
        if (vsCodeInsidersPath is not null)
        {
            var version = GetFileVersion(vsCodeInsidersPath);
            var extensions = GetInstalledExtensions(isInsiders: true);
            instances.Add(CreateVSCodeInstance(vsCodeInsidersPath, version, extensions, isInsiders: true));
        }

        return Task.FromResult<IReadOnlyList<VisualStudioInstance>>(instances);
    }

    private static VisualStudioInstance CreateVSCodeInstance(string executablePath, string version, IReadOnlyList<string> extensions, bool isInsiders)
    {
        var installPath = Path.GetDirectoryName(executablePath) ?? string.Empty;
        var displayName = isInsiders ? "Visual Studio Code - Insiders" : "Visual Studio Code";
        var instanceId = isInsiders ? "vscode-insiders" : "vscode";
        var sku = isInsiders ? VSSku.VSCodeInsiders : VSSku.VSCode;

        return new VisualStudioInstance
        {
            InstanceId = instanceId,
            InstallationPath = installPath,
            InstallationVersion = version,
            DisplayName = displayName,
            ProductPath = executablePath,
            Version = VSVersion.VSCode,
            Sku = sku,
            IsPrerelease = isInsiders,
            InstallDate = GetInstallDate(executablePath),
            ChannelId = isInsiders ? "VSCode.Insiders" : "VSCode.Stable",
            InstalledWorkloads = extensions
        };
    }

    private static IReadOnlyList<string> GetInstalledExtensions(bool isInsiders)
    {
        try
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var extensionsPath = isInsiders
                ? Path.Combine(userProfile, ".vscode-insiders", "extensions")
                : Path.Combine(userProfile, ".vscode", "extensions");

            if (!Directory.Exists(extensionsPath))
            {
                return [];
            }

            var extensions = new List<string>();
            var directories = Directory.GetDirectories(extensionsPath);

            foreach (var dir in directories)
            {
                var dirName = Path.GetFileName(dir);
                if (!string.IsNullOrEmpty(dirName) && !dirName.StartsWith('.'))
                {
                    var parts = dirName.Split('-');
                    if (parts.Length >= 2)
                    {
                        var extensionName = string.Join("-", parts.Take(parts.Length - 1));
                        if (!extensions.Contains(extensionName))
                        {
                            extensions.Add(extensionName);
                        }
                    }
                }
            }

            return extensions.OrderBy(e => e).ToList();
        }
        catch
        {
            return [];
        }
    }

    private static string GetFileVersion(string executablePath)
    {
        try
        {
            var fileInfo = FileVersionInfo.GetVersionInfo(executablePath);
            return fileInfo.ProductVersion ?? fileInfo.FileVersion ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    private static DateTimeOffset GetInstallDate(string executablePath)
    {
        try
        {
            var fileInfo = new FileInfo(executablePath);
            return fileInfo.CreationTime;
        }
        catch
        {
            return DateTimeOffset.Now;
        }
    }
}
