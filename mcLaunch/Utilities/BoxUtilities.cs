using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Contents;
using mcLaunch.Core.Contents.Packs;
using mcLaunch.Core.Contents.Platforms;
using mcLaunch.Core.Utilities;
using mcLaunch.Launchsite.Core;
using mcLaunch.Views.Pages;
using mcLaunch.Views.Popups;

namespace mcLaunch.Utilities;

public static class BoxUtilities
{
    public static async Task ImportBoxAsync(string filename, bool popup = true, bool openBoxAfterImport = true)
    {
        BoxBinaryModificationPack bb = null;

        try
        {
            bb = new BoxBinaryModificationPack(filename);
        }
        catch (Exception e)
        {
            Navigation.ShowPopup(new MessageBoxPopup("Error",
                "Failed to import the box : it may be invalid", MessageStatus.Error));

            return;
        }

        if (popup)
        {
            Navigation.ShowPopup(new StatusPopup($"Importing {bb.Name}", "Please wait for the box to be imported"));
            StatusPopup.Instance.ShowDownloadBanner = true;
        }

        Result<Box> boxResult = await BoxManager.CreateFromModificationPack(bb, "noone", (msg, percent) =>
        {
            if (!popup) return;

            StatusPopup.Instance.Status = msg;
            StatusPopup.Instance.StatusPercent = percent;
        });
        if (boxResult.IsError)
        {
            boxResult.ShowErrorPopup();
            return;
        }

        Box box = boxResult.Data!;

        try
        {
            box.SetAndSaveIcon(new Bitmap(new MemoryStream(bb.IconData)));
            box.SetAndSaveBackground(new Bitmap(new MemoryStream(bb.BackgroundData)));
        }
        catch (Exception)
        {
            // ignored
        }

        if (popup) Navigation.HidePopup();

        if (openBoxAfterImport)
        {
            Navigation.Push(new BoxDetailsPage(box));
            MainPage.Instance.PopulateBoxList();
        }
    }

    public static async Task ImportCurseforgeAsync(string filename, bool popup = true, bool openBoxAfterImport = true)
    {
        CurseForgeModificationPack modpack = null;

        try
        {
            modpack = new CurseForgeModificationPack(filename);
        }
        catch (Exception e)
        {
            Navigation.ShowPopup(new MessageBoxPopup("Error",
                "Failed to import the modpack : it may be invalid", MessageStatus.Error));

            return;
        }

        if (popup)
        {
            Navigation.ShowPopup(new StatusPopup($"Importing {modpack.Name}",
                "Please wait for the modpack to be imported"));
            StatusPopup.Instance.ShowDownloadBanner = true;
        }

        Result<Box> boxResult = await BoxManager.CreateFromModificationPack(modpack, "noone", (msg, percent) =>
        {
            if (!popup) return;

            StatusPopup.Instance.Status = msg;
            StatusPopup.Instance.StatusPercent = percent;
        });
        if (boxResult.IsError)
        {
            boxResult.ShowErrorPopup();
            return;
        }

        Box box = boxResult.Data!;

        if (popup) Navigation.HidePopup();

        try
        {
            Bitmap bmp =
                new Bitmap(AssetLoader.Open(new Uri("avares://mcLaunch/resources/default_cf_modpack_logo.png")));
            box.SetAndSaveIcon(bmp);
        }
        catch (Exception)
        {
            // ignored
        }

        if (openBoxAfterImport)
        {
            Navigation.Push(new BoxDetailsPage(box));
            MainPage.Instance.PopulateBoxList();
        }
    }

    public static async Task ImportModrinthAsync(string filename, bool popup = true, bool openBoxAfterImport = true)
    {
        ModrinthModificationPack modpack = null;

        try
        {
            modpack = new ModrinthModificationPack(filename);
        }
        catch (Exception e)
        {
            Navigation.ShowPopup(new MessageBoxPopup("Error",
                "Failed to import the modpack : it may be invalid", MessageStatus.Error));

            return;
        }

        if (popup)
        {
            Navigation.HidePopup();
            Navigation.ShowPopup(new StatusPopup($"Importing {modpack.Name}",
                "Please wait for the modpack to be imported"));
            StatusPopup.Instance.Status = "Resolving modifications...";
        }

        await modpack.SetupAsync();

        StatusPopup.Instance.ShowDownloadBanner = true;

        Result<Box> boxResult = await BoxManager.CreateFromModificationPack(modpack, "noone", (msg, percent) =>
        {
            if (!popup) return;

            StatusPopup.Instance.Status = $"{msg}";
            StatusPopup.Instance.StatusPercent = percent;
        });
        if (boxResult.IsError)
        {
            boxResult.ShowErrorPopup();
            return;
        }

        Box box = boxResult.Data!;

        if (popup) Navigation.HidePopup();

        try
        {
            if (File.Exists($"{box.Path}/minecraft/icon.png"))
            {
                Bitmap modpackIcon = new Bitmap($"{box.Path}/minecraft/icon.png");
                box.SetAndSaveIcon(modpackIcon);
            }
            else
            {
                Bitmap bmp = new Bitmap(AssetLoader.Open(new Uri("avares://mcLaunch/resources/default_box_logo.png")));
                box.SetAndSaveIcon(bmp);
            }
        }
        catch (Exception)
        {
            // ignored
        }

        if (openBoxAfterImport)
        {
            Navigation.Push(new BoxDetailsPage(box));
            MainPage.Instance.PopulateBoxList();
        }
    }

    public static async Task ImportAsync(string filename, bool popup = true, bool openBoxAfterImport = true)
    {
        switch (Path.GetExtension(filename).TrimStart('.'))
        {
            case "mrpack":
                await ImportModrinthAsync(filename, popup, openBoxAfterImport);
                break;
            case "box":
                await ImportBoxAsync(filename, popup, openBoxAfterImport);
                break;
            case "zip":
                await ImportCurseforgeAsync(filename, popup, openBoxAfterImport);
                break;
        }
    }

    public static async Task<Box> DuplicateAsync(Box sourceBox, string name, string author, bool popup = true)
    {
        if (popup)
        {
            Navigation.ShowPopup(new StatusPopup($"Duplicating {sourceBox.Manifest.Name}",
                "Please wait for the box to be duplicated"));
        }

        string boxId = IdGenerator.Generate();
        string newBoxPath = $"{BoxManager.BoxesPath}/{boxId}";
        Directory.CreateDirectory(newBoxPath);
        FileSystemUtilities.CopyDirectory(sourceBox.Path, newBoxPath);

        Box box = new Box(newBoxPath, false);
        await box.ReloadManifestAsync(true);

        box.Manifest.Id = boxId;
        box.Manifest.Name = name;
        box.Manifest.Author = author;

        await box.SaveManifestAsync();

        if (popup) Navigation.HidePopup();

        return box;
    }

    public static async Task<string> GenerateReportAsync(Box box, bool complete = true)
    {
        StringWriter writer = new();

        BoxStoredContent[] mods = (!complete
                ? box.Manifest.RecentlyAddedContents
                    .Where(content => content.Content!.Type == MinecraftContentType.Modification)
                : box.Manifest.ContentModifications)
            .ToArray();

        await writer.WriteLineAsync($"# Modpack {box.Manifest.Name}");
        await writer.WriteLineAsync(
            $"Minecraft **{box.Manifest.Version}** on **{box.Manifest.ModLoader.Name} {box.Manifest.ModLoaderVersion}**");
        await writer.WriteLineAsync();
        await writer.WriteLineAsync("## Mods");
        foreach (BoxStoredContent mod in mods)
        {
            string url = string.Empty;
            MinecraftContent content = mod.Content!;

            if (string.IsNullOrWhiteSpace(mod.Content!.Url))
                content = await mod.Content!.Platform!.DownloadContentInfosAsync(mod.Content);

            if (content.Platform is ModrinthMinecraftContentPlatform)
                url = $"{content.Url}/version/{mod.VersionId}";
            else if (content.Platform is CurseForgeMinecraftContentPlatform)
                url = $"{content.Url}/files/{mod.VersionId}";
            
            string urlPart = string.IsNullOrWhiteSpace(url) ? string.Empty : $": [link here]({url})";

            await writer.WriteLineAsync($"+ **{mod.Content.Name}** with version id **{mod.VersionId} on {content.Platform.Name}** {urlPart}".Trim());
        }

        return writer.ToString();
    }
}