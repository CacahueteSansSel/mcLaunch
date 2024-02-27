using System.Diagnostics;
using Cacahuete.MinecraftLib.Core;

namespace mcLaunch.Tests.Tests;

public abstract class TestBase
{
    public abstract string Name { get; }
    public TestRunner Runner { get; internal set; }
    public List<Type> Dependencies { get; } = new();

    protected string LatestTestFailure { get; private set; }
    protected MinecraftFolder SystemFolder => Runner.MinecraftFolder;

    public abstract Task<TestResult> RunAsync();

    protected async Task<bool> Test(string name, Func<Task<TestResult>> test)
    {
        Console.Write($"    {name}: ");

        try
        {
            TestResult result = await test.Invoke();

            if (!result.IsSuccess) throw new TestFailedException(name, result.Message);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Success");
            Console.ResetColor();
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Failure");

            Console.ResetColor();
            Console.WriteLine($"    --> {e.GetType().FullName} {e.Message}");

            LatestTestFailure = $"{e.GetType().FullName} {e.Message}";

            if (Debugger.IsAttached) throw;

            return false;
        }

        return true;
    }

    protected void AddDependency<T>() where T : TestBase
    {
        Dependencies.Add(typeof(T));
    }

    protected TestResult Success(string? message = null)
    {
        return new TestResult(true, message, this);
    }

    protected TestResult Failure(string message)
    {
        return new TestResult(false, message, this);
    }

    protected TestResult TestFailure()
    {
        return new TestResult(false, LatestTestFailure, this);
    }
}

public class TestFailedException(string testName, string? message) : Exception($"Test '{testName}' failed: {message}");