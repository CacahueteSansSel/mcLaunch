using Avalonia.Controls;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.MinecraftFormats;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;
using ReactiveUI;

namespace mcLaunch.Views;

public partial class CrashReportList : UserControl
{
    private Box lastBox;
    private string lastQuery;
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
        Data ctx = (Data)DataContext;

        ctx.Reports = reports;

        NtsBanner.IsVisible = reports.Length == 0;
    }

    public void SetLoadingCircle(bool isLoading)
    {
        LoadCircle.IsVisible = isLoading;
    }

    private void ReportsSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0)
        {
            MinecraftCrashReport world = (MinecraftCrashReport)e.AddedItems[0];
            PlatformSpecific.OpenFile(world.CompletePath);
        }

        ReportsList.UnselectAll();
    }

    public class Data : ReactiveObject
    {
        private int page;
        private MinecraftCrashReport[] reports;

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
}