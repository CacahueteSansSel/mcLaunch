using mcLaunch.Views;

namespace mcLaunch.Models;

public class MainWindowDataContext : PageNavigator
{
    public MainWindowDataContext(ITopLevelPageControl? mainPage, bool decorations) : base(mainPage)
    {
        Instance = this;
        ShowDecorations = decorations;
    }

    public static MainWindowDataContext Instance { get; private set; }

    public bool ShowDecorations
    {
        set => MainWindow.Instance.SetDecorations(value);
    }

    public override void ShowLoadingPage()
    {
        base.ShowLoadingPage();
        MainWindow.Instance.SetTitle(CurrentPage.Title);
    }

    public override void ShowStartingPage()
    {
        base.ShowStartingPage();
        MainWindow.Instance.SetTitle(CurrentPage.Title);
    }

    public override void HideLoadingPage()
    {
        base.HideLoadingPage();
        MainWindow.Instance.SetTitle(CurrentPage.Title);
    }

    public override void Push(ITopLevelPageControl value, bool decorations = true)
    {
        base.Push(value, decorations);
        ShowDecorations = decorations;
        MainWindow.Instance.SetTitle(CurrentPage.Title);
    }

    public override void Pop()
    {
        base.Pop();
        MainWindow.Instance.SetTitle(CurrentPage.Title);
    }

    public override void Set<T>()
    {
        base.Set<T>();
        MainWindow.Instance.SetTitle(CurrentPage.Title);
    }

    public override void Set(ITopLevelPageControl value)
    {
        base.Set(value);
        MainWindow.Instance.SetTitle(CurrentPage.Title);
    }
}