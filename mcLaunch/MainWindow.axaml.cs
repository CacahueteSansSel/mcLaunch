using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
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
        
        MainWindowDataContext.Instance.ShowLoadingPage();
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

        bool loggedIn = await AuthenticationManager.TryLoginAsync();

        if (loggedIn)
        {
            if (!await AuthenticationManager.HasMinecraftAsync())
            {
                MainWindowDataContext.Instance.Push(new ErrorPage("This account does not own Minecraft. This launcher only supports paid Minecraft accounts"), false);

                await AuthenticationManager.DisconnectAsync();
                return;
            }
            
            MainWindowDataContext.Instance.Push<MainPage>();
        }
        else
        {
            MainWindowDataContext.Instance.Push<OnBoardingPage>(false);
        }

        if (App.Args.Contains("from-guard"))
        {
            if (!int.TryParse(App.Args.Get("exit-code"), out int exitCode)) 
                return;
            
            Navigation.ShowPopup(new CrashPopup(exitCode, App.Args.Get("box-id")));
        }
    }
}