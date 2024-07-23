using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Westwind.Utilities
{
    /// <summary>
    /// Helper class to run async methods within a sync process.
    /// Source: https://www.ryadel.com/en/asyncutil-c-helper-class-async-method-sync-result-wait/
    /// </summary>
    public static class AsyncUtils
    {
        private static readonly TaskFactory _taskFactory = new
            TaskFactory(CancellationToken.None,
                TaskCreationOptions.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default);

        /// <summary>
        /// Executes an async Task method which has a void return value synchronously
        /// USAGE: AsyncUtil.RunSync(() => AsyncMethod());
        /// </summary>
        /// <param name="task">Task method to execute</param>
        public static void RunSync(Func<Task> task)
            => _taskFactory
                .StartNew(task)
                .Unwrap()
                .GetAwaiter()
                .GetResult();

        /// <summary>
        /// Executes an async Task method which has a void return value synchronously
        /// USAGE: AsyncUtil.RunSync(() => AsyncMethod());
        /// </summary>
        /// <param name="task">Task method to execute</param>
        public static void RunSync(Func<Task> task,
                    CancellationToken cancellationToken,
                    TaskCreationOptions taskCreation = TaskCreationOptions.None,
                    TaskContinuationOptions taskContinuation = TaskContinuationOptions.None,
                    TaskScheduler taskScheduler = null)
        {
            if (taskScheduler == null)
                taskScheduler = TaskScheduler.Default;

            new TaskFactory(cancellationToken,
                    taskCreation,
                    taskContinuation,
                    taskScheduler)
                .StartNew(task)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Executes an async Task&lt;T&gt; method which has a T return type synchronously
        /// USAGE: T result = AsyncUtil.RunSync(() => AsyncMethod&lt;T&gt;());
        /// </summary>
        /// <typeparam name="TResult">Return Type</typeparam>
        /// <param name="task">Task&lt;T&gt; method to execute</param>
        /// <returns></returns>
        public static TResult RunSync<TResult>(Func<Task<TResult>> task)
            => _taskFactory
                .StartNew(task)
                .Unwrap()
                .GetAwaiter()
                .GetResult();


        /// <summary>
        /// Executes an async Task&lt;T&gt; method which has a T return type synchronously
        /// USAGE: T result = AsyncUtil.RunSync(() => AsyncMethod&lt;T&gt;());
        /// </summary>
        /// <typeparam name="TResult">Return Type</typeparam>
        /// <param name="func">Task&lt;T&gt; method to execute</param>
        /// <returns></returns>
        public static TResult RunSync<TResult>(Func<Task<TResult>> func,
            CancellationToken cancellationToken,
            TaskCreationOptions taskCreation = TaskCreationOptions.None,
            TaskContinuationOptions taskContinuation = TaskContinuationOptions.None,
            TaskScheduler taskScheduler = null)
        {
            if (taskScheduler == null)
                taskScheduler = TaskScheduler.Default;

            return new TaskFactory(cancellationToken,
                    taskCreation,
                    taskContinuation,
                    taskScheduler)
                .StartNew(func, cancellationToken)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }


        /// <summary>
        /// Ensures safe operation of a task without await even if
        /// an execution fails with an exception. This forces the
        /// exception to be cleared unlike a non-continued task.
        /// </summary>
        /// <param name="t">Task Instance</param>
        public static void FireAndForget(this Task t)
        {
            t.ContinueWith(tsk => tsk.Exception,
                TaskContinuationOptions.OnlyOnFaulted);
        }


        /// <summary>
        /// Ensures safe operation of a task without await even if
        /// an execution fails with an exception. This forces the
        /// exception to be cleared unlike a non-continued task.
        /// 
        /// This version allows you to capture and respond to any
        /// exceptions caused by the Task code executing.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="del">Action delegate that receives an Exception parameter you can use to log or otherwise handle (or ignore) any exceptions</param>
        public static void FireAndForget(this Task t, Action<Exception> del)
        {
            t.ContinueWith((tsk) => del?.Invoke(tsk.Exception), TaskContinuationOptions.OnlyOnFaulted);
        }

        /// <summary>
        /// Wait for a task to complete with a timeout
        /// 
        /// Exceptions are thrown as normal if the task fails as
        /// </summary>
        /// <param name="task">Task to wait on</param>
        /// <param name="timeoutMs">timeout to allow</param>
        /// <returns>True if completed in time, false if timed out. If true task is completed and you can read the result</returns>
        public static async Task<bool> Timeout(this Task task, int timeoutMs)
        {
            var completed = await Task.WhenAny(task, Task.Delay(timeoutMs));
            return completed == task;
        }


        /// <summary>
        /// Executes an Action after a delay
        /// </summary>
        /// <remarks>
        /// Code is executed on a background thread, so if UI code is executed
        /// make sure you marshal back to the UI thread using a Dispatcher or Control.Invoke().
        /// </remarks>
        /// <param name="delayMs">delay in Milliseconds</param>
        /// <param name="action">Action to execute after delay</param>
        public static void DelayExecution(int delayMs, Action action, Action<Exception> errorHandler = null)
        {
            var t = new System.Timers.Timer();
            t.Interval = delayMs;
            t.AutoReset = false;
            t.Elapsed += (s, e) =>
            {
                t.Stop();
                try
                {
                    action.Invoke();
                }
                catch(Exception ex)
                {
                    errorHandler?.Invoke(ex);
                }
                t.Dispose();
            };
            t.Start();
        }

        /// <summary>
        /// Executes an Action after a delay with a parameter
        /// </summary>
        /// <remarks>
        /// Code is executed on a background thread, so if UI code is executed
        /// make sure you marshal back to the UI thread using a Dispatcher or Control.Invoke().
        /// </remarks>
        /// <param name="delayMs">delay in Milliseconds</param>
        /// <param name="action">Action to execute after delay</param>
        public static void DelayExecution<T>(int delayMs, Action<T> action, T parm = default, Action<Exception> errorHandler = null)
        {
            var t = new System.Timers.Timer();
            t.Interval = delayMs;
            t.AutoReset = false;
            t.Elapsed += (s, e) =>
            {
                t.Stop();
                try
                {
                    action.Invoke(parm);
                }
                catch(Exception ex)
                {
                    errorHandler?.Invoke(ex);
                }
                t.Dispose();
            };
            t.Start();
            return;
        }
    }

    public static class TaskExtensions
    {


    }
}