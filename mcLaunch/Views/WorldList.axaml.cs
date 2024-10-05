using System;
using System.Linq;
using System.Reactive;
using Avalonia.Controls;
using Avalonia.Threading;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.MinecraftFormats;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;
using mcLaunch.Views.Popups;
using mcLaunch.Views.Windows.NbtEditor;
using ReactiveUI;
using SharpNBT;

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

        SetWorlds(new []
        {
            new MinecraftWorld
            {
                Name = "Test world",
                GameMode = MinecraftGameMode.Creative,
                Icon = null,
                LastPlayed = DateTime.Now,
                IsCheats = true,
                Version = "1.21"
            }
        });
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

        ctx.Worlds = worlds.Select(w => new Data.ModelWorld(w)).ToArray();

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
            Data.ModelWorld world = (Data.ModelWorld) e.AddedItems[0];

            Navigation.ShowPopup(new ConfirmMessageBoxPopup($"Launch world {world.World.Name} ?",
                $"Minecraft will start and automatically launch the world {world.World.Name}",
                () => { launchPage.Run(world: world.World); }));
        }

        WorldsList.UnselectAll();
    }

    public class Data : ReactiveObject
    {
        private int page;
        private ModelWorld[] worlds;

        public ModelWorld[] Worlds
        {
            get => worlds;
            set => this.RaiseAndSetIfChanged(ref worlds, value);
        }

        public int Page
        {
            get => page;
            set => this.RaiseAndSetIfChanged(ref page, value);
        }

        public class ModelWorld : ReactiveObject
        {
            public MinecraftWorld World { get; set; }
            public bool ShowAdvancedFeatures => Settings.Instance?.ShowAdvancedFeatures ?? false;

            public ModelWorld(MinecraftWorld world)
            {
                World = world;
            }

            public void OpenLevelDatCommand()
            {
                string levelDatFilename = $"{World.WorldPath}/level.dat";
                
                new NbtEditorWindow(levelDatFilename).Show(MainWindow.Instance);
            }
        }
    }
}