using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using mcLaunch.Launchsite.Core.ModLoaders;
using mcLaunch.Core.Contents;
using mcLaunch.Core.Managers;
using mcLaunch.Utilities;
using mcLaunch.Views.Popups;

namespace mcLaunch.Views.Pages.BoxDetails;

public partial class DirectJarModsSubControl : SubControl
{
    public override string Title => "DIRECT JAR MODS";

    public DirectJarModsSubControl()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            DirectJarList.ItemsSource = new[]
            {
                new DirectJarModEntry("minecraft.zip", "directjar/minecraft.zip")
            };
        }
    }

    public override async Task PopulateAsync()
    {
        List<DirectJarModEntry> entries = [];
        
        foreach (string filename in Box.Manifest.AdditionalModloaderFiles)
        {
            if (!filename.StartsWith("directjar")) continue;
            
            entries.Add(new DirectJarModEntry(Path.GetFileName(filename), filename));
        }

        DirectJarList.ItemsSource = entries;
    }

    private async void AddModsButtonClicked(object? sender, RoutedEventArgs e)
    {
        string[] result = await FileSystemUtilities.PickFiles(true, "Select Direct Jar Mods", ["zip"]);
        foreach (string filename in result) Box.AddDirectJarMod(filename);

        await PopulateAsync();
    }

    private void DirectJarList_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        e.AddedItems.Clear();
        e.RemovedItems.Clear();
    }

    public record DirectJarModEntry(string Name, string Filename);
}