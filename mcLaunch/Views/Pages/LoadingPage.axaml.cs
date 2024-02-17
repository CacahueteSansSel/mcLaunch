using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace mcLaunch.Views.Pages;

public partial class LoadingPage : UserControl, ITopLevelPageControl
{
    public string Title => "Please wait";

    public LoadingPage()
    {
        InitializeComponent();
    }
}