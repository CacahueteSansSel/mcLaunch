using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.MinecraftFormats;

namespace mcLaunch.Views.Pages.BoxDetails;

public partial class ServerListSubControl : SubControl
{
    public override string Title => "SERVERS";
    
    public ServerListSubControl()
    {
        InitializeComponent();
    }
    
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