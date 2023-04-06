using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using ddLaunch.Views.Pages;
using ddLaunch.Views.Popups;
using ReactiveUI;

namespace ddLaunch.Models;

public class MainWindowDataContext : PageNavigator
{
    public static MainWindowDataContext Instance { get; private set; }
    
    Control curPage;
    Control curPopup;
    bool isPopupShown;
    Stack<Control> stack = new();

    public Control CurrentPage
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
    
    public MainWindowDataContext(Control? mainPage, bool decorations)
    {
        Instance = this;

        ShowDecorations = decorations;
        if (mainPage != null) Push(mainPage);
        HidePopup();
    }

    void Set<T>() where T : Control, new()
    {
        CurrentPage = new T();
    }

    void Set(Control value)
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

    public void ShowLoadingPage()
    {
        CurrentPage = new LoadingPage();
    }

    public void HideLoadingPage()
    {
        CurrentPage = stack.Count == 0 ? null : stack.Peek();
    }

    public void Push<T>(bool decorations = true) where T : Control, new()
        => Push(new T(), decorations);

    public void Push(Control value, bool decorations = true)
    {
        stack.Push(value);
        
        CurrentPage = value;
        ShowDecorations = decorations;
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