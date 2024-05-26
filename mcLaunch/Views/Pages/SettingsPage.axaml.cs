using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages.Settings;
using mcLaunch.Views.Windows;

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

        int years = (int) MathF.Floor((float) (DateTime.Now - Constants.McLaunchBirthDate).TotalDays / 365);
        YearsText.Text = $"{years} year{(years > 1 ? "s" : "")} old";
    }

    public string Title => "Settings";

    public void SetSettings(SettingsGroup[] groups)
    {
        SettingsRoot.Children.Clear();

        foreach (SettingsGroup group in groups)
        {
            SettingsSection section = new SettingsSection(group);
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
}