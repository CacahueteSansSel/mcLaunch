using Avalonia;
using Avalonia.Controls;

namespace mcLaunch.Views;

public partial class NothingToShowBanner : UserControl
{
    public static readonly AttachedProperty<string> TextProperty =
        AvaloniaProperty.RegisterAttached<Badge, UserControl, string>(
            nameof(Footer),
            "");

    private string footer;

    public NothingToShowBanner()
    {
        InitializeComponent();
    }

    public string Footer
    {
        get => footer;
        set
        {
            footer = value;
            FooterText.Text = value;

            FooterText.IsVisible = value != null;
        }
    }
}