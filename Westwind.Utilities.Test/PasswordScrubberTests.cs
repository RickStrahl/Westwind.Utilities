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
    public class PasswordScrubberTests
    {
        public PasswordScrubberTests()
        {

        }

        private TestContext testContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        #region Additional test attributes

        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //

        #endregion

        [TestMethod]
        public void SqlConnectionStringTest()
        {
            var scrubber = new PasswordScrubber()
            {
                ObscuredValueBaseDisplay = "*******",
                ShowUnobscuredCharacterCount = 2
            };

            string conn = @"server=.;database=WebStore;uid=WebStoreUser;pwd=superSeekrit#1;encrypt=false;";
            string result = scrubber.ScrubSqlConnectionStringValues(conn, "pwd", "uid");

            Console.WriteLine(result);

            Assert.IsTrue(result.Contains("pwd=su*******"));
            Assert.IsTrue(result.Contains("uid=We*******"));

        }

        [TestMethod]
        public void JsonScrubTest()
        {
            var scrubber = new PasswordScrubber()
            {
                ObscuredValueBaseDisplay = "*******",
                ShowUnobscuredCharacterCount = 2
            };

            var json = """
                       { 
                       	"Name: "Rick",
                       	"Password": "SuperSeekrit56",
                       	"UserId": "rickochet2",                       	
                       }
                       """;
            
            var result = scrubber.ScrubJsonValues(json, "Password", "UserId");

            Console.WriteLine(result);

            Assert.IsTrue(result.Contains("\"Password\": \"Su*******\""));
            Assert.IsTrue(result.Contains("ri*******"));
        }

        [TestMethod]
        public void JsonAndSqlConnectionScrubTest()
        {
            var scrubber = new PasswordScrubber()
            {
                ObscuredValueBaseDisplay = "*******",
                ShowUnobscuredCharacterCount = 2
            };

            var json = """
                   { 
                   	"Name: "Rick",
                   	"Password": "SuperSeekrit56",
                   	"UserId": "rickochet2",
                   	"ConnectionString": "server=.;database=WebStore;uid=WebStoreUser;pwd=superSeekrit#1;encrypt=false;"
                   }
                   """;

            var result = scrubber.ScrubSqlConnectionStringValues(json, "pwd", "uid");
            result = scrubber.ScrubJsonValues(result, "Password", "UserId");

            Console.WriteLine(result);

            Assert.IsTrue(result.Contains("\"Password\": \"Su*******\""));
            Assert.IsTrue(result.Contains("ri*******"));

            Assert.IsTrue(result.Contains("pwd=su*******"));
            Assert.IsTrue(result.Contains("uid=We*******"));
        }
    }
}
