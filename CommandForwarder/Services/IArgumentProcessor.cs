using System;
using System.Collections.Generic;
using System.Linq;

namespace CommandForwarder
{
    internal sealed class ArgumentProcessException : Exception
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

    internal sealed record ArgumentProcessResult(Action Action, int ConsumedArguments);

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

                var matchedAction = current.Actions.FirstOrDefault(a => a.Name.Equals(arg, StringComparison.InvariantCultureIgnoreCase));
                if (matchedAction is not null)
                {
                    return new ArgumentProcessResult(matchedAction, consumedArgumentCount);
                }

                var matchedVerb = current.Verbs.FirstOrDefault(v => v.Name.Equals(arg, StringComparison.InvariantCultureIgnoreCase));
                if (matchedVerb is null)
                {
                    throw new ArgumentProcessException($"No match found (argument '{arg}' did not match any verbs or actions).");
                }

                current = matchedVerb;
            }

            throw new ArgumentProcessException($"No match found (out of arguments).");
        }
    }
}