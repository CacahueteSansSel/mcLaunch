using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ddLaunch.Core.Boxes;

namespace ddLaunch.Views.Pages;

public partial class ModSearchPage : UserControl
{
    public Box Box { get; }

    public ModSearchPage()
    {
        
    }
    
    public ModSearchPage(Box box)
    {
        InitializeComponent();

        Box = box;
        DataContext = Box;
        
        ModList.Search(box, "");
    }
}