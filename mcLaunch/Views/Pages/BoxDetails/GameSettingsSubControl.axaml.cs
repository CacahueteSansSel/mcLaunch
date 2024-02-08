using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Managers;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages.Settings;
using mcLaunch.Views.Popups;

namespace mcLaunch.Views.Pages.BoxDetails;

public partial class GameSettingsSubControl : SubControl
{
    public override string Title => "SETTINGS";
    
    public GameSettingsSubControl()
    {
        InitializeComponent();
    }

    public override async Task PopulateAsync()
    {
        Container.Children.Clear();

        if (Box.Options == null) return;

        foreach (var kv in Box.Options.Where(opt => Box.Options.CanOptionBeChanged(opt.Key)))
        {
            GameSettingElement element = new GameSettingElement(Box, kv.Key);

            Container.Children.Add(element);
        }
    }

    private void SetAsDefaultButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (Box.Options == null) return;
        DefaultsManager.SetDefaultMinecraftOptions(Box.Options);
        
        Navigation.ShowPopup(new MessageBoxPopup("Successful", "These options have been set to default"));
    }
}