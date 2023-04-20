using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ddLaunch.Core.Boxes;
using ddLaunch.Core.MinecraftFormats;

namespace ddLaunch.Views.Pages.BoxDetails;

public partial class ServerListSubControl : UserControl, ISubControl
{
    public ServerListSubControl()
    {
        InitializeComponent();
    }

    public BoxDetailsPage ParentPage { get; set; }
    public Box Box { get; set; }
    public string Title { get; } = "SERVERS";
    
    public async Task PopulateAsync()
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