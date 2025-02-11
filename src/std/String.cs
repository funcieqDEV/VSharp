namespace VSharpLib
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using VSharp;

    [Module]
    class str
    {

        /// <summary>
        /// Concatenates two strings.
        /// </summary>
        /// <param name="a">The first string.</param>
        /// <param name="b">The second string.</param>
        /// <returns>The concatenated string.</returns>
        public string concat(string a, string b)
        {
            return a + b;
        }

        /// <summary>
        /// Returns a substring from a specified string starting at a specified position.
        /// </summary>
        /// <param name="str">The string from which to extract the substring.</param>
        /// <param name="start">The zero-based starting position of the substring.</param>
        /// <param name="length">The number of characters in the substring.</param>
        /// <returns>The substring of the specified string.</returns>
        public string substring(string str, int start, int length)
        {
            if (start < 0 || start >= str.Length || length < 0 || start + length > str.Length)
            {
                throw new ArgumentOutOfRangeException("Start or length is out of bounds.");
            }
            return str.Substring(start, length);
        }

        /// <summary>
        /// Returns the index of the first occurrence of a specified substring within a string.
        /// </summary>
        /// <param name="str">The string to search.</param>
        /// <param name="substr">The substring to find.</param>
        /// <returns>The zero-based index of the first occurrence of the substring, or -1 if not found.</returns>
        public int indexOf(string str, string substr)
        {
            return str.IndexOf(substr);
        }

        /// <summary>
        /// Converts a string to uppercase.
        /// </summary>
        /// <param name="str">The string to convert.</param>
        /// <returns>The string in uppercase.</returns>
        public string toUpper(string str)
        {
            return str.ToUpper();
        }

        /// <summary>
        /// Converts a string to lowercase.
        /// </summary>
        /// <param name="str">The string to convert.</param>
        /// <returns>The string in lowercase.</returns>
        public string toLower(string str)
        {
            return str.ToLower();
        }

        /// <summary>
        /// Checks if the string starts with the specified prefix.
        /// </summary>
        /// <param name="str">The string to check.</param>
        /// <param name="prefix">The prefix to check for.</param>
        /// <returns>True if the string starts with the specified prefix; otherwise, false.</returns>
        public bool startsWith(string str, string prefix)
        {
            return str.StartsWith(prefix);
        }

        /// <summary>
        /// Checks if the string ends with the specified suffix.
        /// </summary>
        /// <param name="str">The string to check.</param>
        /// <param name="suffix">The suffix to check for.</param>
        /// <returns>True if the string ends with the specified suffix; otherwise, false.</returns>
        public bool endsWith(string str, string suffix)
        {
            return str.EndsWith(suffix);
        }

        /// <summary>
        /// Replaces all occurrences of a specified string in the original string with another specified string.
        /// </summary>
        /// <param name="str">The original string.</param>
        /// <param name="oldValue">The string to be replaced.</param>
        /// <param name="newValue">The string to replace with.</param>
        /// <returns>The modified string with replacements.</returns>
        public string replace(string str, string oldValue, string newValue)
        {
            return str.Replace(oldValue, newValue);
        }

        /// <summary>
        /// Splits a string into a list of substrings based on a specified delimiter.
        /// </summary>
        /// <param name="str">The string to split.</param>
        /// <param name="delimiter">The delimiter to split the string by.</param>
        /// <returns>A list of substrings.</returns>
        public List<string> split(string str, string delimiter)
        {
            string[] parts = str.Split(new string[] { delimiter }, StringSplitOptions.None);
            return new List<string>(parts);
        }

        /// <summary>
        /// Trims whitespace from the beginning and end of a string.
        /// </summary>
        /// <param name="str">The string to trim.</param>
        /// <returns>The trimmed string.</returns>
        public string trim(string str)
        {
            return str.Trim();
        }

        /// <summary>
        /// Checks if the string contains the specified substring.
        /// </summary>
        /// <param name="str">The string to check.</param>
        /// <param name="substr">The substring to find.</param>
        /// <returns>True if the string contains the substring; otherwise, false.</returns>
        public bool contains(string str, string substr)
        {
            return str.Contains(substr);
        }

        /// <summary>
        /// Repeats a string a specified number of times.
        /// </summary>
        /// <param name="str">The string to repeat.</param>
        /// <param name="count">The number of times to repeat the string.</param>
        /// <returns>The concatenated string.</returns>
        public string repeat(string str, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("Count must be non-negative.");
            }
            return new StringBuilder().Insert(0, str, count).ToString();
        }

        /// <summary>
        /// Reverses the characters in a string.
        /// </summary>
        /// <param name="str">The string to reverse.</param>
        /// <returns>The reversed string.</returns>
        public string reverse(string str)
        {
            // Check if the input string is null or empty
            if (string.IsNullOrEmpty(str))
            {
                return str; // Return the original string if it's null or empty
            }

            // Convert the string to a char array and reverse it
            char[] charArray = str.ToCharArray();
            System.Array.Reverse(charArray);

            // Create a new string from the reversed char array
            return new string(charArray);
        }


        /// <summary>
        /// Splits a string into a list of substrings based on a single character as the delimiter.
        /// </summary>
        /// <param name="str">The string to split.</param>
        /// <param name="delimiter">The character to split the string by.</param>
        /// <returns>A list of substrings.</returns>
        public List<string> splitByChar(string str, char delimiter)
        {
            return new List<string>(str.Split(delimiter));
        }

        /// <summary>
        /// Joins a list of strings into a single string using a specified delimiter.
        /// </summary>
        /// <param name="strings">The list of strings to join.</param>
        /// <param name="delimiter">The delimiter to use.</param>
        /// <returns>The concatenated string.</returns>
        public string Join(List<string> strings, string delimiter)
        {
            return string.Join(delimiter, strings);
        }

        /// <summary>
        /// Returns the index of the last occurrence of a specified substring within a string.
        /// </summary>
        /// <param name="str">The string to search.</param>
        /// <param name="substr">The substring to find.</param>
        /// <returns>The zero-based index of the last occurrence of the substring, or -1 if not found.</returns>
        public int indexOfLast(string str, string substr)
        {
            return str.LastIndexOf(substr);
        }

        /// <summary>
        /// Counts how many times a specified substring occurs in the string.
        /// </summary>
        /// <param name="str">The string to search.</param>
        /// <param name="substr">The substring to count.</param>
        /// <returns>The number of occurrences of the substring.</returns>
        public int countOccurrences(string str, string substr)
        {
            int count = 0;
            int index = 0;
            while ((index = str.IndexOf(substr, index)) != -1)
            {
                count++;
                index += substr.Length;
            }
            return count;
        }
    }
}
