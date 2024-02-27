using Avalonia.Controls;

namespace mcLaunch.Views.Pages;

public partial class StartingPage : UserControl, ITopLevelPageControl
{
    public StartingPage()
    {
        InitializeComponent();
    }

    public string Title => "mcLaunch";
}