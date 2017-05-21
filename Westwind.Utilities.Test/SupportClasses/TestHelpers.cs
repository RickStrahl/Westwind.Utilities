using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Westwind.Utilities.Configuration.Tests
{
    public class TestHelpers
    {
        public static string GetTestConfigFilePath()
        {
            return (typeof(TestHelpers).Assembly.Location + ".config");
        }

#if NETFULL
		public static string GetApplicationConfigFile()
	    {
			return AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
		}
#endif

        public static void DeleteTestConfigFile()
        {
            string configFile = GetTestConfigFilePath();
            try
            {
                File.Delete(configFile);
            }
            catch { }
        }

    }
}
