
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Utilities.Windows;

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