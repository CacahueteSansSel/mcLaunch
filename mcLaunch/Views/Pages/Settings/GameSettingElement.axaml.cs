using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Utilities;

namespace mcLaunch.Views.Pages.Settings;

public partial class GameSettingElement : UserControl
{
    public Box Box { get; }
    public string SettingKey { get; }

    public GameSettingElement()
    {
        InitializeComponent();
    }

    public GameSettingElement(Box box, string settingKey)
    {
        InitializeComponent();

        Box = box;
        SettingKey = settingKey;

        Label.Text = SettingKey.NormalizeCase();
        BooleanCheckbox.IsVisible = box.Options[settingKey] is bool;
        IntFloatSlider.IsVisible = box.Options[settingKey] is int or float;

        if (BooleanCheckbox.IsVisible) BooleanCheckbox.IsChecked = (bool) box.Options[settingKey];
        if (IntFloatSlider.IsVisible)
        {
            IntFloatSlider.Value = box.Options[settingKey] is int
                ? (int) box.Options[settingKey]
                : (float) box.Options[settingKey];
        }
    }

    private void BooleanCheckboxChecked(object? sender, RoutedEventArgs e)
    {
        Box.Options[SettingKey] = true;
        Box.Options.Save();
    }

    private void BooleanCheckboxUnchecked(object? sender, RoutedEventArgs e)
    {
        Box.Options[SettingKey] = false;
        Box.Options.Save();
    }

    private void IntFloatSliderPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == "Value")
        {
            Box.Options[SettingKey] =
                Box.Options[SettingKey] is int ? (int) IntFloatSlider.Value : IntFloatSlider.Value;
            Box.Options.Save();
        }
    }
}