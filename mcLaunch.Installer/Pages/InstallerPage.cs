using Avalonia.Controls;

namespace mcLaunch.Installer.Pages;

public abstract class InstallerPage : UserControl
{
    public virtual void OnShow() {}
    public virtual void OnHide() {}
}