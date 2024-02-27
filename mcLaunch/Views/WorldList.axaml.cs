using Avalonia.Controls;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.MinecraftFormats;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;
using mcLaunch.Views.Popups;
using ReactiveUI;

namespace mcLaunch.Views;

public partial class WorldList : UserControl
{
    private Box lastBox;
    private string lastQuery;
    private BoxDetailsPage launchPage;

    public WorldList()
    {
        InitializeComponent();

        DataContext = new Data();

        /*
        SetWorlds(new []
        {
            new MinecraftWorld
            {
                Name = "Survie hardcore",
                GameMode = MinecraftGameMode.Creative,
                Icon = null,
                LastPlayed = DateTime.Now,
                IsCheats = true,
                Version = "1.19.84"
            }
        });
        */
    }

    public bool HideInstalledBadges { get; set; }

    public void SetBox(Box box)
    {
        lastBox = box;
    }

    public void SetLaunchPage(BoxDetailsPage page)
    {
        launchPage = page;
    }

    public void SetWorlds(MinecraftWorld[] worlds)
    {
        Data ctx = (Data) DataContext;

        ctx.Worlds = worlds;

        NtsBanner.IsVisible = worlds.Length == 0;
    }

    public void SetLoadingCircle(bool isLoading)
    {
        LoadCircle.IsVisible = isLoading;
    }

    private void WorldSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && launchPage != null && launchPage.Box.SupportsQuickPlay)
        {
            MinecraftWorld world = (MinecraftWorld) e.AddedItems[0];

            Navigation.ShowPopup(new ConfirmMessageBoxPopup($"Launch world {world.Name} ?",
                $"Minecraft will start and automatically launch the world {world.Name}",
                () => { launchPage.Run(world: world); }));
        }

        WorldsList.UnselectAll();
    }

    public class Data : ReactiveObject
    {
        private int page;
        private MinecraftWorld[] worlds;

        public MinecraftWorld[] Worlds
        {
            get => worlds;
            set => this.RaiseAndSetIfChanged(ref worlds, value);
        }

        public int Page
        {
            get => page;
            set => this.RaiseAndSetIfChanged(ref page, value);
        }
    }
}