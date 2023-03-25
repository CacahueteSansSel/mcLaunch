using Cacahuete.MinecraftLib.Download;

namespace Cacahuete.MinecraftLib.Core;

public static class Context
{
    public static ResourceDownloader Downloader { get; private set; }

    public static void Init(ResourceDownloader downloader)
    {
        Downloader = downloader;
    }
}