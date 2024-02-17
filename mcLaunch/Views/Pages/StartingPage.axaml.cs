using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace mcLaunch.Views.Pages;

public partial class StartingPage : UserControl, ITopLevelPageControl
{
    public string Title => "mcLaunch";
    
    public StartingPage()
    {
        InitializeComponent();
    }
}