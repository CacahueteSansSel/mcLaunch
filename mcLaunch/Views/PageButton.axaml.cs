using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace mcLaunch.Views;

public partial class PageButton : UserControl
{
    public int PageIndex { get; set; }
    public PageSelector Parent { get; set; }
    
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

    public void SetLight(bool on)
    {
        Light.IsVisible = on;
    }

    private void ButtonClicked(object? sender, RoutedEventArgs e)
    {
        Parent.SetPage(PageIndex);
    }
}