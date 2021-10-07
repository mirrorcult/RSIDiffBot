using System;
using System.Collections.Generic;
using CommandLine;

namespace RSIDiffBot
{
    public class ActionInputs
    {
        [Option('m', "modified",
            Required = false,
            HelpText = "A list of modified files.")]
        public IEnumerable<string> Modified { get; set; } = null!;
        
        [Option('r', "removed",
            Required = false,
            HelpText = "A list of removed files.")]
        public IEnumerable<string> Removed { get; set; } = null!;
        
        [Option('a', "added",
            Required = false,
            HelpText = "A list of added files.")]
        public IEnumerable<string> Added { get; set; } = null!;

        // for dummies like me
        // base = merging into
        // head = merging from
        
        [Option(longName: "bn",
            Required = true,
            HelpText = "The base repo's full name, i.e. '{username}/{reponame}.")]
        public string BaseName { get; set; } = null!;
        
        [Option(longName: "bs",
            Required = true,
            HelpText = "The commit hash of the base branch.")]
        public string BaseSha { get; set; } = null!;
        
        [Option(longName: "hn",
            Required = true,
            HelpText = "The head repo's full name, i.e. '{username}/{reponame}.")]
        public string HeadName { get; set; } = null!;

        [Option(longName: "hs",
            Required = true,
            HelpText = "The commit hash of the head branch.")]
        public string HeadSha { get; set; } = null!;
    }
}