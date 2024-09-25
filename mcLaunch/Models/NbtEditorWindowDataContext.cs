using mcLaunch.Views;

namespace mcLaunch.Models;

public class NbtEditorWindowDataContext : PageNavigator
{
    public static NbtEditorWindowDataContext Instance { get; private set; }
    
    public NbtEditorWindowDataContext(ITopLevelPageControl? mainPage) : base(mainPage)
    {
        Instance = this;
    }
}