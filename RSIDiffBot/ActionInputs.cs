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
    }
}