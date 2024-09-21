using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using mcLaunch.Core.Managers;
using mcLaunch.Launchsite.Core.ModLoaders;
using mcLaunch.Launchsite.Models;
using mcLaunch.Models;
using mcLaunch.Utilities;
using mcLaunch.Views;
using mcLaunch.Views.Pages;
using mcLaunch.Views.Popups;

namespace mcLaunch.Tests.BuiltInTests;

public class CreateNewBoxTest : UnitTest
{
    const string BoxName = "No Therese Here";
    
    public override async Task RunAsync()
    {
        ButtonClick(FindControlMain<Button>("NewBoxButton")!);
        
        Assert(IsPopupShown<NewBoxPopup>(), "New box popup shown");

        ManifestMinecraftVersion mcVersion = MinecraftManager.ManifestVersions[0];

        TextBox boxNameTbox = FindControlMain<TextBox>("BoxNameTb")!;
        Button createButton = FindControlMain<Button>("CreateButton")!;
        MinecraftVersionSelector versionSelector = FindControlMain<MinecraftVersionSelector>("VersionSelector")!;
        versionSelector.SetVersion(mcVersion);

        Assert(versionSelector.Version.Id == mcVersion.Id, $"Selected Minecraft version is {mcVersion.Id} as requested");
        
        boxNameTbox.Text = BoxName;
        createButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        await Task.Run(async () =>
        {
            while (!IsPageShown<BoxDetailsPage>())
                await Task.Delay(100);
        });

        BoxDetailsPage page = (BoxDetailsPage)MainWindowDataContext.Instance.CurrentPage;
        Assert(page.Box.Manifest.Name == BoxName, "Box's name is as requested");
        Assert(page.Box.Manifest.Version == mcVersion.Id, "Box's version is as requested");
        Assert(page.Box.ModLoader is VanillaModLoaderSupport, "Box is in vanilla as intended");
        Assert(page.Box.Manifest.Contents.Count == 0, "Box doesn't contains any content");
    }
}