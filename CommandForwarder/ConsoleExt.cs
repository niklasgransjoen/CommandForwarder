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

        public static void Write(string value, ConsoleColor foreground)
        {
            var oldForeground = Console.ForegroundColor;
            Console.ForegroundColor = foreground;

            Console.Write(value);

            Console.ForegroundColor = oldForeground;
        }

        public static void WriteLine(string value, ConsoleColor foreground)
        {
            Write(value, foreground);
            Console.WriteLine();
        }
    }
}