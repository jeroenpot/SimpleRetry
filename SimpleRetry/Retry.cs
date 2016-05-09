using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleRetry
{
    public static class Retry
    {
        public static void Execute(Action action, TimeSpan retryInterval, int retryCount, Action<Exception> executeOnEveryException = null, Action<Exception> executeBeforeFinalException = null, params Type[] exceptionTypesToHandle)
        {
            Execute<object>(() =>
            {
                action();
                return null;
            }, retryInterval, retryCount, executeOnEveryException, executeBeforeFinalException, exceptionTypesToHandle);
        }


        public static T Execute<T>(Func<T> action, TimeSpan retryInterval, int retryCount, Action<Exception> executeOnEveryException = null, Action<Exception> executeBeforeFinalException = null, params Type[] exceptionTypesToHandle)
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
                    executeOnEveryException?.Invoke(ex);

                    if (exceptionTypesToHandle != null && exceptionTypesToHandle.Any() && !exceptionTypesToHandle.Any(type => ex.IsOfTypeOrInherits(type)))
                    {
                        throw;
                    }

                    exceptions.Add(ex);
                    if (retry < retryCount)
                    {
                        Thread.Sleep(retryInterval);
                    }
                }
            }

            var exceptionToThrow = new AggregateException(exceptions);
            executeBeforeFinalException?.Invoke(exceptionToThrow);
            throw exceptionToThrow;
        }

        public static async Task ExecuteAsync(Action action, TimeSpan retryInterval, int retryCount, Func<Exception, Task> executeOnEveryException = null, Func<Exception, Task> executeBeforeFinalException = null, params Type[] exceptionTypesToHandle)
        {
            var task = Task<object>.Factory.StartNew(delegate
            {
                action();
                return null;
            });

            await ExecuteAsync(async () => await task, retryInterval, retryCount, executeOnEveryException, executeBeforeFinalException, exceptionTypesToHandle);
        }

        public static async Task<T> ExecuteAsync<T>(Func<Task<T>> action, TimeSpan retryInterval, int retryCount, Func<Exception, Task> executeOnEveryException = null, Func<Exception, Task> executeBeforeFinalException = null, params Type[] exceptionTypesToHandle)
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
                    return await action();
                }
                catch (Exception ex)
                {
                    if (executeOnEveryException != null)
                    {
                        await executeOnEveryException(ex);
                    }
                    

                    if (exceptionTypesToHandle != null && exceptionTypesToHandle.Any() && !exceptionTypesToHandle.Any(type => ex.IsOfTypeOrInherits(type)))
                    {
                        throw;
                    }

                    exceptions.Add(ex);
                    if (retry < retryCount)
                    {
                        Thread.Sleep(retryInterval);
                    }
                }
            }

            var exceptionToThrow = new AggregateException(exceptions);
            if (executeBeforeFinalException != null)
            {
                executeBeforeFinalException?.Invoke(exceptionToThrow);
            }
            throw exceptionToThrow;
        }

        private static bool IsOfTypeOrInherits(this object obj, Type type)
        {
            var objectType = obj.GetType();

            while (true)
            {
                if (objectType == type)
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