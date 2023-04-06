using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using ddLaunch.Core.Managers;
using ddLaunch.Models;
using ddLaunch.Views.Pages;

namespace ddLaunch;

public partial class MainWindow : Window
{
    public static MainWindow Instance { get; private set; }
    
    public MainWindow()
    {
        Instance = this;
        InitializeComponent();

        DataContext = new MainWindowDataContext(null, false);
        
        MainWindowDataContext.Instance.ShowLoadingPage();
        Authenticate();
    }

    public void SetDecorations(bool showDecorations)
    {
        TopBar.IsVisible = showDecorations;
        BottomBar.IsVisible = showDecorations;
        TopHeaderBar.IsVisible = showDecorations;
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
            MainWindowDataContext.Instance.Push<MainPage>();
            
            return;
        }
        
        MainWindowDataContext.Instance.Push<OnBoardingPage>(false);
    }
}