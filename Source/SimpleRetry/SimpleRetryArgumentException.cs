using System;
using System.Runtime.Serialization;

namespace SimpleRetry
{
    [Serializable]
    public class SimpleRetryArgumentException : ArgumentException
    {
        public SimpleRetryArgumentException()
        {
        }

        public SimpleRetryArgumentException(string message) : base(message)
        {
        }

        public SimpleRetryArgumentException(string message, string parameter) : base(message, parameter)
        {
        }

        public SimpleRetryArgumentException(string message, Exception inner) : base(message, inner)
        {
        }

        protected SimpleRetryArgumentException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
