using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Contents;
using mcLaunch.Core.Managers;
using mcLaunch.Launchsite.Core.ModLoaders;
using mcLaunch.Utilities;
using mcLaunch.Views.Popups;

namespace mcLaunch.Views.Pages.BoxDetails;

public partial class ContentsSubControl : SubControl
{
    private readonly bool canUpdate;
    private readonly List<MinecraftContent> updatableContentsList = new();
    private bool isAnyUpdate;
    private bool isUpdating;

    public ContentsSubControl()
    {
        InitializeComponent();
    }

    public ContentsSubControl(MinecraftContentType contentType, bool canUpdate = true)
    {
        InitializeComponent();
        ContentType = contentType;
        this.canUpdate = canUpdate;

        ContentText.Text = ContentNamePlural.ToUpper();
        ContentCountText.Text = string.Empty;
        SearchBox.Watermark = $"Search {ContentNamePlural.ToLower()}";
    }

    public override string Title => ContentNamePlural.ToUpper();
    public MinecraftContentType ContentType { get; set; }
    public string ContentName => ContentType.ToString();
    public string ContentNamePlural => $"{ContentName}s";

    public override async Task PopulateAsync()
    {
        if (Box.Manifest.ModLoader is VanillaModLoaderSupport
            && (ContentType == MinecraftContentType.Modification
                || ContentType == MinecraftContentType.ShaderPack))
        {
            ModsList.HideLoadMoreButton();
            VanillaDisclaimer.IsVisible = true;
            ContentPanel.IsVisible = false;

            return;
        }

        ContentCountText.Text = Box.Manifest.GetContents(ContentType).Length.ToString();

        UpdateAllButton.IsVisible = false;
        if (!isUpdating) updatableContentsList.Clear();

        MigrateToModrinthButton.IsVisible = Box.Manifest.GetContents(ContentType)
            .Count(mod => mod.PlatformId.ToLower() != "modrinth") > 0;
        MigrateToCurseForgeButton.IsVisible = Box.Manifest.GetContents(ContentType)
            .Count(mod => mod.PlatformId.ToLower() != "curseforge") > 0;

        VanillaDisclaimer.IsVisible = false;

        ModsList.HideLoadMoreButton();
        ModsList.SetLoadingCircle(true);

        BoxStoredContent[] storedContents = Box.Manifest.GetContents(ContentType);
        List<MinecraftContent> contents =
        [
            ..storedContents
                .Where(content => content.Content != null)
                .Select(content => content.Content!)
        ];

        /*
        await Parallel.ForEachAsync(Box.Manifest.GetContents(ContentType), async (boxContent, token) =>
        {
            MinecraftContent content = await ModPlatformManager.Platform.GetContentAsync(boxContent.Id);
            //MinecraftContent content = boxContent.ToContent();
            if (content == null) return;

            string folder = $"{MinecraftContentUtils.GetInstallFolderName(ContentType)}/";
            content.Filename = boxContent.Filenames.Length == 0
                ? ""
                : boxContent.Filenames[0]
                    .Replace(folder, "").Trim();
            content.InstalledVersion = boxContent.VersionId;

            contents.Add(content);
        });
        */

        contents.Sort((left, right)
            => string.Compare(left.Name!, right.Name!, StringComparison.Ordinal));

        ModsList.ContentType = ContentType;

        ModsList.SetBox(Box);
        ModsList.SetLoadingCircle(false);
        ModsList.SetContents(contents.ToArray());

        if (!canUpdate)
        {
            UpdateAllButton.IsVisible = false;
            return;
        }

        SearchingForUpdates.IsVisible = true;

        isAnyUpdate = false;

        List<MinecraftContent> toUpdateContents = new();
        bool isChanges = false;

        try
        {
            await Parallel.ForEachAsync(storedContents, async (storedContent, token) =>
            {
                MinecraftContent content = storedContent.Content!;
                ContentVersion[] versions = await ModPlatformManager.Platform.GetContentVersionsAsync(content,
                    Box.Manifest.ModLoaderId, Box.Manifest.Version);

                content.IsInvalid = versions.Length == 0;

                if (content.IsInvalid)
                {
                    isChanges = true;
                    toUpdateContents.Add(content);

                    return;
                }

                content.IsUpdateRequired = versions[0].Id != storedContent.VersionId;
                if (content.IsUpdateRequired)
                {
                    isChanges = true;
                    isAnyUpdate = true;

                    if (!isUpdating) updatableContentsList.Add(content);
                }

                toUpdateContents.Add(content);
            });
        }
        catch (Exception e)
        {
            // ignored
        }

        if (isChanges)
        {
            toUpdateContents.Sort((left, right)
                => string.Compare(left.Name!, right.Name!, StringComparison.Ordinal));

            ModsList.SetContents(toUpdateContents.ToArray());
        }

        SearchingForUpdates.IsVisible = false;

        UpdateAllButton.IsVisible = isAnyUpdate;
        UpdateButtonCountText.Text = updatableContentsList.Count.ToString();
    }

    private void AddModsButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.Push(new ContentSearchPage(Box, ContentType));
    }

    private void MigrateToModrinthButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.ShowPopup(new ConfirmMessageBoxPopup($"Migrate all {ContentNamePlural} to Modrinth",
            $"Every {ContentName} from CurseForge will be migrated to Modrinth equivalents if possible. This action is irreversible",
            async () =>
            {
                Navigation.ShowPopup(new StatusPopup("Migrating to Modrinth...",
                    $"Migrating the box {Box.Manifest.Name}'s {ContentNamePlural} to Modrinth equivalent..."));

                MinecraftContent[] contents = await Box.MigrateToModrinthAsync((mod, index, count) =>
                {
                    StatusPopup.Instance.Status =
                        $"Verifying & installing equivalent ({ContentName} {index}/{count})";
                });

                if (contents.Length == 0)
                {
                    Navigation.ShowPopup(new MessageBoxPopup("Information", $"No {ContentName} have been migrated"));
                    return;
                }

                Navigation.ShowPopup(new ModsPopup($"{contents.Length} {ContentName}(s) migrated",
                    $"The following {ContentNamePlural} have been successfully migrated to their Modrinth equivalent",
                    Box, contents));
            }));
    }

    private async void UpdateAllButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (updatableContentsList.Count == 0)
        {
            UpdateAllButton.IsVisible = false;
            return;
        }

        Navigation.ShowPopup(new StatusPopup($"Updating {ContentNamePlural}",
            $"Please wait while we update {ContentNamePlural} from {Box.Manifest.Name}..."));
        StatusPopup.Instance.ShowDownloadBanner = true;

        isUpdating = true;

        int failedModUpdates = 0;
        int index = 1;

        foreach (MinecraftContent mod in updatableContentsList)
        {
            StatusPopup.Instance.Status = $"Updating {mod.Name} ({index}/{updatableContentsList.Count})";
            StatusPopup.Instance.StatusPercent = (float) index / updatableContentsList.Count;

            if (!await Box.UpdateModAsync(mod)) failedModUpdates++;

            index++;
        }

        StatusPopup.Instance.ShowDownloadBanner = false;
        Navigation.HidePopup();

        if (failedModUpdates > 0)
            Navigation.ShowPopup(new MessageBoxPopup("Warning",
                $"{failedModUpdates} {ContentName}{(failedModUpdates > 1 ? "s" : "")} failed to update"));

        isUpdating = false;
        UpdateAllButton.IsVisible = false;
    }

    private void MigrateToCurseForgeButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.ShowPopup(new ConfirmMessageBoxPopup($"Migrate all {ContentNamePlural} to CurseForge",
            $"Every {ContentName} will be migrated to CurseForge equivalents if possible. This action is irreversible",
            async () =>
            {
                Navigation.ShowPopup(new StatusPopup("Migrating to CurseForge...",
                    $"Migrating the box {Box.Manifest.Name}'s {ContentNamePlural} to CurseForge equivalent..."));
                StatusPopup.Instance.ShowDownloadBanner = true;

                MinecraftContent[] contents = await Box.MigrateToCurseForgeAsync((mod, index, count) =>
                {
                    StatusPopup.Instance.Status =
                        $"Verifying & installing equivalent ({ContentName} {index}/{count})";
                });

                StatusPopup.Instance.ShowDownloadBanner = false;

                if (contents.Length == 0)
                {
                    Navigation.ShowPopup(new MessageBoxPopup("Information", $"No {ContentName} have been migrated"));
                    return;
                }

                Navigation.ShowPopup(new ModsPopup($"{contents.Length} {ContentName}(s) migrated",
                    $"The following {ContentNamePlural} have been successfully migrated to their CurseForge equivalent",
                    Box, contents));
            }));
    }

    private void SearchBoxTextChanged(object? sender, TextChangedEventArgs e)
    {
        ModsList.SetQuery(SearchBox.Text);
    }
}