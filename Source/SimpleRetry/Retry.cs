using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SimpleRetry
{
    /// <summary>
    /// Static class for Retrying
    /// </summary>
    public static class Retry
    {
        /// <summary>
        /// Executes the specified action.
        /// </summary>
        /// <example>
        /// This sample shows how to call the method.
        /// <code>
        /// Execute(() =>
        /// {
        ///     // happy flow
        /// }, TimeSpan.FromMilliseconds(100), 2);
        /// </code>
        /// </example>
        /// <param name="action">The action.</param>
        /// <param name="retryInterval">The retry interval.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="executeOnEveryException">The execute on every exception.</param>
        /// <param name="executeBeforeFinalException">The execute before final exception.</param>
        /// <param name="exceptionTypesToHandle">The exception types to handle.</param>
        public static void Execute(Action action, TimeSpan retryInterval, int retryCount, Action<Exception> executeOnEveryException = null, Action<Exception> executeBeforeFinalException = null, params Type[] exceptionTypesToHandle)
        {
            Execute<object>(() =>
            {
                action();
                return null;
            }, retryInterval, retryCount, executeOnEveryException, executeBeforeFinalException, exceptionTypesToHandle);
        }


        /// <summary>
        /// Executes the specified action.
        /// </summary>
        /// <example>
        /// This sample shows how to call the method.
        /// <code>
        /// int returnValue = Retry.Execute(() =>
        /// {
        ///     // Happy flow
        ///     return 1;
        /// }, TimeSpan.FromMilliseconds(100), 0);
        /// </code>
        /// </example>
        /// <typeparam name="T"></typeparam>
        /// <param name="action">The action.</param>
        /// <param name="retryInterval">The retry interval.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="executeOnEveryException">The execute on every exception.</param>
        /// <param name="executeBeforeFinalException">The execute before final exception.</param>
        /// <param name="exceptionTypesToHandle">The exception types to handle.</param>
        /// <returns></returns>
        public static T Execute<T>(Func<T> action, TimeSpan retryInterval, int retryCount, Action<Exception> executeOnEveryException = null, Action<Exception> executeBeforeFinalException = null, params Type[] exceptionTypesToHandle)
        {
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
                        Task.Delay(retryInterval).Wait();
                    }
                }
            }

            var exceptionToThrow = new AggregateException(exceptions);
            executeBeforeFinalException?.Invoke(exceptionToThrow);
            throw exceptionToThrow;
        }

        /// <summary>
        /// Executes the action asynchronous.
        /// </summary>
        /// <example>
        /// <code>
        /// await Retry.ExecuteAsync(async () =>
        /// {
        ///    // Do work
        /// }, TimeSpan.FromMilliseconds(100), 2);
        /// </code>
        /// </example>
        /// <param name="action">The action.</param>
        /// <param name="retryInterval">The retry interval.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="executeOnEveryException">The execute on every exception.</param>
        /// <param name="executeBeforeFinalException">The execute before final exception.</param>
        /// <param name="exceptionTypesToHandle">The exception types to handle.</param>
        /// <returns></returns>
        public static async Task ExecuteAsync(Action action, TimeSpan retryInterval, int retryCount, Func<Exception, Task> executeOnEveryException = null, Func<Exception, Task> executeBeforeFinalException = null, params Type[] exceptionTypesToHandle)
        {
            var task = Task<object>.Factory.StartNew(delegate
            {
                action();
                return null;
            });

            await ExecuteAsync(async () => await task, retryInterval, retryCount, executeOnEveryException, executeBeforeFinalException, exceptionTypesToHandle);
        }

        /// <summary>
        /// Executes the action asynchronous.
        /// </summary>
        /// <example>
        /// <code>
        /// int i = await Retry.ExecuteAsync(async () =>
        /// {
        ///    // Do work
        ///    return 1;
        /// }, TimeSpan.FromMilliseconds(100), 2);
        /// </code>
        /// </example>
        /// <typeparam name="T"></typeparam>
        /// <param name="action">The action.</param>
        /// <param name="retryInterval">The retry interval.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="executeOnEveryException">The execute on every exception.</param>
        /// <param name="executeBeforeFinalException">The execute before final exception.</param>
        /// <param name="exceptionTypesToHandle">The exception types to handle.</param>
        /// <returns></returns>
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
            executeBeforeFinalException?.Invoke(exceptionToThrow);
            throw exceptionToThrow;
        }

        private static void ValidateParameters(int retryCount, ICollection<Type> types)
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
                var baseType = source.GetTypeInfo().BaseType;
                if ((source == baseType) || (baseType == null))
                {
                    return false;
                }
                source = baseType;
            }
        }
    }
}