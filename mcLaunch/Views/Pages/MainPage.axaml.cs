using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using mcLaunch.Core.Boxes;
using mcLaunch.Managers;

namespace mcLaunch.Views.Pages;

public partial class MainPage : UserControl, ITopLevelPageControl
{
    private AnonymitySession anonSession;
    private List<Box>? loadedBoxes;

    public MainPage()
    {
        Instance = this;

        InitializeComponent();
        anonSession = AnonymityManager.CreateSession();

        PopulateBoxList();
    }

    public static MainPage Instance { get; private set; }
    public string Title => "Your boxes";

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        DiscordManager.SetPresenceBoxList();
    }

    public async void PopulateBoxList(string? query = null, bool reloadAll = true)
    {
        if (reloadAll || loadedBoxes == null)
        {
            loadedBoxes = (await Task.Run(() => BoxManager.LoadLocalBoxes())).ToList();
            loadedBoxes.Sort((l, r) => -l.Manifest.LastLaunchTime.CompareTo(r.Manifest.LastLaunchTime));
        }

        if (string.IsNullOrWhiteSpace(query))
        {
            BoxesContainer.ItemsSource = loadedBoxes;

            return;
        }

        BoxesContainer.ItemsSource = loadedBoxes.Where(box => box.MatchesQuery(query));
    }

    private void SearchBoxTextChanged(object? sender, TextChangedEventArgs e)
    {
        PopulateBoxList(SearchBox.Text, false);
    }
}