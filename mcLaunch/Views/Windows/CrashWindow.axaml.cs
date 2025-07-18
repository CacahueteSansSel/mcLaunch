using Avalonia.Controls;
using Avalonia.Interactivity;
using mcLaunch.Utilities;

namespace mcLaunch.Views.Windows;

public partial class CrashWindow : Window
{
    public CrashWindow()
    {
        InitializeComponent();
        VersionText.Text = CurrentBuild.Version.ToString();
        CommitText.Text = CurrentBuild.Commit;
        DataContext = "none, actually";
    }

    public CrashWindow(string crashReportText)
    {
        InitializeComponent();

        VersionText.Text = CurrentBuild.Version.ToString();
        CommitText.Text = CurrentBuild.Commit;
        DataContext = crashReportText;
    }

    private void CopyButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext == null) return;

        Clipboard.SetTextAsync((string)DataContext);
    }

    private void RestartButtonClicked(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ReportToGitHubButtonClicked(object? sender, RoutedEventArgs e)
    {
        PlatformSpecific.OpenUrl("https://github.com/CacahueteSansSel/mcLaunch/issues/new");
    }
}