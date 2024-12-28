﻿#if NETFULL
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Configuration;

namespace Westwind.Utilities.Configuration.Tests
{
    /// <summary>
    /// Tests default config file implementation that uses
    /// only base constructor behavior - (config file and section config only)    
    /// </summary>
    [TestClass]
    public class AutoConfigFileConfigurationTests
    {

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
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

        [TestMethod]
        public void DefaultConstructorInstanceTest()
        {         
            var config = new AutoConfigFileConfiguration();
            
            // gets .config file, AutoConfigFileConfiguration section
            config.Initialize();

            Assert.IsNotNull(config);
            Assert.IsFalse(string.IsNullOrEmpty(config.ApplicationName));
            Assert.AreEqual(config.MaxDisplayListItems, 15);

	        string outputfile = TestHelpers.GetTestConfigFilePath();

			string text = File.ReadAllText(outputfile);
            Console.WriteLine(text);
        }

        [TestMethod]
        public void DefaultConstructorWithCustomProviderTest()
        {
            var config = new AutoConfigFileConfiguration();

            // Create a customized provider to set provider options
            var provider = new ConfigurationFileConfigurationProvider<AutoConfigFileConfiguration>()
            {
                ConfigurationSection = "CustomConfiguration",
                EncryptionKey = "seekrit123",
                PropertiesToEncrypt = "MailServer,MailServerPassword"                
            };

            config.Initialize(provider);  
            
            // Config File and custom section should have been created in config file
            string text = File.ReadAllText(TestHelpers.GetTestConfigFilePath());

            Assert.IsFalse(string.IsNullOrEmpty(text));
            Assert.IsTrue(text.Contains("<CustomConfiguration>"));

            // MailServer/MailServerPassword value should be encrypted
            Console.WriteLine(text);
        }


        [TestMethod]
        public void DefaultConstructorWithAppSettingsProviderTest()
        {
            var config = new AutoConfigFileConfiguration();

            // Create a customized provider to set provider options
            var provider = new ConfigurationFileConfigurationProvider<AutoConfigFileConfiguration>()
            {
                ConfigurationSection = null, // forces to AppSettings
                EncryptionKey = "seekrit123",
                PropertiesToEncrypt = "MailServer,MailServerPassword"
            };

            config.Initialize(provider);

            // Config File and custom section should have been created in config file
            string text = File.ReadAllText(TestHelpers.GetTestConfigFilePath());

            Assert.IsFalse(string.IsNullOrEmpty(text));
            Assert.IsTrue(text.Contains("<appSettings>"));

            // MailServer/MailServerPassword value should be encrypted
            Console.WriteLine(text);

            config.ApplicationName = "Updated Configuration";
            config.Write();

            config = null;
            config = new AutoConfigFileConfiguration();
            config.Initialize(provider);

            config.Initialize(); // should reload, reread

            Console.WriteLine("Application Name: " + config.ApplicationName);

            Assert.IsTrue(config.ApplicationName == "Updated Configuration");

        }

        [TestMethod]
        public void AutoConfigWriteConfigurationTest()
        {            
            var config = new AutoConfigFileConfiguration();
            config.Initialize();

            Assert.IsNotNull(config);
            Assert.IsFalse(string.IsNullOrEmpty(config.ApplicationName));
            Assert.AreEqual(config.MaxDisplayListItems, 15);

            config.MaxDisplayListItems = 17;
            config.Write();

            var config2 = new AutoConfigFileConfiguration();
            config2.Initialize();

            Assert.AreEqual(config2.MaxDisplayListItems, 17);

            // reset to default val
            config2.MaxDisplayListItems = 15;
            config2.Write();
        }

        [TestMethod]
        public void WriteConfigurationTest()
        {
            var config = new AutoConfigFileConfiguration();
            config.Initialize();

            config.MaxDisplayListItems = 12;
            config.DebugMode = DebugModes.DeveloperErrorMessage;
            config.ApplicationName = "Changed";
            config.SendAdminEmailConfirmations = true;
            
            // update complex type
            config.License.Company = "Updated Company";
            config.License.Name = "New User";
            config.License.LicenseKey = "UpdatedCompanyNewUser-5331231";

            config.Write();

            string text = File.ReadAllText(TestHelpers.GetTestConfigFilePath());
            Console.WriteLine(text);

            Assert.IsTrue(text.Contains(@"<add key=""DebugMode"" value=""DeveloperErrorMessage"" />"));
            Assert.IsTrue(text.Contains(@"<add key=""MaxDisplayListItems"" value=""12"" />"));
            Assert.IsTrue(text.Contains(@"<add key=""SendAdminEmailConfirmations"" value=""True"" />"));

            Assert.IsTrue(text.Contains(@"<add key=""License"" value=""New User,Updated Company,UpdatedCompanyNewUser-5331231"" />"));

            var config2 = new AutoConfigFileConfiguration();
            config2.Initialize();

            Assert.AreEqual(config2.MaxDisplayListItems, 12);
            Assert.AreEqual(config2.ApplicationName, "Changed");
            Assert.AreEqual("Updated Company",config2.License.Company);

            // reset to default val
            config2.MaxDisplayListItems = 15;
            config2.Write();
        }


        /// <summary>
        /// Test without explicit constructor parameter 
        /// </summary>
        [TestMethod]
        public void DefaultConstructor2InstanceTest()
        {
            var config = new AutoConfigFile2Configuration();
            
            // Not required since custom constructor calls this
            //config.Initialize();

            Assert.IsNotNull(config);
            Assert.IsFalse(string.IsNullOrEmpty(config.ApplicationName));
            Assert.AreEqual(config.MaxDisplayListItems, 15);

            string text = File.ReadAllText(TestHelpers.GetTestConfigFilePath());
            Console.WriteLine(text);
        }

        /// <summary>
        /// Write test without explicit constructor
        /// </summary>
        [TestMethod]
        public void WriteConfiguration2Test()
        {
            var config = new AutoConfigFile2Configuration();
            
            // not necesary since constructor calls internally
            //config.Initialize();

            config.MaxDisplayListItems = 12;
            config.DebugMode = DebugModes.DeveloperErrorMessage;
            config.ApplicationName = "Changed";
            config.SendAdminEmailConfirmations = true;
            config.Write();

            string text = File.ReadAllText(TestHelpers.GetTestConfigFilePath());
            Console.WriteLine(text);

            Assert.IsTrue(text.Contains(@"<add key=""DebugMode"" value=""DeveloperErrorMessage"" />"));
            Assert.IsTrue(text.Contains(@"<add key=""MaxDisplayListItems"" value=""12"" />"));
            Assert.IsTrue(text.Contains(@"<add key=""SendAdminEmailConfirmations"" value=""True"" />"));

            // reset to default val
            config.MaxDisplayListItems = 15;
            config.Write();
        }

        [TestMethod]
        public void NoConstructorWriteConfiguration2Test2()
        {
            var config = new NoConstructorConfiguration();
            Assert.IsNotNull(config, "Configuration object is null");

            config = NoConstructorConfiguration.New();
            Assert.IsNotNull(config, "Static New(): Configuration object is null");
        }

        public class NoConstructorConfiguration : AppConfiguration
        {
            public string ApplicationName { get; set; }
            public DebugModes DebugMode { get; set; }
            public int MaxDisplayListItems { get; set; }
            public bool SendAdminEmailConfirmations { get; set; }

            public static NoConstructorConfiguration New()
            {
                var config = new NoConstructorConfiguration();
                config.Initialize();
                return config;
            }
        }

    }
}
#endif