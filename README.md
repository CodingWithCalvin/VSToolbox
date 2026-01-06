# 🛠️ Visual Studio Toolbox

<div align="center">

**Your Visual Studio installations, beautifully organized** ✨

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![WinUI 3](https://img.shields.io/badge/WinUI-3.0-0078D4?style=for-the-badge&logo=windows)](https://microsoft.github.io/microsoft-ui-xaml/)
[![Windows](https://img.shields.io/badge/Windows-11-00A4EF?style=for-the-badge&logo=windows11)](https://www.microsoft.com/windows)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)

</div>

---

## 🎯 What is Visual Studio Toolbox?

Visual Studio Toolbox is a sleek **system tray application** for Windows that helps you manage all your Visual Studio installations in one place. Think of it as your personal command center for Visual Studio! 🚀

> 💡 **Inspired by JetBrains Toolbox** - bringing the same convenience to the Visual Studio ecosystem!

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| 🔍 **Auto-Detection** | Automatically discovers all VS 2019, 2022, and 2026 installations |
| 🎨 **Beautiful UI** | Modern WinUI 3 interface with light/dark mode support |
| 🚀 **Quick Launch** | Launch any VS instance with a single click |
| 🧪 **Experimental Hives** | See and launch experimental/custom VS hives |
| 💻 **Developer Shells** | Launch VS Developer Command Prompt or PowerShell |
| 📁 **Quick Access** | Open installation folders and AppData directories |
| 🖥️ **Windows Terminal** | Integrates with your Windows Terminal profiles |
| 📌 **System Tray** | Lives quietly in your system tray until needed |
| ⚙️ **Configurable** | Startup and window behavior settings |
| 🪟 **Custom Chrome** | Sleek custom title bar with VS purple branding |

---

## 📸 Screenshots

### Instance List
See all your Visual Studio installations at a glance, including version info, build numbers, and channel badges:

![Instance List](assets/instance-list.png)

### Hover State
Hover over any installation to highlight it with the signature purple accent:

![Instance List Hover](assets/instance-list-hover.png)

### Quick Actions Menu
Access powerful options for each installation - open folders, launch dev shells, and more:

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

### 🖱️ Installed Tab
- **Click** the ▶️ play button to launch Visual Studio
- **Click** the ⚙️ gear button for more options:
  - 📂 Open Explorer - Open the VS installation folder
  - 💻 VS CMD Prompt - Launch Developer Command Prompt
  - 🐚 VS PowerShell - Launch Developer PowerShell
  - 📁 Open Local AppData - Access VS settings and extensions

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
│       └── 📁 Services/                      # VS detection & launch
│
└── 📁 tests/                                 # 🧪 Unit tests
```

---

## 🛠️ Tech Stack

| Technology | Purpose |
|------------|---------|
| 💜 **C# 13** | Language |
| 🎯 **.NET 10** | Runtime |
| 🎨 **WinUI 3** | UI Framework |
| 📦 **Windows App SDK 1.8** | Windows APIs |
| 🔔 **H.NotifyIcon.WinUI** | System tray |
| 🧰 **CommunityToolkit.Mvvm** | MVVM pattern |

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
[![CalvinAllen](https://avatars.githubusercontent.com/u/41448698?v=4&s=64)](https://github.com/CalvinAllen) [![isaacrlevin](https://avatars.githubusercontent.com/u/8878502?v=4&s=64)](https://github.com/isaacrlevin) [![timheuer](https://avatars.githubusercontent.com/u/4821?v=4&s=64)](https://github.com/timheuer) 
<!-- readme: contributors -end -->

---

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

---

## 💖 Acknowledgments

- 🙏 Microsoft for Visual Studio and WinUI
- 💡 JetBrains Toolbox for the inspiration
- 🎨 The .NET community for amazing libraries

---

<div align="center">

**Made with 💜 by [Coding with Calvin](https://github.com/CodingWithCalvin)**

⭐ Star this repo if you find it useful! ⭐

</div>
