using System.Threading.Tasks;
using Cacahuete.MinecraftLib.Http;
using mcLaunch.GitHub.Models;

namespace mcLaunch.GitHub;

public static class GitHubRepository
{
    public static async Task<GitHubRelease?> GetLatestReleaseAsync() =>
        await Api.GetAsync<GitHubRelease>("https://api.github.com/repos/CacahueteSansSel/mcLaunch/releases/latest");
}