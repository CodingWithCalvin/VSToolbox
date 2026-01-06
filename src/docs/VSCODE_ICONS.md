# VS Code Integration - Icons Setup

## 📦 Adding VS Code Icons

The application supports custom icons for VS Code and VS Code Insiders. To add them:

### Option 1: Use Official Icons (Recommended)

1. **VS Code Stable:**
   - Download the icon from [VS Code website](https://code.visualstudio.com/)
   - Or extract from your installed `Code.exe`
   - Save as `vscode_icon.png` in the `Assets` folder

2. **VS Code Insiders:**
   - Download from [VS Code Insiders website](https://code.visualstudio.com/insiders)
   - Or extract from your installed `Code - Insiders.exe`
   - Save as `vscode_insiders_icon.png` in the `Assets` folder

### Option 2: Auto-Extract (Default)

If you don't provide custom icons, the application will automatically extract them from the installed executables.

### Icon Specifications

- **Format:** PNG
- **Recommended Size:** 64x64 or 128x128 pixels
- **Minimum Size:** 48x48 pixels
- **Background:** Transparent

## 🎨 Icon File Names

| File Name | Purpose |
|-----------|---------|
| `vscode_icon.png` | Visual Studio Code (Stable) |
| `vscode_insiders_icon.png` | Visual Studio Code Insiders |

## 📝 Notes

- Icons are cached in `%LOCALAPPDATA%\VSToolbox\IconCache`
- If icons don't appear, delete the cache folder and restart the app
- The app will fall back to auto-extraction if custom icons are missing
