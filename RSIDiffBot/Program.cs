using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommandLine;
using Importer;
using static CommandLine.Parser;

namespace RSIDiffBot
{
    internal enum ModifiedType
    {
        Modified,
        Removed,
        Added
    };

    internal class ChangedState
    {
        public string Name;
        public string OldStatePath;
        public string NewStatePath;

        public ChangedState(string name, string oldStatePath, string newStatePath)
        {
            Name = name;
            OldStatePath = oldStatePath;
            NewStatePath = newStatePath;
        }
    }
    
    // ReSharper disable once InconsistentNaming
    public static class RSIDiffBot
    {
        // you can't just do '\n' because github actions for some reason
        // truncates every newline..
        public const string Nl = @"%0A";
        
        static async Task StartDiffAsync(ActionInputs inputs)
        {

            Console.WriteLine($"modified {inputs.Modified}");
            Console.WriteLine($"removed {inputs.Removed}");
            Console.WriteLine($"added {inputs.Added}");

            var states = new List<ChangedState>()
            {
                new ("test_state", "", "b"),
                new("test_state_2", "a", "b"),
                new ("test_state_3", "b", "")
            };

            var title = $@"This PR makes changes to 1 or more RSIs. Here is a summary of all changes:{Nl}{Nl}";
            var summary = title + WrapInCollapsible(CreateTable(states), "test.rsi");

            Console.WriteLine($"::set-output name=summary-details::{summary}");

            Environment.Exit(0);
        }

        /// <summary>
        ///     Wraps a string in a collapsible for GitHubMarkdown.
        /// </summary>
        /// <param name="markdown">The string to wrap</param>
        /// <param name="title">The text to use for</param>
        /// <returns>The wrapped string.</returns>
        static string WrapInCollapsible(string markdown, string title)
        {
            var str = $@"<details><summary>{title}</summary>{Nl}<p>{Nl}{Nl}" + markdown;
            return str + $@"{Nl}{Nl}</p>{Nl}</details>";
        }

        /// <summary>
        ///     Creates a GitHub markdown table for a single RSI's changed states.
        /// </summary>
        /// <param name="states">The list of changed states.</param>
        /// <returns>A string with a formatted markdown table.</returns>
        static string CreateTable(IEnumerable<ChangedState> states)
        {
            var str = $@"| State | Old | New | Status{Nl}| --- | --- | --- | --- |{Nl}";

            foreach (var state in states)
            {
                var status = ModifiedType.Modified;
                if (state.OldStatePath == string.Empty)
                    status = ModifiedType.Added;
                else if (state.NewStatePath == String.Empty)
                    status = ModifiedType.Removed;

                str += $@"| {state.Name} | {state.OldStatePath} | {state.NewStatePath} | {status}{Nl}";
            }

            return str;
        }

        static async Task Main(string[] args)
        {
            var parser = Default.ParseArguments<ActionInputs>(() => new(), args);
            parser.WithNotParsed(
                _ =>
                {
                    Environment.Exit(2);
                });


            await parser.WithParsedAsync(options => StartDiffAsync(options));
        }
    }
}