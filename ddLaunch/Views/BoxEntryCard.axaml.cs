using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ddLaunch.Core.Boxes;
using ddLaunch.Models;
using ddLaunch.Utilities;
using ddLaunch.Views.Pages;

namespace ddLaunch.Views;

public partial class BoxEntryCard : UserControl
{
    public Box Box { get; private set; }

    public BoxEntryCard()
    {
        InitializeComponent();
    }
    
    public BoxEntryCard(Box box)
    {
        InitializeComponent();

        SetBox(box);
    }

    public void SetBox(Box box)
    {
        Box = box;
        DataContext = box.Manifest;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        
        Navigation.Push(new BoxDetailsPage(Box));
    }
}