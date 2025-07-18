using Avalonia.Controls;
using Avalonia.Media;

namespace mcLaunch.Views.Pages.Settings;

public partial class SettingsSection : UserControl
{
    public SettingsSection()
    {
        InitializeComponent();
    }

    public SettingsSection(SettingsGroup group)
    {
        InitializeComponent();
        SettingsRoot.Children.Clear();

        Title.Text = group.Name;

        foreach (Setting setting in group.Settings)
        {
            Panel separator = new()
            {
                Background = new SolidColorBrush(0xFF3C3C3C),
                Height = 0.5f
            };
            SettingsRoot.Children.Add(separator);
            
            SettingElement element = new(setting);
            SettingsRoot.Children.Add(element);
        }
    }
}