using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Mods;
using mcLaunch.Utilities;

namespace mcLaunch.Views.Popups;

public partial class ModsPopup : UserControl
{
    public ModsPopup()
    {
        InitializeComponent();
    }

    public ModsPopup(string title, string text, Box box, Modification[] mods)
    {
        InitializeComponent();

        TitleText.Text = title;
        DescriptionText.Text = text;
        
        DownloadIconAndApplyAsync(box, mods);
    }

    async void DownloadIconAndApplyAsync(Box box, Modification[] mods)
    {
        ModList.SetLoadingCircle(true);
        
        foreach (Modification mod in mods)
            await mod.DownloadIconAsync();

        ModList.SetLoadingCircle(false);
        ModList.SetModifications(mods);
    }

    private void OKButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
    }
}