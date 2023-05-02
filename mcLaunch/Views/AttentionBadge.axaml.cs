using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace mcLaunch.Views;

public partial class AttentionBadge : UserControl
{
    public static readonly AttachedProperty<string> TextProperty =
        AvaloniaProperty.RegisterAttached<Badge, UserControl, string>(
            nameof(Text),
            "Editor",
            inherits: true);

    string text;

    public string Text
    {
        get => text;
        set
        {
            text = value;
            Label.Text = value;
        }
    }
    
    public AttentionBadge()
    {
        InitializeComponent();
    }
}