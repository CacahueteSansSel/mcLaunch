using Avalonia.Controls;
using Avalonia.Interactivity;

namespace mcLaunch.Views;

public partial class PageButton : UserControl
{
    public PageButton() : this(1, null)
    {
    }

    public PageButton(int pageIndex, PageSelector parent)
    {
        InitializeComponent();

        Parent = parent;
        PageIndex = pageIndex;
        PageIndexText.Text = (pageIndex + 1).ToString();

        SetLight(false);
    }

    public int PageIndex { get; set; }
    public PageSelector Parent { get; set; }

    public void SetLight(bool on)
    {
        Light.IsVisible = on;
    }

    private void ButtonClicked(object? sender, RoutedEventArgs e)
    {
        Parent.SetPage(PageIndex);
    }
}