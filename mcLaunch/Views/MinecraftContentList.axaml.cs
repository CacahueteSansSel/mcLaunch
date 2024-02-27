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

    private List<MinecraftContent> fullContentList = new();

    private Box lastBox;
    private string lastQuery;

    public MinecraftContentList()
    {
        InitializeComponent();

        LoadMoreButton.IsVisible = false;

        DataContext = new Data();
    }

    public bool HideInstalledBadges { get; set; }
    public MinecraftContentType ContentType { get; set; }
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
        LoadMoreButton.IsVisible = true;
        LoadMoreButton.IsEnabled = true;
    }

    public void HideLoadMoreButton()
    {
        LoadMoreButton.IsEnabled = false;
        LoadMoreButton.IsVisible = false;
    }

    public void SetContents(MinecraftContent[] contents)
    {
        Data ctx = (Data) DataContext;
        ctx.Contents = contents;

        fullContentList = [..contents];

        ApplyContentAttributes();
    }

    public void SetQuery(string? query)
    {
        Data ctx = (Data) DataContext;
        ctx.Contents = string.IsNullOrWhiteSpace(query)
            ? fullContentList.ToArray()
            : fullContentList.Where(mod => mod.MatchesQuery(query)).ToArray();
    }

    private void ApplyContentAttributes()
    {
        if (lastBox == null) return;

        Data ctx = (Data) DataContext;
        List<MinecraftContent> newList = new List<MinecraftContent>(ctx.Contents);

        foreach (MinecraftContent content in newList)
        {
            content.IsInstalledOnCurrentBox = lastBox.HasContentSoft(content);
            content.IsInstalledOnCurrentBoxUi = !HideInstalledBadges && content.IsInstalledOnCurrentBox;
        }

        ctx.Contents = newList.ToArray();

        NtsBanner.IsVisible = Contents == null || Contents.Length == 0;
        LoadMoreButton.IsVisible = !NtsBanner.IsVisible;
    }

    public void SetLoadingCircle(bool isLoading)
    {
        LoadCircle.IsVisible = isLoading;
        LoadMoreButton.IsVisible = !isLoading && !NtsBanner.IsVisible;
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

        ctx.Contents = await SearchContentsAsync(box, query);

        ApplyContentAttributes();

        LoadCircle.IsVisible = false;
        LoadMoreButton.IsEnabled = ctx.Contents.Length >= 10;
        LoadMoreButton.IsVisible = ctx.Contents.Length >= 10;
    }

    private async Task<MinecraftContent[]> SearchContentsAsync(Box box, string query)
    {
        Data ctx = (Data) DataContext;

        PaginatedResponse<MinecraftContent> mods = await ModPlatformManager.Platform
            .GetContentsAsync(ctx.Page, box, query, ContentType);

        lastBox = box;
        lastQuery = query;

        return mods.Items.ToArray();
    }

    private async void LoadMoreButtonClicked(object? sender, RoutedEventArgs e)
    {
        Data ctx = (Data) DataContext;

        LoadMoreButton.IsEnabled = false;
        LoadCircle.IsVisible = true;

        ctx.Page++;

        List<MinecraftContent> contents = new List<MinecraftContent>(ctx.Contents);
        contents.AddRange(await SearchContentsAsync(lastBox, lastQuery));

        ctx.Contents = contents.ToArray();

        LoadCircle.IsVisible = false;
        LoadMoreButton.IsEnabled = true;
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