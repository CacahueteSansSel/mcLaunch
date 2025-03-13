using mcLaunch.Views;

namespace mcLaunch.Models;

public class NbtEditorWindowDataContext : PageNavigator
{
    public NbtEditorWindowDataContext(ITopLevelPageControl? mainPage) : base(mainPage)
    {
        Instance = this;
    }

    public static NbtEditorWindowDataContext Instance { get; private set; }
}