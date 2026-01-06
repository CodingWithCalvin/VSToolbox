using CodingWithCalvin.VSToolbox.Core.Models;
using CodingWithCalvin.VSToolbox.Services;
using CodingWithCalvin.VSToolbox.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace CodingWithCalvin.VSToolbox.Views;

public sealed partial class MainPage : Page
{
    private static readonly SolidColorBrush AccentBrush = new(Color.FromArgb(255, 138, 43, 226)); // Purple
    private static readonly SolidColorBrush TransparentBrush = new(Colors.Transparent);
    private static readonly SolidColorBrush HoverBackgroundBrush = new(Color.FromArgb(20, 138, 43, 226));

    private readonly SettingsService _settingsService = new();
    private bool _isInitializingSettings;

    public MainPage()
    {
        InitializeComponent();
        ViewModel = new MainViewModel();
        InitializeSettings();
    }

    public MainViewModel ViewModel { get; }

    private void InitializeSettings()
    {
        _isInitializingSettings = true;

        // Sync startup setting with registry state
        _settingsService.SyncStartupSetting();

        // Load current settings into toggles
        LaunchOnStartupToggle.IsOn = _settingsService.LaunchOnStartup;
        StartMinimizedToggle.IsOn = _settingsService.StartMinimized;
        MinimizeToTrayToggle.IsOn = _settingsService.MinimizeToTray;
        CloseToTrayToggle.IsOn = _settingsService.CloseToTray;

        _isInitializingSettings = false;
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        // Set the title bar drag region
        var window = ((App)Application.Current).MainWindow;
        window?.SetTitleBar(AppTitleBar);

        await ViewModel.LoadInstancesCommand.ExecuteAsync(null);
    }

    private void OnLaunchClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is LaunchableInstance instance)
        {
            ViewModel.LaunchInstanceCommand.Execute(instance);
        }
    }

    private void OnOptionsFlyoutOpening(object sender, object e)
    {
        if (sender is not MenuFlyout flyout)
            return;

        var button = flyout.Target as Button;
        if (button?.DataContext is not LaunchableInstance instance)
            return;

        flyout.Items.Clear();

        if (instance.Instance.Version == VSVersion.VSCode)
        {
            var openExtensionsItem = new MenuFlyoutItem
            {
                Text = "Open Extensions Folder",
                Icon = new FontIcon { Glyph = "\uE74C", Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204)) }
            };
            openExtensionsItem.Click += (s, args) => ViewModel.OpenVSCodeExtensionsFolderCommand.Execute(instance);
            flyout.Items.Add(openExtensionsItem);

            var openNewWindowItem = new MenuFlyoutItem
            {
                Text = "Open New Window",
                Icon = new FontIcon { Glyph = "\uE8A7", Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204)) }
            };
            openNewWindowItem.Click += (s, args) => ViewModel.LaunchInstanceCommand.Execute(instance);
            flyout.Items.Add(openNewWindowItem);

            flyout.Items.Add(new MenuFlyoutSeparator());

            var openExplorerItem = new MenuFlyoutItem
            {
                Text = "Open Installation Folder",
                Icon = new FontIcon { Glyph = "\uE838", Foreground = new SolidColorBrush(Color.FromArgb(255, 234, 179, 8)) }
            };
            openExplorerItem.Click += (s, args) => ViewModel.OpenInstanceFolderCommand.Execute(instance);
            flyout.Items.Add(openExplorerItem);

            var appDataItem = new MenuFlyoutItem
            {
                Text = "Open VS Code Data Folder",
                Icon = new FontIcon { Glyph = "\uE8B7", Foreground = new SolidColorBrush(Color.FromArgb(255, 139, 92, 246)) }
            };
            appDataItem.Click += (s, args) => ViewModel.OpenAppDataFolderCommand.Execute(instance);
            flyout.Items.Add(appDataItem);

            return;
        }

        var openExplorerItemVS = new MenuFlyoutItem
        {
            Text = "Open Explorer",
            Icon = new FontIcon { Glyph = "\uE838", Foreground = new SolidColorBrush(Color.FromArgb(255, 234, 179, 8)) }
        };
        openExplorerItemVS.Click += (s, args) => ViewModel.OpenInstanceFolderCommand.Execute(instance);
        flyout.Items.Add(openExplorerItemVS);

        var terminalProfiles = ViewModel.TerminalProfiles;
        var cmdProfiles = terminalProfiles.Where(p => p.ShellType == ShellType.Cmd).ToList();
        var pwshProfiles = terminalProfiles.Where(p => p.ShellType == ShellType.PowerShell).ToList();

        if (cmdProfiles.Count > 0)
        {
            var cmdSubmenu = new MenuFlyoutSubItem
            {
                Text = "VS CMD Prompt",
                Icon = new FontIcon { Glyph = "\uE756", Foreground = new SolidColorBrush(Color.FromArgb(255, 59, 130, 246)) }
            };
            foreach (var profile in cmdProfiles)
            {
                var profileItem = new MenuFlyoutItem { Text = profile.Name };
                var capturedProfile = profile;
                profileItem.Click += (s, args) => ViewModel.LaunchWithTerminalProfile(instance, capturedProfile);
                cmdSubmenu.Items.Add(profileItem);
            }
            flyout.Items.Add(cmdSubmenu);
        }
        else
        {
            var cmdItem = new MenuFlyoutItem
            {
                Text = "Launch VS CMD Prompt",
                Icon = new FontIcon { Glyph = "\uE756", Foreground = new SolidColorBrush(Color.FromArgb(255, 59, 130, 246)) }
            };
            cmdItem.Click += (s, args) => ViewModel.LaunchVsCmdCommand.Execute(instance);
            flyout.Items.Add(cmdItem);
        }

        if (pwshProfiles.Count > 0)
        {
            var pwshSubmenu = new MenuFlyoutSubItem
            {
                Text = "VS PowerShell",
                Icon = new FontIcon { Glyph = "\uE756", Foreground = new SolidColorBrush(Color.FromArgb(255, 6, 182, 212)) }
            };
            foreach (var profile in pwshProfiles)
            {
                var profileItem = new MenuFlyoutItem { Text = profile.Name };
                var capturedProfile = profile;
                profileItem.Click += (s, args) => ViewModel.LaunchWithTerminalProfile(instance, capturedProfile);
                pwshSubmenu.Items.Add(profileItem);
            }
            flyout.Items.Add(pwshSubmenu);
        }
        else
        {
            var pwshItem = new MenuFlyoutItem
            {
                Text = "Launch VS PowerShell",
                Icon = new FontIcon { Glyph = "\uE756", Foreground = new SolidColorBrush(Color.FromArgb(255, 6, 182, 212)) }
            };
            pwshItem.Click += (s, args) => ViewModel.LaunchVsPwshCommand.Execute(instance);
            flyout.Items.Add(pwshItem);
        }

        flyout.Items.Add(new MenuFlyoutSeparator());

        var installerSubmenu = new MenuFlyoutSubItem
        {
            Text = "Visual Studio Installer",
            Icon = new FontIcon { Glyph = "\uE895", Foreground = new SolidColorBrush(Color.FromArgb(255, 104, 33, 122)) }
        };

        var modifyItem = new MenuFlyoutItem
        {
            Text = "Modify Installation",
            Icon = new FontIcon { Glyph = "\uE70F" }
        };
        modifyItem.Click += (s, args) => ViewModel.ModifyVisualStudioInstanceCommand.Execute(instance);
        installerSubmenu.Items.Add(modifyItem);

        var updateItem = new MenuFlyoutItem
        {
            Text = "Update",
            Icon = new FontIcon { Glyph = "\uE896" }
        };
        updateItem.Click += (s, args) => ViewModel.UpdateVisualStudioInstanceCommand.Execute(instance);
        installerSubmenu.Items.Add(updateItem);

        installerSubmenu.Items.Add(new MenuFlyoutSeparator());

        var openInstallerItem = new MenuFlyoutItem
        {
            Text = "Open Installer",
            Icon = new FontIcon { Glyph = "\uE8E1" }
        };
        openInstallerItem.Click += (s, args) => ViewModel.LaunchVisualStudioInstallerCommand.Execute(instance);
        installerSubmenu.Items.Add(openInstallerItem);

        flyout.Items.Add(installerSubmenu);

        flyout.Items.Add(new MenuFlyoutSeparator());

        var appDataItemVS = new MenuFlyoutItem
        {
            Text = "Open Local AppData",
            Icon = new FontIcon { Glyph = "\uE8B7", Foreground = new SolidColorBrush(Color.FromArgb(255, 139, 92, 246)) }
        };
        appDataItemVS.Click += (s, args) => ViewModel.OpenAppDataFolderCommand.Execute(instance);
        flyout.Items.Add(appDataItemVS);
    }

    private void OnRowPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border border)
        {
            border.BorderBrush = AccentBrush;
            border.Background = HoverBackgroundBrush;
        }
    }

    private void OnRowPointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border border)
        {
            border.BorderBrush = TransparentBrush;
            border.Background = TransparentBrush;
        }
    }

    private void OnButtonPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Button button)
        {
            button.Opacity = 1.0;
        }
    }

    private void OnButtonPointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Button button)
        {
            button.Opacity = 0.6;
        }
    }

    private void OnTabChanged(object sender, RoutedEventArgs e)
    {
        // Skip if controls aren't loaded yet
        if (InstalledContent is null || SettingsContent is null || RefreshButton is null)
            return;

        if (sender is not RadioButton radioButton)
            return;

        var isInstalledTab = radioButton == InstalledTab;

        InstalledContent.Visibility = isInstalledTab ? Visibility.Visible : Visibility.Collapsed;
        SettingsContent.Visibility = isInstalledTab ? Visibility.Collapsed : Visibility.Visible;
        RefreshButton.Visibility = isInstalledTab ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnMinimizeClick(object sender, RoutedEventArgs e)
    {
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(((App)Application.Current).MainWindow);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);

        if (_settingsService.MinimizeToTray)
        {
            // Hide to system tray
            appWindow.Hide();
        }
        else if (appWindow.Presenter is OverlappedPresenter presenter)
        {
            // Normal minimize
            presenter.Minimize();
        }
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(((App)Application.Current).MainWindow);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);

        if (_settingsService.CloseToTray)
        {
            // Hide to system tray
            appWindow.Hide();
        }
        else
        {
            // Actually close the app
            Application.Current.Exit();
        }
    }

    private void OnLaunchOnStartupToggled(object sender, RoutedEventArgs e)
    {
        if (_isInitializingSettings) return;
        _settingsService.LaunchOnStartup = LaunchOnStartupToggle.IsOn;
    }

    private void OnStartMinimizedToggled(object sender, RoutedEventArgs e)
    {
        if (_isInitializingSettings) return;
        _settingsService.StartMinimized = StartMinimizedToggle.IsOn;
    }

    private void OnMinimizeToTrayToggled(object sender, RoutedEventArgs e)
    {
        if (_isInitializingSettings) return;
        _settingsService.MinimizeToTray = MinimizeToTrayToggle.IsOn;
    }

    private void OnCloseToTrayToggled(object sender, RoutedEventArgs e)
    {
        if (_isInitializingSettings) return;
        _settingsService.CloseToTray = CloseToTrayToggle.IsOn;
    }
}
