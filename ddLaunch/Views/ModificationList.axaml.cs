using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ddLaunch.Core;
using ddLaunch.Core.Boxes;
using ddLaunch.Core.Managers;
using ddLaunch.Core.Mods;
using ddLaunch.Models;
using ddLaunch.Utilities;
using ddLaunch.Views.Pages;
using ReactiveUI;

namespace ddLaunch.Views;

public partial class ModificationList : UserControl
{
    Box lastBox;
    string lastQuery;
    
    public ModificationList()
    {
        InitializeComponent();

        DataContext = new Data();
    }

    public void SetBox(Box box)
    {
        lastBox = box;
    }

    public void HideLoadMoreButton()
    {
        LoadMoreButton.IsEnabled = false;
        LoadMoreButton.IsVisible = false;
    }

    public void SetModifications(Modification[] mods)
    {
        Data ctx = (Data) DataContext;

        ctx.Modifications = mods;
    }

    public void SetLoadingCircle(bool isLoading)
    {
        LoadCircle.IsVisible = isLoading;
    }

    public async void Search(Box box, string query)
    {
        await SearchAsync(box, query);
    }

    public async Task SearchAsync(Box box, string query)
    {
        LoadMoreButton.IsEnabled = false;
        LoadCircle.IsVisible = true;
        
        Data ctx = (Data) DataContext;

        ctx.Modifications = await SearchModsAsync(box, query);

        LoadCircle.IsVisible = false;
        LoadMoreButton.IsEnabled = true;
    }

    async Task<Modification[]> SearchModsAsync(Box box, string query)
    {
        Data ctx = (Data) DataContext;
        
        var mods = await ModPlatformManager.Platform.GetModsAsync(ctx.Page, box, query);
        
        lastBox = box;
        lastQuery = query;

        return mods;
    }

    public class Data : ReactiveObject
    {
        Modification[] mods;
        int page;
        
        public Modification[] Modifications
        {
            get => mods;
            set => this.RaiseAndSetIfChanged(ref mods, value);
        }

        public int Page
        {
            get => page;
            set => this.RaiseAndSetIfChanged(ref page, value);
        }
    }

    private async void LoadMoreButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (lastBox == null) return;
        
        Data ctx = (Data) DataContext;
        
        LoadMoreButton.IsEnabled = false;
        LoadCircle.IsVisible = true;

        ctx.Page++;

        List<Modification> mods = new List<Modification>(ctx.Modifications);
        mods.AddRange(await SearchModsAsync(lastBox, lastQuery));

        ctx.Modifications = mods.ToArray();

        LoadCircle.IsVisible = false;
        LoadMoreButton.IsEnabled = true;
    }

    private void ModSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0)
        {
            Modification selectedMod = (Modification)e.AddedItems[0];
            Navigation.Push(new ModDetailsPage(selectedMod, lastBox));
        }
    }
}