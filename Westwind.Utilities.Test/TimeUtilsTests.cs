using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace Westwind.Utilities.Test
{
    [TestClass]
    public class TimeUtilsTest
    {
        /// <summary>
        /// Should get a date
        /// </summary>
        [TestMethod]
        public void FriendlyElapsedTimeOverflowTest()
        {
            var ts = TimeSpan.FromHours(1);
            Assert.AreEqual("1h ago", TimeUtils.FriendlyElapsedTimeString(ts));

            ts = TimeSpan.FromMinutes(2);            
            Assert.AreEqual("2m ago", TimeUtils.FriendlyElapsedTimeString(ts));

            ts = TimeSpan.FromSeconds(77);            
            Assert.AreEqual("1m ago", TimeUtils.FriendlyElapsedTimeString(ts));

            ts = TimeSpan.FromSeconds(130);
            Assert.AreEqual("2m ago", TimeUtils.FriendlyElapsedTimeString(ts));

            ts = TimeSpan.FromSeconds(30 * 60);            
            Assert.AreEqual("30m ago", TimeUtils.FriendlyElapsedTimeString(ts));


            ts = TimeSpan.FromSeconds(77 * 60);
            Console.WriteLine(TimeUtils.FriendlyElapsedTimeString(ts));
            Assert.AreEqual("1h ago", TimeUtils.FriendlyElapsedTimeString(ts));

            ts = TimeSpan.FromHours((24 * 60));            
            Assert.AreEqual("2mo ago", TimeUtils.FriendlyElapsedTimeString(ts));

            ts = TimeSpan.FromHours(25);            
            Assert.AreEqual("25h ago", TimeUtils.FriendlyElapsedTimeString(ts));
        }
    }
}
