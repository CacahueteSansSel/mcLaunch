using System.IO;
using System.Linq;
using System.Threading.Tasks;
using mcLaunch.Launchsite.Core.ModLoaders;
using mcLaunch.Models;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;
using mcLaunch.Views.Popups;

namespace mcLaunch.Tests.BuiltInTests;

public class ImportBoxTest : UnitTest
{
    public override async Task RunAsync()
    {
        Assert(File.Exists("TestModpacks/mclaunch.box"), "Test modpack exists");
        
        await BoxUtilities.ImportBoxAsync("TestModpacks/mclaunch.box", false);
        
        Assert(!IsPopupShown<MessageBoxPopup>(), "No message box is shown");
        Assert(IsPageShown<BoxDetailsPage>(), "Shown page is BoxDetailsPage");
        BoxDetailsPage page = (BoxDetailsPage) MainWindowDataContext.Instance.CurrentPage;
        
        Assert(page.Box.Manifest.Name == "test modpack mclaunch", "Box's name matches the modpack's one");
        Assert(page.Box.ModLoader is FabricModLoaderSupport, "Box's modloader is Fabric (as in the modpack)");
        Assert(page.Box.Manifest.Version == "1.21.1", "Box's Minecraft version is 1.21.1 (as in the modpack)");
        Assert(page.Box.Manifest.Contents.Count == 4, "Box does have 4 mods (as in the modpack)");
        Assert(page.Box.Manifest.Contents.Any(content => content.Content?.Name == "Fabric API"), 
            "Box does have Fabric API (as expected)");
        
        Navigation.Pop();
        DeleteBoxAndCheck(page.Box);
    }
}