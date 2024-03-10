using mcLaunch.GitHub.Models;
using mcLaunch.Launchsite.Http;

namespace mcLaunch.GitHub;

public static class GitHubRepository
{
    public static async Task<GitHubRelease?> GetLatestReleaseAsync()
    {
        return await Api.GetAsync<GitHubRelease>(
            "https://api.github.com/repos/CacahueteSansSel/mcLaunch/releases/latest");
    }
}