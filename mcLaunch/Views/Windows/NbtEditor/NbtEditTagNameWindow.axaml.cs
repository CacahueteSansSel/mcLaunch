using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SharpNBT;

namespace mcLaunch.Views.Windows.NbtEditor;

public partial class NbtEditTagNameWindow : Window
{
    public NbtEditTagNameWindow()
    {
        InitializeComponent();
    }

    public NbtEditTagNameWindow(TagType type, string tagName) : this()
    {
        NameTextBox.Text = tagName;

        this.GetControl<Image>($"IconTag{type}").IsVisible = true;
    }

    void ConfirmButtonClicked(object? sender, RoutedEventArgs e)
    {
        Close(NameTextBox.Text);
    }

    void CancelButtonClicked(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}