using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SimpleRetry
{
    public static class Retry
    {
        public static void Execute(Action action, TimeSpan retryInterval, int retryCount, Action executeOnException = null, params Type[] exceptionTypes)
        {
            Execute<object>(() =>
            {
                action();
                return null;
            }, retryInterval, retryCount, executeOnException, exceptionTypes);
        }

        public static T Execute<T>(Func<T> action, TimeSpan retryInterval, int retryCount, Action executeOnException = null, params Type[] exceptionTypes)
        {
            if (retryCount < 0)
            {
                throw new ArgumentException($"Retry count cannot be lower then zero. Given value was {retryCount}");
            }

            var exceptions = new List<Exception>();
            for (int retry = 0; retry < retryCount + 1; retry++)
            {
                try
                {
                    return action();
                }
                catch (Exception ex)
                {

                    if (exceptionTypes != null && exceptionTypes.Any() && !exceptionTypes.Any(type => ex.IsOfTypeOrInherits(type)))
                    {
                        throw;
                    }

                    // ReSharper disable once UseNullPropagation
                    if (executeOnException != null)
                    {
                        executeOnException();
                    }

                    exceptions.Add(ex);
                    if (retry < retryCount)
                    {
                        Thread.Sleep(retryInterval);
                    }
                }
            }

            throw new AggregateException(exceptions);
        }

        private static bool IsOfTypeOrInherits(this object obj, Type type)
        {
            var objectType = obj.GetType();

            while (true)
            {
                if (objectType.Equals(type))
                {
                    return true;
                }
                if ((objectType == objectType.BaseType) || (objectType.BaseType == null))
                {
                    return false;
                }
                objectType = objectType.BaseType;
            }
        }
    }
}