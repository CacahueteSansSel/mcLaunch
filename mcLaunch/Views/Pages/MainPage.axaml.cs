using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using mcLaunch.Core.Boxes;
using mcLaunch.Managers;

namespace mcLaunch.Views.Pages;

public partial class MainPage : UserControl, ITopLevelPageControl
{
    public static MainPage Instance { get; private set; }
    public string Title => $"Your boxes";
    
    private AnonymitySession anonSession;

    public MainPage()
    {
        Instance = this;
        
        InitializeComponent();
        anonSession = AnonymityManager.CreateSession();

        PopulateBoxList();
    }

    public async void PopulateBoxList()
    {
        BoxContainer.Children.Clear();

        List<Box> boxes = new List<Box>(await Task.Run(() => BoxManager.LoadLocalBoxes()));
        boxes.Sort((l, r) => -l.Manifest.LastLaunchTime.CompareTo(r.Manifest.LastLaunchTime));
        
        foreach (Box box in boxes)
        {
            BoxContainer.Children.Add(new BoxEntryCard(box, anonSession));
        }
    }
}