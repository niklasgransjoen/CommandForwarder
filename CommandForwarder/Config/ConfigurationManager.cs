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

            RawConfig? rawConfig;
            try
            {
                var json = File.ReadAllText(configPath);
                rawConfig = JsonSerializer.Deserialize<RawConfig>(json, new JsonSerializerOptions
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

        private static bool ValidateConfig(RawConfig? rawConfig, [NotNullWhen(true)] out Config? config)
        {
            if (rawConfig is null)
            {
                ConsoleExt.Error("Config cannot be null.");
                config = null;
                return false;
            }

            if (rawConfig.Verbs is null)
            {
                ConsoleExt.Error("Configuration must contain at least one verb.");
                config = null;
                return false;
            }

            var verbNames = CreateNameSet();
            if (!ValidateVerbs(rawConfig.Verbs, verbNames, out var verbs))
            {
                config = null;
                return false;
            }

            config = new Config(verbs);
            return true;
        }

        private static bool ValidateVerbs(RawVerb[]? rawVerbs, HashSet<string> seenNames, out ImmutableArray<Verb> verbs)
        {
            if (rawVerbs is null)
            {
                verbs = ImmutableArray<Verb>.Empty;
                return true;
            }

            var builder = ImmutableArray.CreateBuilder<Verb>();

            foreach (var verb in rawVerbs)
            {
                if (string.IsNullOrWhiteSpace(verb.Name))
                {
                    ConsoleExt.Warning("Skipping verb without name.");
                    continue;
                }
                else if (!seenNames.Add(verb.Name))
                {
                    ConsoleExt.Error($"Duplicate verb or action name '{verb.Name}'.");
                    verbs = default;
                    return false;
                }

                var childrenNames = CreateNameSet();
                if (!ValidateVerbs(verb.Verbs, childrenNames, out var childVerbs))
                {
                    verbs = default;
                    return false;
                }

                if (!ValidateActions(verb.Actions, childrenNames, out var actions))
                {
                    verbs = default;
                    return false;
                }

                var name = ParseString(verb.Name);
                var description = ParseString(verb.Description);
                builder.Add(new Verb(name, description, childVerbs, actions));
            }

            verbs = builder.ToImmutable();
            return true;
        }

        private static bool ValidateActions(RawAction[]? rawActions, HashSet<string> seenNames, out ImmutableArray<Action> actions)
        {
            if (rawActions is null)
            {
                actions = ImmutableArray<Action>.Empty;
                return true;
            }

            var builder = ImmutableArray.CreateBuilder<Action>();

            foreach (var action in rawActions)
            {
                if (string.IsNullOrWhiteSpace(action.Name))
                {
                    ConsoleExt.Warning("Skipping action without name.");
                    continue;
                }
                else if (string.IsNullOrWhiteSpace(action.Command))
                {
                    ConsoleExt.Warning("Skipping action without command.");
                    continue;
                }
                else if (!seenNames.Add(action.Name))
                {
                    ConsoleExt.Error($"Duplicate verb or action name '{action.Name}'.");
                    actions = default;
                    return false;
                }

                var name = ParseString(action.Name);
                var description = ParseString(action.Description);
                var command = action.Command;

                builder.Add(new Action(name, description, command));
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