using System;
using System.Configuration;
using System.Data.Common;

#if NETCORE
    using Microsoft.Data.SqlClient;
#else
    using System.Data.SqlClient;
#endif

using Westwind.Utilities.Properties;

namespace Westwind.Utilities.Data
{
    /// <summary>
    /// Used to parse a connection string or connection string name 
    /// into a the base connection  string and dbProvider.
    /// 
    /// If a connection string is passed that's just used.
    /// If a ConnectionString entry name is passed the connection 
    /// string is extracted and the provider parsed.
    /// </summary>
    public class ConnectionStringInfo
    {
        /// <summary>
        /// The default connection string provider
        /// </summary>
        public static string DefaultProviderName = "System.Data.SqlClient";

        /// <summary>
        /// The connection string parsed
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// The DbProviderFactory parsed from the connection string
        /// or default provider
        /// </summary>
        public DbProviderFactory Provider { get; set; }


        /// <summary>
        /// Figures out the Provider and ConnectionString from either a connection string
        /// name in a config file or full  ConnectionString and provider.         
        /// </summary>
        /// <param name="connectionString">Config file connection name or full connection string</param>
        /// <param name="providerName">optional provider name. If not passed with a connection string is considered Sql Server</param>
        /// <param name="factory">optional provider factory. Use for .NET Core to pass actual provider instance since DbproviderFactories doesn't exist</param> 
        public static ConnectionStringInfo GetConnectionStringInfo(string connectionString, string providerName = null, DbProviderFactory factory = null)
        {            
            var info = new ConnectionStringInfo();

            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException(Resources.AConnectionStringMustBePassedToTheConstructor);

            if (!connectionString.Contains("="))
			{
#if NETFULL
				connectionString = RetrieveConnectionStringFromConfig(connectionString, info);
#else
				throw new ArgumentException("Connection string names are not supported with .NET Standard. Please use a full connectionstring.");
#endif
			}
			else
            {
				info.Provider = factory;

				if (factory == null)
				{
					if (providerName == null)
						providerName = DefaultProviderName;

                    // TODO: DbProviderFactories This should get fixed by release of .NET 2.0
#if NETFULL
					info.Provider = DbProviderFactories.GetFactory(providerName);
#else                    
                    info.Provider = SqlClientFactory.Instance;
#endif
				}
            }

            info.ConnectionString = connectionString;

            return info;
        }



#if NETFULL
		/// <summary>
		/// Retrieves a connection string from the Connection Strings configuration settings
		/// </summary>
		/// <param name="connectionStringName"></param>
		/// <param name="info"></param>
		/// <exception cref="InvalidOperationException">Throws when connection string doesn't exist</exception>
		/// <returns></returns>
		public static string RetrieveConnectionStringFromConfig(string connectionStringName, ConnectionStringInfo info)
		{
			// it's a connection string entry
			var connInfo = ConfigurationManager.ConnectionStrings[connectionStringName];
			if (connInfo != null)
			{
				if (!string.IsNullOrEmpty(connInfo.ProviderName))
					info.Provider = DbProviderFactories.GetFactory(connInfo.ProviderName);
				else
					info.Provider = DbProviderFactories.GetFactory(DefaultProviderName);

				connectionStringName = connInfo.ConnectionString;
			}
			else
				throw new InvalidOperationException(Resources.InvalidConnectionStringName + ": " + connectionStringName);
			return connectionStringName;
		}
#endif

    }
}
