using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Contents;
using mcLaunch.Core.Contents.Packs;
using mcLaunch.Utilities;

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
        SaveFileDialog sfd = new SaveFileDialog();
        sfd.Title = $"Export {box.Manifest.Name}";
        sfd.Filters = new List<FileDialogFilter>
        {
            new()
            {
                Extensions = new List<string>
                {
                    extension
                },
                Name = extensionDesc
            }
        };

        string? filename = await sfd.ShowAsync(MainWindow.Instance);
        if (filename == null) return;

        Navigation.ShowPopup(new StatusPopup($"Exporting {box.Manifest.Name}",
            $"Exporting {box.Manifest.Name} to {extensionDesc}"));

        T bb = new();
        await bb.ExportAsync(box, filename);

        Navigation.HidePopup();
        Navigation.ShowPopup(new MessageBoxPopup("Success !",
            $"{box.Manifest.Name} have been exported successfully", MessageStatus.Success));
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
}