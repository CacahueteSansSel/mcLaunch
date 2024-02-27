using Cacahuete.MinecraftLib.Http;
using Cacahuete.MinecraftLib.Models;

namespace Cacahuete.MinecraftLib.Core.ModLoaders;

public class FabricModLoaderVersion : ModLoaderVersion
{
    public override async Task<MinecraftVersion?> GetMinecraftVersionAsync(string minecraftVersionId)
    {
        string url =
            $"{FabricModLoaderSupport.Url}/v2/versions/loader/{minecraftVersionId}/{Name}/profile/json";

        return await Api.GetAsync<MinecraftVersion>(url, true);
    }
}