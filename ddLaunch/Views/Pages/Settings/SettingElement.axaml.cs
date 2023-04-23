using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace ddLaunch.Views.Pages.Settings;

public partial class SettingElement : UserControl
{
    public Setting Setting { get; private set; }
    
    public SettingElement()
    {
        InitializeComponent();
    }

    public SettingElement(Setting setting)
    {
        InitializeComponent();
        
        Setting = setting;

        Label.Text = setting.Name;
        BooleanCheckbox.IsVisible = setting.Type == SettingType.Boolean;

        if (BooleanCheckbox.IsVisible) BooleanCheckbox.IsChecked = (bool)setting.Property.GetValue(Utilities.Settings.Instance)!;
    }

    private void BooleanCheckboxChecked(object? sender, RoutedEventArgs e)
    {
        Setting.Property.SetValue(Utilities.Settings.Instance, true);
        Utilities.Settings.Save();
    }

    private void BooleanCheckboxUnchecked(object? sender, RoutedEventArgs e)
    {
        Setting.Property.SetValue(Utilities.Settings.Instance, false);
        Utilities.Settings.Save();
    }
}