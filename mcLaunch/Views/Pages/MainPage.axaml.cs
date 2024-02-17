using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using mcLaunch.Core.Boxes;
using mcLaunch.Managers;
using ReactiveUI;

namespace mcLaunch.Views.Pages;

public partial class MainPage : UserControl, ITopLevelPageControl
{
    public static MainPage Instance { get; private set; }
    public string Title => $"Your boxes";

    private AnonymitySession anonSession;
    private List<Box>? loadedBoxes;

    public MainPage()
    {
        Instance = this;

        InitializeComponent();
        anonSession = AnonymityManager.CreateSession();

        PopulateBoxList();
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
            BoxesContainer.Children.Clear();
            BoxesContainer.Children.AddRange(loadedBoxes.Select(box => new BoxEntryCard(box, anonSession)));

            return;
        }

        BoxesContainer.Children.Clear();
        BoxesContainer.Children.AddRange(loadedBoxes.Where(box => box.MatchesQuery(query))
            .Select(box => new BoxEntryCard(box, anonSession)));
    }
}