using mcLaunch.Launchsite.Download;

namespace mcLaunch.Launchsite.Core;

public static class Context
{
    public static ResourceDownloader Downloader { get; private set; }

    public static void Init(ResourceDownloader downloader)
    {
        Downloader = downloader;
    }
}