using System;

namespace SimpleRetry
{
    /// <summary>
    /// Argument Exception used in SimpleRetry library.
    /// </summary>
    /// <seealso cref="System.ArgumentException" />
    public class SimpleRetryArgumentException : ArgumentException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleRetryArgumentException"/> class.
        /// </summary>
        public SimpleRetryArgumentException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleRetryArgumentException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public SimpleRetryArgumentException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleRetryArgumentException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="parameter">The parameter.</param>
        public SimpleRetryArgumentException(string message, string parameter) : base(message, parameter)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleRetryArgumentException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public SimpleRetryArgumentException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
