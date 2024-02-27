using Cacahuete.MinecraftLib.Http;
using Cacahuete.MinecraftLib.Models;

namespace Cacahuete.MinecraftLib.Core.ModLoaders;

public class QuiltModLoaderVersion : ModLoaderVersion
{
    public override async Task<MinecraftVersion?> GetMinecraftVersionAsync(string minecraftVersionId)
    {
        string url =
            $"{QuiltModLoaderSupport.Url}/v3/versions/loader/{minecraftVersionId}/{Name}/profile/json";

        return await Api.GetAsync<MinecraftVersion>(url, true);
    }
}