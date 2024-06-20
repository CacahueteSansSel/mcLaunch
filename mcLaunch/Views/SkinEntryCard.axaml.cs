using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using mcLaunch.Launchsite.Models;

namespace mcLaunch.Views;

public partial class SkinEntryCard : UserControl
{
    public MinecraftProfile.ModelSkin Skin { get; private set; }
    
    public SkinEntryCard()
    {
        InitializeComponent();
    }

    public SkinEntryCard(MinecraftProfile.ModelSkin skin)
    {
        InitializeComponent();

        Skin = skin;
        DataContext = skin;
    }
}