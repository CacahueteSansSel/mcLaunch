using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace mcLaunch.Views;

public partial class NothingToShowBanner : UserControl
{
    public static readonly AttachedProperty<string> TextProperty =
        AvaloniaProperty.RegisterAttached<Badge, UserControl, string>(
            nameof(Footer),
            null,
            inherits: true);

    string footer;

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
    
    public NothingToShowBanner()
    {
        InitializeComponent();
    }
}