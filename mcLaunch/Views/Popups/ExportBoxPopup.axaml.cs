using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Contents;
using mcLaunch.Core.Contents.Packs;
using mcLaunch.Core.Utilities;
using mcLaunch.Utilities;
using Microsoft.Identity.Client.Extensions.Msal;

namespace mcLaunch.Views.Popups;

public partial class ExportBoxPopup : UserControl
{
    private readonly Box box;

    public ExportBoxPopup()
    {
        InitializeComponent();
    }

    public ExportBoxPopup(Box box)
    {
        InitializeComponent();

        this.box = box;
    }

    private async void ExportAsync<T>(string extension, string extensionDesc) where T : ModificationPack, new()
    {
        Navigation.HidePopup();
        Navigation.ShowPopup(new ExportBoxFilesSelectionPopup(box, async entries =>
        {
            IStorageProvider storage = MainWindow.Instance.StorageProvider;

            IStorageFile? file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions()
            {
                DefaultExtension = extension, FileTypeChoices = [new FilePickerFileType(extensionDesc) { Patterns = [$"*.{extension}"] }],
                Title = $"Export {extensionDesc}"
            });
        
            if (file == null) return;

            Navigation.ShowPopup(new StatusPopup($"Exporting {box.Manifest.Name}",
                $"Exporting {box.Manifest.Name} to {extensionDesc}"));

            T bb = new();
            await bb.ExportAsync(box, file.Path.AbsolutePath, entries.Select(entry => entry.Name).ToArray());

            Navigation.HidePopup();
            Navigation.ShowPopup(new MessageBoxPopup("Success !",
                $"{box.Manifest.Name} have been exported successfully", MessageStatus.Success));
        }));
    }

    private void ClosePopupButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
    }

    private void ExportBoxButtonClicked(object? sender, RoutedEventArgs e)
    {
        ExportAsync<BoxBinaryModificationPack>("box", "mcLaunch Box Binary file");
    }

    private void ExportCurseForgeModpackButtonClicked(object? sender, RoutedEventArgs e)
    {
        ExportAsync<CurseForgeModificationPack>("zip", "CurseForge modpack profile");
    }

    private void ExportModrinthModpackButtonClicked(object? sender, RoutedEventArgs e)
    {
        ExportAsync<ModrinthModificationPack>("mrpack", "Modrinth modpack");
    }

    private async void ExportZIPButtonClicked(object? sender, RoutedEventArgs e)
    {
        IStorageProvider storage = MainWindow.Instance.StorageProvider;

        IStorageFile? file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            DefaultExtension = "zip", FileTypeChoices = [new FilePickerFileType("ZIP Archive") { Patterns = ["*.zip"] }],
            Title = "Export Instance ZIP Archive"
        });
        
        if (file == null) return;
        
        try
        {
            box.ExportToZip(file.Path.AbsolutePath);
            
            Navigation.ShowPopup(new MessageBoxPopup("Success", $"The box was exported successfully as a ZIP archive", MessageStatus.Success));
        }
        catch (IOException ex)
        {
            Navigation.ShowPopup(new MessageBoxPopup("Error", $"An error occurred while exporting the box as a ZIP archive: {ex.Message}", MessageStatus.Error));
        }
    }
}