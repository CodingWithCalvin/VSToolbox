using CodingWithCalvin.VSToolbox.Core.Models;

namespace CodingWithCalvin.VSToolbox.Core.Services;

public sealed class VSHiveService : IVSHiveService
{
    private static readonly string VisualStudioAppDataPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Microsoft",
        "VisualStudio");

    public IReadOnlyList<VisualStudioHive> GetHivesForInstance(VisualStudioInstance instance)
    {
        var hives = new List<VisualStudioHive>();
        var majorVersion = GetMajorVersion(instance.InstallationVersion);
        var baseFolderName = $"{majorVersion}.0_{instance.InstanceId}";

        try
        {
            if (!Directory.Exists(VisualStudioAppDataPath))
            {
                return hives;
            }

            var directories = Directory.GetDirectories(VisualStudioAppDataPath);
            foreach (var directory in directories)
            {
                var folderName = Path.GetFileName(directory);
                if (!folderName.StartsWith(baseFolderName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var suffix = folderName.Length > baseFolderName.Length
                    ? folderName[baseFolderName.Length..]
                    : string.Empty;

                var hive = new VisualStudioHive
                {
                    Name = folderName,
                    RootSuffix = suffix,
                    DataPath = directory
                };

                hives.Add(hive);
            }
        }
        catch
        {
            // Ignore file system access errors
        }

        // Sort: Default first, then Experimental, then custom alphabetically
        return hives
            .OrderBy(h => h.IsDefault ? 0 : h.IsExperimental ? 1 : 2)
            .ThenBy(h => h.RootSuffix)
            .ToList();
    }

    private static int GetMajorVersion(string version)
    {
        if (Version.TryParse(version, out var parsed))
        {
            return parsed.Major;
        }
        return 0;
    }
}
