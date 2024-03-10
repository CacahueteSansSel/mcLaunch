using System.Text.Json;
using System.Text.RegularExpressions;
using LibGit2Sharp;
using mcLaunch.Build.Core;

namespace mcLaunch.Build.Steps;

public class UpdateLatestCommitStep : BuildStepBase
{
    public override string Name => "Updating latest commit id";

    public override async Task<BuildResult> RunAsync(BuildSystem system)
    {
        try
        {
            Project? mcLaunch = system.GetProject("mcLaunch");
            string buildManifestFile = $"{mcLaunch!.Folder}/resources/settings/build.json";
            Commit commit = system.LatestCommit!;
            Branch? branch = system.Repository.Head.TrackedBranch;

            BuildManifest manifest = new(commit.Id.ToString()[..7],
                branch is null ? string.Empty : branch.FriendlyName.Replace("origin/", ""),
                GenerateChangelog(system.Repository));

            await File.WriteAllTextAsync(buildManifestFile, JsonSerializer.Serialize(manifest));

            return new BuildResult();
        }
        catch (Exception e)
        {
            // Ignore the errors for now
            return new BuildResult();
        }
    }

    private string[] GenerateChangelog(Repository repository)
    {
        Regex commitFormat = RegularExpressions.ProjectCommitFormat();
        List<string> log = [];
        Tag latestTag = repository.Tags.First(tag => tag.Target is Commit);

        foreach (Commit commit in repository.Commits)
        {
            if (commit.Id == ((Commit) latestTag.Target).Id) break;
            if (!commitFormat.IsMatch(commit.MessageShort)) continue;
            if (log.Contains(commit.MessageShort.Trim())) continue;

            log.Add(commit.MessageShort.Trim());
        }

        return log.ToArray();
    }

    public record BuildManifest(string CommitId, string Branch, string[] Changelog);
}