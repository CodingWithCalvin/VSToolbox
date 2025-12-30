using CodingWithCalvin.VSToolbox.Core.Models;
using CodingWithCalvin.VSToolbox.Services;
using CodingWithCalvin.VSToolbox.ViewModels;
using Microsoft.UI;
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

    public MainPage()
    {
        InitializeComponent();
        ViewModel = new MainViewModel();
    }

    public MainViewModel ViewModel { get; }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
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

        // Get the launchable instance from the button's DataContext
        var button = flyout.Target as Button;
        if (button?.DataContext is not LaunchableInstance instance)
            return;

        flyout.Items.Clear();

        // Open Explorer
        var openExplorerItem = new MenuFlyoutItem
        {
            Text = "Open Explorer",
            Icon = new FontIcon { Glyph = "\uE838", Foreground = new SolidColorBrush(Color.FromArgb(255, 234, 179, 8)) }
        };
        openExplorerItem.Click += (s, args) => ViewModel.OpenInstanceFolderCommand.Execute(instance);
        flyout.Items.Add(openExplorerItem);

        // Terminal profiles grouped by shell type
        var terminalProfiles = ViewModel.TerminalProfiles;
        var cmdProfiles = terminalProfiles.Where(p => p.ShellType == ShellType.Cmd).ToList();
        var pwshProfiles = terminalProfiles.Where(p => p.ShellType == ShellType.PowerShell).ToList();

        // CMD: submenu if profiles exist, otherwise direct launch
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

        // PowerShell: submenu if profiles exist, otherwise direct launch
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

        // Separator
        flyout.Items.Add(new MenuFlyoutSeparator());

        // Open Local AppData
        var appDataItem = new MenuFlyoutItem
        {
            Text = "Open Local AppData",
            Icon = new FontIcon { Glyph = "\uE8B7", Foreground = new SolidColorBrush(Color.FromArgb(255, 139, 92, 246)) }
        };
        appDataItem.Click += (s, args) => ViewModel.OpenAppDataFolderCommand.Execute(instance);
        flyout.Items.Add(appDataItem);
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
}
