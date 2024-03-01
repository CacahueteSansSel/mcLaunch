namespace mcLaunch.Build.Steps;

public class CleanOutputDirectoryStep : BuildStepBase
{
    public override string Name => "Clean output directory";
    
    public override async Task<BuildResult> RunAsync(BuildSystem system)
    {
        if (Directory.Exists($"{system.SolutionDirectory}/output"))
            Directory.Delete($"{system.SolutionDirectory}/output", true);

        return new BuildResult();
    }
}