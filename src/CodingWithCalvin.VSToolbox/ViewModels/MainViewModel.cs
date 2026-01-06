using System.Collections.ObjectModel;
using CodingWithCalvin.VSToolbox.Core.Models;
using CodingWithCalvin.VSToolbox.Core.Services;
using CodingWithCalvin.VSToolbox.Services;

namespace CodingWithCalvin.VSToolbox.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    private readonly IVSDetectionService _detectionService;
    private readonly IVSLaunchService _launchService;
    private readonly IVSHiveService _hiveService;
    private readonly IconExtractionService _iconService;
    private readonly WindowsTerminalService _terminalService;
    private readonly IVSCodeDetectionService _vsCodeDetectionService;

    public MainViewModel() : this(new VSDetectionService(), new VSLaunchService(), new VSHiveService(), new IconExtractionService(), new WindowsTerminalService(), new VSCodeDetectionService())
    {
    }

    public MainViewModel(IVSDetectionService detectionService, IVSLaunchService launchService, IVSHiveService hiveService, IconExtractionService iconService, WindowsTerminalService terminalService, IVSCodeDetectionService vsCodeDetectionService)
    {
        _detectionService = detectionService;
        _launchService = launchService;
        _hiveService = hiveService;
        _iconService = iconService;
        _terminalService = terminalService;
        _vsCodeDetectionService = vsCodeDetectionService;
        Title = "VSToolbox";
        StatusText = "Loading...";
    }

    [ObservableProperty]
    public partial string StatusText { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    public ObservableCollection<LaunchableInstance> Instances { get; } = [];

    public IReadOnlyList<TerminalProfile> TerminalProfiles => _terminalService.GetProfiles();

    public bool HasTerminalProfiles => TerminalProfiles.Count > 0;

    [RelayCommand]
    private async Task LoadInstancesAsync()
    {
        IsLoading = true;
        StatusText = "Scanning for Visual Studio and VS Code installations...";

        try
        {
            var allInstances = new List<VisualStudioInstance>();

            if (_detectionService.IsVSWhereAvailable())
            {
                var vsInstances = await _detectionService.GetInstalledInstancesAsync();
                allInstances.AddRange(vsInstances);
            }

            var vsCodeInstances = await _vsCodeDetectionService.GetInstalledInstancesAsync();
            allInstances.AddRange(vsCodeInstances);

            _iconService.ExtractAndCacheIcons(allInstances);

            var launchables = new List<LaunchableInstance>();
            foreach (var instance in allInstances)
            {
                if (instance.Version == VSVersion.VSCode)
                {
                    launchables.Add(new LaunchableInstance { Instance = instance });
                    continue;
                }

                var hives = _hiveService.GetHivesForInstance(instance);

                launchables.Add(new LaunchableInstance { Instance = instance });

                foreach (var hive in hives.Where(h => !h.IsDefault))
                {
                    launchables.Add(new LaunchableInstance { Instance = instance, Hive = hive });
                }
            }

            Instances.Clear();
            foreach (var launchable in launchables)
            {
                Instances.Add(launchable);
            }

            var totalVS = allInstances.Count(i => i.Version != VSVersion.VSCode);
            var totalVSCode = allInstances.Count(i => i.Version == VSVersion.VSCode);

            StatusText = totalVSCode > 0
                ? $"{totalVS} Visual Studio + {totalVSCode} VS Code instance{(totalVSCode != 1 ? "s" : "")} found."
                : launchables.Count switch
                {
                    0 => "No Visual Studio instances found.",
                    1 => "1 Visual Studio instance found.",
                    _ => $"{launchables.Count} Visual Studio instances found."
                };
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void LaunchInstance(LaunchableInstance? launchable)
    {
        if (launchable is null) return;

        try
        {
            _launchService.LaunchInstance(launchable.Instance, launchable.RootSuffix);
        }
        catch (Exception ex)
        {
            StatusText = $"Failed to launch: {ex.Message}";
        }
    }

    [RelayCommand]
    private void OpenInstanceFolder(LaunchableInstance? launchable)
    {
        if (launchable is null) return;

        try
        {
            var path = launchable.Instance.InstallationPath;
            if (Directory.Exists(path))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"\"{path}\"",
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Failed to open folder: {ex.Message}";
        }
    }

    [RelayCommand]
    private void LaunchVsCmd(LaunchableInstance? launchable)
    {
        if (launchable is null) return;

        try
        {
            var vsDevCmdPath = Path.Combine(launchable.Instance.InstallationPath, "Common7", "Tools", "VsDevCmd.bat");
            if (File.Exists(vsDevCmdPath))
            {
                // Set VSINSTALLDIR so VsDevCmd.bat doesn't need to use vswhere
                var installDir = launchable.Instance.InstallationPath;
                if (!installDir.EndsWith("\\")) installDir += "\\";

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/k \"set \"VSINSTALLDIR={installDir}\" && call \"{vsDevCmdPath}\"\"",
                    UseShellExecute = true
                });
            }
            else
            {
                StatusText = "VsDevCmd.bat not found";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Failed to launch CMD: {ex.Message}";
        }
    }

    [RelayCommand]
    private void LaunchVsPwsh(LaunchableInstance? launchable)
    {
        if (launchable is null) return;

        try
        {
            var launchVsDevShellPath = Path.Combine(launchable.Instance.InstallationPath, "Common7", "Tools", "Launch-VsDevShell.ps1");
            if (File.Exists(launchVsDevShellPath))
            {
                // Use -VsInstallPath parameter so it doesn't need to use vswhere
                var installDir = launchable.Instance.InstallationPath;

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "pwsh.exe",
                    Arguments = $"-NoExit -Command \"& '{launchVsDevShellPath}' -VsInstallPath '{installDir}'\"",
                    UseShellExecute = true
                });
            }
            else
            {
                StatusText = "Launch-VsDevShell.ps1 not found";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Failed to launch PowerShell: {ex.Message}";
        }
    }

    [RelayCommand]
    private void OpenAppDataFolder(LaunchableInstance? launchable)
    {
        if (launchable is null) return;

        try
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Microsoft",
                "VisualStudio");

            // Build the hive folder name
            var majorVersion = Version.Parse(launchable.Instance.InstallationVersion).Major;
            var hiveName = $"{majorVersion}.0_{launchable.Instance.InstanceId}";
            if (!string.IsNullOrEmpty(launchable.RootSuffix))
            {
                hiveName += launchable.RootSuffix;
            }

            var hivePath = Path.Combine(appDataPath, hiveName);
            if (Directory.Exists(hivePath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"\"{hivePath}\"",
                    UseShellExecute = true
                });
            }
            else
            {
                StatusText = $"AppData folder not found: {hiveName}";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Failed to open AppData: {ex.Message}";
        }
    }

    public void LaunchWithTerminalProfile(LaunchableInstance launchable, TerminalProfile profile)
    {
        try
        {
            string scriptPath;
            string shellArgs;
            var installDir = launchable.Instance.InstallationPath;

            if (profile.ShellType == ShellType.Cmd)
            {
                scriptPath = Path.Combine(installDir, "Common7", "Tools", "VsDevCmd.bat");
                if (!File.Exists(scriptPath))
                {
                    StatusText = "VsDevCmd.bat not found";
                    return;
                }
                // Set VSINSTALLDIR so VsDevCmd.bat doesn't need to use vswhere
                if (!installDir.EndsWith("\\")) installDir += "\\";
                shellArgs = $"cmd /k \"set \"\"VSINSTALLDIR={installDir}\"\" && call \"\"{scriptPath}\"\"\"";
            }
            else
            {
                scriptPath = Path.Combine(launchable.Instance.InstallationPath, "Common7", "Tools", "Launch-VsDevShell.ps1");
                if (!File.Exists(scriptPath))
                {
                    StatusText = "Launch-VsDevShell.ps1 not found";
                    return;
                }
                // Use -VsInstallPath parameter so it doesn't need to use vswhere
                shellArgs = $"pwsh -NoExit -Command \"& '{scriptPath}' -VsInstallPath '{installDir}'\"";
            }

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "wt.exe",
                Arguments = $"-p \"{profile.Name}\" {shellArgs}",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            StatusText = $"Failed to launch terminal: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadInstancesAsync();
    }
}
