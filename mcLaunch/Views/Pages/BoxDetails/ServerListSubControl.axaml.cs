using System.Threading.Tasks;
using mcLaunch.Core.MinecraftFormats;

namespace mcLaunch.Views.Pages.BoxDetails;

public partial class ServerListSubControl : SubControl
{
    public ServerListSubControl()
    {
        InitializeComponent();
    }

    public override string Title => "SERVERS";

    public override async Task PopulateAsync()
    {
        ServersList.SetBox(Box);
        ServersList.SetLaunchPage(ParentPage);

        ServersList.SetLoadingCircle(true);

        MinecraftServer[] servers = await Task.Run(() => Box.LoadServers());

        await ServersList.SetServersAsync(servers);
        ServersList.SetLoadingCircle(false);

        DataContext = servers.Length;
    }
}