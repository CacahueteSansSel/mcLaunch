﻿using System.Collections.Generic;
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

public partial class ModListSubControl : UserControl, ISubControl
{
    public ModListSubControl()
    {
        InitializeComponent();
    }

    public BoxDetailsPage ParentPage { get; set; }
    public Box Box { get; set; }
    public string Title { get; } = "MODS";

    public async Task PopulateAsync()
    {
        if (Box.Manifest.ModLoader is VanillaModLoaderSupport)
        {
            ModsList.HideLoadMoreButton();
            VanillaDisclaimer.IsVisible = true;
            ContentPanel.IsVisible = false;

            return;
        }

        MigrateToModrinthButton.IsVisible = Box.Manifest.Modifications
            .Count(mod => mod.PlatformId.ToLower() == "curseforge") > 0;

        VanillaDisclaimer.IsVisible = false;

        ModsList.HideLoadMoreButton();
        ModsList.SetLoadingCircle(true);

        List<Modification> mods = new();

        foreach (BoxStoredModification storedMod in Box.Manifest.Modifications)
        {
            Modification mod = await ModPlatformManager.Platform.GetModAsync(storedMod.Id);
            if (mod == null) continue;

            mod.Filename = storedMod.Filenames[0].Replace("mods/", "").Trim();
            mod.InstalledVersion = storedMod.VersionId;

            await mod.DownloadIconAsync();

            mods.Add(mod);
        }

        ModsList.SetBox(Box);
        ModsList.SetModifications(mods.ToArray());
        ModsList.SetLoadingCircle(false);

        SearchingForUpdates.IsVisible = true;

        List<Modification> updateMods = new();
        bool isChanges = false;

        foreach (Modification mod in mods)
        {
            string[] versions = await ModPlatformManager.Platform.GetModVersionList(mod.Id,
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

        SearchingForUpdates.IsVisible = false;
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
}