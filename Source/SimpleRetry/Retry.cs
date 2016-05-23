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

            ValidateParameters(retryCount, exceptionTypesToHandle);

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
            ValidateParameters(retryCount, exceptionTypesToHandle);
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
                        await Task.Delay(retryInterval);
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

        public static void ValidateParameters(int retryCount, ICollection<Type> types)
        {
            ValidateRetryCountParameter(retryCount);
            ValidateTypeParameter(types);
        }

        private static void ValidateRetryCountParameter(int retryCount)
        {
            if (retryCount < 0)
            {
                throw new ArgumentException($"Retry count cannot be lower then zero. Given value was {retryCount}");
            }
        }

        private static void ValidateTypeParameter(ICollection<Type> exceptionTypesToHandle)
        {
            if (exceptionTypesToHandle != null && exceptionTypesToHandle.Any())
            {
                foreach (Type type in exceptionTypesToHandle)
                {
                    if (type.IsAssignableFrom(typeof(Exception)))
                    {
                        // It's an exception!
                    }
                }
                var typesThatAreNotExcpetions = exceptionTypesToHandle.Where(type => IsOfTypeOrInHerits(type, typeof(Exception)) == false).ToList();
                if (typesThatAreNotExcpetions.Any())
                {
                    string notExceptionsMessage = string.Join(", ", typesThatAreNotExcpetions.Select(t => t.Name));
                    throw new SimpleRetryArgumentException(
                        $"All types should be of base type exception. Found {typesThatAreNotExcpetions.Count} type(s) that are not exceptions: {notExceptionsMessage}",
                        nameof(exceptionTypesToHandle));
                }
            }
        }

        private static bool IsOfTypeOrInherits(this object obj, Type type)
        {
            var objectType = obj.GetType();

            return IsOfTypeOrInHerits(objectType, type);
        }

        private static bool IsOfTypeOrInHerits(Type source, Type target)
        {
            while (true)
            {
                if (source == target)
                {
                    return true;
                }
                if ((source == source.BaseType) || (source.BaseType == null))
                {
                    return false;
                }
                source = source.BaseType;
            }
        }
    }
}