using System.Runtime.InteropServices;
using CodingWithCalvin.VSToolbox.Services;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Navigation;
using Windows.Graphics;

namespace CodingWithCalvin.VSToolbox;

public partial class App : Application
{
    private Window? _window;
    private AppWindow? _appWindow;
    private TrayIconService? _trayIconService;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs e)
    {
        _window = new Window
        {
            Title = "Visual Studio Toolbox"
        };

        // Get the AppWindow for advanced window control
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_window);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        _appWindow = AppWindow.GetFromWindowId(windowId);

        // Set up the main content
        var rootFrame = new Frame();
        rootFrame.NavigationFailed += OnNavigationFailed;
        _window.Content = rootFrame;

        _ = rootFrame.Navigate(typeof(MainPage), e.Arguments);

        // Initialize system tray
        _trayIconService = new TrayIconService();
        _trayIconService.Initialize(_window);

        // Handle window close to minimize to tray instead
        _appWindow.Closing += OnAppWindowClosing;

        // Set window size and position to bottom-right
        PositionWindowBottomRight(540, 600);

        _window.Activate();
    }

    private void PositionWindowBottomRight(int width, int height)
    {
        if (_appWindow is null) return;

        // Get the display area for the window
        var displayArea = DisplayArea.GetFromWindowId(_appWindow.Id, DisplayAreaFallback.Primary);
        var workArea = displayArea.WorkArea;

        // Calculate bottom-right position with some padding
        var padding = 12;
        var x = workArea.X + workArea.Width - width - padding;
        var y = workArea.Y + workArea.Height - height - padding;

        _appWindow.MoveAndResize(new RectInt32(x, y, width, height));
    }

    private void OnAppWindowClosing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        // Prevent window from closing - hide it instead
        args.Cancel = true;

        // Hide the window
        _appWindow?.Hide();
    }

    private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new InvalidOperationException($"Failed to load Page {e.SourcePageType.FullName}");
    }
}
