using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Contents;
using mcLaunch.Core.Core;
using mcLaunch.Core.Managers;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;
using ReactiveUI;

namespace mcLaunch.Views;

public partial class MinecraftContentList : UserControl, IBoxEventListener
{
    public static readonly AttachedProperty<bool> HideInstalledBadgesProperty
        = AvaloniaProperty.RegisterAttached<MinecraftContentList, UserControl, bool>(
            nameof(HideInstalledBadges),
            false,
            true
        );

    public static readonly AttachedProperty<MinecraftContentType> ContentTypeProperty
        = AvaloniaProperty.RegisterAttached<MinecraftContentList, UserControl, MinecraftContentType>(
            nameof(ContentType),
            MinecraftContentType.Modification,
            true
        );

    public static readonly AttachedProperty<bool> HidePageSelectorProperty
        = AvaloniaProperty.RegisterAttached<MinecraftContentList, UserControl, bool>(
            nameof(HidePageSelector),
            false,
            true
        );

    private List<MinecraftContent> list = new();

    private PageSelector[] pageSelectors;

    private Box lastBox;
    private string lastQuery;

    public MinecraftContentList()
    {
        InitializeComponent();

        pageSelectors =
        [
            PageSelectorComponentTop,
            PageSelectorComponentBottom
        ];

        foreach (PageSelector component in pageSelectors)
            component.IsVisible = false;

        DataContext = new Data();
    }

    public bool HideInstalledBadges { get; set; }
    public MinecraftContentType ContentType { get; set; }
    public bool HidePageSelector { get; set; }
    public MinecraftContent[] Contents => ((Data) DataContext).Contents;

    public void OnContentAdded(MinecraftContent content)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Data ctx = (Data) DataContext;

            List<MinecraftContent> contents = new List<MinecraftContent>(ctx.Contents);
            contents.Add(content);
            ctx.Contents = contents.ToArray();
        });
    }

    public void OnContentRemoved(string contentId)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Data ctx = (Data) DataContext;

            ctx.Contents = ctx.Contents.Where(content => content.Id != contentId).ToArray();
        });
    }

    public void SetBox(Box box)
    {
        lastBox = box;
        box.EventListener = this;
    }

    public void ShowLoadMoreButton()
    {
        if (HidePageSelector) return;
        
        foreach (PageSelector component in pageSelectors)
        {
            component.IsVisible = true;
            component.IsEnabled = true;
        }
    }

    public void HideLoadMoreButton()
    {
        foreach (PageSelector component in pageSelectors)
            component.IsVisible = false;
    }

    public void SetContents(MinecraftContent[] contents)
    {
        Data ctx = (Data) DataContext;
        
        ctx.Contents = contents;
        list = contents.ToList();

        NtsBanner.IsVisible = false;
        ApplyContentAttributes();
    }

    public void SetQuery(string? query)
    {
        Data ctx = (Data) DataContext;
        ctx.Contents = string.IsNullOrWhiteSpace(query)
            ? list.ToArray()
            : list.Where(mod => mod.MatchesQuery(query)).ToArray();
    }

    private void ApplyContentAttributes()
    {
        if (lastBox == null) return;

        Data ctx = (Data) DataContext;

        foreach (MinecraftContent content in ctx.Contents)
        {
            content.IsInstalledOnCurrentBox = lastBox.HasContentSoft(content);
            content.IsInstalledOnCurrentBoxUi = !HideInstalledBadges && content.IsInstalledOnCurrentBox;
        }

        NtsBanner.IsVisible = ctx.Contents.Length == 0;
        
        foreach (PageSelector component in pageSelectors)
            component.IsVisible = !NtsBanner.IsVisible && !HidePageSelector;
    }

    public void SetLoadingCircle(bool isLoading)
    {
        LoadCircle.IsVisible = isLoading;
        if (isLoading) NtsBanner.IsVisible = false;
        
        foreach (PageSelector component in pageSelectors)
            component.IsVisible = !isLoading;

        if (!isLoading && !NtsBanner.IsVisible) PageSelectorComponentBottom.IsVisible = !HidePageSelector;
    }

    public async void Search(Box box, string query)
    {
        await SearchAsync(box, query);
    }

    public async Task SearchAsync(Box box, string query)
    {
        SetLoadingCircle(true);

        Data ctx = (Data) DataContext;

        ctx.Contents = await SearchContentsAsync(box, query);

        ApplyContentAttributes();

        SetLoadingCircle(false);
        
        foreach (PageSelector component in pageSelectors)
            component.IsVisible = ctx.Contents.Length >= 10 && !HidePageSelector;
    }

    private async Task<MinecraftContent[]> SearchContentsAsync(Box box, string query)
    {
        Data ctx = (Data) DataContext;

        PaginatedResponse<MinecraftContent> mods = await ModPlatformManager.Platform
            .GetContentsAsync(ctx.Page, box, query, ContentType);

        lastBox = box;
        lastQuery = query;
        
        foreach (PageSelector component in pageSelectors)
        {
            component.Setup(mods.TotalPageCount, PageSelectedCallback);
            component.SetPage(ctx.Page, false);
        }

        return mods.Items.ToArray();
    }

    private async void PageSelectedCallback(int index)
    {
        foreach (PageSelector component in pageSelectors)
            component.IsEnabled = false;
        
        Data ctx = (Data) DataContext;
        
        ctx.Page = index;
        ctx.Contents = await SearchContentsAsync(lastBox, lastQuery);

        foreach (PageSelector component in pageSelectors)
            component.IsEnabled = true;
    }

    private void ContentSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0)
        {
            MinecraftContent selectedContent = (MinecraftContent) e.AddedItems[0];
            Navigation.Push(new ContentDetailsPage(selectedContent, lastBox));

            ContentList.UnselectAll();
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        if (lastBox != null && lastBox.EventListener == this)
            lastBox.EventListener = null;

        base.OnUnloaded(e);
    }

    public class Data : ReactiveObject
    {
        private MinecraftContent[] contents;
        private int page;

        public MinecraftContent[] Contents
        {
            get => contents;
            set => this.RaiseAndSetIfChanged(ref contents, value);
        }

        public int Page
        {
            get => page;
            set => this.RaiseAndSetIfChanged(ref page, value);
        }
    }
}