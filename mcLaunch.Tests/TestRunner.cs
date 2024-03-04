using System.Diagnostics;
using mcLaunch.Launchsite.Core;
using mcLaunch.Tests.Tests;

namespace mcLaunch.Tests;

public class TestRunner
{
    private int _testIndex;

    public TestRunner(MinecraftFolder minecraftFolder)
    {
        MinecraftFolder = minecraftFolder;
    }

    public List<TestBase> Tests { get; } = [];
    public List<Type> DoneTestsTypes { get; } = [];
    public MinecraftFolder MinecraftFolder { get; }

    public void RegisterTests(TestBase[] tests)
    {
        Tests.AddRange(tests);
    }

    public TestBase? GetTestByType(Type type)
    {
        return Tests.FirstOrDefault(t => t.GetType() == type);
    }

    private async Task<TestResult> RunTestAsync(TestBase? test)
    {
        if (test == null || DoneTestsTypes.Contains(test.GetType()))
            return null;

        foreach (Type depType in test.Dependencies)
            await RunTestAsync(GetTestByType(depType));

        TestResult result;
        test.Runner = this;

        Logging.Log(LogType.Pending, $"Test {_testIndex}/{Tests.Count} : {test.Name}");

        try
        {
            result = await test.RunAsync();
            result.Test = test;
        }
        catch (Exception e)
        {
            result = new TestResult(false, e.ToString(), test);
            if (Debugger.IsAttached) throw;
        }

        Logging.Log(result.IsSuccess ? LogType.Success : LogType.Failure,
            $"{test.Name}: {result.Message ?? "OK"}");

        DoneTestsTypes.Add(test.GetType());

        return result;
    }

    public async Task<TestResult[]> RunAsync()
    {
        List<TestResult> results = new();

        Logging.Log(LogType.Info, $"Now running {Tests.Count} test(s)...");

        _testIndex = 1;
        foreach (TestBase test in Tests)
        {
            results.Add(await RunTestAsync(test));
            _testIndex++;
        }

        TestResult[] succeededResults = results.Where(result => result.IsSuccess).ToArray();
        TestResult[] failedResults = results.Where(result => !result.IsSuccess).ToArray();

        Console.WriteLine("Test Running Results:");
        Console.WriteLine($"    Succeeded tests: {succeededResults.Length} test(s)");
        Console.WriteLine($"    Failed tests: {failedResults.Length} test(s)");

        int index = 1;
        foreach (TestResult failedTest in failedResults)
        {
            Console.Write("        [");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"Failure #{index}");
            Console.ResetColor();
            Console.Write("] ");

            Console.WriteLine($"{failedTest.Test.Name}: {failedTest.Message}");

            index++;
        }

        return results.ToArray();
    }
}

public class TestResult(bool isSuccess, string? message, TestBase? test = null)
{
    public static TestResult Ok => new(true, null);

    public bool IsSuccess { get; } = isSuccess;
    public string? Message { get; } = message;
    public TestBase? Test { get; set; } = test;

    public static TestResult Error(string message)
    {
        return new TestResult(false, message);
    }
}