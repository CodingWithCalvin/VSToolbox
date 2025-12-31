using System.Runtime.InteropServices;
using System.Threading;
using CodingWithCalvin.VSToolbox.Services;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Navigation;
using Windows.Graphics;

namespace CodingWithCalvin.VSToolbox;

// Windows API for setting window corner preference
internal static class NativeMethods
{
    [DllImport("dwmapi.dll")]
    internal static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    [DllImport("user32.dll")]
    internal static extern int GetWindowLong(IntPtr hwnd, int nIndex);

    [DllImport("user32.dll")]
    internal static extern int SetWindowLong(IntPtr hwnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    internal static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    internal const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
    internal const int DWMWCP_DONOTROUND = 1;
    internal const int DWMWA_CAPTION_COLOR = 35;
    internal const int DWMWA_BORDER_COLOR = 34;

    internal const int GWL_STYLE = -16;
    internal const int WS_CAPTION = 0x00C00000;
    internal const int WS_THICKFRAME = 0x00040000;
    internal const uint SWP_FRAMECHANGED = 0x0020;
    internal const uint SWP_NOMOVE = 0x0002;
    internal const uint SWP_NOSIZE = 0x0001;
    internal const uint SWP_NOZORDER = 0x0004;

    // For finding and showing existing window
    internal delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    internal static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    internal static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    internal static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    internal static extern bool SetForegroundWindow(IntPtr hWnd);

    internal const int SW_RESTORE = 9;
    internal const int SW_SHOW = 5;
}

public partial class App : Application
{
    private const string MutexName = "CodingWithCalvin.VSToolbox.SingleInstance";
    private static Mutex? _mutex;
    private Window? _window;
    private AppWindow? _appWindow;
    private TrayIconService? _trayIconService;
    private SettingsService? _settingsService;

    public Window? MainWindow => _window;
    public SettingsService Settings => _settingsService ??= new SettingsService();

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs e)
    {
        // Check for single instance
        _mutex = new Mutex(true, MutexName, out var createdNew);
        if (!createdNew)
        {
            // Another instance is already running - try to bring it to front
            BringExistingInstanceToFront();
            Environment.Exit(0);
            return;
        }

        _window = new Window
        {
            Title = "Visual Studio Toolbox"
        };

        // Get the AppWindow for advanced window control
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_window);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        _appWindow = AppWindow.GetFromWindowId(windowId);

        // Set the window/taskbar icon
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "vs2026_icon.ico");
        if (File.Exists(iconPath))
        {
            _appWindow.SetIcon(iconPath);
        }

        // Configure custom title bar with square corners
        ConfigureCustomTitleBar();

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

        // Only show window if not starting minimized
        if (!Settings.StartMinimized)
        {
            _window.Activate();
        }
    }

    private void ConfigureCustomTitleBar()
    {
        if (_appWindow is null || _window is null) return;

        // Get the window handle for native API calls
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_window);

        // Set square corners using DWM API
        var cornerPreference = NativeMethods.DWMWCP_DONOTROUND;
        NativeMethods.DwmSetWindowAttribute(hwnd, NativeMethods.DWMWA_WINDOW_CORNER_PREFERENCE,
            ref cornerPreference, sizeof(int));

        // Set caption and border color to purple (#68217A = 0x007A2168 in COLORREF BGR format)
        var purpleColor = 0x007A2168; // BGR format for #68217A
        NativeMethods.DwmSetWindowAttribute(hwnd, NativeMethods.DWMWA_CAPTION_COLOR,
            ref purpleColor, sizeof(int));
        NativeMethods.DwmSetWindowAttribute(hwnd, NativeMethods.DWMWA_BORDER_COLOR,
            ref purpleColor, sizeof(int));

        // Remove the caption from window style to eliminate the title bar area
        var style = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_STYLE);
        style &= ~NativeMethods.WS_CAPTION; // Remove caption
        NativeMethods.SetWindowLong(hwnd, NativeMethods.GWL_STYLE, style);
        NativeMethods.SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0,
            NativeMethods.SWP_FRAMECHANGED | NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOZORDER);

        // Make window borderless (no system title bar at all)
        if (_appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.SetBorderAndTitleBar(false, false);
            presenter.IsResizable = true;
            presenter.IsMaximizable = false;
        }
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
        if (Settings.CloseToTray)
        {
            // Prevent window from closing - hide it instead
            args.Cancel = true;

            // Hide the window
            _appWindow?.Hide();
        }
        else
        {
            // Actually close the app
            _trayIconService?.Dispose();
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
        }
    }

    private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new InvalidOperationException($"Failed to load Page {e.SourcePageType.FullName}");
    }

    private static void BringExistingInstanceToFront()
    {
        const string windowTitle = "Visual Studio Toolbox";
        IntPtr foundWindow = IntPtr.Zero;

        NativeMethods.EnumWindows((hWnd, lParam) =>
        {
            var sb = new System.Text.StringBuilder(256);
            NativeMethods.GetWindowText(hWnd, sb, sb.Capacity);
            if (sb.ToString() == windowTitle)
            {
                foundWindow = hWnd;
                return false; // Stop enumeration
            }
            return true; // Continue enumeration
        }, IntPtr.Zero);

        if (foundWindow != IntPtr.Zero)
        {
            // Show and bring the window to front
            NativeMethods.ShowWindow(foundWindow, NativeMethods.SW_RESTORE);
            NativeMethods.SetForegroundWindow(foundWindow);
        }
    }
}
