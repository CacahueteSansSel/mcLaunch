using Avalonia.Controls;
using mcLaunch.Launchsite.Models;

namespace mcLaunch.Views;

public partial class SkinEntryCard : UserControl
{
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

    public MinecraftProfile.ModelSkin Skin { get; private set; }
}