using System;

namespace CommandForwarder
{
    internal static class VerbConsolePrinter
    {
        public static void PrintHelpText(Verb verb)
        {
            WriteVerbName(verb.Name);
            Console.WriteLine(" - Help Text");
            if (!string.IsNullOrWhiteSpace(verb.Description))
                Console.WriteLine(verb.Description);

            Console.WriteLine();

            PrintSimpleHelpText(verb);
        }

        private static void PrintSimpleHelpText(Verb verb, string indentation = "")
        {
            if (!verb.Verbs.IsEmpty)
            {
                foreach (var childVerb in verb.Verbs)
                {
                    Console.Write(indentation);
                    Console.Write("- ");
                    PrettyPrint(childVerb.Name, childVerb.Description, isVerb: true);
                    PrintSimpleHelpText(childVerb, indentation + "  ");
                }
            }

            if (!verb.Actions.IsEmpty)
            {
                foreach (var action in verb.Actions)
                {
                    Console.Write(indentation);
                    Console.Write("- ");
                    PrettyPrint(action.Name, action.Description, isVerb: false);
                }
            }
        }

        private static void PrettyPrint(string name, string description, bool isVerb)
        {
            if (isVerb)
                WriteVerbName(name);
            else
                WriteActionName(name);

            if (!string.IsNullOrWhiteSpace(description))
            {
                Console.Write(" - ");
                Console.Write(description);
            }

            Console.WriteLine();
        }

        private static void WriteVerbName(string name) => ConsoleExt.Write(name, ConsoleColor.Yellow);

        private static void WriteActionName(string name) => ConsoleExt.Write(name, ConsoleColor.Cyan);
    }
}