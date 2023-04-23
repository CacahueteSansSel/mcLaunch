using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.MinecraftFormats;

namespace mcLaunch.Views.Pages.BoxDetails;

public partial class WorldListSubControl : UserControl, ISubControl
{
    public WorldListSubControl()
    {
        InitializeComponent();
    }

    public BoxDetailsPage ParentPage { get; set; }
    public Box Box { get; set; }
    public string Title { get; } = "WORLDS";
    
    public async Task PopulateAsync()
    {
        WorldsList.SetLoadingCircle(true);
        
        MinecraftWorld[] worlds = await Task.Run(() => Box.LoadWorlds());
        
        WorldsList.SetWorlds(worlds);
        WorldsList.SetLoadingCircle(false);

        DataContext = worlds.Length;
    }
}