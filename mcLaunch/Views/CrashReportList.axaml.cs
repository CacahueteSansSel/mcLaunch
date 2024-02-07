using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using mcLaunch.Core;
using mcLaunch.Core.Managers;
using mcLaunch.Core.Mods;
using mcLaunch.Core.Mods.Platforms;
using mcLaunch.Models;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.MinecraftFormats;
using mcLaunch.Views.Popups;
using ReactiveUI;

namespace mcLaunch.Views;

public partial class CrashReportList : UserControl
{
    Box lastBox;
    string lastQuery;
    private BoxDetailsPage launchPage;

    public CrashReportList()
    {
        InitializeComponent();

        DataContext = new Data();
    }

    public void SetBox(Box box)
    {
        lastBox = box;
    }

    public void SetLaunchPage(BoxDetailsPage page)
    {
        launchPage = page;
    }

    public void SetCrashReports(MinecraftCrashReport[] reports)
    {
        Data ctx = (Data) DataContext;

        ctx.Reports = reports;

        NtsBanner.IsVisible = reports.Length == 0;
    }

    public void SetLoadingCircle(bool isLoading)
    {
        LoadCircle.IsVisible = isLoading;
    }

    public class Data : ReactiveObject
    {
        MinecraftCrashReport[] reports;
        int page;

        public MinecraftCrashReport[] Reports
        {
            get => reports;
            set => this.RaiseAndSetIfChanged(ref reports, value);
        }

        public int Page
        {
            get => page;
            set => this.RaiseAndSetIfChanged(ref page, value);
        }
    }

    private void ReportsSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0)
        {
            MinecraftCrashReport world = (MinecraftCrashReport) e.AddedItems[0];
            PlatformSpecific.OpenFile(world.CompletePath);
        }
        
        ReportsList.UnselectAll();
    }
}