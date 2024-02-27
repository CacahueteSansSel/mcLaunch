using Avalonia.Controls;
using Avalonia.Interactivity;

namespace mcLaunch.Views.Pages.Settings;

public partial class SettingElement : UserControl
{
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

        if (BooleanCheckbox.IsVisible)
            BooleanCheckbox.IsChecked = (bool) setting.Property.GetValue(Utilities.Settings.Instance)!;
    }

    public Setting Setting { get; }

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