namespace VSharpLib
{
    using System;
    using System.Collections.Generic;
    using VSharp;

    [Module]
    class Error
    {
        /// <summary>
        /// Throws a general exception with the specified reason.
        /// </summary>
        /// <param name="reason">The reason for the exception.</param>
        public void Throw(object? reason)
        {
            throw new Exception(reason?.ToString());
        }

        /// <summary>
        /// Throws an exception if the provided object is null.
        /// </summary>
        /// <param name="obj">The object to check for null.</param>
        /// <param name="message">The exception message if the object is null.</param>
        public void ThrowIfNull(object? obj, string message = "Object cannot be null.")
        {
            if (obj == null)
            {
                throw new ArgumentNullException(message);
            }
        }

        /// <summary>
        /// Throws an exception if the provided collection is empty.
        /// </summary>
        /// <param name="collection">The collection to check for emptiness.</param>
        /// <param name="message">The exception message if the collection is empty.</param>
        public void ThrowIfEmpty(IEnumerable<object> collection, string message = "Collection cannot be empty.")
        {
            if (!collection.GetEnumerator().MoveNext())
            {
                throw new ArgumentException(message);
            }
        }

        /// <summary>
        /// Throws a custom exception with a specified name and message.
        /// </summary>
        /// <param name="exceptionName">The type of exception to throw (e.g., ArgumentException).</param>
        /// <param name="message">The message for the exception.</param>
        public void ThrowCustom(string exceptionName, string message)
        {
            switch (exceptionName)
            {
                case "ArgumentException":
                    throw new ArgumentException(message);
                case "InvalidOperationException":
                    throw new InvalidOperationException(message);
                case "NullReferenceException":
                    throw new NullReferenceException(message);
                default:
                    throw new Exception(message);
            }
        }


        /// <summary>
        /// Logs an error message to the console.
        /// </summary>
        /// <param name="errorMessage">The error message to log.</param>
        public void LogError(string errorMessage)
        {
            Console.WriteLine($"Error: {errorMessage}");
        }
    }
}
