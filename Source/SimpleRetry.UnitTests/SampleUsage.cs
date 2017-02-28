
using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace SimpleRetry.UnitTests
{
    public class SampleUsage
    {
        public void RetryWithoutReturnValue()
        {
            // Execute the DoWork a maximum of 3 times (once + two retries)
            Retry.Execute(() =>
            {
                // Happy flow
            }, 
            TimeSpan.FromMilliseconds(100), 2);
        }

        public void RetryWithReturnValue()
        {
            // Execute the DoWork Once (once + no retries)
            int returnValue = Retry.Execute(() =>
            {
                // Happy flow
                return 1;
            }, TimeSpan.FromMilliseconds(100), 0);
        }

        public void RetryOnlySpecificException()
        {
            // Execute the DoWork and only retry with an exception that is of (base)type DbException 
            Retry.Execute(() => DummyMethods.DoWork("Hello world"), TimeSpan.FromMilliseconds(100), 2,
                exceptionTypesToHandle: typeof(DbException));

        }

        public void RetryOnlySpecificExceptions()
        {
            // Execute the DoWork and only retry with an exception that is of (base)type DbException 
            Retry.Execute(() => DummyMethods.DoWork("Hello world"), TimeSpan.FromMilliseconds(100), 2,
                exceptionTypesToHandle: new[] { typeof(ArgumentException), typeof(ArgumentOutOfRangeException) });

        }

        public void ExecuteMethodOnEachException()
        {

            // Execute the DoWork and only Execute a method after each exception is thrown.
            Retry.Execute(() => DummyMethods.DoWork("Hello world"), TimeSpan.FromMilliseconds(100), 2,
            executeOnEveryException: DummyMethods.ExecuteOnEveryException);
        }

        public void ExecuteMethodBeforeFinalExceptionIsThrown()
        {
            // Execute the DoWork and only Execute a method before the final AggregateException is thrown
            Retry.Execute(() => DummyMethods.DoWork("Hello world"), TimeSpan.FromMilliseconds(100), 2,
                executeBeforeFinalException: DummyMethods.ExecuteWhenMaxRetriesReachedBeforeExceptionIsThrown);
        }

        public void AllFeatures()
        {
            // All retry features in one method:
            Retry.Execute(() => DummyMethods.DoWork("Hello world"), TimeSpan.FromMilliseconds(100), 2,
                DummyMethods.ExecuteOnEveryException, DummyMethods.ExecuteWhenMaxRetriesReachedBeforeExceptionIsThrown,
                typeof(ArgumentException), typeof(DbException));
        }

        public async Task RetryAsync()
        {
            // Execute the DoWorkAsync
            await Retry.ExecuteAsync(async () =>
            {
                await DummyMethods.DoWorkAsync();
            }, TimeSpan.FromMilliseconds(100), 2);
        }

        public async Task RetryAsyncWithReturn()
        {
            // Execute the DoWorkAsync
            int i = await Retry.ExecuteAsync(async () =>
            {
                return await DummyMethods.DoWorkAsyncAndReturn();
            }, TimeSpan.FromMilliseconds(100), 2);
        }

        public async Task RetryAsyncWithAllFeatures()
        {
            // Execute the DoWorkAsync
            await Retry.ExecuteAsync(async () => await DummyMethods.DoWorkAsync(), TimeSpan.FromMilliseconds(100), 2, DummyMethods.ExecuteOnExceptionAsync, DummyMethods.ExecuteOnExceptionAsync, typeof(ArgumentException));
        }
    }


    public static class DummyMethods
    {
        public static void DoWork(string value)
        {
        }

        public static int DoWorkAndReturn(string value)
        {
            return 1;
        }

        public static void ExecuteOnEveryException(Exception exception)
        {
            // Could do some warning logging here
        }

        public static void ExecuteWhenMaxRetriesReachedBeforeExceptionIsThrown(Exception exception)
        {
            // Could do some Error logging here
        }

        public static async Task DoWorkAsync()
        {
            await Task.Delay(1);
        }

        public static async Task<int> DoWorkAsyncAndReturn()
        {
            await Task.Delay(1);
            return 1;
        }

        public static async Task ExecuteOnExceptionAsync(Exception exception)
        {
            await Task.Delay(1);
        }
    }
}
