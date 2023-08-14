using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using mcLaunch.Core.Managers;
using mcLaunch.Core.Mods;

namespace mcLaunch.Views.Pages;

public partial class BrowseModpacksPage : UserControl
{
    public BrowseModpacksPage()
    {
        InitializeComponent();
        
        if (!Design.IsDesignMode) LoadModpacksAsync();
    }

    async void LoadModpacksAsync()
    {
        PlatformModpack[] packs = await ModPlatformManager.Platform.GetModpacksAsync(0, "", "");

        foreach (PlatformModpack modpack in packs)
        {
            BoxContainer.Children.Add(new ModpackEntryCard(modpack));
        }
    }
}