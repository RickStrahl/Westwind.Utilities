using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using Westwind.Utilities.Logging;
using System.Diagnostics;
using System.Data.Common;
using Westwind.Data.Test.Models;
using Westwind.Utilities.Test;

namespace Westwind.Utilities.Data.Tests
{
	/// <summary>
	/// Summary description for DataUtilsTests
	/// </summary>
	[TestClass]
    public class DataUtilsTests
    {
		//private const string STR_TestDataConnection = "WestwindToolkitSamples";
		private static string STR_TestDataConnection = TestConfigurationSettings.WestwindToolkitConnectionString;

        [TestMethod]
        public void DataReaderToObjectTest()
        {
            using (SqlDataAccess data = new SqlDataAccess(STR_TestDataConnection))
            {
                IDataReader reader = data.ExecuteReader("select top 1 * from ApplicationLog");
                Assert.IsNotNull(reader, "Couldn't access Data reader. " + data.ErrorMessage);
                Assert.IsTrue(reader.Read(), "Couldn't read from DataReader");
                WebLogEntry entry = new WebLogEntry();
                DataUtils.DataReaderToObject(reader, entry, null);
                Assert.IsNotNull(entry.Message, "Entry Message should not be null");
                Assert.IsTrue(entry.ErrorLevel != ErrorLevels.None, "Entry Error level should not be None (error)");
            }
        }

        [TestMethod]
        public void DataReaderToIEnumerableObjectTest()
        {
            using (SqlDataAccess data = new SqlDataAccess(STR_TestDataConnection))
            {
                DbDataReader reader = data.ExecuteReader("select top 1 * from ApplicationLog");
                reader.Close();

                Stopwatch sw = new Stopwatch();
                sw.Start();

                reader = data.ExecuteReader("select * from ApplicationLog");
                Assert.IsNotNull(reader, "Reader null: " + data.ErrorMessage);
                var entries = DataUtils.DataReaderToIEnumerable<WebLogEntry>(reader);
                foreach (var entry in entries)
                {
                    string name = entry.Message;
                }                
                sw.Stop();

                // run again to check for connections not closed
                reader = data.ExecuteReader("select * from ApplicationLog");
                Assert.IsNotNull(reader, "Reader null: " + data.ErrorMessage);
                entries = DataUtils.DataReaderToIEnumerable<WebLogEntry>(reader);
                foreach (var entry in entries)
                {
                    string name = entry.Message;
                }                
                
                Console.WriteLine("DataReaderToIEnumerable: " + sw.ElapsedMilliseconds.ToString() + " ms");
            }
        }

        [TestMethod]
        public void DataReaderToListTest()
        {
            using (SqlDataAccess data = new SqlDataAccess(STR_TestDataConnection))
            {
                DbDataReader reader = data.ExecuteReader("select top 1 * from ApplicationLog");
                Assert.IsNotNull(reader, data.ErrorMessage);
                reader.Close();

                Stopwatch sw = new Stopwatch();
                sw.Start();

                reader = data.ExecuteReader("select * from ApplicationLog");
                Assert.IsNotNull(reader, "Reader null: " + data.ErrorMessage);
                var entries = DataUtils.DataReaderToObjectList<WebLogEntry>(reader);


                foreach (var entry in entries)
                {
                    string name = entry.Message;
                }
                sw.Stop();

                Console.WriteLine("DataReaderToList: " + sw.ElapsedMilliseconds.ToString() + " ms");
            }
        }


		[TestMethod]
		public void DataTableToListTest()
		{
			var sql = new SqlDataAccess(STR_TestDataConnection);
			var dt = sql.ExecuteTable("items", "select * from ApplicationLog");

			Assert.IsNotNull(dt, "Failed to load test data: " + sql.ErrorMessage);

			var items = DataUtils.DataTableToObjectList<Customer>(dt);

			Assert.IsNotNull(items);
			Assert.IsTrue(items.Count > 0);
			Console.WriteLine(items.Count);
		}

        #region Byte Operations

        [TestMethod]
        public void IndexOfByteArrayTest()
        {
            var bytes = new byte[] {0x31, 0x33, 0x20, 0xe2, 0x80, 0x0a, 0x31, 0x33, 0x20};
            var bytesToFind = new byte[] {0xe2, 0x80};

            Assert.IsTrue(DataUtils.IndexOfByteArray(bytes, bytesToFind) > -1);
        }

        [TestMethod]
        public void IndexOfByteArrayEndTest()
        {
            var bytes = new byte[] {0x31, 0x33, 0x20, 0x0a, 0x31, 0x33, 0x20, 0xe2, 0x80};
            var bytesToFind = new byte[] {0xe2, 0x80};

            Assert.IsTrue(DataUtils.IndexOfByteArray(bytes, bytesToFind) > -1);
        }

        [TestMethod]
        public void IndexOfByteArrayBeginTest()
        {
            var bytes = new byte[] {0xe2, 0x80, 0x31, 0x33, 0x20, 0x0a, 0x31, 0x33, 0x20};
            var bytesToFind = new byte[] {0xe2, 0x80};

            Assert.IsTrue(DataUtils.IndexOfByteArray(bytes, bytesToFind) > -1);
        }

        [TestMethod]
        public void ReplaceBytesTest()
        {
            var bytes = new byte[] {0x31, 0x33, 0x20, 0xe2, 0x80, 0x0a, 0x31, 0x33, 0x20};
            var bytesToRemove = new byte[] {0xe2, 0x80};
            byte[] results = DataUtils.RemoveBytes(bytes, bytesToRemove );

            Console.WriteLine("-- Replace Middle --");
            Console.WriteLine("Before: " + StringUtils.BinaryToBinHex(bytes));
            Console.WriteLine("After : " + StringUtils.BinaryToBinHex(results));

            Assert.IsTrue(results.Length == bytes.Length - 2);
            Assert.IsTrue(DataUtils.IndexOfByteArray(results, bytesToRemove) == -1);
            
        }

        [TestMethod]
        public void ReplaceBytesEndTest()
        {
           Console.WriteLine("-- Replace End --");
            var bytes = new byte[] {0x31, 0x33, 0x20, 0xe2, 0x80 };
            var bytesToRemove = new byte[] {0xe2, 0x80};
            var results = DataUtils.RemoveBytes(bytes, bytesToRemove );

            Console.WriteLine("Before: " + StringUtils.BinaryToBinHex(bytes));
            Console.WriteLine("After : " + StringUtils.BinaryToBinHex(results));

            Assert.IsTrue(results.Length == bytes.Length - 2);
            Assert.IsTrue(DataUtils.IndexOfByteArray(results, bytesToRemove) == -1);
        }

        [TestMethod]
        public void ReplaceBytesBeginningTest()
        {
            Console.WriteLine("-- Replace Beginning --");
            var bytes = new byte[] {0xe2, 0x80, 0x31, 0x33, 0x20 };
            var bytesToRemove = new byte[] {0xe2, 0x80};
            var results = DataUtils.RemoveBytes(bytes, bytesToRemove );

            Console.WriteLine("Before: " + StringUtils.BinaryToBinHex(bytes));
            Console.WriteLine("After : " + StringUtils.BinaryToBinHex(results));

            Assert.IsTrue(results.Length == bytes.Length - 2);
            Assert.IsTrue(DataUtils.IndexOfByteArray(results, bytesToRemove) == -1);
        }

        [TestMethod]
        public void ReplaceBytesBeginningAndEndTest()
        {
            Console.WriteLine("-- Replace Beginning and End --");
            var bytes = new byte[] {0xe2, 0x80, 0x31, 0x33, 0x20, 0xe2, 0x80 };
            var bytesToRemove = new byte[] {0xe2, 0x80};
            var results = DataUtils.RemoveBytes(bytes, bytesToRemove );

            Console.WriteLine("Before: " + StringUtils.BinaryToBinHex(bytes));
            Console.WriteLine("After : " + StringUtils.BinaryToBinHex(results));

            Assert.IsTrue(results.Length == bytes.Length - 4);
            Assert.IsTrue(DataUtils.IndexOfByteArray(results, bytesToRemove) == -1);
        }
        #endregion

#if false
        // Add Npgsql package
        [TestMethod]
        public void GetPostGreSqlProviderTest()
        {
            var provider = DataUtils.GetDbProviderFactory(DataAccessProviderTypes.PostgreSql);
            Assert.IsNotNull(provider);            
        }
#endif


#if false
        // Add MySql.Data Package   
        [TestMethod]
        public void GetMySqlProviderTest()
        {
            var provider = DataUtils.GetDbProviderFactory(DataAccessProviderTypes.MySql);
            Assert.IsNotNull(provider);            
        }
#endif

    }
}
