using System.Diagnostics;

namespace mcLaunch.Tests.Tests;

public abstract class TestBase
{
    public abstract string Name { get; }

    protected string LatestTestFailure { get; private set; }
    public abstract Task<TestResult> RunAsync();

    protected async Task<bool> Test(string name, Func<Task<bool>> test)
    {
        Console.Write($"    {name}: ");

        try
        {
            bool result = await test.Invoke();

            if (!result) throw new TestFailedException(name);
            
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

    protected TestResult Success(string? message = null)
        => new(true, message, this);

    protected TestResult Failure(string message)
        => new(false, message, this);

    protected TestResult TestFailure()
        => new(false, LatestTestFailure, this);
}

public class TestFailedException(string testName) : Exception($"Test '{testName}' failed");