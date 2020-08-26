using System;

namespace CommandForwarder
{
    internal static class ConsoleExt
    {
        public static void Error(string error)
        {
            WriteLine(error, ConsoleColor.Red);
        }

        public static void Error(string error, Exception ex)
        {
            Error(error);
            WriteLine(ex.ToString(), ConsoleColor.Gray);
        }

        public static void Warning(string warning)
        {
            WriteLine("Warning: " + warning, ConsoleColor.Yellow);
        }

        public static void WriteLine(string value, ConsoleColor foreground)
        {
            var oldForeground = Console.ForegroundColor;
            Console.ForegroundColor = foreground;

            Console.WriteLine(value);

            Console.ForegroundColor = oldForeground;
        }
    }
}