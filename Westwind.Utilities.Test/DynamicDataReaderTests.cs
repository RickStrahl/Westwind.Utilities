using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Data.Common;
using Westwind.Utilities;
using Microsoft.CSharp.RuntimeBinder;
using Westwind.Utilities.Data;
using System.Diagnostics;
using System.Threading;
using Westwind.Utilities.Test;

namespace Westwind.Utilities.Data.Tests
{

    /// <summary>
    /// Summary description for DynamicDataRowTests
    /// </summary>
    [TestClass]
    public class DynamicDataReaderTests
    {
		private static string STR_ConnectionString = TestConfigurationSettings.WestwindToolkitConnectionString;
    

        private TestContext testContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext) {

			// warm up dynamic runtime
            dynamic test = new List<int>();
            var val = test.Count;


            //DatabaseInitializer.InitializeDatabase();

            // warm up data connection

            using (SqlDataAccess data = new SqlDataAccess(STR_ConnectionString))
            {
                var readr = data.ExecuteReader("select top 1 * from Customers");
                var x = readr.Read();
            }
        }

        [TestMethod]
        public void BasicDataReaderTimerTests()
        {
            DbDataReader reader;
            using (var data = new SqlDataAccess(STR_ConnectionString))
            {
                reader = data.ExecuteReader("select * from Customers");
                Assert.IsNotNull(reader, "Query Failure: " + data.ErrorMessage);


                StringBuilder sb = new StringBuilder();

                Stopwatch watch = new Stopwatch();
                watch.Start();

                while (reader.Read())
                {
                    string firstName = reader["FirstName"] as string;
                    string lastName = reader["LastName"] as string;
                    string company = reader["Company"] as string;

                    DateTime? entered = reader["Entered"] as DateTime?;
                    string d = entered.HasValue ? entered.Value.ToString("d") : string.Empty;

                    sb.AppendLine(firstName + " " + lastName + " " + company + " - " + entered.Value.ToString("d"));
                }

                watch.Stop();


                Console.WriteLine(watch.ElapsedMilliseconds.ToString());
                Console.WriteLine(sb.ToString());
            }
        }


        [TestMethod]
        public void BasicDynamicDataReaderTimerTest()
        {
            dynamic reader;
            using (var data = new SqlDataAccess(STR_ConnectionString))
            {
                reader = data.ExecuteDynamicDataReader("select * from customers");

                Assert.IsNotNull(reader, "Query Failure: " + data.ErrorMessage);

                StringBuilder sb = new StringBuilder();

                Stopwatch watch = new Stopwatch();
                watch.Start();


                while (reader.Read())
                {

                    string firstName = reader.FirstName;
                    string lastName = reader.LastName;
                    string company = reader.Company;

                    DateTime? entered = reader.Entered as DateTime?;
                    string d = entered.HasValue ? entered.Value.ToString("d") : string.Empty;

                    sb.AppendLine(firstName + " " + lastName + " " + company + " - " + entered.Value.ToString("d"));
                }

                watch.Stop();

                reader.Close();

                Console.WriteLine(watch.ElapsedMilliseconds.ToString());
                Console.WriteLine(sb.ToString());
            }
        }

    }

}
