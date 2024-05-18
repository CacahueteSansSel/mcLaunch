using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Contents;
using mcLaunch.Utilities;

namespace mcLaunch.Views.Popups;

public partial class OptionalModsPopup : UserControl
{
    private readonly Action cancelCallback;
    private readonly Action confirmCallback;

    public OptionalModsPopup()
    {
        InitializeComponent();
    }

    public OptionalModsPopup(Box box, MinecraftContent mod, MinecraftContentPlatform.ContentDependency[] dependencies,
        Action confirm, Action cancel)
    {
        InitializeComponent();

        confirmCallback = confirm;
        cancelCallback = cancel;

        DescriptionText.Text = DescriptionText.Text
            .Replace("$MOD", mod.Name);

        DownloadIconAndApplyAsync(box, mod, dependencies);
    }

    private async void DownloadIconAndApplyAsync(Box box, MinecraftContent mod,
        MinecraftContentPlatform.ContentDependency[] dependencies)
    {
        ModList.SetLoadingCircle(true);

        ModList.SetLoadingCircle(false);
        ModList.SetContents(dependencies.Select(dep => dep.Content).ToArray());
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