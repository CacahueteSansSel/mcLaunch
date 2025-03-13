using System.Linq;
using System.Threading.Tasks;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Contents;
using mcLaunch.Core.Contents.Platforms;
using mcLaunch.Core.Core;

namespace mcLaunch.Tests.BuiltInTests;

public class BoxAddModFromCurseforgeTest : UnitTest
{
    public override async Task RunAsync()
    {
        Box createdBox = await CreateBoxAsync("1.21.1", "fabric");

        PaginatedResponse<MinecraftContent> searchHits =
            await CurseForgeMinecraftContentPlatform.Instance.GetContentsAsync(0, createdBox, "",
                MinecraftContentType.Modification);
        MinecraftContent secondModFromSearch = searchHits.Items[1];
        ContentVersion[] version =
            await CurseForgeMinecraftContentPlatform.Instance.GetContentVersionsAsync(secondModFromSearch,
                createdBox.ModLoader.Id,
                createdBox.Manifest.Version);
        Assert(version.Length > 0, $"A version of {secondModFromSearch.Name} is available " +
                                   $"for {createdBox.Manifest.Version} {createdBox.Manifest.ModLoaderId}");
        Assert(createdBox.Manifest.Version.StartsWith(version[0].MinecraftVersion),
            $"{secondModFromSearch.Name} matches the Box's Minecraft version ({createdBox.Manifest.Version})",
            $"Mod version is {version[0].MinecraftVersion}");

        await CurseForgeMinecraftContentPlatform.Instance.InstallContentAsync(createdBox, secondModFromSearch,
            version[0].Id, false, true);

        Assert(createdBox.Manifest.Contents.Any(c => c.Content.Name == secondModFromSearch.Name),
            $"Box contains {secondModFromSearch.Name} as requested");

        createdBox.Delete();
    }
}