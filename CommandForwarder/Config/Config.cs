using System.Collections.Immutable;

namespace CommandForwarder
{
    internal sealed class Config
    {
        public Config(ImmutableArray<Verb> verbs)
        {
            Verbs = verbs;
        }

        public ImmutableArray<Verb> Verbs { get; }
    }
}