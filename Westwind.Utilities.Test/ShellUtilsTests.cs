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
            int result = ShellUtils.ExecuteProcess("ipconfig.exe", null, 3000, out string output,
                windowStyle: ProcessWindowStyle.Normal);

            Assert.IsTrue(result == 0, "Process should exit with 0");
            Assert.IsNotNull(output,"Output should not be empty");
        }

        [TestMethod]
        public void ExecuteProcess()
        {
            int result = ShellUtils.ExecuteProcess("ipconfig.exe", null, 3000, ProcessWindowStyle.Normal);
            Assert.IsTrue(result == 0, "Process should exit with 0");
        }
    }
}

