using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages.Settings;

namespace mcLaunch.Views.Pages;

public partial class SettingsPage : UserControl
{
    public SettingsPage()
    {
        InitializeComponent();
        
        Utilities.Settings.Load();
        SetSettings(Utilities.Settings.Instance.GetAllGroups());
    }

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
}