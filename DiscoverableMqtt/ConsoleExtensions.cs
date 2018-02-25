using System;
using System.Collections.Generic;
using System.Text;

namespace DiscoverableMqtt
{
    public static class ConsoleExtensions
    {
        public static bool WriteDebugLocationEnabled { get; set; }

        public static void WriteDebugLocation(string value, int windowTop)
        {
            if (!WriteDebugLocationEnabled)
                return;

            var prevCursorLeft = Console.CursorLeft;
            var prevCursorTop = Console.CursorTop;
            var bgcolor = Console.BackgroundColor;

            Console.BackgroundColor = ConsoleColor.DarkBlue;

            var lines = value.Split("\n");
            var windowLine = windowTop;
            foreach (var line in lines)
            {
                var bufferLeft = Console.WindowLeft + Console.WindowWidth - line.Length - 2;
                var bufferTop = Console.WindowTop + windowLine;
                Console.SetCursorPosition(bufferLeft, bufferTop);

                Console.Write("  " + line);
                windowLine += 1;

                if (windowLine >= Console.WindowHeight - 1)
                {
                    break;
                }
            }

            Console.SetCursorPosition(prevCursorLeft, prevCursorTop);
            Console.BackgroundColor = bgcolor;
        }
    }
}
