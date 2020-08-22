namespace CommandForwarder
{
    public sealed class Action
    {
        public Action(string name, string description, string command)
        {
            Name = name;
            Description = description;
            Command = command;
        }

        public string Name { get; }
        public string Description { get; }
        public string Command { get; }
    }
}