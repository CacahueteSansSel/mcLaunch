using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Core;
using mcLaunch.Core.Managers;
using mcLaunch.Launchsite.Core;
using mcLaunch.Launchsite.Core.ModLoaders;
using mcLaunch.Launchsite.Models;
using mcLaunch.Models;

namespace mcLaunch.Tests;

public abstract class UnitTest
{
    public string AssertLog { get; set; }
    public abstract Task RunAsync();

    protected void Assert(bool condition)
    {
        if (!condition)
            throw new Exception("Assertion failed");
    }
    
    protected void Assert(bool condition, string message, string leadingMessage = "")
    {
        AssertLog += $"{message}: {(condition ? "YES" : "NO")}";
        if (!string.IsNullOrEmpty(leadingMessage)) AssertLog += $": {leadingMessage}";

        AssertLog += "\n";
        
        if (!condition)
            throw new Exception($"Assertion failed: {message}" + (!string.IsNullOrEmpty(leadingMessage) ? $": {leadingMessage}" : ""));
    }

    protected void AssertResultSucceeded(Result result, string message)
        => Assert(!result.IsError, message, result.ErrorMessage ?? "");

    protected T? FindControlMain<T>(string name) where T : Control
    {
        foreach (Control control in MainWindow.Instance.GetVisualDescendants().Where(v => v is Control))
        {
            if (control.Name == name) return (T) control;
        }

        return null;
    }

    protected T? FindControlPopup<T>(string name) where T : Control
    {
        foreach (Control control in MainWindowDataContext.Instance.CurrentPopup
                     .GetVisualDescendants().Where(v => v is Control))
        {
            if (control.Name == name) return (T) control;
        }

        return null;
    }

    protected void ButtonClick(Button button)
        => button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

    protected bool IsPopupShown<T>() where T : UserControl
    {
        return MainWindowDataContext.Instance.CurrentPopup is T;
    }

    protected bool IsPageShown<T>() where T : UserControl
    {
        return MainWindowDataContext.Instance.CurrentPage is T;
    }

    protected async Task<Box> CreateBoxAsync(string mcVersion, string modloader)
    {
        ManifestMinecraftVersion version = await MinecraftManager.GetManifestAsync(mcVersion);
        ModLoaderVersion? latestMlVersion = await ModLoaderManager.Get(modloader)!.FetchLatestVersion(mcVersion);
        string boxName = GetType().Name;
        
        Assert(latestMlVersion != null, $"Latest {modloader} version is not null");

        BoxManifest newBox = new BoxManifest(boxName, "", boxName, modloader, 
            latestMlVersion!.Name, IconCollection.Default, version, BoxType.Temporary);

        Result<string> result = await BoxManager.Create(newBox);
        AssertResultSucceeded(result, "Box creation succeeded");
        Assert(Directory.Exists(result.Data), "Box folder exists");

        Box box = new Box(result.Data!, false);
        await box.ReloadManifestAsync(true, false);

        return box;
    }

    protected void DeleteBoxAndCheck(Box box)
    {
        box.Delete();
        Assert(!Directory.Exists(box.Path), "Box was deleted and its folder does not exist anymore");
    }
}