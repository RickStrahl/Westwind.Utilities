using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Utilities;

namespace Westwind.Utilities.Tests
{
    /// <summary>
    /// Summary description for StringUtilsTests
    /// </summary>
    [TestClass]
    public class VersionExtensionsTests
    {
        [TestMethod]
        public void DefaultVersionStringTest()
        {
            // using defaults  2 min, 2 max
            var version = new Version("8.0.0.2");
            string verString = version.FormatVersion();
            Assert.AreEqual(verString, "8.0");


            version = new Version("8.0.1.2");
            verString = version.FormatVersion();
            Assert.AreEqual(verString, "8.0");

            version = new Version("8.3.1.2");
            verString = version.FormatVersion();
            Assert.AreEqual(verString, "8.3");
        }

        [TestMethod]
        public void MinTokenCountProvidedTest()
        {
            // using defaults  2 min, 2 max
            var version = new Version("8.0.0.2");
            string verString = version.FormatVersion(3);
            Assert.AreEqual(verString, "8.0.0");


            version = new Version("8.0.1.2");
            verString = version.FormatVersion(3);
            Assert.AreEqual(verString, "8.0.1");

            version = new Version("8.3.1.2");
            verString = version.FormatVersion(3);
            Assert.AreEqual(verString, "8.3.1");
        }

        [TestMethod]
        public void MaxTokenProvidedTest()
        {
            // using defaults  2 min, 2 max
            var version = new Version("8.0.0.2");
            string verString = version.FormatVersion(3,4);
            Assert.AreEqual(verString, "8.0.0.2");


            version = new Version("8.0.1.0");
            verString = version.FormatVersion(3,4);
            Assert.AreEqual(verString, "8.0.1");

            version = new Version("8.3.1.2");
            verString = version.FormatVersion(3,4);
            Assert.AreEqual(verString, "8.3.1.2");

            version = new Version("8.3.1.2");
            verString = version.FormatVersion(2, 3);
            Assert.AreEqual(verString, "8.3.1");

            version = new Version("8.3.0.2");
            verString = version.FormatVersion(2, 4);
            Assert.AreEqual(verString, "8.3.0.2");
        }


    }
}
