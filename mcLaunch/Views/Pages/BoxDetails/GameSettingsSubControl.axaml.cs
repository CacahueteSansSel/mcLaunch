using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using mcLaunch.Core.Managers;
using mcLaunch.Launchsite.Core;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages.Settings;
using mcLaunch.Views.Popups;

namespace mcLaunch.Views.Pages.BoxDetails;

public partial class GameSettingsSubControl : SubControl
{
    public GameSettingsSubControl()
    {
        InitializeComponent();
    }

    public override string Title => "SETTINGS";

    public override async Task PopulateAsync()
    {
        MinAllocatedRamInput.Text = Box.Manifest.CommandLineSettings.MinimumAllocatedRam.ToString();
        MaxAllocatedRamInput.Text = Box.Manifest.CommandLineSettings.MaximumAllocatedRam.ToString();
        CustomJavaArgsInput.Text = Box.Manifest.CommandLineSettings.CustomJavaArguments;
        
        Container.Children.Clear();

        if (Box.Options == null) return;

        foreach (KeyValuePair<string, object> kv in Box.Options.Where(opt => Box.Options.CanOptionBeChanged(opt.Key)))
        {
            GameSettingElement element = new(Box, kv.Key);

            Container.Children.Add(element);
        }
    }

    private void SetAsDefaultButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (Box.Options == null) return;
        DefaultsManager.SetDefaultMinecraftOptions(Box.Options);

        Navigation.ShowPopup(new MessageBoxPopup("Successful", "These options have been set to default",
            MessageStatus.Success));
    }

    private void CustomJavaArgsInput_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        Box.Manifest.CommandLineSettings.CustomJavaArguments = CustomJavaArgsInput.Text!;
        
        Box.SaveManifest();
    }

    private void MaxAllocatedRamInput_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        bool isValid = int.TryParse(MaxAllocatedRamInput.Text, out int maxRam) && maxRam > Box.Manifest.CommandLineSettings.MinimumAllocatedRam;
        MaxAllocatedRamInput.Foreground =
            isValid ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Red);
        
        if (!isValid) return;
        
        Box.Manifest.CommandLineSettings.MaximumAllocatedRam = maxRam;
        Box.SaveManifest();
    }

    private void MinAllocatedRamInput_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        bool isValid = int.TryParse(MinAllocatedRamInput.Text, out int minRam) && minRam < Box.Manifest.CommandLineSettings.MaximumAllocatedRam && minRam > 256;
        MinAllocatedRamInput.Foreground =
            isValid ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Red);
        
        if (!isValid) return;
        
        Box.Manifest.CommandLineSettings.MinimumAllocatedRam = minRam;
        Box.SaveManifest();
    }
}