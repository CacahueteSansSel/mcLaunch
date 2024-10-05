using System.Diagnostics;
using System.Threading.Tasks;

namespace mcLaunch.Tests.BuiltInTests;

public class CheckMainWindowTest : UnitTest
{
    public override async Task RunAsync()
    {
        Assert(MainWindow.Instance != null && MainWindow.Instance.IsVisible);
    }
}