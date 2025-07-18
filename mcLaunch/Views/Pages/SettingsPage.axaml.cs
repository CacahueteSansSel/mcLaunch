using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using mcLaunch.Managers;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages.Settings;
using mcLaunch.Views.Popups;
using mcLaunch.Views.Windows;
using mcLaunch.Views.Windows.NbtEditor;

namespace mcLaunch.Views.Pages;

public partial class SettingsPage : UserControl, ITopLevelPageControl
{
    public SettingsPage()
    {
        InitializeComponent();

        Utilities.Settings.Load();
        SetSettings(Utilities.Settings.Instance.GetAllGroups());

        VersionText.Text = CurrentBuild.Version.ToString();
        CommitText.Text = CurrentBuild.Commit;
        BranchNameText.Text = CurrentBuild.Branch;

#if !DEBUG
        CrashButton.IsVisible = false;
        ConsoleButton.IsVisible = false;
        NbtButton.IsVisible = false;
#endif

        int years = (int)MathF.Floor((float)(DateTime.Now - Constants.McLaunchBirthDate).TotalDays / 365);
        YearsText.Text = $"{years} year{(years > 1 ? "s" : "")} old";
    }

    public string Title => "Settings";

    public void SetSettings(SettingsGroup[] groups)
    {
        SettingsRoot.Children.Clear();

        foreach (SettingsGroup group in groups)
        {
            SettingsSection section = new(group);
            SettingsRoot.Children.Add(section);
        }
    }

    private void OpenGithubRepoButtonClicked(object? sender, RoutedEventArgs e)
    {
        PlatformSpecific.OpenUrl("https://github.com/CacahueteSansSel/mcLaunch");
    }

    private void CrashLauncherButtonClicked(object? sender, RoutedEventArgs e)
    {
        throw new Exception("That's an user-triggered exception");
    }

    private void AboutButtonClicked(object? sender, RoutedEventArgs e)
    {
        new AboutWindow().Show(MainWindow.Instance);
    }

    private async void UpdateInstallerButtonClicked(object? sender, RoutedEventArgs e)
    {
        await UpdateManager.UpdateInstallerAsync();
    }

    private void UpButtonClicked(object? sender, RoutedEventArgs e)
    {
        ScrollArea.Offset = Vector.Zero;
    }

    private void ConsoleButtonClicked(object? sender, RoutedEventArgs e)
    {
        new ConsoleWindow().Show();
    }

    private void NbtEditorButtonClicked(object? sender, RoutedEventArgs e)
    {
        new NbtEditorWindow("level.dat").Show();
    }

    private async void ReinstallButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (!await UpdateManager.UpdateAsync())
        {
            Navigation.ShowPopup(new MessageBoxPopup("Error",
                "Reinstall failed. Download the build on the GitHub repository manually.", MessageStatus.Error));
        }
    }
}