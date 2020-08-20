using CommandLine;
using DepView.CLI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CommandForwarder
{
    public sealed class Options
    {
        public const string DefaultConfig = "config.json";

        [Option('c', "config", HelpText = "The full path of the json configuration. A local '" + DefaultConfig + "' is searched for if not specified.")]
        public string? Definition { get; set; }

        [Option('a', "args", Separator = ' ', HelpText = "The arguments to process and forward. Once a match is found, all remaining arguments are forwarded.", Required = true, Min = 1)]
        public IEnumerable<string> Args { get; set; } = Enumerable.Empty<string>();
    }

    internal static class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                Parser.Default.ParseArguments<Options>(args)
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
            var config = ConfigurationManager.ReadConfig(options.Definition);
            if (config is null)
                return;

            var definitions = config.Definitions;
            var args = options.Args.ToArray();
            ProcessArguments(definitions, args);
        }

        private static void ProcessArguments(IReadOnlyList<Definition> definitions, Span<string> args)
        {
            var definitionLookup = definitions.ToDictionary(d => d.Name, StringComparer.InvariantCultureIgnoreCase);

            if (args.Length == 0)
            {
                ConsoleExt.Error("No match found (out of arguments).");
                return;
            }

            var arg = args[0];
            var forwaredArgs = args.Slice(1);

            if (!definitionLookup.TryGetValue(arg, out var matchedDefinition))
            {
                ConsoleExt.Error($"No match found (argument '{arg}' did not match any defintion).");
            }
            else if (matchedDefinition.Command != null)
            {
                ExecuteCommand(matchedDefinition.Command, forwaredArgs);
            }
            else if (forwaredArgs.Length > 0)
            {
                ProcessArguments(matchedDefinition.Definitions, forwaredArgs);
            }
            else
            {
                ConsoleExt.Error($"No match found (argument '{arg}' does not have a command, but contains other definitions. Are you missing an argument?).");
            }
        }

        private static void ExecuteCommand(string command, Span<string> args)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = command,
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
                ConsoleExt.Error($"Failed to execute command '{command}'.", ex);
            }
        }
    }
}