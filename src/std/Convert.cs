namespace VSharpLib
{
    using System;
    using VSharp;

    [Module]
    class conv
    {
        /// <summary>
        /// Converts an object to an integer, if possible.
        /// </summary>
        /// <param name="num">The object to convert.</param>
        /// <returns>The integer value of the object, or null if conversion is not possible.</returns>
        public int? toInt(object? num)
        {
            try
            {
                return Convert.ToInt32(num);
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
        public string? toString(object? s)
        {
            return Convert.ToString(s);
        }

        /// <summary>
        /// Converts an object to a float, if possible.
        /// </summary>
        /// <param name="num">The object to convert.</param>
        /// <returns>The float value of the object, or null if conversion is not possible.</returns>
        public float? toFloat(object? num)
        {
            try
            {
                return Convert.ToSingle(num);
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
        public bool? toBool(object? value)
        {
            try
            {
                return Convert.ToBoolean(value);
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
        public double? toDouble(object? num)
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
        public decimal? toDecimal(object? num)
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
        public char? toChar(object? value)
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
        public DateTime? toDateTime(object? value)
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
