using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Contents;
using mcLaunch.Utilities;

namespace mcLaunch.Views.Popups;

public partial class OptionalModsPopup : UserControl
{
    private Action confirmCallback;
    private Action cancelCallback;
    
    public OptionalModsPopup()
    {
        InitializeComponent();
    }

    public OptionalModsPopup(Box box, MinecraftContent mod, MinecraftContentPlatform.ContentDependency[] dependencies, Action confirm, Action cancel)
    {
        InitializeComponent();

        confirmCallback = confirm;
        cancelCallback = cancel;

        DescriptionText.Text = DescriptionText.Text
            .Replace("$MOD", mod.Name);
        
        DownloadIconAndApplyAsync(box, mod, dependencies);
    }

    async void DownloadIconAndApplyAsync(Box box, MinecraftContent mod, MinecraftContentPlatform.ContentDependency[] dependencies)
    {
        ModList.SetLoadingCircle(true);
        
        foreach (MinecraftContentPlatform.ContentDependency dep in dependencies)
            dep.Content.DownloadIconAsync();

        ModList.SetLoadingCircle(false);
        ModList.SetModifications(dependencies.Select(dep => dep.Content).ToArray());
    }

    private void OKButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
        confirmCallback?.Invoke();
    }

    private void CancelButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
        cancelCallback?.Invoke();
    }
}