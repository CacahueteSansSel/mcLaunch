using Avalonia.Controls;

namespace mcLaunch.Views.Pages;

public partial class LoadingPage : UserControl, ITopLevelPageControl
{
    public LoadingPage()
    {
        InitializeComponent();
    }

    public string Title => "Please wait";
}