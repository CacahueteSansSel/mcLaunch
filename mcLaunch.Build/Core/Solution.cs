namespace mcLaunch.Build.Core;

public static class Solution
{
    public static Project[] GetProjects(string rootDirectory)
    {
        List<Project> projects = [];

        foreach (string dir in Directory.GetDirectories(rootDirectory))
        {
            string dirName = Path.GetFileName(dir);
            string csprojFilename = $"{dir}/{dirName}.csproj";

            if (!File.Exists(csprojFilename)) continue;

            projects.Add(new Project(dir));
        }

        return projects.ToArray();
    }
}