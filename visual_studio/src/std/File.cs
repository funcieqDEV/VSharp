namespace VSharpLib
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using VSharp;

    [Module]
    class File
    {
        /// <summary>
        /// Reads the entire contents of a file and returns it as a string.
        /// </summary>
        /// <param name="name">The file path to read.</param>
        /// <returns>The content of the file as a string, or null if reading fails.</returns>
        public string? ReadFile(object name)
        {
            try
            {
                return System.IO.File.ReadAllText(name.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Writes the specified content to a file, overwriting it if it already exists.
        /// </summary>
        /// <param name="name">The file path to write to.</param>
        /// <param name="value">The content to write to the file.</param>
        public void WriteFile(object name, object value)
        {
            try
            {
                System.IO.File.WriteAllText(name.ToString(), value.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to file: {ex.Message}");
            }
        }

        /// <summary>
        /// Appends the specified content to the end of a file.
        /// </summary>
        /// <param name="name">The file path to append to.</param>
        /// <param name="value">The content to append to the file.</param>
        public void AppendToFile(object name, object value)
        {
            try
            {
                System.IO.File.AppendAllText(name.ToString(), value.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error appending to file: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if a file exists at the specified path.
        /// </summary>
        /// <param name="name">The file path to check.</param>
        /// <returns>True if the file exists, otherwise false.</returns>
        public bool FileExists(object name)
        {
            return System.IO.File.Exists(name.ToString());
        }

        /// <summary>
        /// Deletes the specified file if it exists.
        /// </summary>
        /// <param name="name">The file path to delete.</param>
        public void DeleteFile(object name)
        {
            try
            {
                if (FileExists(name))
                {
                    System.IO.File.Delete(name.ToString());
                }
                else
                {
                    Console.WriteLine("File does not exist.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting file: {ex.Message}");
            }
        }

        /// <summary>
        /// Reads the contents of a file line by line and returns it as a list of strings.
        /// </summary>
        /// <param name="name">The file path to read.</param>
        /// <returns>A list of strings representing each line in the file, or null if reading fails.</returns>
        public List<string>? ReadLines(object name)
        {
            try
            {
                return new List<string>(System.IO.File.ReadLines(name.ToString()));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading lines from file: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Copies a file from the specified source path to a new destination path.
        /// </summary>
        /// <param name="source">The source file path.</param>
        /// <param name="destination">The destination file path.</param>
        /// <param name="overwrite">Whether to overwrite the file if it already exists at the destination.</param>
        public void CopyFile(object source, object destination, bool overwrite = false)
        {
            try
            {
                System.IO.File.Copy(source.ToString(), destination.ToString(), overwrite);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error copying file: {ex.Message}");
            }
        }
    }
}
