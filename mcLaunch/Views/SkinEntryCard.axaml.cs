using System;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using mcLaunch.Core.Managers;
using mcLaunch.Core.Utilities;
using mcLaunch.Launchsite.Models;

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
}