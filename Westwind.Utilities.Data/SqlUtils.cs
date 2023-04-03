using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Westwind.Utilities.Properties;


#if NETCORE
    using Microsoft.Data.SqlClient;
#else
    using System.Data.SqlClient;
#endif

namespace Westwind.Utilities.Data
{
    public  class SqlUtils
    {

        #region Provider Factories

        /// <summary>
        /// Loads a SQL Provider factory based on the DbFactory type name and assembly.       
        /// </summary>
        /// <param name="dbProviderFactoryTypename">Type name of the DbProviderFactory</param>
        /// <param name="assemblyName">Short assembly name of the provider factory. Note: Host project needs to have a reference to this assembly</param>
        /// <returns></returns>
        public static DbProviderFactory GetDbProviderFactory(string dbProviderFactoryTypename, string assemblyName)
        {
            var instance = ReflectionUtils.GetStaticProperty(dbProviderFactoryTypename, "Instance");
            if (instance == null)
            {
                var a = ReflectionUtils.LoadAssembly(assemblyName);
                if (a != null)
                    instance = ReflectionUtils.GetStaticProperty(dbProviderFactoryTypename, "Instance");
            }

            if (instance == null)
                throw new InvalidOperationException(string.Format(Resources.UnableToRetrieveDbProviderFactoryForm, dbProviderFactoryTypename));

            return instance as DbProviderFactory;
        }

        /// <summary>
        /// This method loads various providers dynamically similar to the 
        /// way that DbProviderFactories.GetFactory() works except that
        /// this API is not available on .NET Standard 2.0
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static DbProviderFactory GetDbProviderFactory(DataAccessProviderTypes type)
        {
            if (type == DataAccessProviderTypes.SqlServer)
                return SqlClientFactory.Instance; // this library has a ref to SqlClient so this works

            if (type == DataAccessProviderTypes.SqLite)
                return GetDbProviderFactory("System.Data.SQLite.SQLiteFactory", "System.Data.SQLite");
            if (type == DataAccessProviderTypes.MySql)
                return GetDbProviderFactory("MySql.Data.MySqlClient.MySqlClientFactory", "MySql.Data");
            if (type == DataAccessProviderTypes.PostgreSql)
                return GetDbProviderFactory("Npgsql.NpgsqlFactory", "Npgsql");
#if NETFULL
            if (type == DataAccessProviderTypes.OleDb)
                return System.Data.OleDb.OleDbFactory.Instance;
            if (type == DataAccessProviderTypes.SqlServerCompact)
                return DbProviderFactories.GetFactory("System.Data.SqlServerCe.4.0");                
#endif

            throw new NotSupportedException(string.Format(Resources.UnsupportedProviderFactory, type.ToString()));
        }



        /// <summary>
        /// Returns a provider factory using the old Provider Model names from full framework .NET.
        /// Simply calls DbProviderFactories.
        /// </summary>
        /// <param name="providerName"></param>
        /// <returns></returns>
        public static DbProviderFactory GetDbProviderFactory(string providerName)
        {
#if NETFULL
            return DbProviderFactories.GetFactory(providerName);
#else
            var lowerProvider = providerName.ToLower();

            if (lowerProvider == "system.data.sqlclient")
                return GetDbProviderFactory(DataAccessProviderTypes.SqlServer);
            if (lowerProvider == "system.data.sqlite" || lowerProvider == "microsoft.data.sqlite")
                return GetDbProviderFactory(DataAccessProviderTypes.SqLite);
            if (lowerProvider == "mysql.data.mysqlclient" || lowerProvider == "mysql.data")
                return GetDbProviderFactory(DataAccessProviderTypes.MySql);
            if (lowerProvider == "npgsql")
                return GetDbProviderFactory(DataAccessProviderTypes.PostgreSql);

            throw new NotSupportedException(string.Format(Resources.UnsupportedProviderFactory, providerName));
#endif
        }

        #endregion


        #region Minimal Sql Data Access Function

        /// <summary>
        /// Creates a Command object and opens a connection
        /// </summary>
        /// <param name="ConnectionString"></param>
        /// <param name="Sql"></param>
        /// <returns></returns>
        public static SqlCommand GetSqlCommand(string ConnectionString, string Sql, params SqlParameter[] Parameters)
        {
            SqlCommand Command = new SqlCommand();
            Command.CommandText = Sql;

            try
            {
#if NETFULL
				if (!ConnectionString.Contains(';'))
                    ConnectionString =  ConfigurationManager.ConnectionStrings[ConnectionString].ConnectionString;
#endif

                Command.Connection = new SqlConnection(ConnectionString);
                Command.Connection.Open();
            }
            catch
            {
                return null;
            }


            if (Parameters != null)
            {
                foreach (SqlParameter Parm in Parameters)
                {
                    Command.Parameters.Add(Parm);
                }
            }

            return Command;
        }

        /// <summary>
        /// Returns a SqlDataReader object from a SQL string.
        /// 
        /// Please ensure you close the Reader object
        /// </summary>
        /// <param name="ConnectionString"></param>
        /// <param name="Sql"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public static SqlDataReader GetSqlDataReader(string ConnectionString, string Sql, params SqlParameter[] Parameters)
        {
            SqlCommand Command = GetSqlCommand(ConnectionString, Sql, Parameters);
            if (Command == null)
                return null;

            SqlDataReader Reader = null;
            try
            {
                Reader = Command.ExecuteReader();
            }
            catch
            {
                CloseConnection(Command);
                return null;
            }

            return Reader;
        }

        /// <summary>
        /// Returns a DataTable from a Sql Command string passed in.
        /// </summary>
        /// <param name="Tablename"></param>
        /// <param name="ConnectionString"></param>
        /// <param name="Sql"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public static DataTable GetDataTable(string Tablename, string ConnectionString, string Sql, params SqlParameter[] Parameters)
        {
            SqlCommand Command = GetSqlCommand(ConnectionString, Sql, Parameters);
            if (Command == null)
                return null;

            SqlDataAdapter Adapter = new SqlDataAdapter(Command);

            DataTable dt = new DataTable(Tablename);

            try
            {
                Adapter.Fill(dt);
            }
            catch
            {
                return null;
            }
            finally
            {
                CloseConnection(Command);
            }

            return dt;
        }


        /// <summary>
        /// Closes a connection
        /// </summary>
        /// <param name="Command"></param>
        public static void CloseConnection(SqlCommand Command)
        {
            if (Command.Connection != null &&
                Command.Connection.State == ConnectionState.Open)
                Command.Connection.Close();
        }
        #endregion

    }
}
