using mcLaunch.Launchsite.Http;
using mcLaunch.Launchsite.Models;

namespace mcLaunch.Launchsite.Core.ModLoaders;

public class QuiltModLoaderVersion : ModLoaderVersion
{
    public override async Task<Result<MinecraftVersion>> GetMinecraftVersionAsync(string minecraftVersionId)
    {
        string url =
            $"{QuiltModLoaderSupport.Url}/v3/versions/loader/{minecraftVersionId}/{Name}/profile/json";

        return new Result<MinecraftVersion>(await Api.GetAsync<MinecraftVersion>(url, true));
    }
}