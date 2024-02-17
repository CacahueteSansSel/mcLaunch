using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using mcLaunch.Views;
using mcLaunch.Views.Pages;
using mcLaunch.Views.Popups;
using ReactiveUI;

namespace mcLaunch.Models;

public class MainWindowDataContext : PageNavigator
{
    public static MainWindowDataContext Instance { get; private set; }
    
    ITopLevelPageControl curPage;
    Control curPopup;
    bool isPopupShown;
    Stack<ITopLevelPageControl> stack = new();

    public ITopLevelPageControl CurrentPage
    {
        get => curPage;
        set => this.RaiseAndSetIfChanged(ref curPage, value);
    }

    public Control CurrentPopup
    {
        get => curPopup;
        set => this.RaiseAndSetIfChanged(ref curPopup, value);
    }

    public bool IsPopup
    {
        get => isPopupShown;
        set => this.RaiseAndSetIfChanged(ref isPopupShown, value);
    }

    public bool ShowDecorations
    {
        set => MainWindow.Instance.SetDecorations(value);
    }
    
    public MainWindowDataContext(ITopLevelPageControl? mainPage, bool decorations)
    {
        Instance = this;

        ShowDecorations = decorations;
        if (mainPage != null) Push(mainPage);
        HidePopup();
    }

    void Set<T>() where T : ITopLevelPageControl, new()
    {
        CurrentPage = new T();
        
        MainWindow.Instance.SetTitle(CurrentPage.Title);
    }

    void Set(ITopLevelPageControl value)
    {
        CurrentPage = value;
        
        MainWindow.Instance.SetTitle(CurrentPage.Title);
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

    public void ShowLoadingPage()
    {
        CurrentPage = new LoadingPage();
        
        MainWindow.Instance.SetTitle(CurrentPage.Title);
    }

    public void ShowStartingPage()
    {
        CurrentPage = new StartingPage();
        
        MainWindow.Instance.SetTitle(CurrentPage.Title);
    }

    public void HideLoadingPage()
    {
        CurrentPage = stack.Count == 0 ? null : stack.Peek();
        
        MainWindow.Instance.SetTitle(CurrentPage.Title);
    }

    public void Push<T>(bool decorations = true) where T : ITopLevelPageControl, new()
        => Push(new T(), decorations);

    public void Push(ITopLevelPageControl value, bool decorations = true)
    {
        stack.Push(value);
        
        CurrentPage = value;
        ShowDecorations = decorations;
        
        MainWindow.Instance.SetTitle(CurrentPage.Title);
    }

    public void Pop()
    {
        if (stack.Count <= 1) return;

        stack.Pop();
        
        // Force the current page to update
        CurrentPage = null;
        this.RaisePropertyChanged(nameof(CurrentPage));
        CurrentPage = stack.Peek();
        this.RaisePropertyChanged(nameof(CurrentPage));
        
        MainWindow.Instance.SetTitle(CurrentPage.Title);
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