using System.Diagnostics;
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
            instances.Add(CreateVSCodeInstance(vsCodePath, version, isInsiders: false));
        }

        var vsCodeInsidersPath = VSCodeInsidersPaths.FirstOrDefault(File.Exists);
        if (vsCodeInsidersPath is not null)
        {
            var version = GetFileVersion(vsCodeInsidersPath);
            instances.Add(CreateVSCodeInstance(vsCodeInsidersPath, version, isInsiders: true));
        }

        return Task.FromResult<IReadOnlyList<VisualStudioInstance>>(instances);
    }

    private static VisualStudioInstance CreateVSCodeInstance(string executablePath, string version, bool isInsiders)
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
            InstalledWorkloads = []
        };
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
