using Avalonia.Controls;

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
            SettingElement element = new(setting);
            SettingsRoot.Children.Add(element);
        }
    }
}