namespace CommandForwarder
{
    internal sealed class RawDefinition
    {
        public RawDefinition()
        {
        }

        public string? Name { get; set; }
        public RawDefinition[]? Definitions { get; set; }
        public string? Command { get; set; }
    }
}