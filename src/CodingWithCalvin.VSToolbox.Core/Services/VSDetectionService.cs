using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using CodingWithCalvin.VSToolbox.Core.Models;

namespace CodingWithCalvin.VSToolbox.Core.Services;

public sealed class VSDetectionService : IVSDetectionService
{
    private static readonly string VSWherePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
        "Microsoft Visual Studio",
        "Installer",
        "vswhere.exe");

    public bool IsVSWhereAvailable() => File.Exists(VSWherePath);

    public async Task<IReadOnlyList<VisualStudioInstance>> GetInstalledInstancesAsync(CancellationToken cancellationToken = default)
    {
        if (!IsVSWhereAvailable())
        {
            return [];
        }

        var json = await RunVSWhereAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        var vsWhereResults = JsonSerializer.Deserialize<List<VSWhereResult>>(json, JsonOptions);
        if (vsWhereResults is null)
        {
            return [];
        }

        return vsWhereResults
            .Where(r => IsSupportedVersion(r))
            .Select(MapToInstance)
            .ToList();
    }

    private static async Task<string> RunVSWhereAsync(CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = VSWherePath,
            Arguments = "-all -prerelease -products \"*\" -format json -utf8",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        return output;
    }

    private static bool IsSupportedVersion(VSWhereResult result)
    {
        if (!Version.TryParse(result.InstallationVersion, out var version))
        {
            return false;
        }

        // Support VS 2019 (16.x), 2022 (17.x), and 2026 (18.x)
        return version.Major is >= 16 and <= 18;
    }

    private static VisualStudioInstance MapToInstance(VSWhereResult result)
    {
        var version = Version.Parse(result.InstallationVersion);
        var vsVersion = version.Major switch
        {
            16 => VSVersion.VS2019,
            17 => VSVersion.VS2022,
            18 => VSVersion.VS2026,
            _ => VSVersion.VS2022 // Fallback
        };

        var sku = ParseSku(result.ProductId);

        return new VisualStudioInstance
        {
            InstanceId = result.InstanceId,
            InstallationPath = result.InstallationPath,
            InstallationVersion = result.InstallationVersion,
            DisplayName = result.DisplayName,
            ProductPath = result.ProductPath,
            Version = vsVersion,
            Sku = sku,
            IsPrerelease = result.IsPrerelease,
            InstallDate = result.InstallDate,
            ChannelId = result.ChannelId ?? string.Empty,
            InstalledWorkloads = [] // TODO: Get workloads with additional vswhere call
        };
    }

    private static VSSku ParseSku(string productId)
    {
        if (string.IsNullOrEmpty(productId))
            return VSSku.Unknown;

        if (productId.Contains("Community", StringComparison.OrdinalIgnoreCase))
            return VSSku.Community;
        if (productId.Contains("Professional", StringComparison.OrdinalIgnoreCase))
            return VSSku.Professional;
        if (productId.Contains("Enterprise", StringComparison.OrdinalIgnoreCase))
            return VSSku.Enterprise;
        if (productId.Contains("BuildTools", StringComparison.OrdinalIgnoreCase))
            return VSSku.BuildTools;

        return VSSku.Unknown;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private sealed class VSWhereResult
    {
        public string InstanceId { get; set; } = string.Empty;
        public string InstallationPath { get; set; } = string.Empty;
        public string InstallationVersion { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string ProductPath { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public bool IsPrerelease { get; set; }
        public DateTimeOffset InstallDate { get; set; }
        public string? ChannelId { get; set; }
    }
}
