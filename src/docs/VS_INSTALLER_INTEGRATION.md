# Visual Studio Installer Integration

## 🛠️ Visual Studio Installer Commands

The application now integrates with the Visual Studio Installer, allowing developers to manage their Visual Studio installations directly from VSToolbox.

---

## 📋 Available Commands

### 1. **Modify Installation**
Opens the Visual Studio Installer in modify mode for the selected instance.

**What it does:**
- Allows you to add/remove workloads
- Install/uninstall individual components
- Change installation options

**Command:**
```bash
vs_installer.exe modify --installPath "C:\Path\To\VS"
```

---

### 2. **Update**
Checks for and installs updates for the selected Visual Studio instance.

**What it does:**
- Downloads and installs available updates
- Runs in passive mode (minimal UI)
- Updates the VS instance to the latest version

**Command:**
```bash
vs_installer.exe update --installPath "C:\Path\To\VS" --passive
```

---

### 3. **Open Installer**
Launches the Visual Studio Installer main window.

**What it does:**
- Opens the VS Installer dashboard
- Shows all installed instances
- Allows managing all VS installations

**Command:**
```bash
vs_installer.exe
```

---

## 🎯 How to Access

### Method 1: Context Menu
1. Right-click the ⚙️ gear icon on any Visual Studio instance
2. Navigate to **"Visual Studio Installer"** submenu
3. Choose your action:
   - **Modify Installation** - Add/remove features
   - **Update** - Install updates
   - **Open Installer** - Launch VS Installer

### Method 2: Keyboard Shortcuts
*(Coming soon)*

---

## 📸 Menu Structure

```
Visual Studio Instance (gear icon) ⚙️
├─ Open Explorer
├─ VS CMD Prompt
├─ VS PowerShell
├─ ─────────────────────
├─ Visual Studio Installer
│  ├─ Modify Installation
│  ├─ Update
│  ├─ ─────────────
│  └─ Open Installer
├─ ─────────────────────
└─ Open Local AppData
```

---

## 🔧 Technical Details

### Installer Location
```
%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vs_installer.exe
```

### Command Line Arguments

| Argument | Description |
|----------|-------------|
| `modify --installPath "path"` | Opens modify dialog for specific instance |
| `update --installPath "path" --passive` | Updates instance with minimal UI |
| *(no args)* | Opens main installer window |

---

## ✨ Features

### ✅ **Modify Installation**
- 🎨 Add/remove workloads (.NET, C++, Azure, etc.)
- 🧩 Install/uninstall individual components
- 🔧 Configure installation options
- 💾 Change installation location (limited)

### ✅ **Update**
- 📥 Download latest updates
- 🔄 Install updates automatically
- ⚡ Runs in passive mode (faster)
- 🔔 Notifies when update completes

### ✅ **Open Installer**
- 📊 View all VS installations
- 🔍 Check for updates across all instances
- 🗑️ Uninstall instances
- 📦 Install new VS versions

---

## 🚀 Usage Examples

### Example 1: Update a Specific Instance
```
User Action: Right-click gear → Visual Studio Installer → Update
Result: VS Installer updates that specific VS 2022 instance
```

### Example 2: Modify Workloads
```
User Action: Right-click gear → Visual Studio Installer → Modify Installation
Result: Opens modify dialog to add/remove workloads
```

### Example 3: Open Installer Dashboard
```
User Action: Right-click gear → Visual Studio Installer → Open Installer
Result: VS Installer main window opens showing all installations
```

---

## ⚠️ Important Notes

1. **Administrator Rights:**
   - Modifying and updating may require administrator privileges
   - Windows will prompt for UAC elevation if needed

2. **VS Must Be Closed:**
   - Visual Studio should be closed before modifying or updating
   - The installer will notify if VS is running

3. **Network Connection:**
   - Updates require internet connection
   - Download size varies based on installed components

4. **Passive Mode:**
   - Update runs with minimal UI (`--passive` flag)
   - Progress is shown in a simplified window
   - No user interaction required

---

## 🔍 Troubleshooting

### Installer Not Found
**Problem:** "Visual Studio Installer not found" message

**Solution:**
- Ensure Visual Studio is properly installed
- Check path: `%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\`
- Reinstall Visual Studio if installer is missing

### Update Fails
**Problem:** Update command doesn't work

**Solution:**
- Close all Visual Studio instances
- Run VSToolbox as administrator
- Check internet connection
- Try using "Open Installer" and update manually

### Modify Opens Wrong Instance
**Problem:** Wrong VS instance is being modified

**Solution:**
- This is unlikely but if it happens:
- Use "Open Installer" instead
- Select correct instance manually
- Report as a bug

---

## 📚 Related Documentation

- [Visual Studio Installer Command-Line Parameters](https://docs.microsoft.com/en-us/visualstudio/install/use-command-line-parameters-to-install-visual-studio)
- [Update Visual Studio](https://docs.microsoft.com/en-us/visualstudio/install/update-visual-studio)
- [Modify Visual Studio](https://docs.microsoft.com/en-us/visualstudio/install/modify-visual-studio)

---

## 🎉 Benefits

✅ **No need to search for VS Installer**
✅ **Quick access to update functionality**
✅ **Modify specific instances easily**
✅ **All VS management in one place**
✅ **Saves time for developers**

---

## 📝 Implementation Details

### Commands Added to MainViewModel.cs:
```csharp
[RelayCommand]
private void LaunchVisualStudioInstaller(LaunchableInstance? launchable)

[RelayCommand]
private void ModifyVisualStudioInstance(LaunchableInstance? launchable)

[RelayCommand]
private void UpdateVisualStudioInstance(LaunchableInstance? launchable)
```

### Menu Integration in MainPage.xaml.cs:
- Added submenu "Visual Studio Installer"
- 3 menu items with icons
- Only visible for Visual Studio instances (not VS Code)

---

**Status:** ✅ Implemented and ready to use!
