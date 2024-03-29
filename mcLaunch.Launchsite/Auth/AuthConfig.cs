﻿using Microsoft.Identity.Client.Extensions.Msal;

namespace mcLaunch.Launchsite.Auth;

public class AuthConfig
{
    // Cache settings
    public const string CacheFileName = "mcl_msal.txt";

    public const string KeyChainServiceName = "mcl_msal_service";
    public const string KeyChainAccountName = "mcl_msal_account";

    public const string LinuxKeyRingSchema = "dev.cacahuete.mclaunch.tokencache";
    public const string LinuxKeyRingCollection = MsalCacheHelper.LinuxKeyRingDefaultCollection;
    public const string LinuxKeyRingLabel = "MSAL tokens cache for mcLaunch Microsoft Auth";
    public static readonly KeyValuePair<string, string> LinuxKeyRingAttr1 = new("Version", "1");
    public static readonly KeyValuePair<string, string> LinuxKeyRingAttr2 = new("ProductGroup", "mcLaunch");
}