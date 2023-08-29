
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Westwind.Utilities.Test
{
    [TestClass]
    public class WindowsUtilsTests
    {
        [TestMethod]
        public void GetDotnetVersionTest()
        {
            var version = WindowsUtils.GetDotnetVersion();
            Console.WriteLine(version);
#if NETFULL
            StringAssert.StartsWith(version, "4.");            
#else
            StringAssert.StartsWith(version, ".NET");            
#endif
        }

        [TestMethod]
        public void GetWindowsVersionTest()
        {
            var version = WindowsUtils.GetWindowsVersion();
            Console.WriteLine(version);


            StringAssert.StartsWith(version, "10.");
        }
    }
}