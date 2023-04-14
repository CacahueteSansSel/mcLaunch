using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ddLaunch.Core.Boxes;
using ddLaunch.Core.Managers;
using ddLaunch.Core.Mods;

namespace ddLaunch.Views.Pages.BoxDetails;

public partial class ModListSubControl : UserControl, ISubControl
{
    public ModListSubControl()
    {
        InitializeComponent();
    }

    public Box Box { get; set; }
    public string Title { get; } = "MODS";

    public async Task PopulateAsync()
    {
        ModsList.HideLoadMoreButton();
        ModsList.SetLoadingCircle(true);

        List<Modification> mods = new();

        foreach (BoxStoredModification storedMod in Box.Manifest.Modifications)
        {
            Modification mod = await ModPlatformManager.Platform.GetModAsync(storedMod.Id);
            mod.InstalledVersion = storedMod.VersionId;

            await mod.DownloadIconAsync();

            mods.Add(mod);
        }

        ModsList.SetBox(Box);
        ModsList.SetModifications(mods.ToArray());
        ModsList.SetLoadingCircle(false);

        List<Modification> updateMods = new();
        bool isChanges = false;

        foreach (Modification mod in mods)
        {
            string[] versions = await ModPlatformManager.Platform.GetVersionsForMinecraftVersionAsync(mod.Id,
                Box.Manifest.ModLoaderId, Box.Manifest.Version);

            mod.IsInvalid = versions.Length == 0;

            if (mod.IsInvalid)
            {
                isChanges = true;
                updateMods.Add(mod);
                continue;
            }
            
            mod.IsUpdateRequired = versions[0] != mod.InstalledVersion;
            if (mod.IsUpdateRequired) isChanges = true;

            updateMods.Add(mod);
        }
        
        if (isChanges) ModsList.SetModifications(updateMods.ToArray());
    }
}