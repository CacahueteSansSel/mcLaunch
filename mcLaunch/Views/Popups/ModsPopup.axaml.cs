using Avalonia.Controls;
using Avalonia.Interactivity;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Contents;
using mcLaunch.Utilities;

namespace mcLaunch.Views.Popups;

public partial class ModsPopup : UserControl
{
    public ModsPopup()
    {
        InitializeComponent();
    }

    public ModsPopup(string title, string text, Box box, MinecraftContent[] mods)
    {
        InitializeComponent();

        TitleText.Text = title;
        DescriptionText.Text = text;

        DownloadIconAndApplyAsync(box, mods);
    }

    private async void DownloadIconAndApplyAsync(Box box, MinecraftContent[] mods)
    {
        ModList.SetLoadingCircle(true);

        foreach (MinecraftContent mod in mods)
            await mod.DownloadIconAsync();

        ModList.SetLoadingCircle(false);
        ModList.SetContents(mods);
    }

    private void OKButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
    }
}