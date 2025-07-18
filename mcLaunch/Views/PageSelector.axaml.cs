﻿using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace mcLaunch.Views;

public partial class PageSelector : UserControl
{
    private Action<int>? _onPageChanged;

    public PageSelector()
    {
        InitializeComponent();
    }

    public int PageIndex { get; private set; }
    public int PageCount { get; private set; }

    public void Setup(int pageCount, Action<int>? onPageChangedCallback)
    {
        _onPageChanged = onPageChangedCallback;
        PageCount = pageCount;

        ButtonsContainer.Children.Clear();
        for (int i = 0; i < Math.Min(pageCount, 9); i++)
        {
            PageButton button = new(i, this);
            ButtonsContainer.Children.Add(button);
        }
    }

    public void SetPage(int index, bool runCallback = true)
    {
        PageIndex = index;
        if (runCallback) _onPageChanged?.Invoke(index);

        if (index >= 8)
        {
            int minimum = index - 7;
            for (int i = 0; i < ButtonsContainer.Children.Count; i++)
            {
                PageButton button = (PageButton)ButtonsContainer.Children[i];
                button.SetPage(minimum + i);
                button.SetLight(i == index - minimum);
            }
        }
        else
        {
            for (int i = 0; i < ButtonsContainer.Children.Count; i++)
            {
                PageButton button = (PageButton)ButtonsContainer.Children[i];
                button.SetLight(i == index);
            }
        }
    }

    private void PreviousButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (PageIndex <= 0) return;

        SetPage(PageIndex - 1);
    }

    private void NextButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (PageIndex > PageCount - 1) return;

        SetPage(PageIndex + 1);
    }
}