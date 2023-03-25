using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ddLaunch.Core;
using ddLaunch.Models;

namespace ddLaunch.Views;

public partial class ModificationList : UserControl
{
    public ModificationList()
    {
        InitializeComponent();

        DataContext = new ModificationListDataContext();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}