using Avalonia.Controls;
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

        DataContext = new MainWindowDataContext(new MainPage());
    }
}