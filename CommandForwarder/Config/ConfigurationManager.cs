using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;

namespace CommandForwarder
{
    internal static class ConfigurationManager
    {
        public static Config? ReadConfig(string? path)
        {
            var configPath = path ?? Path.Combine(Directory.GetCurrentDirectory(), Options.DefaultConfig);
            if (!File.Exists(configPath))
            {
                ConsoleExt.Error($"Configuration file '{configPath}' does not exist.");
                return null;
            }

            Config? rawConfig;
            try
            {
                var json = File.ReadAllText(configPath);
                rawConfig = JsonSerializer.Deserialize<Config>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });
            }
            catch (JsonException ex)
            {
                ConsoleExt.Error("Failed to deserialize configuration.", ex);
                return null;
            }

            if (ValidateConfig(rawConfig, out var config))
                return config;

            return null;
        }

        private static bool ValidateConfig(Config? rawConfig, [NotNullWhen(true)] out Config? config)
        {
            if (rawConfig is null)
            {
                ConsoleExt.Error("Config cannot be null.");
                config = null;
                return false;
            }

            var verbNames = CreateNameSet();
            if (!ValidateVerbs(rawConfig.Verbs, verbNames, out var verbs))
            {
                config = null;
                return false;
            }

            if (verbs.IsEmpty)
            {
                ConsoleExt.Error("Configuration must contain at least one valid verb.");
                config = null;
                return false;
            }

            config = new Config(verbs);
            return true;
        }

        private static bool ValidateVerbs(ImmutableArray<Verb> rawVerbs, HashSet<string> seenNames, out ImmutableArray<Verb> verbs)
        {
            if (rawVerbs.IsDefaultOrEmpty)
            {
                verbs = ImmutableArray<Verb>.Empty;
                return true;
            }

            var builder = ImmutableArray.CreateBuilder<Verb>();

            foreach (var rawVerb in rawVerbs)
            {
                if (string.IsNullOrWhiteSpace(rawVerb.Name))
                {
                    ConsoleExt.Warning("Skipping verb without name.");
                    continue;
                }
                else if (!seenNames.Add(rawVerb.Name))
                {
                    ConsoleExt.Error($"Duplicate verb or action name '{rawVerb.Name}'.");
                    verbs = default;
                    return false;
                }

                var childrenNames = CreateNameSet();
                if (!ValidateVerbs(rawVerb.Verbs, childrenNames, out var childVerbs))
                {
                    verbs = default;
                    return false;
                }

                if (!ValidateActions(rawVerb.Actions, childrenNames, out var actions))
                {
                    verbs = default;
                    return false;
                }

                builder.Add(rawVerb with
                {
                    Name = ParseString(rawVerb.Name),
                    Description = ParseString(rawVerb.Description),
                    Verbs = childVerbs,
                    Actions = actions,
                });
            }

            verbs = builder.ToImmutable();
            return true;
        }

        private static bool ValidateActions(ImmutableArray<Action> rawActions, HashSet<string> seenNames, out ImmutableArray<Action> actions)
        {
            if (rawActions.IsDefaultOrEmpty)
            {
                actions = ImmutableArray<Action>.Empty;
                return true;
            }

            var builder = ImmutableArray.CreateBuilder<Action>();

            foreach (var rawAction in rawActions)
            {
                if (string.IsNullOrWhiteSpace(rawAction.Name))
                {
                    ConsoleExt.Warning("Skipping action without name.");
                    continue;
                }
                else if (string.IsNullOrWhiteSpace(rawAction.Command))
                {
                    ConsoleExt.Warning("Skipping action without command.");
                    continue;
                }
                else if (!seenNames.Add(rawAction.Name))
                {
                    ConsoleExt.Error($"Duplicate verb or action name '{rawAction.Name}'.");
                    actions = default;
                    return false;
                }

                builder.Add(rawAction with
                {
                    Name = ParseString(rawAction.Name),
                    Description = ParseString(rawAction.Description),
                });
            }

            actions = builder.ToImmutable();
            return true;
        }

        #region Utilities

        private static HashSet<string> CreateNameSet() => new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        private static string ParseString(string? str) => str?.Trim() ?? string.Empty;

        #endregion Utilities
    }
}