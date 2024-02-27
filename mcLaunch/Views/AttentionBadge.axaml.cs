using Avalonia;
using Avalonia.Controls;

namespace mcLaunch.Views;

public partial class AttentionBadge : UserControl
{
    public static readonly AttachedProperty<string> TextProperty =
        AvaloniaProperty.RegisterAttached<Badge, UserControl, string>(
            nameof(Text),
            "Editor",
            true);

    private string text;

    public AttentionBadge()
    {
        InitializeComponent();
    }

    public string Text
    {
        get => text;
        set
        {
            text = value;
            Label.Text = value;
        }
    }
}