using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using mcLaunch.Core.Managers;
using mcLaunch.Launchsite.Models;

namespace mcLaunch.Views.Pages;

public partial class SkinsPage : UserControl, ITopLevelPageControl
{
    public string Title => "Manage skins";

    public SkinsPage()
    {
        InitializeComponent();
        
        FetchSkins();
    }

    async void FetchSkins()
    {
        SkinCardsContainer.Children.Clear();

        MinecraftProfile? profile = await AuthenticationManager.FetchProfileAsync();
        if (profile == null) return;

        foreach (MinecraftProfile.ModelSkin skin in profile.Skins)
        {
            SkinEntryCard card = new(skin);
            SkinCardsContainer.Children.Add(card);
        }
    }
}