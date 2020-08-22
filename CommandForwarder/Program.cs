using CommandLine;
using DepView.CLI;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace CommandForwarder
{
    public sealed class Options
    {
        public const string DefaultConfig = "config.json";

        [Option('c', "config", HelpText = "The full path of the json configuration. A local '" + DefaultConfig + "' is searched for if not specified.")]
        public string? Config { get; set; }

        [Option('a', "args", Separator = ' ', HelpText = "The arguments to process and forward. Once a match is found, all remaining arguments are forwarded.", Required = true, Min = 1)]
        public IEnumerable<string> Args { get; set; } = Enumerable.Empty<string>();
    }

    internal static class Program
    {
        private static void Main(string[] args)
        {
            using var parser = new Parser(settings =>
            {
                settings.CaseSensitive = true;
                settings.AutoHelp = true;
                settings.AutoVersion = true;
                settings.HelpWriter = Console.Out;

                settings.EnableDashDash = true;
            });

            try
            {
                parser.ParseArguments<Options>(args)
                    .WithParsed(Run);
            }
            catch (Exception ex)
            {
                ConsoleExt.Error("Unhandled exception", ex);
                throw;
            }
        }

        private static void Run(Options options)
        {
            var config = ConfigurationManager.ReadConfig(options.Config);
            if (config is null)
                return;

            var verbs = config.Verbs;
            var actions = ImmutableArray<Action>.Empty;
            var args = options.Args.ToArray();

            ProcessArguments(verbs, actions, args);
        }

        private static void ProcessArguments(ImmutableArray<Verb> verbs, ImmutableArray<Action> actions, Span<string> args)
        {
            if (args.Length == 0)
            {
                ConsoleExt.Error("No match found (out of arguments).");
                return;
            }

            var arg = args[0];
            var forwaredArgs = args.Slice(1);

            if (TryMatchAction(actions, arg, forwaredArgs))
                return;

            var verbLookup = verbs.ToDictionary(v => v.Name, StringComparer.InvariantCultureIgnoreCase);
            if (!verbLookup.TryGetValue(arg, out var matchedVerb))
            {
                ConsoleExt.Error($"No match found (argument '{arg}' did not match any verbs or actions).");
            }
            else if (forwaredArgs.Length > 0)
            {
                ProcessArguments(matchedVerb.Verbs, matchedVerb.Actions, forwaredArgs);
            }
            else
            {
                ConsoleExt.Error($"No match found (argument '{arg}' matched a verb. Are you missing an argument?).");
            }
        }

        private static bool TryMatchAction(ImmutableArray<Action> actions, string arg, Span<string> forwaredArgs)
        {
            if (actions.IsEmpty)
                return false;

            var actionLookup = actions.ToDictionary(c => c.Name, StringComparer.InvariantCultureIgnoreCase);
            if (actionLookup.TryGetValue(arg, out var matchedAction))
            {
                ExecuteAction(matchedAction, forwaredArgs);
                return true;
            }

            return false;
        }

        private static void ExecuteAction(Action action, Span<string> args)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = action.Command,
                };

                foreach (var arg in args)
                {
                    startInfo.ArgumentList.Add(arg);
                }

                var process = Process.Start(startInfo);
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                ConsoleExt.Error($"Failed to execute action '{action.Name}'.", ex);
            }
        }
    }
}