using Avalonia.Controls;
using Avalonia.Interactivity;
using mcLaunch.Utilities;

namespace mcLaunch.Views.Popups;

public partial class LauncherUpdatedPopup : UserControl
{
    public LauncherUpdatedPopup()
    {
        InitializeComponent();

        VersionText.Text = $"v{CurrentBuild.Version.ToString(3)}";
        ChangelogTextBox.Text = string.Join("\n", CurrentBuild.Changelog);
    }

    private void CloseButtonClicked(object? sender, RoutedEventArgs e)
    {
        Settings.MarkCurrentVersionAsSeen();
        Navigation.HidePopup();
    }
}