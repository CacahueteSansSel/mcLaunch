using Microsoft.Identity.Client.Extensions.Msal;

namespace Cacahuete.MinecraftLib.Auth;

public class AuthConfig
{
    // Cache settings
    public const string CacheFileName = "mcl_msal.txt";

    public const string KeyChainServiceName = "mcl_msal_service";
    public const string KeyChainAccountName = "mcl_msal_account";

    public const string LinuxKeyRingSchema = "dev.cacahuete.mclaunch.tokencache";
    public const string LinuxKeyRingCollection = MsalCacheHelper.LinuxKeyRingDefaultCollection;
    public const string LinuxKeyRingLabel = "MSAL tokens cache for mcLaunch Microsoft Auth";
    public static readonly KeyValuePair<string, string> LinuxKeyRingAttr1 = new KeyValuePair<string, string>("Version", "1");
    public static readonly KeyValuePair<string, string> LinuxKeyRingAttr2 = new KeyValuePair<string, string>("ProductGroup", "mcLaunch");

}