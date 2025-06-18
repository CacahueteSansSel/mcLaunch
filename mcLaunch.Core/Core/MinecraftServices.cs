using mcLaunch.Core.Managers;
using mcLaunch.Core.Models;
using mcLaunch.Launchsite.Http;
using mcLaunch.Launchsite.Models;

namespace mcLaunch.Core.Core;

public static class MinecraftServices
{
    public static async Task<MinecraftProfile?> UploadSkinAsync(string filename, SkinType type)
    {
        MultipartFormDataContent form = new();
        form.Add(new StringContent(type.ToString().ToLower()), "variant");
        form.Add(new ByteArrayContent(await File.ReadAllBytesAsync(filename)), "file", Path.GetFileName(filename));

        MinecraftProfile? profile = await Api.PostFormAuthAsync<MinecraftProfile>(
            "https://api.minecraftservices.com/minecraft/profile/skins", form,
            AuthenticationManager.Account!.AccessToken);

        return profile;
    }
    
    public static async Task<MinecraftProfile?> ChangeSkinAsync(string url, SkinType type)
    {
        MinecraftProfile? profile = await Api.PostAsync<ChangeSkinPayload, MinecraftProfile>(
            "https://api.minecraftservices.com/minecraft/profile/skins",
            new ChangeSkinPayload {Variant = type.ToString().ToLower(), Url = url},
            AuthenticationManager.Account!.AccessToken);

        return profile;
    }
}

public enum SkinType
{
    Classic,
    Slim
}