using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.MinecraftFormats;

namespace mcLaunch.Views.Pages.BoxDetails;

public partial class WorldListSubControl : SubControl
{
    public override string Title => "WORLDS";
    
    public WorldListSubControl()
    {
        InitializeComponent();
    }
    
    public override async Task PopulateAsync()
    {
        WorldsList.SetLoadingCircle(true);
        WorldsList.SetLaunchPage(ParentPage);
        
        MinecraftWorld[] worlds = await Task.Run(() => Box.LoadWorlds());
        
        WorldsList.SetWorlds(worlds);
        WorldsList.SetLoadingCircle(false);

        DataContext = worlds.Length;
    }
}