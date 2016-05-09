//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.CompilerServices;
//using System.Threading;
//using System.Threading.Tasks;

//namespace SimpleRetry
//{
//    public static class CopyRetry
//    {
//        public static void Execute(Action action, TimeSpan retryInterval, int retryCount, Action<Exception> executeOnEveryException = null, Action<Exception> executeBeforeFinalException = null, params Type[] exceptionTypesToHandle)
//        {
//            Execute<object>(() =>
//            {
//                action();
//                return null;
//            }, retryInterval, retryCount, executeOnEveryException, executeBeforeFinalException, exceptionTypesToHandle);


//        }

//        public static T Execute<T>(Func<T> action, TimeSpan retryInterval, int retryCount, Action<Exception> executeOnEveryException = null, Action<Exception> executeBeforeFinalException = null, params Type[] exceptionTypesToHandle)
//        {
//            Task<T> mainTask = new Task<T>(action);
//            Func<Exception, Task> executeOnEveryExceptionFunc = exception => Task.Run(() =>
//            {
//                executeOnEveryException?.Invoke(exception);
//            });

//            Func<Exception, Task> executeBeforeFinalExceptionFunc = exception => Task.Run(() =>
//            {
//                executeBeforeFinalException?.Invoke(exception);
//            });

//            return ExecuteAsync(() => mainTask, retryInterval, retryCount, executeOnEveryExceptionFunc, executeBeforeFinalExceptionFunc, exceptionTypesToHandle).ConfigureAwait(false).GetAwaiter().GetResult();
//        }

//        public static async Task ExecuteAsync(Action action, TimeSpan retryInterval, int retryCount, Func<Exception, Task> executeOnEveryException = null, Func<Exception, Task> executeBeforeFinalException = null, params Type[] exceptionTypesToHandle)
//        {
//            await ExecuteAsync<object>(() =>
//            {
//                action();
//                return null;
//            }, retryInterval, retryCount, executeOnEveryException, executeBeforeFinalException, exceptionTypesToHandle);
//        }

//        public static async Task<T> ExecuteAsync<T>(Func<Task<T>> action, TimeSpan retryInterval, int retryCount, Func<Exception, Task> executeOnEveryException = null, Func<Exception, Task> executeBeforeFinalException = null, params Type[] exceptionTypesToHandle)
//        {
//            if (retryCount < 0)
//            {
//                throw new ArgumentException($"Retry count cannot be lower then zero. Given value was {retryCount}");
//            }

//            var exceptions = new List<Exception>();
//            for (int retry = 0; retry < retryCount + 1; retry++)
//            {
//                try
//                {
//                    return await action().ConfigureAwait(false);
//                }
//                catch (Exception ex)
//                {
//                    if (executeOnEveryException != null)
//                    {
//                        await executeOnEveryException(ex).ConfigureAwait(false); 
//                    }
                    

//                    if (exceptionTypesToHandle != null && exceptionTypesToHandle.Any() && !exceptionTypesToHandle.Any(type => ex.IsOfTypeOrInherits(type)))
//                    {
//                        throw;
//                    }

//                    exceptions.Add(ex);
//                    if (retry < retryCount)
//                    {
//                        Thread.Sleep(retryInterval);
//                    }
//                }
//            }

//            var exceptionToThrow = new AggregateException(exceptions);
//            if (executeBeforeFinalException != null)
//            {
//                await executeBeforeFinalException(exceptionToThrow).ConfigureAwait(false); ;
//            }
//            throw exceptionToThrow;
//        }

//        private static bool IsOfTypeOrInherits(this object obj, Type type)
//        {
//            var objectType = obj.GetType();

//            while (true)
//            {
//                if (objectType == type)
//                {
//                    return true;
//                }
//                if ((objectType == objectType.BaseType) || (objectType.BaseType == null))
//                {
//                    return false;
//                }
//                objectType = objectType.BaseType;
//            }
//        }

//        static Task<T> AsAsync<T>(Action<Action<T>> target)
//        {
//            var tcs = new TaskCompletionSource<T>();
//            try
//            {
//                target(t => tcs.SetResult(t));
//            }
//            catch (Exception ex)
//            {
//                tcs.SetException(ex);
//            }
//            return tcs.Task;
//        }
//    }
//}