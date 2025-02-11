using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSharp;

namespace VSharpLib
{
    [Module]
    public class sys
    {
        /// <summary>
        /// Exits the application with the specified exit code.
        /// </summary>
        /// <param name="code">The exit code.</param>
        public void exit(int code)
        {
            Environment.Exit(code);
        }

        /// <summary>
        /// Sets the foreground and background color of the console.
        /// </summary>
        /// <param name="fg">Foreground color.</param>
        /// <param name="bg">Background color.</param>
        public void setColor(string fg, string bg)
        {
            Console.ForegroundColor = strToColor(fg);
            Console.BackgroundColor = strToColor(bg);
        }

        /// <summary>
        /// Sets the background color of the console.
        /// </summary>
        /// <param name="c">Color name.</param>
        public void setBackgroundColor(string c)
        {
            Console.BackgroundColor = strToColor(c);
        }

        /// <summary>
        /// Sets the foreground color of the console.
        /// </summary>
        /// <param name="c">Color name.</param>
        public void setForegroundColor(string c)
        {
            Console.ForegroundColor = strToColor(c);
        }

        /// <summary>
        /// Converts a string representation of a color to ConsoleColor.
        /// </summary>
        /// <param name="str">Color name.</param>
        /// <returns>Corresponding ConsoleColor.</returns>
        public ConsoleColor strToColor(string str)
        {
            return str switch
            {
                "red" => ConsoleColor.Red,
                "white" => ConsoleColor.White,
                "magenta" => ConsoleColor.Magenta,
                "yellow" => ConsoleColor.Yellow,
                "black" => ConsoleColor.Black,
                "blue" => ConsoleColor.Blue,
                "cyan" => ConsoleColor.Cyan,
                "green" => ConsoleColor.Green,
                _ => ConsoleColor.White
            };
        }

        /// <summary>
        /// Executes a system command and prints the output.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        public void execute(string command)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo()
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process() { StartInfo = psi })
                {
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    if (!string.IsNullOrEmpty(output))
                        Console.WriteLine(output);

                    if (!string.IsNullOrEmpty(error))
                        Console.WriteLine($"Error: {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Execution failed: {ex.Message}");
            }
        }
    }
}
