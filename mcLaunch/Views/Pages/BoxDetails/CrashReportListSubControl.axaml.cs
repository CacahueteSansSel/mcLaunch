using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.MinecraftFormats;

namespace mcLaunch.Views.Pages.BoxDetails;

public partial class CrashReportListSubControl : SubControl
{
    public override string Title => "CRASH REPORTS";
    
    public CrashReportListSubControl()
    {
        InitializeComponent();
    }
    
    public override async Task PopulateAsync()
    {
        CrashReportsList.SetLoadingCircle(true);
        CrashReportsList.SetLaunchPage(ParentPage);
        
        MinecraftCrashReport[] reports = await Task.Run(() => Box.LoadCrashReports());
        
        CrashReportsList.SetCrashReports(reports);
        CrashReportsList.SetLoadingCircle(false);

        DataContext = reports.Length;
    }
}