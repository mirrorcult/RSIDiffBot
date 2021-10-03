using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using CommandLine;
using RSIDiffBot;
using static CommandLine.Parser;
using static RSIDiffBot.Const;

static async Task StartDiffAsync(ActionInputs inputs)
{
    // you can't just do '\n' because github actions for some reason
    // truncates every newline..

    Console.WriteLine($"modified {inputs.Modified}");
    Console.WriteLine($"removed {inputs.Removed}");
    Console.WriteLine($"added {inputs.Added}");

    List<ChangedState> states = new List<ChangedState>()
    {
        new ChangedState("test_state", "", "b"),
        new ChangedState("test_state_2", "a", "b"),
        new ChangedState("test_state_3", "b", "")
    };

    var summary = WrapInCollapsible(CreateTable(states), "test.rsi");
    
    Console.WriteLine($"::set-output name=summary-details::{summary}");

    Environment.Exit(0);
}

// Wraps some string in GitHub collapsible markdown
static string WrapInCollapsible(string markdown, string title)
{
    var str = $@"<details><summary>{title}</summary>{nl}<p>" + markdown;
    return str + $@"</p>{nl}</details>";
}

static string CreateTable(IEnumerable<ChangedState> states)
{
    var str = $@"| State | Old | New | Status{nl}| --- | --- | --- | --- |{nl}";
    
    
    foreach (var state in states)
    {
        var status = ModifiedType.Modified;
        if(state.OldStatePath == string.Empty)
            status = ModifiedType.Added;
        else if (state.NewStatePath == String.Empty) 
            status = ModifiedType.Removed;
        
        str += $@"| {state.Name} | {state.OldStatePath} | {state.NewStatePath} | {status}{nl}";
    }

    return str;
}

var parser = Default.ParseArguments<ActionInputs>(() => new(), args);
parser.WithNotParsed(
    _ =>
    {
        Environment.Exit(2);
    });

await parser.WithParsedAsync(options => StartDiffAsync(options));

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

    public static class Const
    {
        public const string nl = @"%0A";
    }
}