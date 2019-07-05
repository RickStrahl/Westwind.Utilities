using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Westwind.Utilities.Test
{
    [TestClass]
    public class ShellUtilsTests
    {
        [TestMethod]
        public void ExecuteProcessAndGetOutput()
        {
            int result = ShellUtils.ExecuteProcess("ipconfig.exe", null, 3000,
                out StringBuilder output);

            Assert.IsTrue(result == 0, "Process should exit with 0");
            Assert.IsNotNull(output.Length > 0,"Output should not be empty");

            Console.WriteLine("Complete output:");
            Console.WriteLine("----------------");
            Console.WriteLine(output.ToString());
        }

        [TestMethod]
        public void ExecuteProcessAndCaptureOutputAction()
        {
            var action = new Action<string>((s) => Console.WriteLine(s));

            Console.WriteLine("Output Captured from Action:");
            Console.WriteLine("----------------------------");

            int result = ShellUtils.ExecuteProcess("ipconfig.exe", null,
                3000,
                action);

            Assert.IsTrue(result == 0, "Process should exit with 0");
        }

        [TestMethod]
        public void ExecuteProcess()
        {
            int result = ShellUtils.ExecuteProcess("ipconfig.exe", null, 3000, ProcessWindowStyle.Normal);
            Assert.IsTrue(result == 0, "Process should exit with 0");
        }
    }
}

