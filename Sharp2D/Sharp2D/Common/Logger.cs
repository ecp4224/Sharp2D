using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Sharp2D.Core;

namespace Sharp2D
{
    public class Logger
    {
        private static TextWriter currentWriter;
        internal static void Redirect(bool value)
        {
            if (value)
            {
                currentWriter = new ParallelStringWriter(Console.Out);
                Console.SetOut(currentWriter);
            }
            else
            {
                var standardOutput = new StreamWriter(Console.OpenStandardOutput());
                standardOutput.AutoFlush = true;
                Console.SetOut(standardOutput);

                currentWriter = standardOutput;
            }
        }

        internal static void SaveLog()
        {
            if (GlobalSettings.EngineSettings.WriteLog && currentWriter is ParallelStringWriter)
            {
                string file = "log." + DateTime.Now.ToString("o").Replace(':', '.') + ".txt";
                foreach (var c in Path.GetInvalidFileNameChars())
                {
                    file.Replace(c, '.');
                }

                string contents = currentWriter.ToString();

                File.WriteAllText(file, contents);
            }
        }

        public static void Warn(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("WARNING: " + message);
        }

        public static void Debug(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(message);
        }

        public static void Log(string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message);
        }

        public static void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(message);
        }

        public static void CaughtException(Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("An exception has occured!");
            Console.WriteLine("Luckly, we caught this one..");
            Console.WriteLine("====== DETAILS =====");
            Console.WriteLine(e.ToString());
            Console.WriteLine("=====================");
        }

        public static void UncaughtException(Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("An exception has occured!");
            Console.WriteLine("====== DETAILS =====");
            Console.WriteLine(e.ToString());
            Console.WriteLine("=====================");
        }

        public static void WriteAt(int x, int y, string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            int ox = Console.CursorLeft;
            int oy = Console.CursorTop;

            Console.CursorLeft = x;
            Console.CursorTop = y;
            Console.WriteLine("                                                  ");
            Console.WriteLine(message);

            Console.CursorLeft = ox;
            Console.CursorTop = oy;
        }

        public static void PrintStackTrace()
        {
            Logger.Error("========= StackTrace =========");
            System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
            Logger.Error(t.ToString());
            Logger.Error("==============================");
        }
    }
}
