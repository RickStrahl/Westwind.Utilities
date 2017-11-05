using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Data.Test;
using Westwind.Data.Test.Models;
using Westwind.Utilities.Data;
using System.Data;
using Westwind.Utilities;
using Westwind.Utilities.Logging;
using System.Diagnostics;
using Westwind.Utilities.Test;

namespace Westwind.Utilities.Data.Tests
{
    /// <summary>
    /// Summary description for DataUtilsTests
    /// </summary>
    [TestClass]
    public class SqlDataAccessSqliteTests
    {
        //private string STR_ConnectionString = "WestwindToolkitSamples";
        static string STR_ConnectionString = TestConfigurationSettings.SqliteConnectionString;

        public SqlDataAccessSqliteTests()
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

        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void Initialize(TestContext testContext)
        {
            //DatabaseInitializer.InitializeDatabase();

            // warm up data connection
            //#if NETCORE
            //            SqlDataAccess data = new SqlDataAccess(STR_ConnectionString, Microsoft.Data.Sqlite.SqliteFactory.Instance);
            //#else
            //            SqlDataAccess data = new SqlDataAccess(STR_ConnectionString, System.Data.SQLite.SQLiteFactory.Instance);
            //#endif
            //            var readr = data.ExecuteReader("select * from artists");

            //            readr.Read();
            //            readr.Close();

            //            // warm up DLR load time
            //            dynamic ddata = data;
            //            string err = ddata.ErrorMessage;
        }


        [TestMethod]
        public void GetSqliteInstance()
        {
            using (var data = new SqlDataAccess(STR_ConnectionString, DataAccessProviderTypes.SqLite))
            {
                Assert.IsNotNull(data, data.ErrorMessage);
                Assert.IsNotNull(data.dbProvider, data.ErrorMessage);
            }
        }

        [TestMethod]
        public void ExecuteReaderTest()
        {
            using (var data = new SqlDataAccess(STR_ConnectionString, DataAccessProviderTypes.SqLite))
            {
                var reader = data.ExecuteReader("select * from artists");

                Assert.IsTrue(reader.HasRows, data.ErrorMessage);

                while (reader.Read())
                {
                    string txt = reader["ArtistName"].ToString();
                    Console.WriteLine(txt);
                }
            }
        }

        [TestMethod]
        public void CreateTable()
        {
            using (var data = new SqlDataAccess(STR_ConnectionString, DataAccessProviderTypes.SqLite))
            {
                var script = @"CREATE TABLE [Localizations] (
 [Pk] INTEGER PRIMARY KEY 
, [ResourceId] nvarchar(1024) COLLATE NOCASE NOT NULL
, [Value] ntext  NULL
, [LocaleId] nvarchar(10) COLLATE NOCASE DEFAULT '' NULL
, [ResourceSet] nvarchar(512) COLLATE NOCASE DEFAULT ''  NULL
, [Type] nvarchar(512) DEFAULT '' NULL
, [BinFile] image NULL
, [TextFile] ntext NULL
, [Filename] nvarchar(128) NULL
, [Comment] nvarchar(512) NULL
, [ValueType] unsigned integer(2) DEFAULT 0
, [Updated] datetime NULL
);

INSERT INTO [Localizations] (ResourceId,Value,LocaleId,ResourceSet) VALUES ('HelloWorld','Hello Cruel World (SqlLite)','','Resources');
INSERT INTO [Localizations] (ResourceId,Value,LocaleId,ResourceSet) VALUES ('HelloWorld','Hallo schnöde Welt (SqlLite)','de','Resources');
INSERT INTO [Localizations] (ResourceId,Value,LocaleId,ResourceSet) VALUES ('HelloWorld','Bonjour tout le monde (SqlLite)','fr','Resources');
INSERT INTO [Localizations] (ResourceId,Value,LocaleId,ResourceSet) VALUES ('Yesterday','Yesterday (invariant)','','Resources');
INSERT INTO [Localizations] (ResourceId,Value,LocaleId,ResourceSet) VALUES ('Yesterday','Gestern','de','Resources');
INSERT INTO [Localizations] (ResourceId,Value,LocaleId,ResourceSet) VALUES ('Yesterday','Hier','fr','Resources');
INSERT INTO [Localizations] (ResourceId,Value,LocaleId,ResourceSet) VALUES ('Today','Today (invariant)','','Resources');
INSERT INTO [Localizations] (ResourceId,Value,LocaleId,ResourceSet) VALUES ('Today','Heute','de','Resources');
INSERT INTO [Localizations] (ResourceId,Value,LocaleId,ResourceSet) VALUES ('Today','Aujourd''hui','fr','Resources');

INSERT INTO [Localizations] (ResourceId,Value,LocaleId,ResourceSet,ValueType) VALUES ('MarkdownText','This is **MarkDown** formatted *HTML Text*','','Resources',2);
INSERT INTO [Localizations] (ResourceId,Value,LocaleId,ResourceSet,ValueType) VALUES ('MarkdownText','Hier ist **MarkDown** formatierter *HTML Text*','de','Resources',2);
INSERT INTO [Localizations] (ResourceId,Value,LocaleId,ResourceSet,ValueType) VALUES ('MarkdownText','Ceci est **MarkDown** formaté *HTML Texte*','fr','Resources',2);

INSERT INTO [Localizations] (ResourceId,Value,LocaleId,ResourceSet) VALUES ('lblHelloWorldLabel.Text','Hello Cruel World (local)','','ResourceTest.aspx');
INSERT INTO [Localizations] (ResourceId,Value,LocaleId,ResourceSet) VALUES ('lblHelloWorldLabel.Text','Hallo Welt (lokal)','de','ResourceTest.aspx');
INSERT INTO [Localizations] (ResourceId,Value,LocaleId,ResourceSet) VALUES ('lblHelloWorldLabel.Text','Bonjour monde (local)','fr','ResourceTest.aspx');
";
                var result = data.RunSqlScript(script);
                Assert.IsTrue(result, data.ErrorMessage);
            }
        }
    }
}
