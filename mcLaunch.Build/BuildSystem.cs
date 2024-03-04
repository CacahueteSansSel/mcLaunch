using LibGit2Sharp;
using mcLaunch.Build.Core;

namespace mcLaunch.Build;

public class BuildSystem
{
    private List<BuildStepBase> steps = [];
    
    public Project[] Projects { get; }
    public string SolutionDirectory { get; }
    public Repository Repository { get; }
    public Commit? LatestCommit => Repository.Head.Commits.FirstOrDefault();

    public BuildSystem(string solutionDirectory)
    {
        SolutionDirectory = Path.GetFullPath(solutionDirectory);
        Projects = Solution.GetProjects(solutionDirectory);
        
        Repository = new Repository($"{solutionDirectory}/.git");
    }

    public BuildSystem With<T>() where T : BuildStepBase, new()
    {
        steps.Add(new T());
        return this;
    }

    public BuildSystem With(params BuildStepBase[] steps)
    {
        this.steps.AddRange(steps);
        return this;
    }

    public Project? GetProject(string name)
        => Projects.FirstOrDefault(project => project.Name == name);
    
    public async Task<bool> BuildAsync()
    {
        Console.WriteLine($"Now running {steps.Count} build step(s)...");

        int index = 1;
        foreach (BuildStepBase step in steps)
        {
            Console.Write($"[{index}/{steps.Count}] {step.Name}: ");

            if (!step.IsSupportedOnThisPlatform())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Not supported on {Utilities.GetPlatformName()}");
                Console.ResetColor();

                index++;
                continue;
            }

            BuildResult result = await step.RunAsync(this);
            if (result.IsError)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error");
                Console.WriteLine(result.ErrorMessage);
                Console.ResetColor();
        
                Console.WriteLine($"Ran {index}/{steps.Count} build steps with failure");

                return false;
            }
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("OK");
            Console.ResetColor();

            index++;
        }
        
        Console.WriteLine($"Ran {index-1}/{steps.Count} build steps without failure");

        return true;
    }
}