using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Cacahuete.MinecraftLib.Auth;
using ddLaunch.Core.Managers;
using ddLaunch.Models;
using ddLaunch.Utilities;
using ddLaunch.Views.Pages;
using ddLaunch.Views.Popups;
using ReactiveUI;

namespace ddLaunch.Views;

public partial class ToolButtonsBar : UserControl
{
    public ToolButtonsBar()
    {
        InitializeComponent();

        DataContext = new Data();
    }

    private void NewBoxButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.ShowPopup(new NewBoxPopup());
    }

    private void ImportBoxButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.ShowPopup(new ImportBoxPopup());
    }
    
    public class Data : ReactiveObject
    {
        AuthenticationResult? account;

        public Data()
        {
            AuthenticationManager.OnLogin += result =>
            {
                Account = result;
            };
        }

        public AuthenticationResult? Account
        {
            get => account;
            set => this.RaiseAndSetIfChanged(ref account, value);
        }
    }

    private async void DisconnectButtonClicked(object? sender, RoutedEventArgs e)
    {
        await AuthenticationManager.DisconnectAsync();
        
        MainWindowDataContext.Instance.Reset();
        MainWindowDataContext.Instance.Push<OnBoardingPage>(false);
    }
}