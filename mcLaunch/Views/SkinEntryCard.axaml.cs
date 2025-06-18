using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using mcLaunch.Core.Core;
using mcLaunch.Core.Managers;
using mcLaunch.Core.Utilities;
using mcLaunch.Launchsite.Models;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;
using mcLaunch.Views.Popups;

namespace mcLaunch.Views;

public partial class SkinEntryCard : UserControl
{
    public ManifestSkin Skin { get; private set; }
    
    public SkinEntryCard()
    {
        InitializeComponent();
    }

    public SkinEntryCard(ManifestSkin skin)
    {
        InitializeComponent();

        Skin = skin;
        DataContext = skin;
        
        LoadSkinAsync();
    }

    async void LoadSkinAsync()
    {
        SkinPreview.Texture = Skin.Bitmap;
        SkinPreview.InvalidateVisual();

        SkinNameText.Text = Skin.Name;
    }

    private async void ApplySkinButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.ShowPopup(new StatusPopup("Applying skin...", "Requesting to change the skin..."));
        StatusPopup.Instance.StatusIndeterminate = true;
        
        MinecraftProfile? profile = await MinecraftServices.ChangeSkinAsync(Skin.Url, Skin.Type);
        
        Navigation.HidePopup();

        if (profile != null)
        {
            Navigation.ShowPopup(new MessageBoxPopup("Skin applied", "The skin was applied", MessageStatus.Success));
            SkinsPage.Current.FetchCurrentSkin();
        }
        else
        {
            Navigation.ShowPopup(new MessageBoxPopup("Error", "The skin failed to apply", MessageStatus.Error));
        }
    }
}