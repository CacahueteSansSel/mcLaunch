using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using mcLaunch.Core.Boxes;

namespace mcLaunch.Views.Pages;

public partial class MainPage : UserControl
{
    public static MainPage Instance { get; private set; }

    public MainPage()
    {
        Instance = this;
        
        InitializeComponent();

        PopulateBoxList();
    }

    public async void PopulateBoxList()
    {
        BoxContainer.Children.Clear();

        List<Box> boxes = new List<Box>(await Task.Run(() => BoxManager.LoadLocalBoxes()));
        boxes.Sort((l, r) => -l.Manifest.LastLaunchTime.CompareTo(r.Manifest.LastLaunchTime));
        
        foreach (Box box in boxes)
        {
            BoxContainer.Children.Add(new BoxEntryCard(box));
        }
    }
}