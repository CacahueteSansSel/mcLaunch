using System;
using System.Linq;
using System.Reflection;

namespace mcLaunch.Tests;

public static class TestsManager
{
    public static UnitTest[] Tests { get; private set; }

    public static void Load()
    {
        Tests = Assembly.GetExecutingAssembly().GetTypes()
            .Where(test => test.IsSubclassOf(typeof(UnitTest)))
            .Select(type => (UnitTest)Activator.CreateInstance(type)!)
            .ToArray();
    }
}