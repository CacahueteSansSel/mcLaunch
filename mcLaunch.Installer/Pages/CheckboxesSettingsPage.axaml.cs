using Avalonia.Interactivity;

namespace mcLaunch.Installer.Pages;

public partial class CheckboxesSettingsPage : InstallerPage
{
    public CheckboxesSettingsPage()
    {
        InitializeComponent();

        DesktopShortcutCheckbox.IsChecked = MainWindow.Instance.Parameters.PlaceShortcutOnDesktop;
        RegisterProgramListCheckbox.IsChecked = MainWindow.Instance.Parameters.RegisterInApplicationList;
    }

    private void DesktopShortcutCheckbox_OnChecked(object? sender, RoutedEventArgs e)
    {
        MainWindow.Instance.Parameters.PlaceShortcutOnDesktop = true;
    }

    private void DesktopShortcutCheckbox_OnUnchecked(object? sender, RoutedEventArgs e)
    {
        MainWindow.Instance.Parameters.PlaceShortcutOnDesktop = false;
    }

    private void RegisterProgramListCheckbox_OnChecked(object? sender, RoutedEventArgs e)
    {
        MainWindow.Instance.Parameters.RegisterInApplicationList = true;
    }

    private void RegisterProgramListCheckbox_OnUnchecked(object? sender, RoutedEventArgs e)
    {
        MainWindow.Instance.Parameters.RegisterInApplicationList = false;
    }
}