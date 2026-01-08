using System.Text.Json;
using CodingWithCalvin.VSToolbox.Core.Models;
using Microsoft.Win32;

namespace CodingWithCalvin.VSToolbox.Core.Services;

public sealed class RecentProjectsService : IRecentProjectsService
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
            var hivePath = Path.Combine(VisualStudioAppDataPath, $"{majorVersion}.0_{instance.InstanceId}");

            // Primary source: ApplicationPrivateSettings.xml with CodeContainers.Offline (VS 2022+)
            var privateSettingsPath = Path.Combine(hivePath, "ApplicationPrivateSettings.xml");
            if (File.Exists(privateSettingsPath))
            {
                var projectsFromSettings = ParseApplicationPrivateSettings(privateSettingsPath);
                recentProjects.AddRange(projectsFromSettings);
            }

            // If no projects found, try alternative locations
            if (recentProjects.Count == 0)
            {
                // Try to read from RecentlyOpened.json (VS 2022+ alternate source)
                var recentlyOpenedPath = Path.Combine(hivePath, "RecentlyOpened.json");
                if (File.Exists(recentlyOpenedPath))
                {
                    var projectsFromRecent = ParseRecentlyOpened(recentlyOpenedPath);
                    recentProjects.AddRange(projectsFromRecent);
                }

                // Also try RecentProjects.json
                var recentProjectsPath = Path.Combine(hivePath, "RecentProjects.json");
                if (File.Exists(recentProjectsPath))
                {
                    var projectsFromRecent = ParseRecentProjectsJson(recentProjectsPath);
                    recentProjects.AddRange(projectsFromRecent);
                }

                // Try CodeContainers.json file (standalone file in some versions)
                var codeContainersPath = Path.Combine(hivePath, "CodeContainers.json");
                if (File.Exists(codeContainersPath))
                {
                    var projectsFromContainers = ParseCodeContainersFile(codeContainersPath);
                    recentProjects.AddRange(projectsFromContainers);
                }

                // Try MRU from registry
                var registryProjects = GetRecentProjectsFromRegistry(majorVersion, instance.InstanceId);
                recentProjects.AddRange(registryProjects);
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

    private static IEnumerable<RecentProject> ParseRecentlyOpened(string filePath)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            var projects = new List<RecentProject>();

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Try different property names
            var propertyNames = new[] { "Entries", "entries", "Items", "items", "Projects", "projects" };
            
            foreach (var propName in propertyNames)
            {
                if (root.TryGetProperty(propName, out var entries))
                {
                    foreach (var entry in entries.EnumerateArray())
                    {
                        var path = GetPathFromEntry(entry);
                        if (!string.IsNullOrEmpty(path) && (File.Exists(path) || Directory.Exists(path)))
                        {
                            var lastAccessed = GetLastAccessedFromEntry(entry);
                            projects.Add(new RecentProject
                            {
                                Name = Path.GetFileName(path),
                                Path = path,
                                LastAccessed = lastAccessed
                            });
                        }
                    }
                    break;
                }
            }

            // If root is an array directly
            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var entry in root.EnumerateArray())
                {
                    var path = GetPathFromEntry(entry);
                    if (!string.IsNullOrEmpty(path) && (File.Exists(path) || Directory.Exists(path)))
                    {
                        var lastAccessed = GetLastAccessedFromEntry(entry);
                        projects.Add(new RecentProject
                        {
                            Name = Path.GetFileName(path),
                            Path = path,
                            LastAccessed = lastAccessed
                        });
                    }
                }
            }

            return projects;
        }
        catch
        {
            return [];
        }
    }

    private static string? GetPathFromEntry(JsonElement entry)
    {
        // Try different property names for path
        var pathProps = new[] { "Path", "path", "FullPath", "fullPath", "Key", "key", "LocalPath", "localPath", "Value", "value" };
        
        foreach (var prop in pathProps)
        {
            if (entry.TryGetProperty(prop, out var pathElement))
            {
                var path = pathElement.GetString();
                if (!string.IsNullOrEmpty(path))
                {
                    return path;
                }
            }
        }

        // If entry is a string directly
        if (entry.ValueKind == JsonValueKind.String)
        {
            return entry.GetString();
        }

        return null;
    }

    private static DateTimeOffset GetLastAccessedFromEntry(JsonElement entry)
    {
        var dateProps = new[] { "LastAccessed", "lastAccessed", "LastOpened", "lastOpened", "Timestamp", "timestamp", "Date", "date" };
        
        foreach (var prop in dateProps)
        {
            if (entry.TryGetProperty(prop, out var dateElement))
            {
                if (dateElement.TryGetDateTimeOffset(out var dateOffset))
                {
                    return dateOffset;
                }
                if (dateElement.TryGetDateTime(out var dateTime))
                {
                    return new DateTimeOffset(dateTime);
                }
                // Try parsing as Unix timestamp (milliseconds)
                if (dateElement.TryGetInt64(out var unixMs))
                {
                    return DateTimeOffset.FromUnixTimeMilliseconds(unixMs);
                }
            }
        }

        return DateTimeOffset.Now;
    }

    private static IEnumerable<RecentProject> ParseRecentProjectsJson(string filePath)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            var projects = new List<RecentProject>();

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            void ProcessElement(JsonElement element)
            {
                if (element.ValueKind == JsonValueKind.Object)
                {
                    var path = GetPathFromEntry(element);
                    if (!string.IsNullOrEmpty(path) && 
                        (path.EndsWith(".sln", StringComparison.OrdinalIgnoreCase) || 
                         path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)) &&
                        File.Exists(path))
                    {
                        projects.Add(new RecentProject
                        {
                            Name = Path.GetFileName(path),
                            Path = path,
                            LastAccessed = GetLastAccessedFromEntry(element)
                        });
                    }
                }
                else if (element.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in element.EnumerateArray())
                    {
                        ProcessElement(item);
                    }
                }
            }

            ProcessElement(root);
            return projects;
        }
        catch
        {
            return [];
        }
    }

    private static IEnumerable<RecentProject> ParseApplicationPrivateSettings(string settingsPath)
    {
        try
        {
            var content = File.ReadAllText(settingsPath);
            var projects = new List<RecentProject>();

            var doc = System.Xml.Linq.XDocument.Parse(content);

            // Look for CodeContainers.Offline and CodeContainers.Roaming collections
            var codeContainerCollections = doc.Descendants("collection")
                .Where(e => e.Attribute("name")?.Value is "CodeContainers.Offline" or "CodeContainers.Roaming");

            foreach (var collection in codeContainerCollections)
            {
                // Get the value element that contains the JSON
                var valueElement = collection.Elements("value")
                    .FirstOrDefault(v => v.Attribute("name")?.Value == "value");

                if (valueElement is null) continue;

                var jsonContent = valueElement.Value?.Trim();
                if (string.IsNullOrEmpty(jsonContent)) continue;

                // Parse the JSON array of Key/Value pairs
                var parsedProjects = ParseCodeContainersJson(jsonContent);
                projects.AddRange(parsedProjects);
            }

            return projects;
        }
        catch
        {
            return [];
        }
    }

    private static IEnumerable<RecentProject> ParseCodeContainersJson(string jsonContent)
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
                string? path = null;
                DateTimeOffset lastAccessed = DateTimeOffset.MinValue;

                // Get path from Key or Value.LocalProperties.FullPath
                if (item.TryGetProperty("Key", out var keyElement))
                {
                    path = keyElement.GetString();
                }

                if (item.TryGetProperty("Value", out var valueElement))
                {
                    // Try to get FullPath from LocalProperties
                    if (valueElement.TryGetProperty("LocalProperties", out var localProps))
                    {
                        if (localProps.TryGetProperty("FullPath", out var fullPath))
                        {
                            path = fullPath.GetString() ?? path;
                        }
                    }

                    // Get LastAccessed
                    if (valueElement.TryGetProperty("LastAccessed", out var lastAccessedElement))
                    {
                        if (lastAccessedElement.TryGetDateTimeOffset(out var parsed))
                        {
                            lastAccessed = parsed;
                        }
                        else if (lastAccessedElement.ValueKind == JsonValueKind.String)
                        {
                            if (DateTimeOffset.TryParse(lastAccessedElement.GetString(), out var parsedString))
                            {
                                lastAccessed = parsedString;
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(path))
                {
                    // Filter to only include solutions and projects (not folders)
                    var isSolutionOrProject = path.EndsWith(".sln", StringComparison.OrdinalIgnoreCase) ||
                                              path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) ||
                                              path.EndsWith(".vbproj", StringComparison.OrdinalIgnoreCase) ||
                                              path.EndsWith(".fsproj", StringComparison.OrdinalIgnoreCase);

                    if (isSolutionOrProject && File.Exists(path))
                    {
                        projects.Add(new RecentProject
                        {
                            Name = Path.GetFileName(path),
                            Path = path,
                            LastAccessed = lastAccessed != DateTimeOffset.MinValue ? lastAccessed : GetFileLastAccess(path)
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

    private static IEnumerable<RecentProject> ParseCodeContainersFile(string containersPath)
    {
        try
        {
            var json = File.ReadAllText(containersPath);
            var projects = new List<RecentProject>();

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("CodeContainers", out var containers))
            {
                foreach (var container in containers.EnumerateArray())
                {
                    string? path = null;

                    if (container.TryGetProperty("LocalProperties", out var localProps))
                    {
                        if (localProps.TryGetProperty("FullPath", out var fullPath))
                        {
                            path = fullPath.GetString();
                        }
                    }

                    // Also try direct Path property
                    if (string.IsNullOrEmpty(path) && container.TryGetProperty("Path", out var pathProp))
                    {
                        path = pathProp.GetString();
                    }

                    if (!string.IsNullOrEmpty(path) && (File.Exists(path) || Directory.Exists(path)))
                    {
                        var lastAccessed = DateTimeOffset.Now;
                        if (container.TryGetProperty("LastAccessed", out var lastAccessedProp))
                        {
                            if (lastAccessedProp.TryGetDateTimeOffset(out var parsed))
                            {
                                lastAccessed = parsed;
                            }
                            else if (lastAccessedProp.TryGetInt64(out var unixMs))
                            {
                                lastAccessed = DateTimeOffset.FromUnixTimeMilliseconds(unixMs);
                            }
                        }

                        projects.Add(new RecentProject
                        {
                            Name = Path.GetFileName(path),
                            Path = path,
                            LastAccessed = lastAccessed
                        });
                    }
                }
            }

            return projects;
        }
        catch
        {
            return [];
        }
    }

    private static IEnumerable<RecentProject> GetRecentProjectsFromRegistry(int majorVersion, string instanceId)
    {
        var projects = new List<RecentProject>();

        try
        {
            var registryPaths = new[]
            {
                $@"Software\Microsoft\VisualStudio\{majorVersion}.0_{instanceId}\MRUItems",
                $@"Software\Microsoft\VisualStudio\{majorVersion}.0_{instanceId}\ProjectMRUList",
                $@"Software\Microsoft\VisualStudio\{majorVersion}.0_{instanceId}\FileMRUList",
                $@"Software\Microsoft\VisualStudio\{majorVersion}.0\ProjectMRUList",
                $@"Software\Microsoft\VisualStudio\{majorVersion}.0_{instanceId}_Config\MRU",
                $@"Software\Microsoft\VisualStudio\{majorVersion}.0_{instanceId}_Config\FileMRUList",
                $@"Software\Microsoft\VisualStudio\{majorVersion}.0_{instanceId}_Config\ProjectMRUList"
            };

            foreach (var regPath in registryPaths)
            {
                try
                {
                    using var key = Registry.CurrentUser.OpenSubKey(regPath);
                    if (key is null) continue;

                    foreach (var valueName in key.GetValueNames())
                    {
                        var value = key.GetValue(valueName)?.ToString();
                        if (string.IsNullOrEmpty(value)) continue;

                        var path = ExtractPathFromValue(value);
                        if (!string.IsNullOrEmpty(path) && 
                            (path.EndsWith(".sln", StringComparison.OrdinalIgnoreCase) ||
                             path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)) &&
                            File.Exists(path))
                        {
                            projects.Add(new RecentProject
                            {
                                Name = Path.GetFileName(path),
                                Path = path,
                                LastAccessed = GetFileLastAccess(path)
                            });
                        }
                    }
                }
                catch
                {
                    // Skip this registry path
                }
            }
        }
        catch
        {
            // Ignore registry access errors
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

            // Try storage.json first
            var storagePath = Path.Combine(appDataPath, codeFolderName, "User", "globalStorage", "storage.json");

            if (File.Exists(storagePath))
            {
                var json = File.ReadAllText(storagePath);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Try openedPathsList
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

                // Also try windowsState for recent folders
                if (root.TryGetProperty("windowsState", out var windowsState))
                {
                    if (windowsState.TryGetProperty("lastActiveWindow", out var lastWindow))
                    {
                        if (lastWindow.TryGetProperty("folder", out var folder))
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
                                        LastAccessed = DateTimeOffset.Now
                                    });
                                }
                            }
                        }
                    }
                }
            }

            // Try backup locations
            var backupStoragePath = Path.Combine(appDataPath, codeFolderName, "storage.json");
            if (File.Exists(backupStoragePath) && projects.Count == 0)
            {
                // Same parsing logic for backup location
                var json = File.ReadAllText(backupStoragePath);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Try openedPathsList
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
        // VS Code stores paths with file:// prefix
        if (path.StartsWith("file:///", StringComparison.OrdinalIgnoreCase))
        {
            path = path[8..];
            // Handle Windows paths like /C:/folder
            if (path.Length > 2 && path[0] == '/' && path[2] == ':')
            {
                path = path[1..];
            }
        }
        else if (path.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
        {
            path = path[7..];
        }

        // Decode URL encoding
        path = Uri.UnescapeDataString(path);
        
        // Normalize path separators
        path = path.Replace('/', '\\');

        return path;
    }

    private static string ExtractPathFromValue(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        // Handle pipe-separated format
        if (value.Contains('|'))
        {
            var parts = value.Split('|');
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (trimmed.Length > 3 && trimmed[1] == ':' && 
                    (trimmed.EndsWith(".sln", StringComparison.OrdinalIgnoreCase) || 
                     trimmed.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)))
                {
                    return trimmed;
                }
            }
        }

        // Check if it looks like a Windows path
        if (value.Length > 3 && value[1] == ':')
        {
            return value.Trim();
        }

        return value;
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
