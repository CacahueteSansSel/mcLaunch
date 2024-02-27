using System.Threading.Tasks;

namespace mcLaunch.Views.Pages.BoxDetails;

public partial class ScreenshotListSubControl : SubControl
{
    public ScreenshotListSubControl()
    {
        InitializeComponent();
    }

    public override string Title => "SCREENSHOTS";

    public override async Task PopulateAsync()
    {
        Container.Children.Clear();

        foreach (string path in Box.GetScreenshotPaths())
            Container.Children.Add(new PictureFrame(path, Box));

        NtsBanner.IsVisible = Container.Children.Count == 0;
    }
}