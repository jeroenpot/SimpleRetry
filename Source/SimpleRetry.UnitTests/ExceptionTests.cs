using System.Reflection;
using AutoTest.Exceptions;
using NUnit.Framework;

namespace SimpleRetry.UnitTests
{
    [TestFixture]
    public class ExceptionTests
    {
        [Test]
        public void TestAllExceptions()
        {
            ExceptionTester.TestAllExceptions(Assembly.GetAssembly(typeof(SimpleRetryArgumentException)));
        }
    }
}
