using System;
using System.Collections.Generic;
using System.Text;

namespace DiscoverableMqtt
{
    public static class ConsoleExtensions
    {
        private static object _ConsoleLock = new object();

        public static bool WriteDebugLocationEnabled { get; set; }

        public static void WriteDebugLocation(string value, int windowTop)
        {
            if (!WriteDebugLocationEnabled)
                return;
            
            lock (_ConsoleLock)
            {
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

        public static void WriteLine(string value)
        {
            lock (_ConsoleLock)
            {
                Console.WriteLine(value);
            }
        }

        public static void Write(string value)
        {
            lock (_ConsoleLock)
            {
                Console.Write(value);
            }
        }
    }
}
