using System;
using System.Diagnostics;
using FluentAssertions;
using NUnit.Framework;

namespace SimpleRetry.UnitTests
{

    [TestFixture]
    public class RetryTests
    {

        private static int _timesCalled;

        [SetUp]
        public void InitTest()
        {
            _timesCalled = 0;
        }

        [Test]
        public void should_execute_once_and_then_retry_once()
        {
            Retry.Execute(() => AddOne(2), TimeSpan.FromMilliseconds(100), 1);
            _timesCalled.Should().Be(2);
        }

        [Test]
        public void should_throw_exception_when_retry_is_reached()
        {
            Action action = () => Retry.Execute(() => AddOne(100), TimeSpan.FromMilliseconds(1), 2);
            action.ShouldThrow<NotSupportedException>();
        }

        [Test]
        public void should_sleep_given_timespan_between_exceptions()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Retry.Execute(() => AddOne(4), TimeSpan.FromMilliseconds(500), 3);

            stopwatch.Stop();
            stopwatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(1500);
            // Do not sleep after last call or before first call.
            stopwatch.ElapsedMilliseconds.Should().BeLessOrEqualTo(2000);
        }

        [Test]
        public void should_throw_exception_when_retry_is_smaller_then_zero()
        {
            Action action = () => Retry.Execute(() => AddOne(100), TimeSpan.FromMilliseconds(1), -1);
            action.ShouldThrow<ArgumentException>();
        }

        [Test]
        public void should_only_catch_given_exceptions()
        {
            Action action = () => Retry.Execute(() => AddOne(100), TimeSpan.FromMilliseconds(1), 10, null, typeof(DivideByZeroException));
            action.ShouldThrow<NotSupportedException>();
        }

        [Test]
        public void should_catch_child_exception()
        {
            Retry.Execute(() => AddOne(2), TimeSpan.FromMilliseconds(100), 1, null, typeof(SystemException));
            _timesCalled.Should().Be(2);
        }


        public void AddOne(int stopThrowingExceptionAt)
        {
            _timesCalled++;
            if (_timesCalled < stopThrowingExceptionAt)
            {
                throw new NotSupportedException();
            }
        }
    }
}
