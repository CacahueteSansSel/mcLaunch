using System.IO.Compression;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using mcLaunch.Core.Utilities;
using mcLaunch.Launchsite.Core;
using mcLaunch.Launchsite.Download;

namespace mcLaunch.Core.Managers;

public static class DownloadManager
{
    private static string? currentSectionName;
    private static List<DownloadEntry> currentSectionEntries = [];
    private static readonly List<DownloadSection> sections = [];
    private static HttpClient client;

    public static DownloadSection? CurrentSection { get; private set; }
    public static int PendingSectionCount => sections.Count;
    public static string DescriptionLine => CurrentSection == null ? "No pending download" : CurrentSection.Name;
    public static bool IsProcessing { get; private set; }

    public static event Action<string, float, int>? OnDownloadProgressUpdate;
    public static event Action? OnDownloadFinished;
    public static event Action<string>? OnDownloadPrepareStarting;
    public static event Action<string, string>? OnDownloadError;
    public static event Action? OnDownloadPrepareEnding;
    public static event Action<string, int>? OnDownloadSectionStarting;

    public static void Init()
    {
        Context.Init(new Downloader());
    }

    public static void Begin(string name)
    {
        if (IsProcessing) return;
        if (currentSectionName != null)
        {
            throw new InvalidOperationException($"Last download event '{currentSectionName}' is not finished");
            return;
        }

        currentSectionName = name;

        client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("mcLaunch", "1.0.0"));

        OnDownloadPrepareStarting?.Invoke(name);
    }

    public static void End()
    {
        if (IsProcessing) return;
        sections.Add(new DownloadSection
        {
            Entries = new List<DownloadEntry>(currentSectionEntries),
            Name = currentSectionName
        });

        currentSectionName = null;
        currentSectionEntries = new List<DownloadEntry>();
        OnDownloadPrepareEnding?.Invoke();
    }

    public static async Task WaitForPendingProcesses()
    {
        await Task.Run(async () =>
        {
            while (IsProcessing)
                await Task.Delay(1);
        });
    }

    public static void Add(string source, string target, string? hash, EntryAction action)
    {
        currentSectionEntries.Add(new DownloadEntry {Source = source, Target = target, Hash = hash, Action = action});
    }

    private static async Task DownloadEntryAsync(DownloadEntry entry, DownloadSection section, int sectionIndex,
        int progress)
    {
        try
        {
            // Some files can have empty source link, we ignore those
            if (string.IsNullOrWhiteSpace(entry.Source)) return;

            if (File.Exists(entry.Target) && entry.Hash != null)
            {
                string localFileHash = Convert.ToHexString(
                    SHA1.HashData(await File.ReadAllBytesAsync(entry.Target))).ToLower();

                if (localFileHash == entry.Hash.ToLower())
                    return;
            }

            if (entry.Hash == null && File.Exists(entry.Target)) return;

            HttpResponseMessage resp = await client.GetAsync(entry.Source,
                HttpCompletionOption.ResponseHeadersRead);
            resp.EnsureSuccessStatusCode();

            string folder = entry.Target.Replace(
                Path.GetFileName(entry.Target), "").Trim('/');
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            Stream downloadStream = await resp.Content.ReadAsStreamAsync();
            long size = resp.Content.Headers.ContentLength ?? 0;
            MemoryStream ramStream = new();
            long b = 0;
            long blockSize = 1024;
            long lastBytesInSecond = 0;
            long bytesInSecond = 0;
            DateTime lastSecond = DateTime.Now;

            while (true)
            {
                float oneEntryMax = 1f / section.Entries.Count;
                double byteProgress = (double) b / size * oneEntryMax;

                byte[] buffer = new byte[blockSize];
                int input = await downloadStream.ReadAsync(buffer);
                if (input == 0) break;

                await ramStream.WriteAsync(buffer.AsMemory(0, input));

                OnDownloadProgressUpdate?.Invoke(entry.Source,
                    (float) progress / section.Entries.Count + (float) byteProgress,
                    sectionIndex + 1);

                DateTime now = DateTime.Now;
                if ((now - lastSecond).TotalSeconds > 0)
                {
                    long delta = bytesInSecond - lastBytesInSecond;

                    if (delta > 0 && blockSize < size) blockSize += 10; // Faster, increase block size
                    else if (blockSize > 25) blockSize -= 10; // Slower, decrease block size

                    bytesInSecond = 0;
                }

                lastSecond = now;

                b += input;
                bytesInSecond += input;
            }

            string pathFolders = entry.Target.Replace(Path.GetFileName(entry.Target), "").Trim();

            if (!Directory.Exists(pathFolders)) Directory.CreateDirectory(pathFolders);

            byte[] data = ramStream.ToArray();

            bool fileWritten = false;
            for (int i = 0; i < 5; i++)
                try
                {
                    await File.WriteAllBytesAsync(entry.Target, data);
                    fileWritten = true;

                    break;
                }
                catch (Exception e)
                {
                    await Task.Delay(500);
                }

            if (!fileWritten)
                throw new Exception($"File {entry.Source} at {entry.Target} download failed : " +
                                    $"file can't be accessed");

            ramStream.Close();
        }
        catch (InvalidProgramException e)
        {
            OnDownloadError?.Invoke(section.Name, entry.Source);
        }
    }

    private static Task ExtractEntryAsync(DownloadEntry entry)
    {
        if (!Directory.Exists(entry.Target)) Directory.CreateDirectory(entry.Target);

        return Task.Run(() => ZipFile.ExtractToDirectory(entry.Source, entry.Target, true));
    }

    private static async Task ChmodEntryAsync(DownloadEntry entry)
    {
        string perms = entry.Source;
        string file = entry.Target;

        await Unix.ChmodAsync(file, perms);
    }

    public static async Task ProcessAll()
    {
        if (IsProcessing) return;
        IsProcessing = true;

        int sectionIndex = 0;
        foreach (DownloadSection section in sections)
        {
            CurrentSection = section;
            OnDownloadSectionStarting?.Invoke(section.Name, sectionIndex + 1);

            int progress = 0;
            float progressPercent = 0f;
            await Parallel.ForEachAsync(section.Entries.Where(entry => entry.Action == EntryAction.Download),
                async (entry, token) =>
                {
                    await DownloadEntryAsync(entry, section, sectionIndex, progress);

                    progress++;
                    float percent = (float) progress / section.Entries.Count;

                    if (progressPercent < percent)
                    {
                        progressPercent = percent;
                        OnDownloadProgressUpdate?.Invoke(entry.Source, progressPercent, sectionIndex + 1);
                    }
                });

            foreach (DownloadEntry entry in section.Entries.Where(entry => entry.Action != EntryAction.Download))
            {
                switch (entry.Action)
                {
                    case EntryAction.Extract:
                        await ExtractEntryAsync(entry);
                        break;
                    case EntryAction.Chmod:
                        await ChmodEntryAsync(entry);
                        break;
                }

                progress++;
                float percent = (float) progress / section.Entries.Count;

                if (progressPercent < percent)
                {
                    progressPercent = percent;
                    OnDownloadProgressUpdate?.Invoke(entry.Source, progressPercent, sectionIndex + 1);
                }
            }

            sectionIndex++;
        }

        CurrentSection = null;

        IsProcessing = false;
        sections.Clear();
        currentSectionEntries.Clear();
        OnDownloadFinished?.Invoke();
    }

    public static async Task<Stream?> DownloadFileAsync(string url)
    {
        HttpResponseMessage resp = await client.GetAsync(url);
        if (!resp.IsSuccessStatusCode) return null;

        return await resp.Content.ReadAsStreamAsync();
    }

    public class Downloader : ResourceDownloader
    {
        public override async Task<bool> DownloadAsync(string url, string target, string? hash)
        {
            Add(url, target, hash, EntryAction.Download);

            return true;
        }

        public override async Task<bool> ExtractAsync(string sourceArchive, string targetDir)
        {
            Add(sourceArchive, targetDir, null, EntryAction.Extract);

            return true;
        }

        public override async Task<bool> ChmodAsync(string target, string perms)
        {
            Add(perms, target, null, EntryAction.Chmod);

            return true;
        }

        public override async Task BeginSectionAsync(string sectionName, bool immediate)
        {
            if (immediate)
            {
                CurrentSection = new DownloadSection {Entries = [], Name = sectionName};
                OnDownloadSectionStarting?.Invoke(sectionName, 0);
                return;
            }

            Begin(sectionName);
        }

        public override async Task EndSectionAsync(bool immediate)
        {
            if (immediate)
            {
                OnDownloadFinished?.Invoke();
                return;
            }

            End();
        }

        public override async Task FlushAsync()
        {
            await ProcessAll();
        }

        public override Task WaitForPendingProcessesAsync()
        {
            return WaitForPendingProcesses();
        }

        public override async Task SetSectionProgressAsync(string itemName, float progressPercent)
        {
            OnDownloadProgressUpdate?.Invoke(itemName, progressPercent, 0);
        }
    }
}

public class DownloadEntry
{
    public string Source { get; init; }
    public string Target { get; init; }
    public string? Hash { get; init; }
    public long? Size { get; init; }
    public EntryAction Action { get; init; }
}

public class DownloadSection
{
    public List<DownloadEntry> Entries { get; init; }
    public string Name { get; init; }
}

public enum EntryAction
{
    Download,
    Extract,
    Chmod
}