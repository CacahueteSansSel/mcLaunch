using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using Cacahuete.MinecraftLib.Core;
using Cacahuete.MinecraftLib.Download;
using System;

namespace ddLaunch.Core.Managers;

public static class DownloadManager
{
    static string currentSectionName;
    static List<DownloadEntry> currentSectionEntries = new();

    static List<DownloadSection> sections = new();

    static HttpClient client;

    public static DownloadSection? CurrentSection { get; private set; }
    public static int PendingSectionCount => sections.Count;
    public static string DescriptionLine => CurrentSection == null ? "No pending download" : CurrentSection.Name;
    public static bool IsDownloadInProgress { get; private set; }

    public static event Action<string, float, int> OnDownloadProgressUpdate;
    public static event Action OnDownloadFinished;
    public static event Action<string> OnDownloadPrepareStarting;
    public static event Action<string, string> OnDownloadError;
    public static event Action OnDownloadPrepareEnding;
    public static event Action<string, int> OnDownloadSectionStarting;

    public static void Init()
    {
        Context.Init(new Downloader());
    }

    public static void Begin(string name)
    {
        if (IsDownloadInProgress) return;
        if (currentSectionName != null)
        {
            throw new InvalidOperationException($"Last download event '{currentSectionName}' is not finished");
            return;
        }

        currentSectionName = name;

        client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ddLaunch", "1.0.0"));

        OnDownloadPrepareStarting?.Invoke(name);
    }

    public static void End()
    {
        if (IsDownloadInProgress) return;
        sections.Add(new DownloadSection
        {
            Entries = new List<DownloadEntry>(currentSectionEntries),
            Name = currentSectionName
        });

        currentSectionName = null;
        currentSectionEntries = new List<DownloadEntry>();
        OnDownloadPrepareEnding?.Invoke();
    }

    public static async Task WaitForPendingDownloads()
    {
        await Task.Run(async () =>
        {
            while (IsDownloadInProgress)
                await Task.Delay(1);
        });
    }

    public static void Add(string source, string target, EntryAction action)
    {
        currentSectionEntries.Add(new DownloadEntry {Source = source, Target = target, Action = action});
    }

    public static async Task DownloadAll()
    {
        if (IsDownloadInProgress) return;
        IsDownloadInProgress = true;

        int sectionIndex = 0;
        foreach (DownloadSection section in sections)
        {
            CurrentSection = section;
            OnDownloadSectionStarting?.Invoke(section.Name, sectionIndex);

            int progress = 0;
            foreach (DownloadEntry entry in section.Entries)
            {
                switch (entry.Action)
                {
                    case EntryAction.Download:
                        try
                        {
                            // Some files can have empty source link, we ignore those
                            if (string.IsNullOrWhiteSpace(entry.Source)) continue;
                            
                            HttpResponseMessage resp = await client.GetAsync(entry.Source,
                                HttpCompletionOption.ResponseHeadersRead);
                            resp.EnsureSuccessStatusCode();

                            string folder = entry.Target.Replace(Path.GetFileName(entry.Target), "").Trim('/');
                            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                            Stream downloadStream = await resp.Content.ReadAsStreamAsync();
                            long size = resp.Content.Headers.ContentLength ?? 0;
                            FileStream fs = new FileStream(entry.Target, FileMode.Create, FileAccess.Write);
                            long b = 0;

                            while (true)
                            {
                                float oneEntryMax = 1f / section.Entries.Count;
                                double byteProgress = (double) b / size * oneEntryMax;

                                byte[] buffer = new byte[25];
                                int input = await downloadStream.ReadAsync(buffer);
                                if (input == 0) break;

                                await fs.WriteAsync(buffer, 0, input);

                                OnDownloadProgressUpdate?.Invoke(entry.Source,
                                    (float) progress / section.Entries.Count + (float) byteProgress,
                                    sectionIndex);

                                b += 25;
                            }

                            fs.Close();
                        }
                        catch (InvalidProgramException e)
                        {
                            OnDownloadError?.Invoke(section.Name, entry.Source);
                        }

                        break;
                    case EntryAction.Extract:
                        if (!Directory.Exists(entry.Target)) Directory.CreateDirectory(entry.Target);

                        ZipFile.ExtractToDirectory(entry.Source, entry.Target, true);
                        break;
                }

                progress++;
                OnDownloadProgressUpdate?.Invoke(entry.Source, (float) progress / section.Entries.Count,
                    sectionIndex);
            }

            sectionIndex++;
        }

        CurrentSection = null;

        IsDownloadInProgress = false;
        sections.Clear();
        currentSectionEntries.Clear();
        OnDownloadFinished?.Invoke();

        Debug.WriteLine("Download Jobs Finished");
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

public class DownloadSection
{
    public List<DownloadEntry> Entries { get; init; }
    public string Name { get; init; }
}

public enum EntryAction
{
    Download,
    Extract
}