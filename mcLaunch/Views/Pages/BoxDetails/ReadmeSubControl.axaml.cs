using System.Threading.Tasks;

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

    public override string Title => "README";

    public override async Task PopulateAsync()
    {
    }
}