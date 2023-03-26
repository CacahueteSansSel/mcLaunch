using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ddLaunch.Core;
using ddLaunch.Core.Boxes;
using ddLaunch.Core.Managers;
using ddLaunch.Core.Mods;
using ddLaunch.Models;
using ReactiveUI;

namespace ddLaunch.Views;

public partial class ModificationList : UserControl
{
    public ModificationList()
    {
        InitializeComponent();

        DataContext = new Data();
    }

    public async void Search(Box box, string query)
    {
        await SearchAsync(box, query);
    }

    public async Task SearchAsync(Box box, string query)
    {
        // TODO: Loading indicator
        Data ctx = (Data) DataContext;

        ctx.Modifications = await ModPlatformManager.Platform.GetModsAsync(0, box, query);
    }
    
    public class Data : ReactiveObject
    {
        Modification[] mods;
        
        public Modification[] Modifications
        {
            get => mods;
            set => this.RaiseAndSetIfChanged(ref mods, value);
        }
    }
}