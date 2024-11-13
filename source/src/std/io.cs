namespace VSharpLib
{
    using System;
    using VSharp;

    [Module]
    class io
    {
        /// <summary>
        /// Prints the specified object followed by a new line.
        /// If the object is null, it prints "null".
        /// </summary>
        /// <param name="arg">The object to print.</param>
        public void println(object? arg)
        {
            Console.WriteLine(arg?.ToString() ?? "null");
        }

        /// <summary>
        /// Prompts the user for input with a specified message and returns the input as a string.
        /// </summary>
        /// <param name="message">The message to display to the user.</param>
        /// <returns>The user input as a string.</returns>
        public string? input(object? message)
        {
            Console.Write(message);
            return Console.ReadLine();
        }

        /// <summary>
        /// Prompts the user for input and returns the input as a string.
        /// </summary>
        /// <returns>The user input as a string.</returns>
        public string? input()
        {
            return Console.ReadLine();
        }
    }
}
