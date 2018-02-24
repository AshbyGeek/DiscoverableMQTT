using System;
using System.Collections.Generic;
using System.Text;

namespace DiscoverableMqtt
{
    public static class ConsoleExtensions
    {
        public static void WriteDebugLocation(string value)
        {
            var prevCursorLeft = Console.CursorLeft;
            var prevCursorTop = Console.CursorTop;

            Console.SetCursorPosition(0, Console.WindowHeight - 2);
            int numLines = value.Split('\n').Length;
            for (int i = 0; i < numLines; i++)
            {
                Console.WriteLine();
            }

            Console.SetCursorPosition(0, Console.WindowHeight - 2 - numLines);
            Console.WriteLine(value);

            Console.SetCursorPosition(prevCursorLeft, Console.WindowHeight -2);
        }
    }
}
