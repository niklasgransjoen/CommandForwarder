using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

    internal sealed class Program
    {
        private static void Main(string[] args)
        {
            using var parser = new Parser(settings =>
            {
                settings.CaseSensitive = true;
                settings.AutoHelp = true;
                settings.AutoVersion = true;
                settings.HelpWriter = Console.Out;

                // Allow escaping commands
                settings.EnableDashDash = true;
            });

            try
            {
                parser.ParseArguments<Options>(args)
                    .WithParsed(options => BuildServiceProvider()
                        .GetRequiredService<Program>()
                        .Run(options)
                    );
            }
            catch (Exception ex)
            {
                ConsoleExt.Error("Unhandled exception.", ex);
            }
        }

        private static IServiceProvider BuildServiceProvider()
        {
            return new ServiceCollection()
                .AddTransient<Program>()
                .AddTransient<IArgumentProcessor, ArgumentProcessor>()
                .AddTransient<IActionExecutor, ActionExecutor>()
                .BuildServiceProvider();
        }

        private readonly IArgumentProcessor _argumentProcessor;
        private readonly IActionExecutor _actionExecutor;

        public Program(IArgumentProcessor argumentProcessor, IActionExecutor actionExecutor)
        {
            _argumentProcessor = argumentProcessor;
            _actionExecutor = actionExecutor;
        }

        private void Run(Options options)
        {
            var config = ConfigurationManager.ReadConfig(options.Config);
            if (config is null)
                return;

            var proxyVerb = new Verb("Root", string.Empty, config.Verbs, ImmutableArray<Action>.Empty);
            var args = options.Args;

            try
            {
                var result = _argumentProcessor.ProcessArguments(proxyVerb, args);

                var action = result.Action;
                var commandArgs = args.Skip(result.ConsumedArguments);
                _actionExecutor.Execute(action, commandArgs);
            }
            catch (ArgumentProcessException ex)
            {
                VerbConsolePrinter.PrintHelpText(ex.CurrentVerb);
                Console.WriteLine();
                ConsoleExt.Error(ex.Message);
            }
            catch (ActionExecutionException ex)
            {
                ConsoleExt.Error(ex.Message);
            }
        }
    }
}