using DepView.CLI;
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
                ConsoleExt.Error($"Failed to resolve config '{configPath}'.");

            RawConfig rawConfig;
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

        private static bool ValidateConfig(RawConfig rawConfig, [NotNullWhen(true)] out Config? config)
        {
            if (rawConfig.Definitions is null)
            {
                ConsoleExt.Error("Configuration contains no definitions.");
                config = null;
                return false;
            }

            if (!ValidateDefinitions(rawConfig.Definitions, out var definitions))
            {
                config = null;
                return false;
            }

            config = new Config(definitions);
            return true;
        }

        private static bool ValidateDefinitions(RawDefinition[] rawDefinitions, out ImmutableArray<Definition> definitions)
        {
            var usedNames = new HashSet<string>();
            var builder = ImmutableArray.CreateBuilder<Definition>();

            foreach (var def in rawDefinitions)
            {
                if (string.IsNullOrWhiteSpace(def.Name))
                {
                    ConsoleExt.Warning("Skipping definition without name.");
                    continue;
                }
                else if (!usedNames.Add(def.Name))
                {
                    ConsoleExt.Error($"Duplicate definition name '{def.Name}'.");
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(def.Command))
                {
                    if (def.Definitions != null)
                    {
                        ConsoleExt.Warning("Command and defintion are both specified. Command will be prioritized.");
                        return false;
                    }

                    builder.Add(new Definition(def.Name, def.Command));
                }
                else
                {
                    if (def.Definitions is null)
                    {
                        ConsoleExt.Error($"Definition '{def.Name}' has neither child definitions nor a command. Definitions must have one of these.");
                        return false;
                    }

                    if (!ValidateDefinitions(def.Definitions, out var childDefinitions))
                        return false;

                    builder.Add(new Definition(def.Name, childDefinitions));
                }
            }

            definitions = builder.ToImmutable();
            return true;
        }
    }
}