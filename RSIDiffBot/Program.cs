﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using RSIDiffBot;
using static CommandLine.Parser;

static async Task StartDiffAsync(ActionInputs inputs)
{
    // https://docs.github.com/actions/reference/workflow-commands-for-github-actions#setting-an-output-parameter
    var summary = "**Hello**\n*Test*";

    Console.WriteLine($"modified {inputs.Modified}");
    Console.WriteLine($"removed {inputs.Removed}");
    Console.WriteLine($"added {inputs.Added}");
    
    Console.WriteLine($"::set-output name=updated-metrics::{summary}");

    Environment.Exit(0);
}

var parser = Default.ParseArguments<ActionInputs>(() => new(), args);
parser.WithNotParsed(
    _ =>
    {
        Environment.Exit(2);
    });

await parser.WithParsedAsync(options => StartDiffAsync(options));