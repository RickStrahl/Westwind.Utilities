using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
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
		private string STR_ConnectionString = TestConfigurationSettings.WebStoreConnectionString;
    

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

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext) {

			// warm up dynamic runtime
            dynamic test = new List<int>();
            var val = test.Count;
        }
        #endregion

            [TestMethod]
            public void BasicDataReaderTimerTests()
            {
                var data = new SqlDataAccess(STR_ConnectionString);
                var reader = data.ExecuteReader("select * from wws_items");
                Assert.IsNotNull(reader, "Query Failure: " + data.ErrorMessage);
                
                StringBuilder sb = new StringBuilder();

                Stopwatch watch = new Stopwatch();
                watch.Start();
                
                while (reader.Read())
                {
                    string sku  = reader["sku"] as string;
                    string descript = reader["description"] as string;

                    decimal? price;
                    object t = reader["Price"];
                    if (t == DBNull.Value)
                        price = null;
                    else
                        price = (decimal)t;
                    
                    
                    sb.AppendLine(sku + " " + descript + " " + price.Value.ToString("n2"));                    
                }

                watch.Stop();

                reader.Close();

                Console.WriteLine(watch.ElapsedMilliseconds.ToString());
                Console.WriteLine(sb.ToString());                                
            }


            [TestMethod]
            public void BasicDynamicDataReaderTimerTest()
            {
                var data = new SqlDataAccess(STR_ConnectionString);
                dynamic reader = data.ExecuteDynamicDataReader("select * from wws_items");

                Assert.IsNotNull(reader, "Query Failure: " + data.ErrorMessage);


                StringBuilder sb = new StringBuilder();

                Stopwatch watch = new Stopwatch();
                watch.Start();


                while (reader.Read())
                {
                    string sku = reader.Sku;
                    string descript = reader.Description;
                    decimal? price = reader.Price;

                    sb.AppendLine(sku + " " + descript + " " + price.Value.ToString("n2"));
                }

                watch.Stop();

                reader.Close();

                Console.WriteLine(watch.ElapsedMilliseconds.ToString());
                Console.WriteLine(sb.ToString());
            }

    }

}
