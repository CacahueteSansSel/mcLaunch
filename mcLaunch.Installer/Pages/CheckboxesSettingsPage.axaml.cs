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

    private void DesktopShortcutCheckbox_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        MainWindow.Instance.Parameters.RegisterInApplicationList = DesktopShortcutCheckbox.IsChecked!.Value;
    }

    private void RegisterProgramListCheckbox_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        MainWindow.Instance.Parameters.RegisterInApplicationList = RegisterProgramListCheckbox.IsChecked!.Value;
    }
}