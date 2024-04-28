﻿// #define TEST_FOXPRO_DATA
// IMPORTANT: For this to work the host project has to be targeting 32 bit Windows

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
using System.Diagnostics;
using System.IO;
using Westwind.Utilities.Test;

#if NETFULL && TEST_FOXPRO_DATA

// Remove TestFoxProDriver from Compiler Options if you don't have the FoxPro OleDb Driver installed

namespace Westwind.Utilities.Data.Tests
{
    /// <summary>
    /// Summary description for DataUtilsTests    
    /// </summary>
    /// <remarks>
    /// Requires that the FoxPro Visual FoxPro database driver is installed
    /// </remarks>
    [TestClass]
    public class SqlDataAccessFoxProTests
    {
        string connString =
                @"Provider=vfpoledb.1;Data Source={0};Exclusive=false;Deleted=true;Nulls=false;";

        public SqlDataAccessFoxProTests()
        {
            connString = string.Format(connString, Path.Combine(Environment.CurrentDirectory, "supportFiles\\"));

        }

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

        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void Initialize(TestContext testContext)
        {
        }

        [TestMethod]
        public void SimpleSelectOleDbTest()
        {
            Console.WriteLine(connString);
            using (var data = new SqlDataAccess(connString, DataAccessProviderTypes.OleDb)
                   {
                       ParameterPrefix = "?",
                       UsePositionalParameters = true,
                       LeftFieldBracket = "",
                       RightFieldBracket = ""
                   })
            {
                var dt = data.ExecuteTable("TCustomers","select * from customers");

                Assert.IsNotNull(dt, data.ErrorMessage);
                Assert.IsTrue(dt.Rows.Count > 0);

            }

            

        }

            [TestMethod]
        public void InsertEntityOleDbTest()
        {
            Console.WriteLine(connString);
            using (var data = new SqlDataAccess(connString, "System.Data.OleDb")
            {
                ParameterPrefix = "?",
                UsePositionalParameters = true,
                LeftFieldBracket = "",
                RightFieldBracket = ""
            })
            {
                var customer = new
                {
                    Id = StringUtils.NewStringId(),
                    FirstName = "Mike",
                    LastName = "Smith",
                    Company = "Smith & Smith",
                    Entered = DateTime.UtcNow                    
                };

                // insert into customers and skip Id,Order properties and return id
                object newId = data.InsertEntity(customer, "Customers", null, false);

                Console.WriteLine(data.LastSql);
                Assert.IsNotNull(newId, data.ErrorMessage);
                Console.WriteLine(newId);
            }
        }

        [TestMethod]
        public void UpdateEntityOleDbTest()
        {
            using (var data = new SqlDataAccess(connString, "System.Data.OleDb")
            {
                LeftFieldBracket = "",
                RightFieldBracket = "",
                ParameterPrefix = "?",
                UsePositionalParameters = true
            })
            {
                var customer = new
                {
                    Id = "_adasdasd  ",
                    FirstName = "Mike Updated",
                    LastName = "Smith",
                    Company = "Smith & Smith",
                    Entered = DateTime.UtcNow,                    
                };

                // insert into customers and skip Id,Order properties and return id
                object result= data.UpdateEntity(customer, "Customers", "Id", null, "FirstName,Entered,Address");
                
                Console.WriteLine(data.LastSql);
                Assert.IsNotNull(result, data.ErrorMessage);
                Console.WriteLine(result);
                Console.WriteLine(data.ErrorMessage);
                Assert.IsNotNull(result,data.ErrorMessage);
            }
        }

    }

}
#endif