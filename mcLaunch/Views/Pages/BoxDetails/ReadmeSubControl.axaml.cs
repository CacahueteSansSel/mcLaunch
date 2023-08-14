using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace mcLaunch.Views.Pages.BoxDetails;

public partial class ReadmeSubControl : SubControl
{
    public ReadmeSubControl()
    {
        InitializeComponent();
        DataContext = "# My wholesome book !";
    }

    public ReadmeSubControl(string readmeText)
    {
        InitializeComponent();
        DataContext = readmeText;
    }

    public override async Task PopulateAsync()
    {
        
    }
}