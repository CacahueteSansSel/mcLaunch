﻿using System.IO.Compression;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using Downloader;
using mcLaunch.Core.Core;
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
    private static string userAgent;

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

    public static void Init(string version)
    {
        Context.Init(new Downloader());
        userAgent = $"mcLaunch/{version}";
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
        currentSectionEntries.Add(new DownloadEntry { Source = source, Target = target, Hash = hash, Action = action });
    }

    private static async Task<bool> UseFallbackDownloader(string sourceUrl, string targetFilename,
        Action<float> progressUpdated, Action<bool, Exception> finished)
    {
        Console.WriteLine($"Using Fallback Downloader for {sourceUrl}");

        using FallbackDownloader downloader = new(userAgent);
        downloader.ProgressUpdated += progressUpdated;
        downloader.Finished += finished;

        return await downloader.DownloadAsync(sourceUrl, targetFilename);
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

            string file = Path.GetFileName(entry.Target);
            string folder = entry.Target.Replace(file, "").TrimEnd('/');
            if (!Directory.Exists(folder))
            {
                try
                {
                    Directory.CreateDirectory(folder);
                }
                catch (Exception e)
                {
                }
            }

            IDownload? download = DownloadBuilder.New()
                .WithConfiguration(new DownloadConfiguration
                {
                    RequestConfiguration = new RequestConfiguration
                    {
                        UserAgent = userAgent,
                        Accept = "*/*",
                        AllowAutoRedirect = false,
                        AuthenticationLevel = AuthenticationLevel.None,
                        AutomaticDecompression = DecompressionMethods.All,
                        PreAuthenticate = false
                    }
                })
                .WithUrl(entry.Source)
                .WithFolder(new DirectoryInfo(folder))
                .WithFileName(Path.GetFileName(entry.Target))
                .Build();

            download.DownloadProgressChanged += (sender, args) =>
            {
                OnDownloadProgressUpdate?.Invoke(entry.Source,
                    progress / (float)section.Entries.Count +
                    (float)(args.ProgressPercentage / 100) * (1 / (float)section.Entries.Count),
                    sectionIndex + 1);
            };

            Stream? stream = await download.StartAsync();

            if (download.Package.Status == DownloadStatus.Failed || !File.Exists(entry.Target))
            {
                await UseFallbackDownloader(entry.Source, entry.Target, pp =>
                {
                    OnDownloadProgressUpdate?.Invoke(entry.Source,
                        (float)progress / section.Entries.Count + pp,
                        sectionIndex + 1);
                }, (success, error) =>
                {
                    if (!success) Console.WriteLine($"Fallback Downloader error : {error}");

                    OnDownloadError?.Invoke(section.Name, entry.Source);
                });
            }
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
            foreach (DownloadEntry entry in section.Entries.Where(entry => entry.Action == EntryAction.Download))
            {
                await DownloadEntryAsync(entry, section, sectionIndex, progress);

                progress++;
                float percent = (float)progress / section.Entries.Count;

                if (progressPercent < percent)
                {
                    progressPercent = percent;
                    OnDownloadProgressUpdate?.Invoke(entry.Source, progressPercent, sectionIndex + 1);
                }
            }

            /*
             *await Parallel.ForEachAsync(section.Entries.Where(entry => entry.Action == EntryAction.Download),
               async (entry, token) =>
               {
                   if (entry.Source.Contains(".jar")) Console.WriteLine(entry.Source);
                   await DownloadEntryAsync(entry, section, sectionIndex, progress);

                   progress++;
                   float percent = (float) progress / section.Entries.Count;

                   if (progressPercent < percent)
                   {
                       progressPercent = percent;
                       OnDownloadProgressUpdate?.Invoke(entry.Source, progressPercent, sectionIndex + 1);
                   }
               });
             */

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
                float percent = (float)progress / section.Entries.Count;

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
                CurrentSection = new DownloadSection { Entries = [], Name = sectionName };
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

        public override Task WaitForPendingProcessesAsync() => WaitForPendingProcesses();

        public override async Task SetSectionProgressAsync(string itemName, float progressPercent) =>
            OnDownloadProgressUpdate?.Invoke(itemName, progressPercent, 0);
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