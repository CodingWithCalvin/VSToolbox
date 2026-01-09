using System.Text.Json;
using System.Text.RegularExpressions;
using CodingWithCalvin.VSToolbox.Core.Models;
using Microsoft.Win32;

namespace CodingWithCalvin.VSToolbox.Core.Services;

public sealed partial class RecentProjectsService : IRecentProjectsService
{
    private static readonly string VisualStudioAppDataPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Microsoft",
        "VisualStudio");

    public IReadOnlyList<RecentProject> GetRecentProjects(VisualStudioInstance instance, int maxCount = 10)
    {
        if (instance.Version == VSVersion.VSCode)
        {
            return GetVSCodeRecentProjects(instance, maxCount);
        }

        return GetVisualStudioRecentProjects(instance, maxCount);
    }

    private IReadOnlyList<RecentProject> GetVisualStudioRecentProjects(VisualStudioInstance instance, int maxCount)
    {
        var recentProjects = new List<RecentProject>();

        try
        {
            var majorVersion = GetMajorVersion(instance.InstallationVersion);
            
            // Search all VS hive folders matching the major version (e.g., 17.0*)
            // This matches the pattern: {majorVersion}.0* (e.g., 17.0_xxxxxxxx)
            if (Directory.Exists(VisualStudioAppDataPath))
            {
                var hiveFolders = Directory.GetDirectories(VisualStudioAppDataPath, $"{majorVersion}.0*");
                
                foreach (var hivePath in hiveFolders)
                {
                    var privateSettingsPath = Path.Combine(hivePath, "ApplicationPrivateSettings.xml");
                    if (File.Exists(privateSettingsPath))
                    {
                        var projectsFromSettings = ParseApplicationPrivateSettingsXml(privateSettingsPath);
                        recentProjects.AddRange(projectsFromSettings);
                    }
                }
            }
        }
        catch
        {
            // Ignore errors reading recent projects
        }

        // Remove duplicates and sort by last accessed
        return recentProjects
            .GroupBy(p => p.Path.ToLowerInvariant())
            .Select(g => g.OrderByDescending(p => p.LastAccessed).First())
            .Where(p => p.Exists)
            .OrderByDescending(p => p.LastAccessed)
            .Take(maxCount)
            .ToList();
    }

    private static IEnumerable<RecentProject> ParseApplicationPrivateSettingsXml(string settingsPath)
    {
        var projects = new List<RecentProject>();

        try
        {
            var xmlContent = File.ReadAllText(settingsPath);
            var doc = System.Xml.Linq.XDocument.Parse(xmlContent);

            // Find CodeContainers.Offline collection
            var codeContainersNode = doc.Descendants("collection")
                .FirstOrDefault(c => c.Attribute("name")?.Value == "CodeContainers.Offline");

            if (codeContainersNode is null)
                return projects;

            // Get the value element
            var valueNode = codeContainersNode.Elements("value")
                .FirstOrDefault(v => v.Attribute("name")?.Value == "value");

            if (valueNode is null)
                return projects;

            var jsonContent = valueNode.Value?.Trim();
            if (string.IsNullOrEmpty(jsonContent))
                return projects;

            // Parse JSON to extract projects
            projects.AddRange(ParseCodeContainersJsonFromXml(jsonContent));
        }
        catch
        {
            // Ignore parsing errors
        }

        return projects;
    }

    private static IEnumerable<RecentProject> ParseCodeContainersJsonFromXml(string jsonContent)
    {
        var projects = new List<RecentProject>();

        try
        {
            using var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Array)
                return projects;

            foreach (var item in root.EnumerateArray())
            {
                string? fullPath = null;
                DateTimeOffset lastAccessed = DateTimeOffset.MinValue;

                // Get path from Value.LocalProperties.FullPath
                if (item.TryGetProperty("Value", out var valueElement))
                {
                    if (valueElement.TryGetProperty("LocalProperties", out var localProps))
                    {
                        if (localProps.TryGetProperty("FullPath", out var fullPathElement))
                        {
                            fullPath = fullPathElement.GetString();
                        }
                    }

                    // Get LastAccessed timestamp
                    if (valueElement.TryGetProperty("LastAccessed", out var lastAccessedElement))
                    {
                        if (lastAccessedElement.TryGetDateTimeOffset(out var parsed))
                        {
                            lastAccessed = parsed;
                        }
                        else if (lastAccessedElement.ValueKind == JsonValueKind.String)
                        {
                            var dateStr = lastAccessedElement.GetString();
                            if (!string.IsNullOrEmpty(dateStr) && DateTimeOffset.TryParse(dateStr, out var parsedStr))
                            {
                                lastAccessed = parsedStr;
                            }
                        }
                    }
                }

                // Fallback: try Key property
                if (string.IsNullOrEmpty(fullPath) && item.TryGetProperty("Key", out var keyElement))
                {
                    fullPath = keyElement.GetString();
                }

                if (!string.IsNullOrEmpty(fullPath))
                {
                    // Normalize path (replace double backslashes)
                    fullPath = fullPath.Replace("\\\\", "\\");

                    // Only include solutions and projects
                    var isSolutionOrProject = 
                        fullPath.EndsWith(".sln", StringComparison.OrdinalIgnoreCase) ||
                        fullPath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) ||
                        fullPath.EndsWith(".vbproj", StringComparison.OrdinalIgnoreCase) ||
                        fullPath.EndsWith(".fsproj", StringComparison.OrdinalIgnoreCase) ||
                        fullPath.EndsWith(".vcxproj", StringComparison.OrdinalIgnoreCase);

                    if (isSolutionOrProject && File.Exists(fullPath))
                    {
                        projects.Add(new RecentProject
                        {
                            Name = Path.GetFileName(fullPath),
                            Path = fullPath,
                            LastAccessed = lastAccessed != DateTimeOffset.MinValue 
                                ? lastAccessed 
                                : GetFileLastAccess(fullPath)
                        });
                    }
                }
            }
        }
        catch
        {
            // Ignore JSON parsing errors
        }

        return projects;
    }

    private IReadOnlyList<RecentProject> GetVSCodeRecentProjects(VisualStudioInstance instance, int maxCount)
    {
        var projects = new List<RecentProject>();

        try
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var codeFolderName = instance.Sku == VSSku.VSCodeInsiders ? "Code - Insiders" : "Code";

            var storagePath = Path.Combine(appDataPath, codeFolderName, "User", "globalStorage", "storage.json");

            if (File.Exists(storagePath))
            {
                var json = File.ReadAllText(storagePath);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("openedPathsList", out var pathsList))
                {
                    // workspaces3
                    if (pathsList.TryGetProperty("workspaces3", out var workspaces))
                    {
                        foreach (var workspace in workspaces.EnumerateArray())
                        {
                            var path = workspace.GetString();
                            if (!string.IsNullOrEmpty(path))
                            {
                                path = CleanVSCodePath(path);
                                if (Directory.Exists(path) || File.Exists(path))
                                {
                                    projects.Add(new RecentProject
                                    {
                                        Name = Path.GetFileName(path.TrimEnd('/', '\\')),
                                        Path = path,
                                        LastAccessed = GetFileLastAccess(path)
                                    });
                                }
                            }
                        }
                    }

                    // folders3
                    if (pathsList.TryGetProperty("folders3", out var folders))
                    {
                        foreach (var folder in folders.EnumerateArray())
                        {
                            var path = folder.GetString();
                            if (!string.IsNullOrEmpty(path))
                            {
                                path = CleanVSCodePath(path);
                                if (Directory.Exists(path))
                                {
                                    projects.Add(new RecentProject
                                    {
                                        Name = Path.GetFileName(path.TrimEnd('/', '\\')),
                                        Path = path,
                                        LastAccessed = GetFileLastAccess(path)
                                    });
                                }
                            }
                        }
                    }

                    // entries (newer format)
                    if (pathsList.TryGetProperty("entries", out var entries))
                    {
                        foreach (var entry in entries.EnumerateArray())
                        {
                            string? path = null;
                            
                            if (entry.TryGetProperty("folderUri", out var folderUri))
                            {
                                path = folderUri.GetString();
                            }
                            else if (entry.TryGetProperty("fileUri", out var fileUri))
                            {
                                path = fileUri.GetString();
                            }

                            if (!string.IsNullOrEmpty(path))
                            {
                                path = CleanVSCodePath(path);
                                if (Directory.Exists(path) || File.Exists(path))
                                {
                                    projects.Add(new RecentProject
                                    {
                                        Name = Path.GetFileName(path.TrimEnd('/', '\\')),
                                        Path = path,
                                        LastAccessed = GetFileLastAccess(path)
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // Ignore errors
        }

        return projects
            .GroupBy(p => p.Path.ToLowerInvariant())
            .Select(g => g.First())
            .OrderByDescending(p => p.LastAccessed)
            .Take(maxCount)
            .ToList();
    }

    private static string CleanVSCodePath(string path)
    {
        if (path.StartsWith("file:///", StringComparison.OrdinalIgnoreCase))
        {
            path = path[8..];
            if (path.Length > 2 && path[0] == '/' && path[2] == ':')
            {
                path = path[1..];
            }
        }
        else if (path.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
        {
            path = path[7..];
        }

        path = Uri.UnescapeDataString(path);
        path = path.Replace('/', '\\');

        return path;
    }

    private static DateTimeOffset GetFileLastAccess(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                return new FileInfo(path).LastAccessTime;
            }
            if (Directory.Exists(path))
            {
                return new DirectoryInfo(path).LastAccessTime;
            }
        }
        catch
        {
            // Ignore
        }
        return DateTimeOffset.MinValue;
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
