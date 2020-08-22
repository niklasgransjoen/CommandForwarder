namespace CommandForwarder
{
    internal sealed class RawVerb
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public RawVerb[]? Verbs { get; set; }
        public RawAction[]? Actions { get; set; }
    }
}