using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
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
            var rsiStates = new Dictionary<string, List<ChangedState>>();

            var mod = inputs.Modified.Split(" ");
            var rem = inputs.Removed.Split(" ");
            var add = inputs.Added.Split(" ");

            bool CheckApplicable(string state)
            {
                if (Path.GetExtension(state) != ".png") return false;
                if (!state.Contains(".rsi")) return false;
                return true;
            }

            void AddToRsiStates(string key, ChangedState value)
            {
                if (rsiStates.TryGetValue(key, out var val))
                {
                    val.Add(value);
                    rsiStates[key] = val;
                }
                else
                {
                    rsiStates.Add(key, new List<ChangedState>() { value });
                }
            }

            // yes yes duplication for these 3 lists, oh well
            foreach (var state in mod)
            {
                if (!CheckApplicable(state)) continue;
                
                var rsi = Path.GetDirectoryName(state) ?? "fuck";
                var changed = new ChangedState(
                    Path.GetFileNameWithoutExtension(state),
                    GetGitHubRawImageLink(inputs.BaseName, inputs.BaseSha, state),
                    GetGitHubRawImageLink(inputs.HeadName, inputs.HeadSha, state)
                );

                AddToRsiStates(rsi, changed);
            }

            foreach (var state in rem)
            {
                if (!CheckApplicable(state)) continue;
                
                var rsi = Path.GetDirectoryName(state) ?? "fuck";
                var changed = new ChangedState(
                    Path.GetFileNameWithoutExtension(state),
                    GetGitHubRawImageLink(inputs.BaseName, inputs.BaseSha, state),
                    null
                );
                
                AddToRsiStates(rsi, changed);
            }
            
            foreach (var state in add)
            {
                if (!CheckApplicable(state)) continue;
                
                var rsi = Path.GetDirectoryName(state) ?? "fuck";
                var changed = new ChangedState(
                    Path.GetFileNameWithoutExtension(state),
                    null,
                    GetGitHubRawImageLink(inputs.HeadName, inputs.HeadSha, state)
                    );
                
                AddToRsiStates(rsi, changed);
            }

            var sb = new StringBuilder();
            sb.Append($@"RSI Diff Bot; head commit {inputs.HeadSha} merging into {inputs.BaseSha}{Nl}");
            sb.Append($@"This PR makes changes to 1 or more RSIs. Here is a summary of all changes:{Nl}{Nl}");

            foreach (var kvp in rsiStates)
            {
                sb.Append(WrapInCollapsible(CreateTable(kvp.Value), kvp.Key));
            }

            Console.WriteLine($@"::set-output name=summary-details::{sb.ToString()}");
            
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
            var sb = new StringBuilder();
            sb.Append($@"<details><summary>{title}</summary>{Nl}<p>{Nl}{Nl}" + markdown);
            sb.Append($@"{Nl}{Nl}</p>{Nl}</details>");
            return sb.ToString();
        }

        /// <summary>
        ///     Creates a GitHub markdown table for a single RSI's changed states.
        /// </summary>
        /// <param name="states">The list of changed states.</param>
        /// <returns>A string with a formatted markdown table.</returns>
        static string CreateTable(IEnumerable<ChangedState> states)
        {
            var sb = new StringBuilder();
            sb.Append($@"| State | Old | New | Status{Nl}| --- | --- | --- | --- |{Nl}");

            foreach (var state in states)
            {
                var status = ModifiedType.Modified;
                if (state.OldStatePath == null)
                    status = ModifiedType.Added;
                else if (state.NewStatePath == null)
                    status = ModifiedType.Removed;

                sb.Append($@"| {state.Name} | ![]({state.OldStatePath}) | ![]({state.NewStatePath}) | {status}{Nl}");
            }

            return sb.ToString();
        }

        /// <summary>
        ///     Returns a raw githubusercontent link for a given file at a point in time.
        /// </summary>
        static string GetGitHubRawImageLink(string fullName, string sha, string location)
        {
            // fullName here is {username}/{name}
            return $@"https://raw.githubusercontent.com/{fullName}/{sha}/{location}";
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