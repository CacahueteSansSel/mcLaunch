using Avalonia.Controls;
using Avalonia.Interactivity;
using mcLaunch.Core.Managers;
using mcLaunch.Core.Utilities;
using mcLaunch.Launchsite.Models;
using mcLaunch.Utilities;
using mcLaunch.Views.Popups;

namespace mcLaunch.Views.Pages;

public partial class SkinsPage : UserControl, ITopLevelPageControl
{
    public SkinsPage()
    {
        InitializeComponent();

        LoadLocalSkins();
        FetchSkins();
    }

    public string Title => "Manage skins";

    private async void LoadLocalSkins()
    {
        SkinsManager.LoadAllSkins();
        
        foreach (ManifestSkin skin in SkinsManager.Skins)
        {
            SkinEntryCard card = new(skin);
            SkinCardsContainer.Children.Add(card);
        }
    }

    private async void FetchSkins()
    {
        MinecraftProfile? profile = await AuthenticationManager.FetchProfileAsync();
        if (profile == null) return;
        if (profile.Skins.Length == 0) return;

        MinecraftProfile.ModelSkin skin = profile.Skins[0];
        CurrentSkinPreview.Texture = (await BitmapUtilities.LoadBitmapAsync(skin.Url, 64))!;
        CurrentSkinPreview.InvalidateVisual();
    }

    private void AddSkinButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.ShowPopup(new SelectSkinPopup());
    }
}