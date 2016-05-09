using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace SimpleRetry.UnitTests
{

    [TestFixture]
    public class RetryTests
    {
        private int _timesCalled;

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
            stopwatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(1499); // Weird, should be 1500, but is sometimes faster (1499)
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
            Action action = () => Retry.Execute(() => AddOne(100), TimeSpan.FromMilliseconds(1), 10, null, null, typeof(DivideByZeroException));
            action.ShouldThrow<NotSupportedException>();
        }

        [Test]
        public void should_catch_child_exception()
        {
            Retry.Execute(() => AddOne(2), TimeSpan.FromMilliseconds(100), 1, null, null, typeof(SystemException));
            _timesCalled.Should().Be(2);
        }

        [Test]
        public void should_call_action_when_each_exception_occures()
        {
            var logger = A.Fake<ILog>();

            Retry.Execute(() => AddOne(5), TimeSpan.FromMilliseconds(100), 100, exception => logger.Warn(exception), exception => logger.Error(exception));

            A.CallTo(() => logger.Warn(A<Exception>.That.Matches(x => x.GetType() == typeof(NotSupportedException)))).MustHaveHappened(Repeated.Exactly.Times(4));
            A.CallTo(() => logger.Error(A<Exception>._)).MustHaveHappened(Repeated.Never);
        }

        [Test]
        public void should_call_action_when_final_exception_occures()
        {
            var logger = A.Fake<ILog>();
            Action action = () => Retry.Execute(() => AddOne(100), TimeSpan.FromMilliseconds(1), 9, exception => logger.Warn(exception), exception => logger.Error(exception));
            action.ShouldThrow<NotSupportedException>();
            A.CallTo(() => logger.Error(A<Exception>.That.Matches(x => x.GetType() == typeof(AggregateException)))).MustHaveHappened(Repeated.Exactly.Times(1));
            A.CallTo(() => logger.Warn(A<Exception>.That.Matches(x => x.GetType() == typeof(NotSupportedException)))).MustHaveHappened(Repeated.Exactly.Times(10));
        }

        [Test]
        public void should_call_action_and_return_value()
        {
            int result = Retry.Execute(() => AddOneTime(2), TimeSpan.FromMilliseconds(100), 1);

            result.Should().Be(1);
            _timesCalled.Should().Be(2);
        }

        [Test]
        public async Task should_execute_once_and_then_retry_once_async()
        {
            await Retry.ExecuteAsync(async () => await AddOneAsync(2), TimeSpan.FromMilliseconds(100), 1);
            //_timesCalled.Should().Be(2);
        }

        [Test]
        public async Task should_execute_once_and_then_retry_once_async_and_return()
        {
            int returnValue = await Retry.ExecuteAsync(async () => await AddOneTimeAsync(2), TimeSpan.FromMilliseconds(100), 1);
            returnValue.Should().Be(1);
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

        public async Task AddOneAsync(int stopThrowingExceptionAt)
        {
            _timesCalled++;
            await Task.Delay(1);
            if (_timesCalled < stopThrowingExceptionAt)
            {
                throw new NotSupportedException();
            }
        }

        public async Task<int> AddOneTimeAsync(int stopThrowingExceptionAt)
        {
            _timesCalled++;
            await Task.Delay(1);
            if (_timesCalled < stopThrowingExceptionAt)
            {
                throw new NotSupportedException();
            }

            return 1;
        }

        public int AddOneTime(int stopThrowingExceptionAt)
        {
            _timesCalled++;
            if (_timesCalled < stopThrowingExceptionAt)
            {
                throw new NotSupportedException();
            }

            return 1;
        }
    }

    public interface ILog
    {
        void Warn(Exception exception);
        void Error(Exception exception);
    }
}
