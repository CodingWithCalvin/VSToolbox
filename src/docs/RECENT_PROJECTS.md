# Recent Projects Feature

## 📋 Overview

VSToolbox now includes a **Recent Projects** feature that displays recently opened solutions and projects for each Visual Studio and VS Code installation.

---

## ✨ Features

### For Visual Studio:
- 📂 Shows recent solutions (.sln)
- 📁 Shows recent projects (.csproj, .vbproj, etc.)
- 🕐 Sorted by last access time
- ✅ Only shows existing files
- 🚀 Click to open directly in VS

### For VS Code:
- 📁 Shows recent folders/workspaces
- 🕐 Sorted by last access time
- ✅ Only shows existing paths
- 🚀 Click to open directly in VS Code

---

## 🎯 How It Works

### Visual Studio
The service reads recent projects from multiple sources:

1. **ApplicationPrivateSettings.xml**
   - Location: `%LOCALAPPDATA%\Microsoft\VisualStudio\{version}_{instanceId}\`
   - Contains MRU (Most Recently Used) lists

2. **CodeContainers.json**
   - Location: `%LOCALAPPDATA%\Microsoft\VisualStudio\{version}_{instanceId}\`
   - Contains recent container/project information with timestamps

3. **Windows Registry**
   - Keys under `HKCU\Software\Microsoft\VisualStudio\{version}\`
   - Contains MRU project lists

### VS Code
The service reads from:

1. **storage.json**
   - Location: `%APPDATA%\Code\User\globalStorage\`
   - Contains `openedPathsList` with workspaces and folders

2. **Support for both stable and Insiders**
   - Stable: `%APPDATA%\Code\`
   - Insiders: `%APPDATA%\Code - Insiders\`

---

## 📸 Menu Structure

### Visual Studio:
```
Visual Studio 2022 Enterprise ⚙️
├─ 📂 Recent Projects ⭐ NEW!
│  ├─ VSToolbox.sln
│  ├─ MyWebApp.sln
│  ├─ ConsoleApp1.csproj
│  └─ ...
├─ ─────────────────────
├─ Open Explorer
├─ VS CMD Prompt
├─ VS PowerShell
├─ Visual Studio Installer
└─ Open Local AppData
```

### VS Code:
```
VS Code ⚙️
├─ 📂 Recent Folders ⭐ NEW!
│  ├─ VSToolbox
│  ├─ my-react-app
│  ├─ dotnet-microservices
│  └─ ...
├─ ─────────────────────
├─ Open Extensions Folder
├─ Open New Window
├─ Open Installation Folder
└─ Open VS Code Data Folder
```

---

## 🔧 Technical Implementation

### New Files Created:

1. **`RecentProject.cs`** - Model class
```csharp
public sealed class RecentProject
{
    public required string Name { get; init; }
    public required string Path { get; init; }
    public required DateTimeOffset LastAccessed { get; init; }
    public bool IsSolution { get; }
    public bool IsFolder { get; }
    public string DisplayName { get; }
    public string ProjectType { get; }
    public bool Exists { get; }
}
```

2. **`IRecentProjectsService.cs`** - Interface
```csharp
public interface IRecentProjectsService
{
    IReadOnlyList<RecentProject> GetRecentProjects(
        VisualStudioInstance instance, 
        int maxCount = 10);
}
```

3. **`RecentProjectsService.cs`** - Implementation
   - Reads from multiple VS and VS Code sources
   - Deduplicates entries
   - Sorts by last access time
   - Filters non-existing files

### Modified Files:

1. **`MainViewModel.cs`**
   - Added `IRecentProjectsService` dependency
   - Added `GetRecentProjects()` method
   - Added `OpenRecentProject()` method

2. **`MainPage.xaml.cs`**
   - Added "Recent Projects" submenu for VS
   - Added "Recent Folders" submenu for VS Code

---

## 📊 Data Sources

### Visual Studio MRU Locations:

| Source | Path | Format |
|--------|------|--------|
| ApplicationPrivateSettings | `%LOCALAPPDATA%\Microsoft\VisualStudio\{ver}_{id}\` | XML |
| CodeContainers | `%LOCALAPPDATA%\Microsoft\VisualStudio\{ver}_{id}\` | JSON |
| Registry MRU | `HKCU\Software\Microsoft\VisualStudio\{ver}\ProjectMRUList` | Registry |

### VS Code Storage Locations:

| Source | Path | Format |
|--------|------|--------|
| storage.json | `%APPDATA%\Code\User\globalStorage\` | JSON |
| state.vscdb | `%APPDATA%\Code\User\globalStorage\` | SQLite |

---

## ⚙️ Configuration

### Maximum Items
By default, the menu shows up to **10** recent projects. This can be changed:

```csharp
var recentProjects = ViewModel.GetRecentProjects(instance, maxCount: 15);
```

### Filtering
Projects are automatically filtered:
- ✅ Only existing files/folders shown
- ✅ Duplicates removed
- ✅ Sorted by last access time (newest first)

---

## 🎨 Icons

| Project Type | Icon |
|--------------|------|
| Solution (.sln) | 📄 `\uE8A5` |
| Folder | 📁 `\uE8B7` |
| Project | 📄 `\uE8A5` |

---

## 🚀 Usage

1. **Click** the ⚙️ gear button on any instance
2. **Hover** over "Recent Projects" (VS) or "Recent Folders" (VS Code)
3. **Click** any project to open it directly

---

## ⚠️ Limitations

1. **SQLite Database (VS Code)**
   - Newer VS Code versions use `state.vscdb` (SQLite)
   - Currently reads from `storage.json` fallback
   - SQLite support would require additional dependencies

2. **VS Registry Format**
   - Registry format varies by VS version
   - Service attempts multiple key locations

3. **Performance**
   - Menu builds list on-demand
   - May have brief delay for large MRU lists

---

## 🗺️ Future Improvements

- [ ] Add SQLite support for VS Code state.vscdb
- [ ] Add "Pin" functionality for favorite projects
- [ ] Add "Remove from list" option
- [ ] Add project type icons (C#, VB, F#, etc.)
- [ ] Add workspace support for VS Code
- [ ] Add search/filter in submenu

---

## 📝 Example Output

### Visual Studio Recent Projects:
```
1. VSToolbox.sln (Last: 2 hours ago)
2. MyWebApp.sln (Last: Yesterday)
3. ConsoleApp1.csproj (Last: 3 days ago)
4. DataProcessor.sln (Last: 1 week ago)
```

### VS Code Recent Folders:
```
1. VSToolbox (Last: 1 hour ago)
2. my-react-app (Last: Today)
3. python-scripts (Last: 2 days ago)
4. dotnet-api (Last: 1 week ago)
```

---

**Status:** ✅ Implemented and ready to use!
