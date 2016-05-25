Simple Retry
==================
Retry pattern for .NET
Features:
- Supports sync and async
- Specify time between retries
- Target specific exceptions
- Specify actions to execute when exceptions occur
 
Build status| Coverage Status| NuGet downloads
----------- | -------------- | --------------- 
[![Build status](https://ci.appveyor.com/api/projects/status/h0vo52hogp69ju2t?svg=true)](https://ci.appveyor.com/project/jeroenpot/simpleretry)|[![Coverage Status](https://coveralls.io/repos/github/jeroenpot/SimpleRetry/badge.svg?branch=master)](https://coveralls.io/github/jeroenpot/SimpleRetry?branch=master)|[![NuGet downloads](https://img.shields.io/nuget/v/simpleretry.svg?maxAge=2592000)](https://www.nuget.org/packages/simpleretry/)


##Usage##

Install via package console.

```sh
Install-Package SimpleRetry
```

A set of examples can be found here:
https://github.com/jeroenpot/SimpleRetry/blob/master/Source/SimpleRetry.UnitTests/SampleUsage.cs

```cs
public class SampleUsage
    {
        public void RetryWithoutReturnValue()
        {
            // Execute the DoWork a maximum of 3 times (once + two retries)
            Retry.Execute(() => DummyMethods.DoWork("Hello world"), TimeSpan.FromMilliseconds(100), 2);
        }

        public void RetryWithReturnValue()
        {
            // Execute the DoWork Once (once + no retries)
            int returnValue = Retry.Execute(() => DummyMethods.DoWorkAndReturn("Hello world"), TimeSpan.FromMilliseconds(100), 0);
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
            await Retry.ExecuteAsync(async () => await DummyMethods.DoWorkAsync(), TimeSpan.FromMilliseconds(100), 2);
        }

        public async Task RetryAsyncWithAllFeatures()
        {
            // Execute the DoWorkAsync
            await Retry.ExecuteAsync(async () => await DummyMethods.DoWorkAsync(), TimeSpan.FromMilliseconds(100), 2, DummyMethods.ExecuteOnExceptionAsync, DummyMethods.ExecuteOnExceptionAsync, typeof(ArgumentException));
        }
    }
```

## License

The MIT License (MIT)

Copyright (c) 2016 Jeroen Pot

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
