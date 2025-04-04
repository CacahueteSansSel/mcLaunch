using System.Text.Json;
using mcLaunch.Launchsite.Http;
using mcLaunch.Launchsite.Models;

namespace mcLaunch.Launchsite.Core.ModLoaders;

public class FabricModLoaderVersion : ModLoaderVersion
{
    public override async Task<Result<MinecraftVersion>> GetMinecraftVersionAsync(string minecraftVersionId)
    {
        try
        {
            string url =
                $"{FabricModLoaderSupport.Url}/v2/versions/loader/{minecraftVersionId}/{Name}/profile/json";

            return new Result<MinecraftVersion>(await Api.GetAsync<MinecraftVersion>(url, true));
        }
        catch (JsonException e)
        {
            return Result<MinecraftVersion>.Error($"Remote JSON exception: {e.Message}");
        }
    }
}