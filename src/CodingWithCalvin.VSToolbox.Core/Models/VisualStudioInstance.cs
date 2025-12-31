namespace CodingWithCalvin.VSToolbox.Core.Models;

public sealed class VisualStudioInstance
{
    public required string InstanceId { get; init; }
    public required string InstallationPath { get; init; }
    public required string InstallationVersion { get; init; }
    public required string DisplayName { get; init; }
    public string? ProductPath { get; init; }
    public required VSVersion Version { get; init; }
    public required VSSku Sku { get; init; }
    public required bool IsPrerelease { get; init; }
    public required DateTimeOffset InstallDate { get; init; }
    public required string ChannelId { get; init; }
    public IReadOnlyList<string> InstalledWorkloads { get; init; } = [];
    public IReadOnlyList<VisualStudioHive> Hives { get; set; } = [];

    public IReadOnlyList<VisualStudioHive> AdditionalHives => Hives.Where(h => !h.IsDefault).ToList();

    public string? IconPath { get; set; }

    public string BuildNumber => InstallationVersion;

    public string ChannelType => ParseChannelType(ChannelId);

    private static string ParseChannelType(string channelId)
    {
        // ChannelId format: VisualStudio.{majorVersion}.{channel}
        // e.g., VisualStudio.17.Release, VisualStudio.17.Preview, VisualStudio.17.Canary
        var parts = channelId.Split('.');
        if (parts.Length < 3)
            return "Unknown";

        return parts[^1] switch
        {
            "Release" => "Stable",
            "Preview" => "Preview",
            "Canary" => "Canary",
            "IntPreview" => "Internal Preview",
            _ => parts[^1]
        };
    }

    public bool CanLaunch => !string.IsNullOrEmpty(ProductPath) &&
        ProductPath.EndsWith("devenv.exe", StringComparison.OrdinalIgnoreCase);

    public string ShortDisplayName => $"Visual Studio {GetVersionYear()} {Sku}";

    public string VersionYear => GetVersionYear();

    private string GetVersionYear() => Version switch
    {
        VSVersion.VS2019 => "2019",
        VSVersion.VS2022 => "2022",
        VSVersion.VS2026 => "2026",
        _ => "Unknown"
    };
}
