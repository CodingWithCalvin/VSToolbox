using System.Drawing;
using System.Windows.Input;
using H.NotifyIcon;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CodingWithCalvin.VSToolbox.Services;

public sealed class TrayIconService : IDisposable
{
    private TaskbarIcon? _taskbarIcon;
    private Window? _window;

    public void Initialize(Window window)
    {
        _window = window;

        _taskbarIcon = new TaskbarIcon
        {
            ToolTipText = "Visual Studio Toolbox",
            ContextMenuMode = ContextMenuMode.SecondWindow,
            Icon = GetAppIcon()
        };

        // Set up context menu
        var contextMenu = new MenuFlyout();

        var showItem = new MenuFlyoutItem { Text = "Show" };
        showItem.Click += (_, _) => ShowWindow();
        contextMenu.Items.Add(showItem);

        contextMenu.Items.Add(new MenuFlyoutSeparator());

        var exitItem = new MenuFlyoutItem { Text = "Exit" };
        exitItem.Click += (_, _) => ExitApplication();
        contextMenu.Items.Add(exitItem);

        _taskbarIcon.ContextFlyout = contextMenu;

        // Handle tray icon click
        _taskbarIcon.LeftClickCommand = new SimpleCommand(ShowWindow);

        // Create and set the icon
        _taskbarIcon.ForceCreate();
    }

    private static Icon? GetAppIcon()
    {
        // Try to load the VS icon from Assets
        var appDir = AppContext.BaseDirectory;
        var iconPath = Path.Combine(appDir, "Assets", "vs2026_icon.png");

        if (File.Exists(iconPath))
        {
            try
            {
                using var bitmap = new Bitmap(iconPath);
                var hIcon = bitmap.GetHicon();
                return Icon.FromHandle(hIcon);
            }
            catch
            {
                // Fall back to default
            }
        }

        return SystemIcons.Application;
    }

    public void ShowWindow()
    {
        _window?.Activate();
    }

    private void ExitApplication()
    {
        Dispose();
        Application.Current.Exit();
    }

    public void Dispose()
    {
        _taskbarIcon?.Dispose();
        _taskbarIcon = null;
    }

    private sealed class SimpleCommand(Action execute) : ICommand
    {
#pragma warning disable CS0067 // Event is never used - required by ICommand interface
        public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => execute();
    }
}
