namespace CodingWithCalvin.VSToolbox.Core.Models;

public sealed class LaunchableInstance
{
    public required VisualStudioInstance Instance { get; init; }
    public VisualStudioHive? Hive { get; init; }

    public bool IsDefaultHive => Hive is null || Hive.IsDefault;
    public string? RootSuffix => Hive?.RootSuffix;

    public string DisplayName => IsDefaultHive
        ? Instance.ShortDisplayName
        : $"{Instance.ShortDisplayName} ({Hive!.DisplayName})";

    public string BuildNumber => Instance.BuildNumber;
    public string InstallationPath => Instance.InstallationPath;
    public bool IsPrerelease => Instance.IsPrerelease;
    public string ChannelType => Instance.ChannelType;
    public string? IconPath => Instance.IconPath;
    public bool CanLaunch => Instance.CanLaunch;

    public string ActionTooltip => CanLaunch ? "Launch" : "Open folder";
}
