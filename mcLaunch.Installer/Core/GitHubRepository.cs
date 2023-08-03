using System.Threading.Tasks;
using Cacahuete.MinecraftLib.Http;
using Cacahuete.MinecraftLib.Models.GitHub;

namespace mcLaunch.Installer.Core;

public static class GitHubRepository
{
    public static async Task<GitHubRelease?> GetLatestReleaseAsync() =>
        await Api.GetAsync<GitHubRelease>("https://api.github.com/repos/CacahueteSansSel/mcLaunch/releases/latest");
}