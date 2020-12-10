using System.Collections.Immutable;

namespace CommandForwarder
{
    internal sealed record Config(ImmutableArray<Verb> Verbs);

    internal sealed record Verb(string Name, string Description, ImmutableArray<Verb> Verbs, ImmutableArray<Action> Actions);

    internal sealed record Action(string Name, string Description, string Command);
}