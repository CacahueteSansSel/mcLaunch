using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Cacahuete.MinecraftLib.Auth;
using mcLaunch.Core.Managers;
using mcLaunch.Models;
using mcLaunch.Utilities;
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

        MinecraftAuthenticationResult? auth = await AuthenticationManager.AuthenticateAsync(result =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                Navigation.ShowPopup(new MessageBoxPopup("Login with Microsoft", $"{result.Message}"));
                PlatformSpecific.OpenUrl(result.Url);
            });
        });

        if (auth != null && auth.IsSuccess)
        {
            MainWindowDataContext.Instance.Reset();
            MainWindowDataContext.Instance.Push<MainPage>();

            MainWindowDataContext.Instance.HideLoadingPopup();
        }
        else
        {
            MainWindowDataContext.Instance.ShowPopup(new ConfirmMessageBoxPopup("Unable to authenticate",
                $"An error occurred : {auth.Message}\nClear the cache and try to reconnect ?", async () =>
                {
                    await AuthenticationManager.DisconnectAsync();
                    
                    LoginWithMicrosoftButton(null, null);
                }));
        }
    }
}