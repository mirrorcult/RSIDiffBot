using System;
using System.Collections.Generic;
using CommandLine;

namespace RSIDiffBot
{
    public class ActionInputs
    {
        [Option('f', "files",
            Required = true,
            HelpText = "A list of changed files.")]
        public IEnumerable<string> Files { get; set; } = null!;
    }
}