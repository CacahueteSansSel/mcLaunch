using System.IO.Compression;
using System.Net.Http.Headers;
using Cacahuete.MinecraftLib.Core;
using Cacahuete.MinecraftLib.Download;

namespace ddLaunch.Core.Managers;

public static class DownloadManager
{
    static List<DownloadEntry> entries = new();
    static HttpClient client;

    public static string DownloadName { get; private set; }

    public static event Action<string, float> OnDownloadProgressUpdate;
    public static event Action OnDownloadFinished;
    public static event Action<string> OnDownloadStarting;

    public static void Init()
    {
        Context.Init(new Downloader());
    }

    public static void Begin(string name)
    {
        DownloadName = name;

        client = new HttpClient();
        //client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ddLaunch"));
        
        OnDownloadStarting?.Invoke(name);
    }

    public static void Add(string source, string target, EntryAction action)
    {
        entries.Add(new DownloadEntry {Source = source, Target = target, Action = action});
    }

    public static async Task DownloadAll()
    {
        int progress = 0;
        foreach (DownloadEntry entry in entries)
        {
            switch (entry.Action)
            {
                case EntryAction.Download:
                    HttpResponseMessage resp = await client.GetAsync(entry.Source);
                    resp.EnsureSuccessStatusCode();

                    await File.WriteAllBytesAsync(entry.Target, await resp.Content.ReadAsByteArrayAsync());
                    break;
                case EntryAction.Extract:
                    ZipFile.ExtractToDirectory(entry.Source, entry.Target, true);
                    break;
            }

            progress++;
            OnDownloadProgressUpdate?.Invoke(entry.Source, (float) progress / entries.Count);
        }

        DownloadName = string.Empty;
        entries.Clear();
        OnDownloadFinished?.Invoke();
    }

    public class Downloader : ResourceDownloader
    {
        public override async Task<bool> DownloadAsync(string url, string target)
        {
            Add(url, target, EntryAction.Download);

            return true;
        }

        public override async Task<bool> ExtractAsync(string sourceArchive, string targetDir)
        {
            Add(sourceArchive, targetDir, EntryAction.Extract);

            return true;
        }
    }
}

public class DownloadEntry
{
    public string Source { get; init; }
    public string Target { get; init; }
    public EntryAction Action { get; init; }
}

public enum EntryAction
{
    Download,
    Extract
}