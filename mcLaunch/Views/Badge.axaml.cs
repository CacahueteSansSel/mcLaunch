using Avalonia;
using Avalonia.Controls;
using ReactiveUI;

namespace mcLaunch.Views;

public partial class Badge : UserControl
{
    public static readonly AttachedProperty<string> TextProperty =
        AvaloniaProperty.RegisterAttached<Badge, UserControl, string>(
            nameof(Text),
            "Editor",
            true);

    public Badge()
    {
        InitializeComponent();
    }

    public string? Text
    {
        get => Label.Text;
        set
        {
            Label.Text = value;
        }
    }
}