using System.Collections.Immutable;

namespace CommandForwarder
{
    internal sealed class Definition
    {
        public Definition(string name, ImmutableArray<Definition> definitions)
        {
            Name = name;
            Definitions = definitions;
        }

        public Definition(string name, string command)
        {
            Name = name;
            Definitions = ImmutableArray<Definition>.Empty;
            Command = command;
        }

        public string Name { get; }
        public ImmutableArray<Definition> Definitions { get; }
        public string? Command { get; }
    }
}