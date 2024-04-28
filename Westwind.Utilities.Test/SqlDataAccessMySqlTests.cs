using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Utilities.Test;

namespace Westwind.Utilities.Data.Tests
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class SqlDataAccessMySqlTests
    {
        //private string STR_ConnectionString = "WestwindToolkitSamples";
        static string STR_ConnectionString = TestConfigurationSettings.MySqlConnectionString;

        public SqlDataAccessMySqlTests()
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
        public void GetMySqlInstance()
        {
            using (var data = new SqlDataAccess(STR_ConnectionString, DataAccessProviderTypes.MySql))
            {
                Assert.IsNotNull(data, data.ErrorMessage);
                Assert.IsNotNull(data.dbProvider, data.ErrorMessage);
            }
        }

        [TestMethod]
        public void ExecuteReaderTest()
        {
            using (var data = new SqlDataAccess(STR_ConnectionString, DataAccessProviderTypes.MySql))
            {
                var reader = data.ExecuteReader("select * from Localizations");

                Assert.IsTrue(reader.HasRows,data.ErrorMessage);

                while (reader.Read())
                {
                    string id = reader["ResourceId"].ToString();
                    string val = reader["ResourceId"].ToString();
                    Console.WriteLine($"{id} - {val}");
                }
            }            
        }

        [TestMethod]
        public void CreateTable()
        {
            using (var data = new SqlDataAccess(STR_ConnectionString, DataAccessProviderTypes.MySql))
            {
                var script = @"CREATE TABLE `{0}` (
   pk int(11) NOT NULL AUTO_INCREMENT,
  ResourceId varchar(1024) DEFAULT NULL,
  Value varchar(2048) DEFAULT NULL,
  LocaleId varchar(10) DEFAULT NULL,
  ResourceSet varchar(512) DEFAULT NULL,
  Type varchar(512) DEFAULT NULL,
  BinFile blob,
  TextFile text,
  Filename varchar(128) DEFAULT NULL,
  Comment varchar(512) DEFAULT NULL,
  ValueType int(2) DEFAULT 0,
  Updated datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY(`pk`)
) ENGINE = InnoDB AUTO_INCREMENT = 1 DEFAULT CHARSET = utf8;

                INSERT INTO `{0}` (ResourceId, Value, LocaleId, ResourceSet) VALUES('HelloWorld','Hello Cruel World (MySql)','','Resources');
                INSERT INTO `{0}` (ResourceId, Value, LocaleId, ResourceSet) VALUES('HelloWorld','Hallo schnöde Welt (MySql)','de','Resources');
                INSERT INTO `{0}` (ResourceId, Value, LocaleId, ResourceSet) VALUES('HelloWorld','Bonjour tout le monde','fr','Resources');
                INSERT INTO `{0}` (ResourceId, Value, LocaleId, ResourceSet) VALUES('Yesterday','Yesterday (invariant)','','Resources');
                INSERT INTO `{0}` (ResourceId, Value, LocaleId, ResourceSet) VALUES('Yesterday','Gestern','de','Resources');
                INSERT INTO `{0}` (ResourceId, Value, LocaleId, ResourceSet) VALUES('Yesterday','Hier','fr','Resources');
                INSERT INTO `{0}` (ResourceId, Value, LocaleId, ResourceSet) VALUES('Today','Today (invariant)','','Resources');
                INSERT INTO `{0}` (ResourceId, Value, LocaleId, ResourceSet) VALUES('Today','Heute','de','Resources');
                INSERT INTO `{0}` (ResourceId, Value, LocaleId, ResourceSet) VALUES('Today','Aujourd''hui','fr','Resources');

                INSERT INTO `{0}` (ResourceId, Value, LocaleId, ResourceSet, ValueType) VALUES('MarkdownText','This is **MarkDown** formatted *HTML Text*','','Resources',2);
                INSERT INTO `{0}` (ResourceId, Value, LocaleId, ResourceSet, ValueType) VALUES('MarkdownText','Hier ist **MarkDown** formatierter *HTML Text*','de','Resources',2);
                INSERT INTO `{0}` (ResourceId, Value, LocaleId, ResourceSet, ValueType) VALUES('MarkdownText','Ceci est **MarkDown** formaté *HTML Texte*','fr','Resources',2);

                INSERT INTO `{0}` (ResourceId, Value, LocaleId, ResourceSet) VALUES('lblHelloWorldLabel.Text','Hello Cruel World (local)','','ResourceTest.aspx');
                INSERT INTO `{0}` (ResourceId, Value, LocaleId, ResourceSet) VALUES('lblHelloWorldLabel.Text','Hallo Welt (lokal)','de','ResourceTest.aspx');
                INSERT INTO `{0}` (ResourceId, Value, LocaleId, ResourceSet) VALUES('lblHelloWorldLabel.Text','Bonjour monde (local)','fr','ResourceTest.aspx');
";

                var result = data.RunSqlScript(string.Format(script,"Localizations"));
                Assert.IsTrue(result, data.ErrorMessage);
            }
        }
    }
}
