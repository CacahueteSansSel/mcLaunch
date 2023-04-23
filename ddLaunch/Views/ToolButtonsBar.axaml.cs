using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
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
        private AuthenticationResult? account;
        private Bitmap head;

        public Data()
        {
            AuthenticationManager.OnLogin += async result =>
            {
                Account = result;
                string cacheName = $"user-{account.Uuid}";
                
                if (CacheManager.Has(cacheName))
                {
                    await Task.Run(() =>
                    {
                        HeadIcon = CacheManager.LoadBitmap(cacheName);
                    });

                    return;
                }
                
                using (var imageStream = await LoadIconStreamAsync(result))
                {
                    if (imageStream == null) return;
            
                    HeadIcon = await Task.Run(() =>
                    {
                        try
                        {
                            return Bitmap.DecodeToWidth(imageStream, 400);
                        }
                        catch (Exception e)
                        {
                            return null;
                        }
                    });
            
                    CacheManager.Store(HeadIcon, cacheName);
                }
            };
        }
        
        async Task<Stream> LoadIconStreamAsync(AuthenticationResult? account)
        {
            if (account == null) return null;
        
            HttpClient client = new HttpClient();

            try
            {
                HttpResponseMessage resp = await client.GetAsync($"https://crafatar.com/renders/head/{account.Uuid}");
                if (!resp.IsSuccessStatusCode) return null;

                return await resp.Content.ReadAsStreamAsync();
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public AuthenticationResult? Account
        {
            get => account;
            set => this.RaiseAndSetIfChanged(ref account, value);
        }

        public Bitmap? HeadIcon
        {
            get => head;
            set => this.RaiseAndSetIfChanged(ref head, value);
        }
    }

    private async void DisconnectButtonClicked(object? sender, RoutedEventArgs e)
    {
        await AuthenticationManager.DisconnectAsync();
        
        MainWindowDataContext.Instance.Reset();
        MainWindowDataContext.Instance.Push<OnBoardingPage>(false);
    }

    private void SettingsButtonClicked(object? sender, RoutedEventArgs e)
    {
        MainWindowDataContext.Instance.Push<SettingsPage>();
    }
}