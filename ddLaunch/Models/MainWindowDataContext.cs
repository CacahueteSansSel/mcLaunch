using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
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
    
    public MainWindowDataContext(Control mainPage)
    {
        Instance = this;
        
        Push(mainPage);
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

    public void Push<T>() where T : Control, new()
        => Push(new T());

    public void Push(Control value)
    {
        stack.Push(value);
        CurrentPage = value;
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