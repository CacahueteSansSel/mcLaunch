using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading.Tasks;
using Avalonia.Controls;
using Cacahuete.MinecraftLib.Auth;
using mcLaunch.Core.Managers;
using mcLaunch.Models;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;
using mcLaunch.Views.Popups;

namespace mcLaunch;

public partial class MainWindow : Window
{
    public static MainWindow Instance { get; private set; }
    
    public MainWindow()
    {
        Instance = this;
        InitializeComponent();

        DataContext = new MainWindowDataContext(null, false);

        DownloadManager.OnDownloadPrepareStarting += _ =>
        {
            SetBottomBarShown(true);
        };

        DownloadManager.OnDownloadFinished += () =>
        {
            SetBottomBarShown(false);
        };
        
        MainWindowDataContext.Instance.ShowStartingPage();
        Authenticate();
    }

    public void SetDecorations(bool showDecorations)
    {
        TopBar.IsVisible = showDecorations;
        TopHeaderBar.IsVisible = showDecorations;
    }

    public void SetBottomBarShown(bool show)
    {
        BottomBar.IsVisible = show;
    }

    async void Authenticate()
    {
        await Task.Run(async () =>
        {
            while (!AuthenticationManager.IsInitialized) 
                await Task.Delay(1);
        });

        if (App.Args.Contains("from-guard"))
        {
            if (!int.TryParse(App.Args.Get("exit-code"), out int exitCode)) 
                return;
            
            Navigation.ShowPopup(new CrashPopup(exitCode, App.Args.Get("box-id")));
        }

        MinecraftAuthenticationResult? authResult;

        if (App.Args.Contains("fast"))
        {
            authResult = AuthenticationManager.Cache.Get<MinecraftAuthenticationResult>("msaCacheFast") 
                         ?? await AuthenticationManager.TryLoginAsync();
        }
        else
        {
            authResult = await AuthenticationManager.TryLoginAsync();
        }

        if (authResult != null && authResult.IsSuccess)
        {
            if (!authResult.Validate())
            {
                MainWindowDataContext.Instance.Push<OnBoardingPage>(false);
                return;
            }
            
            AuthenticationManager.SetAccount(authResult);
            
            if (!await AuthenticationManager.HasMinecraftAsync())
            {
                MainWindowDataContext.Instance.Push(new ErrorPage("This account does not own Minecraft. This launcher only supports paid Minecraft accounts"), false);

                await AuthenticationManager.DisconnectAsync();
                return;
            }

            if (App.Args.Contains("fast"))
            {
                AuthenticationManager.Cache.Set("msaCacheFast", authResult);
            }
            
            MainWindowDataContext.Instance.Push<MainPage>();
        }
        else
        {
            MainWindowDataContext.Instance.Push<OnBoardingPage>(false);
        }
    }
}