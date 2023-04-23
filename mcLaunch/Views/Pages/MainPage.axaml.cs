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

    public void PopulateBoxList()
    {
        BoxContainer.Children.Clear();
        
        foreach (Box box in BoxManager.LoadLocalBoxes())
        {
            BoxContainer.Children.Add(new BoxEntryCard(box));
        }
    }
}