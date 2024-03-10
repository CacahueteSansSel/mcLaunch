using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using mcLaunch.Launchsite.Models;
using mcLaunch.Core.Managers;
using mcLaunch.Utilities;

namespace mcLaunch.Views.Windows;

public partial class VersionSelectWindow : Window
{
    private ManifestMinecraftVersion? selectedVersion;
    private readonly ManifestMinecraftVersion[] versions;

    public VersionSelectWindow()
    {
        InitializeComponent();

        versions = Settings.Instance.EnableSnapshots
            ? MinecraftManager.Manifest!.Versions
            : MinecraftManager.ManifestVersions;

        DataContext = versions;
    }

    public VersionSelectWindow(string title) : this()
    {
        Title = title;
    }

    private ManifestMinecraftVersion[] RunSearch(string? query)
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
        if (selectedVersion != null)
            Close(selectedVersion);
        
        ManifestMinecraftVersion? exactMatch = versions
            .FirstOrDefault(v => v.Id.Trim() == SearchTextBox.Text?.Trim());

        if (exactMatch != null)
            Close(exactMatch);
    }

    private void SearchVersionTextBoxTextChanged(object? sender, TextChangedEventArgs e)
    {
        ManifestMinecraftVersion? exactMatch = versions
            .FirstOrDefault(v => v.Id.Trim() == SearchTextBox.Text.Trim());

        if (exactMatch != null)
        {
            ModList.SelectedItem = exactMatch;
            SelectButton.IsVisible = true;
        }
        else
        {
            ModList.UnselectAll();
            SelectButton.IsVisible = false;
        }

        DataContext = RunSearch(SearchTextBox.Text);
    }

    private void VersionListDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (selectedVersion != null)
            Close(selectedVersion);
    }
}