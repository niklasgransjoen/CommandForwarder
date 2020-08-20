using System.Collections.Immutable;

namespace CommandForwarder
{
    internal sealed class Config
    {
        public Config(ImmutableArray<Definition> definitions)
        {
            Definitions = definitions;
        }

        public ImmutableArray<Definition> Definitions { get; }
    }
}