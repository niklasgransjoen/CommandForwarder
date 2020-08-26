using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CommandForwarder
{
    public sealed class ArgumentProcessException : Exception
    {
        public ArgumentProcessException()
        {
        }

        public ArgumentProcessException(string message) : base(message)
        {
        }

        public ArgumentProcessException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    internal sealed class ArgumentProcessResult
    {
        public ArgumentProcessResult(Action action, int consumedArguments)
        {
            Action = action;
            ConsumedArguments = consumedArguments;
        }

        public Action Action { get; }
        public int ConsumedArguments { get; }
    }

    internal interface IArgumentProcessor
    {
        /// <summary>
        /// Processes a sequence of arguments against a verb.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="ArgumentProcessException">Processing fails.</exception>
        ArgumentProcessResult ProcessArguments(Verb verb, IEnumerable<string> args);
    }

    internal sealed class ArgumentProcessor : IArgumentProcessor
    {
        public ArgumentProcessResult ProcessArguments(Verb verb, IEnumerable<string> args)
        {
            var current = verb;

            int consumedArgumentCount = 0;
            foreach (var arg in args)
            {
                consumedArgumentCount++;

                if (TryMatchAction(current.Actions, arg, out var matchedAction))
                {
                    return new ArgumentProcessResult(matchedAction, consumedArgumentCount);
                }

                var verbLookup = current.Verbs.ToDictionary(v => v.Name, StringComparer.InvariantCultureIgnoreCase);
                if (!verbLookup.TryGetValue(arg, out var matchedVerb))
                {
                    throw new ArgumentProcessException($"No match found (argument '{arg}' did not match any verbs or actions).");
                }

                current = matchedVerb;
            }

            throw new ArgumentProcessException($"No match found (out of arguments).");
        }

        private static bool TryMatchAction(ImmutableArray<Action> actions, string arg, [NotNullWhen(true)] out Action? matchedAction)
        {
            if (actions.IsEmpty)
            {
                matchedAction = null;
                return false;
            }

            var actionLookup = actions.ToDictionary(c => c.Name, StringComparer.InvariantCultureIgnoreCase);
            if (actionLookup.TryGetValue(arg, out matchedAction))
            {
                return true;
            }

            return false;
        }
    }
}