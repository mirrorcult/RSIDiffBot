﻿#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
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
        public string? OldStatePath;
        public string? NewStatePath;

        public ChangedState(string name, string? oldStatePath, string? newStatePath)
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
            
            var rsiStates = new Dictionary<string, List<ChangedState>>();

            // yes yes duplication for these 3 lists, oh well
            foreach (var state in inputs.Modified)
            {
                if (state != null)
                {
                    if (!Path.HasExtension(".png")) return;
                    if (!state.Contains(".rsi")) return;
                    var rsi = Path.GetDirectoryName(state) ?? "fuck";
                    var list = rsiStates.GetValueOrDefault(rsi);
                    list?.Add(new ChangedState(
                            Path.GetFileNameWithoutExtension(state),
                        GetGitHubRawImageLink(inputs.BaseName, inputs.BaseSha, state),
                        GetGitHubRawImageLink(inputs.HeadName,  inputs.HeadSha, state)
                        )
                    );
                }
            }

            foreach (var state in inputs.Removed)
            {
                if (state != null)
                {
                    if (!Path.HasExtension(".png")) return;
                    if (!state.Contains(".rsi")) return;
                    var rsi = Path.GetDirectoryName(state) ?? "fuck";
                    var list = rsiStates.GetValueOrDefault(rsi);
                    list?.Add(new ChangedState(
                            Path.GetFileNameWithoutExtension(state),
                            GetGitHubRawImageLink(inputs.BaseName, inputs.BaseSha, state),
                            null
                        )
                    );
                }
            }
            
            foreach (var state in inputs.Added)
            {
                if (state != null)
                {
                    if (!Path.HasExtension(".png")) return;
                    if (!state.Contains(".rsi")) return;
                    var rsi = Path.GetDirectoryName(state) ?? "fuck";
                    var list = rsiStates.GetValueOrDefault(rsi);
                    list?.Add(new ChangedState(
                            Path.GetFileNameWithoutExtension(state),
                            null,
                            GetGitHubRawImageLink(inputs.HeadName, inputs.HeadSha, state)
                        )
                    );
                }
            }

            var summary = $@"RSI Diff Bot; head commit {inputs.HeadSha} merging into {inputs.BaseSha}{Nl}";
            summary += $@"This PR makes changes to 1 or more RSIs. Here is a summary of all changes:{Nl}{Nl}";

            foreach (var kvp in rsiStates)
            {
                summary += WrapInCollapsible(CreateTable(kvp.Value), kvp.Key);
            }

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
                if (state.OldStatePath == null)
                    status = ModifiedType.Added;
                else if (state.NewStatePath == null)
                    status = ModifiedType.Removed;

                str += $@"| {state.Name} | ![]({state.OldStatePath}) | ![]({state.NewStatePath}) | {status}{Nl}";
            }

            return str;
        }

        /// <summary>
        ///     Returns a raw githubusercontent link for a given file at a point in time.
        /// </summary>
        static string GetGitHubRawImageLink(string fullName, string sha, string location)
        {
            // fullName here is {username}/{name}
            return $"https://raw.githubusercontent.com/{fullName}/{sha}/{location}";
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