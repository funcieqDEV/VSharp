namespace VSharpLib
{
    using System;
    using VSharp;

    [Module]
    class Convert
    {
        /// <summary>
        /// Converts an object to an integer, if possible.
        /// </summary>
        /// <param name="num">The object to convert.</param>
        /// <returns>The integer value of the object, or null if conversion is not possible.</returns>
        public int? ToInt(object? num)
        {
            try
            {
                return System.Convert.ToInt32(num);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Converts an object to a string.
        /// </summary>
        /// <param name="s">The object to convert.</param>
        /// <returns>The string representation of the object, or null if conversion is not possible.</returns>
        public string? ToString(object? s)
        {
            return System.Convert.ToString(s);
        }

        /// <summary>
        /// Converts an object to a float, if possible.
        /// </summary>
        /// <param name="num">The object to convert.</param>
        /// <returns>The float value of the object, or null if conversion is not possible.</returns>
        public float? ToFloat(object? num)
        {
            try
            {
                return System.Convert.ToSingle(num);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Converts an object to a boolean, if possible.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The boolean value of the object, or null if conversion is not possible.</returns>
        public bool? ToBool(object? value)
        {
            try
            {
                return System.Convert.ToBoolean(value);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Converts an object to a double, if possible.
        /// </summary>
        /// <param name="num">The object to convert.</param>
        /// <returns>The double value of the object, or null if conversion is not possible.</returns>
        public double? ToDouble(object? num)
        {
            try
            {
                return System.Convert.ToDouble(num);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Converts an object to a decimal, if possible.
        /// </summary>
        /// <param name="num">The object to convert.</param>
        /// <returns>The decimal value of the object, or null if conversion is not possible.</returns>
        public decimal? ToDecimal(object? num)
        {
            try
            {
                return System.Convert.ToDecimal(num);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Converts an object to a character, if possible.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The char representation of the object, or null if conversion is not possible.</returns>
        public char? ToChar(object? value)
        {
            try
            {
                return System.Convert.ToChar(value);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Converts an object to a DateTime, if possible.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The DateTime representation of the object, or null if conversion is not possible.</returns>
        public DateTime? ToDateTime(object? value)
        {
            try
            {
                return System.Convert.ToDateTime(value);
            }
            catch
            {
                return null;
            }
        }

        
    }
}
