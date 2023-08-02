using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Cacahuete.MinecraftLib.Models;
using mcLaunch.Core.Managers;
using mcLaunch.Utilities;

namespace mcLaunch.Views.Windows;

public partial class VersionSelectWindow : Window
{
    ManifestMinecraftVersion[] versions;
    ManifestMinecraftVersion selectedVersion;
    
    public VersionSelectWindow()
    {
        InitializeComponent();
        
#if DEBUG
        this.AttachDevTools();
#endif
        
        versions = Settings.Instance.EnableSnapshots
            ? MinecraftManager.Manifest!.Versions
            : MinecraftManager.ManifestVersions;

        DataContext = versions;
    }

    ManifestMinecraftVersion[] RunSearch(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return versions;

        string trimmedQuery = query.Trim();

        return versions
            .Where(v => v.Id.Contains(trimmedQuery) || v.Type.Contains(trimmedQuery))
            .ToArray();
    }
    
    private void VersionSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0)
        {
            selectedVersion = (ManifestMinecraftVersion) e.AddedItems[0];
            SelectButton.IsVisible = true;
        }
    }

    private void SelectVersionButtonClicked(object? sender, RoutedEventArgs e)
    {
        Close(selectedVersion);
    }

    private void SearchVersionTextBoxInput(object? sender, TextInputEventArgs e)
    {
    }

    private void SearchButtonClicked(object? sender, RoutedEventArgs e)
    {
        DataContext = RunSearch(SearchTextBox.Text);
    }
}