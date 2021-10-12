using Moq;
using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AsyncVerifyForMoq
{
    /// <summary>
    /// Allows to do a async Verify() that wait for the Verification expression to become verified within a given timeout.
    /// </summary>
    public static class MoqAsyncVerifyExtensions
    {
        const int GRANULARITY = 100;

        /// <summary>
        /// Prevents having multiple implementations for the timeout handling.
        /// </summary>
        /// <param name="action">The verification action that is called.</param>
        /// <param name="timeout">The timeout within the Verification should be successful.</param>
        /// <returns></returns>
        public async static Task SomeVerifyWithTimeout(Action action, TimeSpan timeout)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            Exception verificationException = null;

            while (stopWatch.Elapsed < timeout)
            {
                try
                {
                    action();
                    return;
                }
                catch (MockException mockException)
                {
                    verificationException = mockException;
                    await Task.Delay(GRANULARITY);
                }
            }
            throw new TimeoutException($"Verification expression always failed within given timespan of {timeout}.", verificationException);
        }

        /// <summary>
        /// Extension for Mock<typeparamref name="T"/> allowing a asynchronous Verify() that should be successful in a specified time period.
        /// </summary>
        /// <typeparam name="T">Type to mock, which can be an interface, a class, or a delegate.</typeparam>
        /// <typeparam name="TExpressionResult">Result type of the expression used for verification.</typeparam>
        /// <param name="mock">The mock that is used as this for this extension</param>
        /// <param name="expression">Expression to verify.</param>
        /// <param name="times">The number of times a method is expected to be called as in the regular Verify() call.</param>
        /// <param name="timeout">The timeout within the verify should be successful.</param>
        /// <returns>A task to wait on. The task will fail with a TimeoutException in case the Verify() was not successful within the given timeout.</returns>
        public async static Task AsyncVerify<T, TExpressionResult>(this Mock<T> mock, Expression<Func<T, TExpressionResult>> expression, Times times, TimeSpan timeout) where T : class
        {
            await SomeVerifyWithTimeout(() => mock.Verify(expression, times), timeout);
        }

        /// <summary>
        /// Extension for Mock<typeparamref name="T"/> allowing a asynchronous Verify() that should be successful in a specified time period.
        /// Variant for expressions without a return value and one parameter.
        /// </summary>
        /// <typeparam name="T">Type to mock, which can be an interface, a class, or a delegate.</typeparam>
        /// <param name="mock">The mock that is used as this for this extension</param>
        /// <param name="expression">Expression to verify.</param>
        /// <param name="times">The number of times a method is expected to be called as in the regular Verify() call.</param>
        /// <param name="timeout">The timeout within the verify should be successful.</param>
        /// <returns>A task to wait on. The task will fail with a TimeoutException in case the Verify() was not successful within the given timeout.</returns>
        public async static Task AsyncVerify<T>(this Mock<T> mock, Expression<Action<T>> expression, Times times, TimeSpan timeout) where T : class
        {
            await SomeVerifyWithTimeout(() => mock.Verify(expression, times), timeout);
        }
    }
}
