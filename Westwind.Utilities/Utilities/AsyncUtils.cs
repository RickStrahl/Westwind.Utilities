﻿using System;
using System.Collections.Generic;
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
            t.ContinueWith( (tsk) => del?.Invoke(tsk.Exception), TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
