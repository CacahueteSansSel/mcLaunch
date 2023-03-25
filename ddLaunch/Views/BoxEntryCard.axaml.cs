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
        
    }
    
    public BoxEntryCard(Box box)
    {
        InitializeComponent();

        Box = box;
        DataContext = box.Manifest;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        
        Navigation.Push(new BoxDetailsPage(Box));
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}