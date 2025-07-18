using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using mcLaunch.Core.Core;
using mcLaunch.Core.Managers;
using mcLaunch.Core.Utilities;
using mcLaunch.Launchsite.Models;
using mcLaunch.Utilities;
using mcLaunch.Views.Popups;

namespace mcLaunch.Views.Pages;

public partial class SkinsPage : UserControl, ITopLevelPageControl
{
    public static SkinsPage Current { get; private set; } = null!;
    
    public SkinsPage()
    {
        InitializeComponent();

        LoadLocalSkins();
        FetchCurrentSkin();
    }

    public string Title => "Manage skins";

    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        Current = this;
    }

    private async void LoadLocalSkins()
    {
        if (Design.IsDesignMode) return;
        
        SkinsManager.LoadAllSkins();
        
        SkinCardsContainer.Children.RemoveAll(SkinCardsContainer.Children.Where(card => card is SkinEntryCard));
        
        foreach (ManifestSkin skin in SkinsManager.Skins)
        {
            SkinEntryCard card = new(skin);
            SkinCardsContainer.Children.Add(card);
        }
    }

    public async void FetchCurrentSkin()
    {
        MinecraftProfile? profile = await AuthenticationManager.FetchProfileAsync();
        if (profile == null) return;
        if (profile.Skins.Length == 0) return;

        MinecraftProfile.ModelSkin skin = profile.Skins[0];
        SkinType type = Enum.Parse<SkinType>(skin.Variant, true);
        
        CurrentSkinPreview.Texture = (await BitmapUtilities.LoadBitmapAsync(skin.Url, 512, BitmapInterpolationMode.None))!;
        CurrentSkinPreview.Type = type;
        
        CurrentSkinPreview.InvalidateVisual();
    }

    private void AddSkinButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.ShowPopup(new SelectSkinPopup(async (filename, name, type) =>
        {
            Navigation.ShowPopup(new StatusPopup("Uploading skin...", "Uploading the skin to Minecraft..."));
            StatusPopup.Instance.StatusIndeterminate = true;
            StatusPopup.Instance.Status = "Uploading...";
            
            await SkinsManager.AddSkin(filename, name, type);
            
            StatusPopup.Instance.Status = "Loading local skins...";
            LoadLocalSkins();
            
            Navigation.HidePopup();
        }));
    }
}