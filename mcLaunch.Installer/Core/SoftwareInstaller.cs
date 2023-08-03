namespace mcLaunch.Installer.Core;

public class SoftwareInstaller
{
    public InstallerParameters Parameters { get; private set; }
    
    public SoftwareInstaller(InstallerParameters parameters)
    {
        Parameters = parameters;
    }
}