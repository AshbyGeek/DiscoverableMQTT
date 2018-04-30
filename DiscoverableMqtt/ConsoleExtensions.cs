using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiscoverableMqtt
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static class ConsoleExtensions
    {
        private static object _ConsoleLock = new object();

        public static bool WriteDebugLocationEnabled { get; set; }

        public static void WriteDebugLocation(string value, int windowTop)
        {
            if (!WriteDebugLocationEnabled)
                return;

            // Using Console.CursorLeft blocks until no other threads are waiting
            // for read calls or writing, which is problematical since the
            // main thread is always waiting for user input. 
            // So for now we just skip writing debug info entirely on linux
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                //WriteLine(value);
                return;
            }

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
