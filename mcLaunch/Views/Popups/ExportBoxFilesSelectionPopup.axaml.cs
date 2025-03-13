using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using mcLaunch.Core.Boxes;
using mcLaunch.Utilities;

namespace mcLaunch.Views.Popups;

public partial class ExportBoxFilesSelectionPopup : UserControl
{
    private readonly Entry[] entries;
    private readonly Action<Entry[]> entriesSelectedCallback;
    private Box box;

    public ExportBoxFilesSelectionPopup()
    {
        InitializeComponent();
    }

    public ExportBoxFilesSelectionPopup(Box box, Action<Entry[]> entriesSelectedCallback)
    {
        InitializeComponent();

        this.box = box;
        this.entriesSelectedCallback = entriesSelectedCallback;

        entries = GetDirectoryEntries(box.Folder.CompletePath);
        FilesList.ItemsSource = entries;
    }

    private bool IsPathMandatory(string path, bool isDirectory)
    {
        if (isDirectory && path == "mods") return true;
        if (isDirectory && path == "resourcepacks") return true;
        if (isDirectory && path == "shaderpacks") return true;

        return false;
    }

    private string? GetEntryComment(string path, bool isDirectory)
    {
        if (isDirectory)
        {
            switch (path)
            {
                case "config":
                    return "Contains mods and modloader config";
                case "crash-reports":
                    return "Contains the crash reports";
                case "logs":
                    return "Contains Minecraft's logs";
                case "mods":
                    return "Contains your mods JAR files";
                case "resourcepacks":
                    return "Contains your resource packs";
                case "shaderpacks":
                    return "Contains your shaders";
                case "saves":
                    return "Contains your worlds (very large !)";
            }

            return null;
        }

        switch (path)
        {
            case "options.txt":
                return "Contains your Minecraft options";
        }

        return null;
    }

    private Entry[] GetDirectoryEntries(string path)
    {
        List<Entry> entries = new();
        DirectoryInfo dir = new(path);

        foreach (DirectoryInfo childDir in dir.GetDirectories())
        {
            entries.Add(new Entry
            {
                Name = childDir.Name, IsDirectory = true, IsMandatory = IsPathMandatory(childDir.Name, true),
                Comment = GetEntryComment(childDir.Name, true)
            });
        }

        foreach (FileInfo childFile in dir.GetFiles())
        {
            entries.Add(new Entry
            {
                Name = childFile.Name, IsDirectory = false, IsMandatory = IsPathMandatory(childFile.Name, false),
                Comment = GetEntryComment(childFile.Name, false)
            });
        }

        return entries.ToArray();
    }

    private void CancelButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
    }

    private void ExportButtonClicked(object? sender, RoutedEventArgs e)
    {
        Entry[] selectedEntries = entries.Where(entry => entry.IsChecked).ToArray();
        Navigation.HidePopup();

        entriesSelectedCallback?.Invoke(selectedEntries);
    }

    public class Entry
    {
        public string Name { get; set; }
        public string? Comment { get; set; }
        public bool IsDirectory { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsChecked { get; set; }
    }
}