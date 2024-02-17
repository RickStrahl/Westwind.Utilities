﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;

namespace Westwind.Utilities.Configuration.Tests
{
    /// <summary>
    /// Tests default config file implementation that uses
    /// only base constructor behavior - (config file and section config only)    
    /// </summary>
    [TestClass]
    public class JsonFileConfigurationTests
    {
        /// <summary>
        /// Note: For Web Apps this should be a complete path.
        /// Here the filename references the current directory
        /// </summary>
        public string STR_JSONCONFIGFILE = "JsonConfiguration.txt";

        public JsonFileConfigurationTests()
        {
            // force Json ref to load since we dynamically load
            var json = new JsonSerializerSettings();

			// explicitly write file location since tests are in indeterminate folder
			// especially running .NET Core
	        STR_JSONCONFIGFILE = Path.Combine(Path.GetTempPath(), STR_JSONCONFIGFILE);
        }
		
        [TestMethod]
        public void DefaultConstructorInstanceTest()
        {
            var config = new JsonFileConfiguration();            
            config.Initialize(configData: STR_JSONCONFIGFILE);
            
            Assert.IsNotNull(config);
            Assert.IsFalse(string.IsNullOrEmpty(config.ApplicationName));

            string text = File.ReadAllText(STR_JSONCONFIGFILE);
            Console.WriteLine(text);
        }

        [TestMethod]
        public void WriteConfigurationTest()
        {
            var config = new JsonFileConfiguration();
            config.Initialize(STR_JSONCONFIGFILE);

            config.MaxDisplayListItems = 12;
            config.DebugMode = DebugModes.DeveloperErrorMessage;
            config.ApplicationName = "Changed";
            config.SendAdminEmailConfirmations = true;

            // secure properties
            config.Password = "seekrit2";
            config.AppConnectionString = "server=.;database=unsecured";

            config.Write();

            string jsonConfig = File.ReadAllText(STR_JSONCONFIGFILE);
            Console.WriteLine(jsonConfig);

            Assert.IsTrue(jsonConfig.Contains(@"""DebugMode"": ""DeveloperErrorMessage"""));
            Assert.IsTrue(jsonConfig.Contains(@"""MaxDisplayListItems"": 12") );
            Assert.IsTrue(jsonConfig.Contains(@"""SendAdminEmailConfirmations"": true"));

			// Password and AppSettings  should be encrypted in config file
			Assert.IsTrue(!jsonConfig.Contains($"\"Password\": \"seekrit\""));			
		}

        [TestMethod]
        public void WriteEncryptedConfigurationTest()
        {
            File.Delete(STR_JSONCONFIGFILE);

            var config = new JsonFileConfiguration();
            config.Initialize(STR_JSONCONFIGFILE);

            // write secure properties
            config.Password = "seekrit2";
            config.AppConnectionString = "server=.;database=unsecured";

            config.License.Company = "West Wind 2";
            config.License.LicenseKey = "RickWestWind2-51123222";

            config.Write();

            // completely reload settings
            config = new JsonFileConfiguration();
            config.Initialize(STR_JSONCONFIGFILE);

            Assert.IsTrue(config.Password == "seekrit2");
            Assert.IsTrue(config.License.Company == "West Wind 2");
            Assert.IsTrue(config.License.LicenseKey == "RickWestWind2-51123222");

            string jsonConfig = File.ReadAllText(STR_JSONCONFIGFILE);
            Console.WriteLine(jsonConfig);

			// Password and AppSettings  should be encrypted in config file
			Assert.IsTrue(!jsonConfig.Contains("\"Password\": \"seekrit2\""));

			// now re-read settings into a new object
			var config2 = new JsonFileConfiguration();
            config2.Initialize(STR_JSONCONFIGFILE);

            // check secure properties
            Assert.IsTrue(config.Password == "seekrit2");
            Assert.IsTrue(config.AppConnectionString == "server=.;database=unsecured");
        }

        [TestMethod]
        public void RawJsonFileWithSingletonTest()
        {
            var config = MyJsonConfiguration.Current;

            Assert.IsNotNull(config);
            
            Console.WriteLine(JsonSerializationUtils.Serialize(config, false, true));            

        }
    }

    public class MyJsonConfiguration : AppConfiguration 
    { 
        public static MyJsonConfiguration Current { get; set; }

        static MyJsonConfiguration()
        {
            var config = new MyJsonConfiguration();
            // assign a provider explicitly
            config.Provider = new JsonFileConfigurationProvider<MyJsonConfiguration>()
            {
                JsonConfigurationFile = "./SupportFiles/_MyJsonConfiguration.json"
            };

            // load and read the configuration
            //config.Initialize();
            config.Read();

            Current = config;
        }


        public string MyString { get; set; } = "Default String";
        public int SomeValue { get; set; } = -1;

        
    }

}