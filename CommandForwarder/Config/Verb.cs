using System.Collections.Immutable;

namespace CommandForwarder
{
    internal sealed class Verb
    {
        public Verb(string name, string description, ImmutableArray<Verb> verbs, ImmutableArray<Action> actions)
        {
            Name = name;
            Description = description;
            Verbs = verbs;
            Actions = actions;
        }

        public string Name { get; }
        public string Description { get; }
        public ImmutableArray<Verb> Verbs { get; }
        public ImmutableArray<Action> Actions { get; }
    }
}