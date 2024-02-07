using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Cacahuete.MinecraftLib.Core.ModLoaders;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Managers;
using mcLaunch.Core.Mods;
using mcLaunch.Utilities;
using mcLaunch.Views.Popups;

namespace mcLaunch.Views.Pages.BoxDetails;

public partial class ModListSubControl : SubControl
{
    bool isAnyUpdate = false;
    List<Modification> updatableModsList = new();

    public override string Title => "MODS";

    public ModListSubControl()
    {
        InitializeComponent();
    }

    public override async Task PopulateAsync()
    {
        if (Box.Manifest.ModLoader is VanillaModLoaderSupport)
        {
            ModsList.HideLoadMoreButton();
            VanillaDisclaimer.IsVisible = true;
            ContentPanel.IsVisible = false;

            return;
        }

        UpdateAllButton.IsVisible = false;
        updatableModsList.Clear();

        MigrateToModrinthButton.IsVisible = Box.Manifest.Modifications
            .Count(mod => mod.PlatformId.ToLower() != "modrinth") > 0;
        MigrateToCurseForgeButton.IsVisible = Box.Manifest.Modifications
            .Count(mod => mod.PlatformId.ToLower() != "curseforge") > 0;

        VanillaDisclaimer.IsVisible = false;

        ModsList.HideLoadMoreButton();
        ModsList.SetLoadingCircle(true);

        List<Modification> mods = new();

        foreach (BoxStoredModification storedMod in Box.Manifest.Modifications)
        {
            Modification mod = await ModPlatformManager.Platform.GetModAsync(storedMod.Id);
            if (mod == null) continue;

            mod.Filename = storedMod.Filenames.Length == 0 ? "" : storedMod.Filenames[0].Replace("mods/", "").Trim();
            mod.InstalledVersion = storedMod.VersionId;

            mod.DownloadIconAsync();

            mods.Add(mod);
        }

        ModsList.SetBox(Box);
        ModsList.SetModifications(mods.ToArray());
        ModsList.SetLoadingCircle(false);

        SearchingForUpdates.IsVisible = true;

        isAnyUpdate = false;

        List<Modification> updateMods = new();
        bool isChanges = false;

        foreach (Modification mod in mods)
        {
            ModVersion[] versions = await ModPlatformManager.Platform.GetModVersionsAsync(mod,
                Box.Manifest.ModLoaderId, Box.Manifest.Version);

            mod.IsInvalid = versions.Length == 0;

            if (mod.IsInvalid)
            {
                isChanges = true;
                updateMods.Add(mod);
                continue;
            }

            mod.IsUpdateRequired = versions[0].Id != mod.InstalledVersion;
            if (mod.IsUpdateRequired)
            {
                isChanges = true;
                isAnyUpdate = true;
                
                updatableModsList.Add(mod);
            }

            updateMods.Add(mod);
        }

        if (isChanges) ModsList.SetModifications(updateMods.ToArray());

        SearchingForUpdates.IsVisible = false;

        UpdateAllButton.IsVisible = isAnyUpdate;
        UpdateButtonCountText.Text = updatableModsList.Count.ToString();
    }

    private void AddModsButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.Push(new ModSearchPage(Box));
    }

    private void MigrateToModrinthButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.ShowPopup(new ConfirmMessageBoxPopup("Migrate all mods to Modrinth",
            "Every mod from CurseForge will be migrated to Modrinth equivalents if possible. This action is irreversible",
            async () =>
            {
                Navigation.ShowPopup(new StatusPopup("Migrating to Modrinth...",
                    $"Migrating the box {Box.Manifest.Name}'s mods to Modrinth equivalent..."));

                Modification[] mods = await Box.MigrateToModrinthAsync((mod, index, count) =>
                {
                    StatusPopup.Instance.Status = $"Verifying & installing equivalent (mod {index}/{count})";
                });

                if (mods.Length == 0)
                {
                    Navigation.ShowPopup(new MessageBoxPopup("Information", "No mod have been migrated"));
                    return;
                }

                Navigation.ShowPopup(new ModsPopup($"{mods.Length} mod(s) migrated",
                    $"The following mods have been successfully migrated to their Modrinth equivalent", Box, mods));
            }));
    }

    private async void UpdateAllButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (updatableModsList.Count == 0)
        {
            UpdateAllButton.IsVisible = false;
            return;
        }

        Navigation.ShowPopup(new StatusPopup("Updating mods", $"Please wait while we update mods from {Box.Manifest.Name}..."));
        
        int failedModUpdates = 0;
        int index = 1;
        
        foreach (Modification mod in updatableModsList)
        {
            StatusPopup.Instance.Status = $"Updating {mod.Name} ({index}/{updatableModsList.Count})";
            StatusPopup.Instance.StatusPercent = (float) index / updatableModsList.Count;
            
            if (!await Box.UpdateModAsync(mod, false))
            {
                failedModUpdates++;
            }

            index++;
        }
        
        Navigation.HidePopup();

        if (failedModUpdates > 0)
        {
            Navigation.ShowPopup(new MessageBoxPopup("Warning", $"{failedModUpdates} mod{(failedModUpdates > 1 ? "s" : "")} failed to update"));
        }
        
        UpdateAllButton.IsVisible = false;
    }

    private void MigrateToCurseForgeButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.ShowPopup(new ConfirmMessageBoxPopup("Migrate all mods to CurseForge",
            "Every mod will be migrated to CurseForge equivalents if possible. This action is irreversible",
            async () =>
            {
                Navigation.ShowPopup(new StatusPopup("Migrating to CurseForge...",
                    $"Migrating the box {Box.Manifest.Name}'s mods to CurseForge equivalent..."));

                Modification[] mods = await Box.MigrateToCurseForgeAsync((mod, index, count) =>
                {
                    StatusPopup.Instance.Status = $"Verifying & installing equivalent (mod {index}/{count})";
                });

                if (mods.Length == 0)
                {
                    Navigation.ShowPopup(new MessageBoxPopup("Information", "No mod have been migrated"));
                    return;
                }

                Navigation.ShowPopup(new ModsPopup($"{mods.Length} mod(s) migrated",
                    $"The following mods have been successfully migrated to their CurseForge equivalent", Box, mods));
            }));
    }
}