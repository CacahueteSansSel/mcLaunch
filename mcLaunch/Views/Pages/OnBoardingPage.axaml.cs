using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using mcLaunch.Core.Managers;
using mcLaunch.Models;
using mcLaunch.Views.Popups;

namespace mcLaunch.Views.Pages;

public partial class OnBoardingPage : UserControl
{
    public OnBoardingPage()
    {
        InitializeComponent();

        try
        {
            MainWindowDataContext.Instance.ShowDecorations = false;
        }
        catch (Exception e)
        {
            
        }
    }

    private async void LoginWithMicrosoftButton(object? sender, RoutedEventArgs e)
    {
        MainWindowDataContext.Instance.ShowLoadingPopup();
        
        bool success = await AuthenticationManager.AuthenticateAsync();

        if (success)
        {
            MainWindowDataContext.Instance.Reset();
            MainWindowDataContext.Instance.Push<MainPage>();
            
            MainWindowDataContext.Instance.HideLoadingPopup();
        }
        else
        {
            MainWindowDataContext.Instance.ShowPopup(new MessageBoxPopup("Unable to authenticate",
                "An unknown error occurred"));
        }
    }
}