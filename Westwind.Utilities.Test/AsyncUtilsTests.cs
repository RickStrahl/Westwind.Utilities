using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Westwind.Utilities.Test
{
    [TestClass]
    public class AsyncUtilsTests
    {
        [TestMethod]
        public async Task TimeoutAfter2SecondsTest()
        {
            var taskToComplete = Task.Run(async () =>
            {
                await Task.Delay(7000);
            });

            bool isComplete = await taskToComplete.Timeout(2000);

            // should time out after 2 seconds
            Assert.IsFalse(isComplete, "Task did not complete in time");
        }

        [TestMethod]
        public async Task SucceedTimeoutAfter2SecondsTest()
        {
            var taskToComplete = DoSomething(1000);

            bool isComplete = false;
            try
            {
                isComplete = await taskToComplete.Timeout(2000);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            // should time out after 2 seconds
            Assert.IsTrue(isComplete, "Task did not complete in time");

            // Retrieve task result
            int result = taskToComplete.Result;
            Assert.AreEqual(result, 100);
        }

        [TestMethod]
        public async Task ExceptionTimeoutAfter2SecondsTest()
        {
            var taskToComplete = DoThrow(1000);

            bool isComplete = false;
            try
            {
                // Shouldn't timeout, but throw an exception
                isComplete = await taskToComplete.Timeout(2000);
            }
            catch (Exception ex)
            {
                // regular exception handling
                Console.WriteLine(ex.Message);  // message here
            }

                        
            Assert.IsTrue(taskToComplete.IsFaulted, "Task threw an exception: " + taskToComplete.Exception.GetBaseException().Message);

            // Or we can pull the exception off the task (aggregate exception so we need base Exception)
            Console.WriteLine(taskToComplete.Exception.GetBaseException().Message);
        }


#if NETCORE
        [TestMethod]
        public async Task TimeoutWithResultAfter2SecondsTest()
        {
            var taskToComplete = DoSomething(10_000);

            // returns int - 100 on success and 0 (default) on failure
            var result = await taskToComplete.TimeoutWithResult<int>(2000);

            // fails: should return 0. timed out after 2 seconds
            Assert.IsTrue(result == 0, "Task did not complete in time");                        
        }

        [TestMethod]
        public async Task NoTimeoutWithResultAfter2SecondsTest()
        {            
            var result = await DoSomething(1500).TimeoutWithResult<int>(2000);

            // No timeout since task completes in 1.5 seconds  
            Assert.IsTrue(result==100, "Task did not complete in time");
        }

#endif


        private async Task<int> DoSomething(int timeout)
        {
            await Task.Delay(timeout);
       
            return 100;
        }
        private async Task<int> DoThrow(int timeout)
        {
            await Task.Delay(timeout);
            throw new Exception("Exception thrown in task method DoThrow");
            return 100;
        }
    }

}
