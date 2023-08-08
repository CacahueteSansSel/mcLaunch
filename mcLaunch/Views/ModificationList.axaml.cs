using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using mcLaunch.Core;
using mcLaunch.Core.Mods.Platforms;
using mcLaunch.Models;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Managers;
using mcLaunch.Core.Mods;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;
using ReactiveUI;

namespace mcLaunch.Views;

public partial class ModificationList : UserControl
{
    public static AttachedProperty<bool> HideInstalledBadgesProperty
        = AvaloniaProperty.RegisterAttached<ModificationList, UserControl, bool>(
            nameof(HideInstalledBadges),
            false,
            true
        );

    Box lastBox;
    string lastQuery;

    public bool HideInstalledBadges { get; set; }

    public ModificationList()
    {
        InitializeComponent();

        LoadMoreButton.IsVisible = false;

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

        SetModificationsAttributes();
    }

    void SetModificationsAttributes()
    {
        if (lastBox == null) return;

        Data ctx = (Data) DataContext;
        List<Modification> newList = new List<Modification>(ctx.Modifications);
        
        Bitmap bmp = new Bitmap(AssetLoader.Open(new Uri($"avares://mcLaunch/resources/default_mod_logo.png")));

        foreach (Modification mod in newList)
        {
            mod.IsInstalledOnCurrentBox = lastBox.HasModificationSoft(mod);
            mod.IsInstalledOnCurrentBoxUi = !HideInstalledBadges && mod.IsInstalledOnCurrentBox;

            if (mod.Icon == null) mod.Icon = bmp;
        }

        ctx.Modifications = newList.ToArray();

        LoadMoreButton.IsVisible = true;
    }

    public void SetLoadingCircle(bool isLoading)
    {
        LoadCircle.IsVisible = isLoading;
        LoadMoreButton.IsVisible = !isLoading;
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

        SetModificationsAttributes();

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
            Modification selectedMod = (Modification) e.AddedItems[0];
            Navigation.Push(new ModDetailsPage(selectedMod, lastBox));

            ModList.UnselectAll();
        }
    }
}