namespace mcLaunch.Core.Contents;

public static class MinecraftContentUtils
{
    public static string GetInstallFolderName(MinecraftContentType contentType)
    {
        return contentType switch
        {
            MinecraftContentType.Modification => "mods",
            MinecraftContentType.ResourcePack => "resourcepacks",
            MinecraftContentType.ShaderPack => "shaderpacks",
            MinecraftContentType.DataPack => "datapacks",
            MinecraftContentType.World => "saves",
            _ => string.Empty
        };
    }
}