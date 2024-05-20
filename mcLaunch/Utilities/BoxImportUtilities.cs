using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Contents.Packs;
using mcLaunch.Launchsite.Core;
using mcLaunch.Views.Pages;
using mcLaunch.Views.Popups;

namespace mcLaunch.Utilities;

public static class BoxImportUtilities
{
    public static async Task ImportBoxAsync(string filename, bool popup = true, bool openBoxAfterImport = true)
    {
        BoxBinaryModificationPack bb = new(filename);

        if (popup)
        {
            Navigation.ShowPopup(new StatusPopup($"Importing {bb.Name}", "Please wait for the modpack to be imported"));
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
        CurseForgeModificationPack modpack = new CurseForgeModificationPack(filename);

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
        ModrinthModificationPack modpack = new ModrinthModificationPack(filename);

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
}