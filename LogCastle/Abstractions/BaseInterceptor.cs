using Castle.DynamicProxy;
using System;
using System.Diagnostics;

namespace LogCastle.Abstractions
{
    public abstract class BaseInterceptor : IInterceptor
    {
        private Stopwatch Stopwatch;

        protected BaseInterceptor()
        {
            Stopwatch = new Stopwatch();
        }

        protected virtual void OnBefore(IInvocation invocation)
        {
            Stopwatch.Restart();
        }

        protected virtual void OnAfter(IInvocation invocation)
        {
            Stopwatch.Stop();
        }

        protected virtual void OnException(IInvocation invocation, Exception ex)
        {
            Stopwatch.Stop();
        }

        protected virtual void OnSuccess(IInvocation invocation)
        {
        }

        public void Intercept(IInvocation invocation)
        {
            OnBefore(invocation);

            try
            {
                invocation.Proceed();
                OnSuccess(invocation);
            }
            catch (Exception ex)
            {
                OnException(invocation, ex);
                throw;
            }
            finally
            {
                OnAfter(invocation);
            }
        }
    }
}
