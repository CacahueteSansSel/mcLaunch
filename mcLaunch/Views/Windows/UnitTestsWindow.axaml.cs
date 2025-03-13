using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using mcLaunch.Tests;
using ReactiveUI;

namespace mcLaunch.Views.Windows;

public partial class UnitTestsWindow : Window
{
    public enum UnitTestStateType
    {
        NotTested,
        Loading,
        Succeeded,
        Failed
    }

    private readonly List<UnitTestEntry> entries = [];

    public UnitTestsWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode) TestsManager.Load();

        foreach (UnitTest test in TestsManager.Tests) entries.Add(new UnitTestEntry(test, UnitTestStateType.NotTested));

        DataContext = new Data { Entries = entries.ToArray() };
    }

    private async Task RunTestsAsync()
    {
        RunTestsButton.IsEnabled = false;

        foreach (UnitTestEntry entry in entries)
        {
            try
            {
                Stopwatch stopwatch = new();
                stopwatch.Start();

                await entry.Test.RunAsync();

                stopwatch.Stop();

                entry.State = UnitTestStateType.Succeeded;
                entry.Output =
                    $"{entry.TestName} succeeded in {stopwatch.ElapsedMilliseconds} ms\n{entry.Test.AssertLog}";

                UpdateEntries();
            }
            catch (Exception e)
            {
                entry.State = UnitTestStateType.Failed;
                entry.Output = $"{entry.TestName} failed\n{entry.Test.AssertLog}\n{e}";

                UpdateEntries();
            }

            // Reset the assert log for any future tests
            entry.Test.AssertLog = "";

            await Task.Delay(10);
        }

        RunTestsButton.IsEnabled = true;
    }

    private void UpdateEntries() => ((Data)DataContext).Entries = entries.ToArray();

    private async void RunAllTestsButtonClicked(object? sender, RoutedEventArgs e)
    {
        foreach (UnitTestEntry entry in entries)
            entry.State = UnitTestStateType.Loading;
        UpdateEntries();

        await RunTestsAsync();
    }

    private void ListItemSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Text.Text = "";

        if (e.AddedItems.Count > 0)
        {
            UnitTestEntry entry = (UnitTestEntry)e.AddedItems[0];
            Text.Text = entry.Output;
        }
    }

    public class UnitTestEntry
    {
        public UnitTestEntry(UnitTest test, UnitTestStateType state)
        {
            Test = test;
            State = state;
        }

        public UnitTest Test { get; }
        public UnitTestStateType State { get; set; }
        public string Output { get; set; }

        // Used in XAML
        public string TestName => Test.GetType().Name;
        public bool IsNotTested => State == UnitTestStateType.NotTested;
        public bool IsLoading => State == UnitTestStateType.Loading;
        public bool IsSucceeded => State == UnitTestStateType.Succeeded;
        public bool IsFailed => State == UnitTestStateType.Failed;
    }

    public class Data : ReactiveObject
    {
        private UnitTestEntry[] entries;

        public UnitTestEntry[] Entries
        {
            get => entries;
            set => this.RaiseAndSetIfChanged(ref entries, value);
        }
    }
}