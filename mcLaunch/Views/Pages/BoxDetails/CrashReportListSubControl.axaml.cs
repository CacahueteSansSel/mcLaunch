using System.Threading.Tasks;
using mcLaunch.Core.MinecraftFormats;

namespace mcLaunch.Views.Pages.BoxDetails;

public partial class CrashReportListSubControl : SubControl
{
    public CrashReportListSubControl()
    {
        InitializeComponent();
    }

    public override string Title => "CRASH REPORTS";

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