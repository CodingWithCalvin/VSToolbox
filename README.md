# 🛠️ Visual Studio Toolbox

<div align="center">

**Your Visual Studio and VS Code installations, beautifully organized** ✨

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![WinUI 3](https://img.shields.io/badge/WinUI-3.0-0078D4?style=for-the-badge&logo=windows)](https://microsoft.github.io/microsoft-ui-xaml/)
[![Windows](https://img.shields.io/badge/Windows-11-00A4EF?style=for-the-badge&logo=windows11)](https://www.microsoft.com/windows)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)

</div>

---

## 🎯 What is Visual Studio Toolbox?

Visual Studio Toolbox is a sleek **system tray application** for Windows that helps you manage all your **Visual Studio** and **Visual Studio Code** installations in one place. Think of it as your personal command center for all your development tools! 🚀

> 💡 **Inspired by JetBrains Toolbox** - bringing the same convenience to the Microsoft development ecosystem!

---

## ✨ Features

### 🎨 **Core Features**

| Feature | Description |
|---------|-------------|
| 🔍 **Auto-Detection** | Automatically discovers VS 2019, 2022, 2026, VS Code, and VS Code Insiders |
| 🎨 **Beautiful UI** | Modern WinUI 3 interface with light/dark mode support |
| 🚀 **Quick Launch** | Launch any installation with a single click |
| 🧪 **Experimental Hives** | See and launch experimental/custom VS hives |
| 📌 **System Tray** | Lives quietly in your system tray until needed |
| ⚙️ **Configurable** | Startup and window behavior settings |
| 🪟 **Custom Chrome** | Sleek custom title bar with VS purple branding |

### 💻 **Visual Studio Features**

| Feature | Description |
|---------|-------------|
| 💻 **Developer Shells** | Launch VS Developer Command Prompt or PowerShell |
| 📁 **Quick Access** | Open installation folders and AppData directories |
| 🖥️ **Windows Terminal** | Integrates with your Windows Terminal profiles |
| 🛠️ **VS Installer Integration** | Modify, update, or manage installations directly |
| 📦 **Workload Detection** | View installed workloads for each instance |

### 📝 **VS Code Features** ⭐ **NEW!**

| Feature | Description |
|---------|-------------|
| 🧩 **Extension Detection** | Automatically detects installed VS Code extensions |
| 📂 **Quick Access** | Open extensions folder, data folder, and installation directory |
| 🪟 **New Window** | Launch new VS Code windows quickly |
| 🎨 **Custom Icons** | Support for custom VS Code icons |

---

## 📸 Screenshots

### Instance List
See all your Visual Studio and VS Code installations at a glance, including version info, build numbers, and channel badges:

![Instance List](assets/instance-list.png)

### Hover State
Hover over any installation to highlight it with the signature purple accent:

![Instance List Hover](assets/instance-list-hover.png)

### Quick Actions Menu
Access powerful options for each installation - open folders, launch dev shells, manage with VS Installer, and more:

![Instance Menu](assets/instance-list-menu.png)

### Settings
Configure startup behavior and window preferences:

![Settings](assets/settings.png)

---

## 🚀 Getting Started

### Prerequisites

- 🪟 Windows 10/11
- 📦 [.NET 10 SDK](https://dotnet.microsoft.com/download)
- 🎨 [Windows App SDK 1.8+](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/)

### Build & Run

```bash
# Clone the repository
git clone https://github.com/CalvinAllen/VSToolbox.git

# Navigate to the project
cd VSToolbox

# Build and run
dotnet run --project src/CodingWithCalvin.VSToolbox
```

---

## 🎮 Usage

### 🖱️ Visual Studio Instances

**Click** the ▶️ play button to launch Visual Studio, or **click** the ⚙️ gear button for more options:

#### 📋 **Visual Studio Menu:**
- 📂 **Open Explorer** - Open the VS installation folder
- 💻 **VS CMD Prompt** - Launch Developer Command Prompt
- 🐚 **VS PowerShell** - Launch Developer PowerShell
- 🛠️ **Visual Studio Installer** ⭐ **NEW!**
  - 🔧 **Modify Installation** - Add/remove workloads and components
  - 📥 **Update** - Install available updates
  - 🚀 **Open Installer** - Launch VS Installer dashboard
- 📁 **Open Local AppData** - Access VS settings and extensions

### 🖱️ VS Code Instances ⭐ **NEW!**

**Click** the ▶️ play button to launch VS Code, or **click** the ⚙️ gear button for more options:

#### 📋 **VS Code Menu:**
- 🧩 **Open Extensions Folder** - Browse installed extensions
- 🪟 **Open New Window** - Launch a new VS Code window
- 📂 **Open Installation Folder** - Browse VS Code files
- 📁 **Open VS Code Data Folder** - Access settings and configuration

### ⚙️ Settings Tab
- **Launch on startup** - Start Visual Studio Toolbox when Windows starts
- **Start minimized** - Launch directly to the system tray
- **Minimize to tray** - Hide to system tray when minimizing
- **Close to tray** - Hide to system tray instead of exiting

### 📌 System Tray
- **Click** the tray icon to show/hide the window
- **Right-click** for quick menu (Show / Exit)

---

## 🏗️ Architecture

```
VSToolbox/
├── 📁 src/
│   ├── 📁 CodingWithCalvin.VSToolbox/        # 🎨 WinUI 3 Application
│   │   ├── 📁 Views/                         # XAML pages
│   │   ├── 📁 ViewModels/                    # MVVM view models
│   │   └── 📁 Services/                      # App services
│   │
│   └── 📁 CodingWithCalvin.VSToolbox.Core/   # 📦 Core Library
│       ├── 📁 Models/                        # Data models
│       └── 📁 Services/                      # VS & VS Code detection
│
├── 📁 docs/                                  # 📚 Documentation
│   ├── VSCODE_INTEGRATION.md                # VS Code features guide
│   ├── VS_INSTALLER_INTEGRATION.md          # VS Installer guide
│   └── VSCODE_ICONS.md                      # Icon setup guide
│
├── 📁 scripts/                               # 🔧 Helper scripts
│   └── extract_vscode_icons.ps1             # Extract VS Code icons
│
└── 📁 tests/                                 # 🧪 Unit tests
```

---

## 🛠️ Tech Stack

| Technology | Purpose |
|------------|---------|
| 💜 **C# 14** | Language |
| 🎯 **.NET 10** | Runtime |
| 🎨 **WinUI 3** | UI Framework |
| 📦 **Windows App SDK 1.8** | Windows APIs |
| 🔔 **H.NotifyIcon.WinUI** | System tray |
| 🧰 **CommunityToolkit.Mvvm** | MVVM pattern |

---

## 🆕 What's New

### 🎉 **Latest Features**

#### ✅ **VS Code Integration** ⭐
- Detects Visual Studio Code and VS Code Insiders
- Shows installed extensions
- Quick access to VS Code folders
- Custom icon support

#### ✅ **Visual Studio Installer Integration** ⭐
- Modify installations directly from VSToolbox
- Update Visual Studio with one click
- Quick access to VS Installer dashboard

#### ✅ **Enhanced Detection**
- Faster and more reliable detection
- Support for multiple VS Code installation locations
- Extension discovery and counting

See [VSCODE_INTEGRATION.md](docs/VSCODE_INTEGRATION.md) and [VS_INSTALLER_INTEGRATION.md](docs/VS_INSTALLER_INTEGRATION.md) for detailed documentation.

---

## 📚 Documentation

- 📖 [VS Code Integration Guide](docs/VSCODE_INTEGRATION.md)
- 🛠️ [Visual Studio Installer Integration](docs/VS_INSTALLER_INTEGRATION.md)
- 🎨 [VS Code Icons Setup](docs/VSCODE_ICONS.md)
- 📝 [Implementation Details](docs/VS_INSTALLER_IMPLEMENTATION.md)

---

## 🔧 Advanced Features

### **Extract VS Code Icons**

Run the included PowerShell script to extract icons from your VS Code installations:

```powershell
.\scripts\extract_vscode_icons.ps1
```

Options:
```powershell
# Custom output directory
.\scripts\extract_vscode_icons.ps1 -OutputDir "C:\custom\path"

# Custom icon size
.\scripts\extract_vscode_icons.ps1 -Size 256
```

### **Visual Studio Installer Commands**

Use the context menu to access VS Installer features:
- **Modify** - Opens the installer to add/remove workloads
- **Update** - Automatically updates the VS instance
- **Open Installer** - Launches the main installer window

---

## 🤝 Contributing

Contributions are welcome! Feel free to:

1. 🍴 Fork the repository
2. 🌿 Create a feature branch (`git checkout -b feature/amazing-feature`)
3. 💾 Commit your changes (`git commit -m 'Add amazing feature'`)
4. 📤 Push to the branch (`git push origin feature/amazing-feature`)
5. 🎉 Open a Pull Request

## 👥 Contributors

<!-- readme: contributors -start -->
[![CalvinAllen](https://avatars.githubusercontent.com/u/41448698?v=4&s=64)](https://github.com/CalvinAllen) [![timheuer](https://avatars.githubusercontent.com/u/4821?v=4&s=64)](https://github.com/timheuer) 
<!-- readme: contributors -end -->

---

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

---

## 💖 Acknowledgments

- 🙏 Microsoft for Visual Studio, VS Code, and WinUI
- 💡 JetBrains Toolbox for the inspiration
- 🎨 The .NET community for amazing libraries
- 🌟 All contributors and users of this project

---

## 🗺️ Roadmap

Future enhancements we're considering:

- [ ] VS Code workspace detection
- [ ] VS Code extension management
- [ ] More Visual Studio Installer commands
- [ ] Custom launch arguments
- [ ] Keyboard shortcuts
- [ ] Recent projects list
- [ ] Solution file associations

---

<div align="center">

**Made with 💜 by [Coding with Calvin](https://github.com/CodingWithCalvin)**

⭐ **Star this repo if you find it useful!** ⭐

🐛 [Report a bug](https://github.com/CalvinAllen/VSToolbox/issues) · 💡 [Request a feature](https://github.com/CalvinAllen/VSToolbox/issues)

</div>
