using System.Threading.Tasks;
using mcLaunch.Core.MinecraftFormats;

namespace mcLaunch.Views.Pages.BoxDetails;

public partial class WorldListSubControl : SubControl
{
    public WorldListSubControl()
    {
        InitializeComponent();
    }

    public override string Title => "WORLDS";

    public override async Task PopulateAsync()
    {
        QuickPlayBanner.IsVisible = Box.SupportsQuickPlay;

        WorldsList.SetLoadingCircle(true);
        WorldsList.SetLaunchPage(ParentPage);

        MinecraftWorld[] worlds = await Task.Run(() => Box.LoadWorlds());

        WorldsList.SetWorlds(worlds);
        WorldsList.SetLoadingCircle(false);

        DataContext = worlds.Length;
    }
}