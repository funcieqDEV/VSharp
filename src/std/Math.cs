namespace VSharpLib
{
    using System;
    using VSharp;

    [Module]
    class math
    {
        /// <summary>
        /// Generates a random integer between the specified minimum and maximum values.
        /// </summary>
        /// <param name="min">The inclusive lower bound of the random number returned.</param>
        /// <param name="max">The exclusive upper bound of the random number returned.</param>
        /// <returns>A random integer between min (inclusive) and max (exclusive).</returns>
        public int? randInt(int min, int max)
        {
            Random rnd = new Random();
            return rnd.Next(min, max);
        }
        /// <summary>
        /// Generates a random double between the specified minimum and maxiumus values
        /// </summary>
        /// <param name="min">The inclusive lower bound of the random number returned.</param>
        /// <param name="max">The exclusive upper bound of the random number returned.</param>
        /// <returns>A random double between min and max</returns>
        public double? randFloat(double min,double max) {
            Random random = new Random();
            return random.NextDouble() * (max - min) + min;
        }

        /// <summary>
        /// Gets the value of Pi (π).
        /// </summary>
        /// <returns>The value of Pi.</returns>
        public double getPI()
        {
            return System.Math.PI;
        }

        /// <summary>
        /// Returns the absolute value of a specified number.
        /// </summary>
        /// <param name="value">The number to calculate the absolute value for.</param>
        /// <returns>The absolute value of the specified number.</returns>
        public double abs(double value)
        {
            return System.Math.Abs(value);
        }

        /// <summary>
        /// Returns the larger of two specified numbers.
        /// </summary>
        /// <param name="a">The first number to compare.</param>
        /// <param name="b">The second number to compare.</param>
        /// <returns>The larger of a and b.</returns>
        public double max(double a, double b)
        {
            return System.Math.Max(a, b);
        }

        /// <summary>
        /// Returns the smaller of two specified numbers.
        /// </summary>
        /// <param name="a">The first number to compare.</param>
        /// <param name="b">The second number to compare.</param>
        /// <returns>The smaller of a and b.</returns>
        public double min(double a, double b)
        {
            return System.Math.Min(a, b);
        }

        /// <summary>
        /// Returns a specified number raised to the specified power.
        /// </summary>
        /// <param name="x">The base number.</param>
        /// <param name="y">The exponent.</param>
        /// <returns>The result of raising x to the power of y.</returns>
        public double pow(double x, double y)
        {
            return System.Math.Pow(x, y);
        }

        /// <summary>
        /// Returns the square root of a specified number.
        /// </summary>
        /// <param name="value">The number whose square root is to be found.</param>
        /// <returns>The square root of the specified number.</returns>
        public double sqrt(double value)
        {
            return System.Math.Sqrt(value);
        }

        /// <summary>
        /// Returns the sine of the specified angle.
        /// </summary>
        /// <param name="angle">The angle, in radians, for which to calculate the sine.</param>
        /// <returns>The sine of the specified angle.</returns>
        public double sin(double angle)
        {
            return System.Math.Sin(angle);
        }

        /// <summary>
        /// Returns the cosine of the specified angle.
        /// </summary>
        /// <param name="angle">The angle, in radians, for which to calculate the cosine.</param>
        /// <returns>The cosine of the specified angle.</returns>
        public double cos(double angle)
        {
            return System.Math.Cos(angle);
        }

        /// <summary>
        /// Returns the tangent of the specified angle.
        /// </summary>
        /// <param name="angle">The angle, in radians, for which to calculate the tangent.</param>
        /// <returns>The tangent of the specified angle.</returns>
        public double tan(double angle)
        {
            return System.Math.Tan(angle);
        }

        /// <summary>
        /// Returns the angle whose sine is the specified number.
        /// </summary>
        /// <param name="value">A number representing the sine of the angle.</param>
        /// <returns>The angle in radians whose sine is the specified number.</returns>
        public double asin(double value)
        {
            return System.Math.Asin(value);
        }

        /// <summary>
        /// Returns the angle whose cosine is the specified number.
        /// </summary>
        /// <param name="value">A number representing the cosine of the angle.</param>
        /// <returns>The angle in radians whose cosine is the specified number.</returns>
        public double acos(double value)
        {
            return System.Math.Acos(value);
        }

        /// <summary>
        /// Returns the angle whose tangent is the specified number.
        /// </summary>
        /// <param name="value">A number representing the tangent of the angle.</param>
        /// <returns>The angle in radians whose tangent is the specified number.</returns>
        public double atan(double value)
        {
            return System.Math.Atan(value);
        }

        /// <summary>
        /// Rounds a specified number to the nearest whole number.
        /// </summary>
        /// <param name="value">The number to round.</param>
        /// <returns>The closest integer to the specified number.</returns>
        public double round(double value)
        {
            return System.Math.Round(value);
        }

        /// <summary>
        /// Rounds a number up to the nearest whole number.
        /// </summary>
        /// <param name="value">The number to round up.</param>
        /// <returns>The smallest integer greater than or equal to the specified number.</returns>
        public double ceiling(double value)
        {
            return System.Math.Ceiling(value);
        }

        /// <summary>
        /// Generates a random double between the specified minimum and maximum values.
        /// </summary>
        /// <param name="min">The inclusive lower bound of the random number returned.</param>
        /// <param name="max">The exclusive upper bound of the random number returned.</param>
        /// <returns>A random double between min (inclusive) and max (exclusive).</returns>
        public double randDouble(double min, double max)
        {
            Random rnd = new Random();
            return rnd.NextDouble() * (max - min) + min;
        }
    }
}
