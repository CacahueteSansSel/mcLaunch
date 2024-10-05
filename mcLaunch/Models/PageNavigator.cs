using System.Collections.Generic;
using Avalonia.Controls;
using mcLaunch.Views;
using mcLaunch.Views.Pages;
using mcLaunch.Views.Popups;
using ReactiveUI;

namespace mcLaunch.Models;

public abstract class PageNavigator : ReactiveObject
{
    readonly Stack<ITopLevelPageControl> stack = new();
    ITopLevelPageControl? curPage;
    Control? curPopup;
    bool isPopupShown;

    public PageNavigator(ITopLevelPageControl? mainPage)
    {
        if (mainPage != null) Push(mainPage);
        HidePopup();
    }

    public ITopLevelPageControl? CurrentPage
    {
        get => curPage;
        set => this.RaiseAndSetIfChanged(ref curPage, value);
    }

    public Control? CurrentPopup
    {
        get => curPopup;
        set => this.RaiseAndSetIfChanged(ref curPopup, value);
    }

    public bool IsPopup
    {
        get => isPopupShown;
        set => this.RaiseAndSetIfChanged(ref isPopupShown, value);
    }

    public virtual void Set<T>() where T : ITopLevelPageControl, new()
    {
        CurrentPage = new T();
    }

    public virtual void Set(ITopLevelPageControl value)
    {
        CurrentPage = value;
    }

    public void Reset()
    {
        stack.Clear();
    }

    public void ShowLoadingPopup()
    {
        ShowPopup(new LoadingPopup());
    }

    public void HideLoadingPopup()
    {
        HidePopup();
    }

    public virtual void ShowLoadingPage()
    {
        CurrentPage = new LoadingPage();
    }

    public virtual void ShowStartingPage()
    {
        CurrentPage = new StartingPage();
    }

    public virtual void HideLoadingPage()
    {
        CurrentPage = stack.Count == 0 ? null : stack.Peek();
    }

    public void Push<T>(bool decorations = true) where T : ITopLevelPageControl, new()
    {
        Push(new T(), decorations);
    }

    public virtual void Push(ITopLevelPageControl value, bool decorations = true)
    {
        stack.Push(value);

        CurrentPage = value;
    }

    public virtual void Pop()
    {
        if (stack.Count <= 1) return;

        stack.Pop();

        // Force the current page to update
        CurrentPage = null;
        this.RaisePropertyChanged(nameof(CurrentPage));
        CurrentPage = stack.Peek();
        this.RaisePropertyChanged(nameof(CurrentPage));
    }

    public void ShowPopup(Control popup)
    {
        CurrentPopup = popup;
        IsPopup = true;
    }

    public void HidePopup()
    {
        CurrentPopup = null;
        IsPopup = false;
    }
}