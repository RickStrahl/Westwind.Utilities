﻿#region License
//#define SupportWebRequestProvider
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2009
 *          http://www.west-wind.com/
 * 
 * Created: 09/12/2009
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 **************************************************************  
*/
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Westwind.Utilities.Properties;


#if NETCORE
    using Microsoft.Data.SqlClient;
#else
    using System.Data.SqlClient;
#endif

namespace Westwind.Utilities.Data
{
    /// <summary>
    /// Base Data Access Layer (DAL) for ADO.NET SQL operations.
    /// Provides easy, single method operations to retrieve DataReader,
    /// DataTable, DataSet and Entities, perform non-query operations,
    /// call stored procedures.
    /// 
    /// This abstract class implements most data operations using a
    /// configured DbProvider. Subclasses implement specific database
    /// providers and override a few methods that might have provider
    /// specific SQL Syntax.
    /// </summary>
    [DebuggerDisplay("{ ErrorMessage } {ConnectionString} {LastSql}")]
    public abstract class DataAccessBase : IDisposable
    {        

		/// <summary>
		/// Default constructor that should be called back to 
		/// by subclasses. Parameterless assumes default provider
		/// and no connection string which must be explicitly set.
		/// </summary>
		protected DataAccessBase()
		{
		    dbProvider = SqlClientFactory.Instance;
        }

        /// <summary>
        /// Most common constructor that expects a connection string or 
        /// connection string name from a .config file. If a connection
        /// string is provided the default provider is used.
        /// </summary>
        /// <param name="connectionString"></param>
        protected DataAccessBase(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException(Resources.AConnectionStringMustBePassedToTheConstructor);

            // sets dbProvider and ConnectionString properties
            // based on connectionString Name or full connection string
            GetConnectionInfo(connectionString,null);            
        }

#if NETFULL
		/// <summary>
		/// Constructor that expects a full connection string and provider
		/// for creating a SQL instance. To be called by the same implementation
		/// on a subclass.
		/// </summary>
		/// <param name="connectionString"></param>
		/// <param name="providerName"></param>
		protected DataAccessBase(string connectionString, string providerName)
        {
            ConnectionString = connectionString;
            dbProvider = DbProviderFactories.GetFactory(providerName);

            var cmd = dbProvider.CreateCommand();
            cmd.CommandText = "select * from customers";
            cmd.Connection = dbProvider.CreateConnection();
            cmd.Connection.ConnectionString = ConnectionString;
            var reader = cmd.ExecuteReader();

        }
#endif

        /// <summary>
        /// Constructor that expects a full connection string and provider
        /// for creating a SQL instance. To be called by the same implementation
        /// on a subclass.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="providerName"></param>
        protected DataAccessBase(string connectionString, DbProviderFactory provider)
		{
			ConnectionString = connectionString;
			dbProvider = provider;			
		}


        /// <summary>
        /// Create a DataAccess component with a specific database provider
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="providerType"></param>
        public DataAccessBase(string connectionString, DataAccessProviderTypes providerType)
        {
            ConnectionString = connectionString;
            DbProviderFactory instance = DataUtils.GetDbProviderFactory(providerType);

            ConnectionString = connectionString;
            dbProvider = instance ?? throw new InvalidOperationException("Can't load database provider: " + providerType.ToString());
        }




        /// <summary>
        /// Figures out the dbProvider and Connection string from a 
        /// connectionString name in a config file or explicit 
        /// ConnectionString and provider.         
        /// </summary>
        /// <param name="connectionString">Config file connection name or full connection string</param>
        /// <param name="providerName">optional provider name. If not passed with a connection string is considered Sql Server</param>
        public void GetConnectionInfo(string connectionString, string providerName = null)
        {
            // throws if connection string is invalid or missing
            var connInfo = ConnectionStringInfo.GetConnectionStringInfo(connectionString, providerName);

            ConnectionString = connInfo.ConnectionString;
            dbProvider = connInfo.Provider;                       
        }

        /// <summary>
        /// The internally used dbProvider
        /// </summary>
        public DbProviderFactory dbProvider = null;
        
        /// <summary>
        /// An error message if a method fails
        /// </summary>
        public virtual string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Optional error number returned by failed SQL commands
        /// </summary>
        public int ErrorNumber { get; set; } = 0;

        public Exception ErrorException { get; set; }

        public bool ThrowExceptions { get; set; } = false;


        /// <summary>
        /// The prefix used by the provider
        /// </summary>
        public string ParameterPrefix { get; set; } = "@";

        /// <summary>
        /// Determines whether parameters are positional or named. Positional
        /// parameters are added without adding the name using just the ParameterPrefix
        /// </summary>
        public bool UsePositionalParameters { get; set; } = false;

        /// <summary>
        /// Character used for the left bracket on field names. Can be empty or null to use none
        /// </summary>
        public string LeftFieldBracket { get; set; } = "[";

        /// <summary>
        /// Character used for the right bracket on field names. Can be empty or null to use none
        /// </summary>
        public string RightFieldBracket { get; set; } = "]";

        /// <summary>
        /// ConnectionString for the data access component
        /// </summary>
        public virtual string ConnectionString { get; set; } = string.Empty;



        /// <summary>
        /// A SQL Transaction object that may be active. You can 
        /// also set this object explcitly
        /// </summary>
        public virtual DbTransaction Transaction { get; set; }


        /// <summary>
        /// The SQL Connection object used for connections
        /// </summary>
        public virtual DbConnection Connection
        {
            get { return _Connection; }
            set { _Connection = value; }
        }
        protected DbConnection _Connection = null;

        /// <summary>
        /// The Sql Command execution Timeout in seconds.
        /// Set to -1 for whatever the system default is.
        /// Set to 0 to never timeout (not recommended).
        /// </summary>
        public int Timeout { get; set; } = -1;


        /// <summary>
        /// Determines whether extended schema information is returned for 
        /// queries from the server. Useful if schema needs to be returned
        /// as part of DataSet XML creation 
        /// </summary>
        public virtual bool ExecuteWithSchema { get; set; } = false;

        /// <summary>
        /// Holds the last SQL string executed
        /// </summary>
        public string LastSql { get; set; }

#region Connection Operations
        /// <summary>
        /// Opens a Sql Connection based on the connection string.
        /// Called internally but externally accessible. Sets the internal
        /// _Connection property.
        /// </summary>
        /// <returns></returns>
        /// <summary>
        /// Opens a Sql Connection based on the connection string.
        /// Called internally but externally accessible. Sets the internal
        /// _Connection property.
        /// </summary>
        /// <returns></returns>
        public virtual bool OpenConnection()
        {
            try
            {
                if (_Connection == null)
                {
                    if (ConnectionString.Contains("="))
                    {
                        _Connection = dbProvider.CreateConnection();
                        _Connection.ConnectionString = ConnectionString;
                    }
                    else
                    {
                        var connInfo = ConnectionStringInfo.GetConnectionStringInfo(ConnectionString);
                        if (connInfo == null)
                        {
                            SetError(Resources.InvalidConnectionString);

                            if (ThrowExceptions)
                                throw new ApplicationException(ErrorMessage);

                            return false;
                        }

                        dbProvider = connInfo.Provider;
                        ConnectionString = connInfo.ConnectionString;
                        
                        _Connection = dbProvider.CreateConnection();
                        _Connection.ConnectionString = ConnectionString;
                    }
                }
                
                if (_Connection.State != ConnectionState.Open)
                    _Connection.Open();
            }
            catch (SqlException ex)
            {
                SetError(string.Format(Resources.ConnectionOpeningFailure, ex.Message));
                return false;
            }
//#if !NETCORE
//            catch (OleDbException ex)
//            {
//                SetError(ex);
//                return false;
//            }
//#endif
            catch (DbException ex)
            {
                SetError(string.Format(Resources.ConnectionOpeningFailure, ex.Message));
                return false;
            }
            catch (Exception ex)
            {
                SetError(string.Format(Resources.ConnectionOpeningFailure, ex.GetBaseException().Message));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Closes a connection
        /// </summary>
        /// <param name="Command"></param>
        public virtual void CloseConnection(DbCommand Command)
        {
            if (Transaction != null)
                return;

            if (Command.Connection != null &&
                Command.Connection.State != ConnectionState.Closed)
                Command.Connection.Close();
            
            _Connection = null;
        }
        /// <summary>
        /// Closes an active connection. If a transaction is pending the 
        /// connection is held open.
        /// </summary>
        public virtual void CloseConnection()
        {
            if (Transaction != null)
                return;

            if (_Connection != null &&
                _Connection.State != ConnectionState.Closed)
                _Connection.Close();

            _Connection = null;
        }

#endregion

#region Core Operations

        /// <summary>
        /// Creates a Command object and opens a connection
        /// </summary>        
        /// <param name="sql">Sql string to execute</param>
        /// <param name="parameters">Either values mapping to @0,@1,@2 etc. or DbParameter objects created with CreateParameter()</param>
        /// <returns>Command object or null on error</returns>
        public virtual DbCommand CreateCommand(string sql, CommandType commandType, params object[] parameters)
        {
            SetError();

            DbCommand command = dbProvider.CreateCommand();
            command.CommandType = commandType;
            command.CommandText = sql;
            if (Timeout > -1)
                command.CommandTimeout = Timeout;

            try
            {
                if (Transaction != null)
                {
                    command.Transaction = Transaction;
                    command.Connection = Transaction.Connection;
                }
                else
                {
                    if (!OpenConnection())
                        return null;

                    command.Connection = _Connection;
                }
            }
            catch (Exception ex)
            {
                SetError(ex);
                return null;
            }

            if (parameters != null)
                AddParameters(command,parameters);
            

            return command;
        }

        /// <summary>
        /// Creates a Command object and opens a connection
        /// </summary>        
        /// <param name="sql">Sql string to execute</param>
        /// <param name="parameters">Either values mapping to @0,@1,@2 etc. or DbParameter objects created with CreateParameter()</param>
        /// <returns>command object</returns>
        public virtual DbCommand CreateCommand(string sql, params object[] parameters)
        {
            return CreateCommand(sql, CommandType.Text, parameters);
        }

        /// <summary>
        /// Adds parameters to a DbCommand instance. Parses value and DbParameter parameters
        /// properly into the command's Parameters collection.
        /// </summary>
        /// <param name="command">A preconfigured DbCommand object that should have all connection information set</param>
        /// <param name="parameters">
        /// DbParameters (CreateParameter()) for named parameters
        /// or use @0,@1 parms in SQL and plain values
        /// </param>
        protected void AddParameters(DbCommand command, object[] parameters)
        {
            if (parameters != null)
            {
                var parmCount = 0;
                foreach (var parameter in parameters)
                {
                    if (parameter is DbParameter)
                        command.Parameters.Add(parameter);
                    else
                    {                        
                        var parm = CreateParameter(ParameterPrefix + parmCount, parameter);
                        command.Parameters.Add(parm);
                        parmCount++;                        
                    }
                }
            }

        }
        
        /// <summary>
        /// Used to create named parameters to pass to commands or the various
        /// methods of this class.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public virtual DbParameter CreateParameter(string parameterName, object value)
        {
            DbParameter parm = dbProvider.CreateParameter();
            parm.ParameterName = parameterName;
            if (value == null)
                value = DBNull.Value;
            parm.Value = value;
            return parm;
        }


        /// <summary>
        /// Used to create named parameters to pass to commands or the various
        /// methods of this class.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public virtual DbParameter CreateParameter(string parameterName, object value, ParameterDirection parameterDirection = ParameterDirection.Input)
        {
            DbParameter parm = CreateParameter(parameterName, value);
            parm.Direction = parameterDirection;
            return parm;
        }

        /// <summary>
        /// Used to create named parameters to pass to commands or the various
        /// methods of this class.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public virtual DbParameter CreateParameter(string parameterName, object value, int size)
        {
            DbParameter parm = CreateParameter(parameterName, value);
            parm.Size = size;
            return parm;
        }

        /// <summary>
        /// Used to create named parameters to pass to commands or the various
        /// methods of this class.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public virtual DbParameter CreateParameter(string parameterName, object value, DbType type)
        {
            DbParameter parm = CreateParameter(parameterName, value);
            parm.DbType = type;
            return parm;
        }

        /// <summary>
        /// Used to create named parameters to pass to commands or the various
        /// methods of this class.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public virtual DbParameter CreateParameter(string parameterName, object value, DbType type, int size)
        {
            DbParameter parm = CreateParameter(parameterName, value);
            parm.DbType = type;
            parm.Size = size;
            return parm;
        }

        #endregion

        #region Transactions
        /// <summary>
        /// Starts a new transaction on this connection/instance
        /// </summary>
        /// <remarks>Opens a Connection and keeps it open for the duration of the transaction. Calls to `.CloseConnection` while the transaction is active have no effect.</remarks>
        /// <returns></returns>
        public virtual bool BeginTransaction()
        {
            if (_Connection == null)
            {
                if (!OpenConnection())
                    return false;
            }            

            Transaction = _Connection.BeginTransaction();
            if (Transaction == null)
                return false;

            return true;
        }

        /// <summary>
        /// Commits all changes to the database and ends the transaction
        /// </summary>
        /// <remarks>Closes Connection</remarks>
        /// <returns></returns>
        public virtual bool CommitTransaction()
        {
            if (Transaction == null)
            {
                SetError(Resources.NoActiveTransactionToCommit);
                if (ThrowExceptions)
                    new InvalidOperationException(Resources.NoActiveTransactionToCommit);
                return false;
            }

            Transaction.Commit();
            Transaction = null;

            CloseConnection();

            return true;
        }

        /// <summary>
        /// Rolls back a transaction
        /// </summary>
        /// <remarks>Closes Connection</remarks>
        /// <returns></returns>
        public virtual bool RollbackTransaction()
        {
            if (Transaction == null)
                return true;

            Transaction.Rollback();
            Transaction = null;

            CloseConnection();

            return true;
        }

        #endregion

        #region Non-list Sql Commands 
        /// <summary>
        /// Executes a non-query command and returns the affected records
        /// </summary>
        /// <param name="Command">Command should be created with GetSqlCommand to have open connection</param>       
        /// <returns>Affected Record count or -1 on error</returns>
        public virtual int ExecuteNonQuery(DbCommand Command)
        {
            SetError();

            int RecordCount = 0;

            try
            {
                LastSql = Command.CommandText;

                RecordCount = Command.ExecuteNonQuery();
                if (RecordCount == -1)
                    RecordCount = 0;
            }
            catch (DbException ex)
            {
                RecordCount = -1;
                SetError(ex);;
            }
            catch (Exception ex)
            {
                RecordCount = -1;
                SetError(ex);
            }
            finally
            {
                CloseConnection();
            }

            return RecordCount;
        }


        /// <summary>
        /// Executes a command that doesn't return any data. The result
        /// returns the number of records affected or -1 on error.
        /// </summary>
        /// <param name="sql">SQL statement as a string</param>
        /// <param name="parameters">
        /// DbParameters (CreateParameter()) for named parameters
        /// or use @0,@1 parms in SQL and plain values
        /// </param>
        /// <returns></returns>
        /// <summary>
        /// Executes a command that doesn't return a data result. You can return
        /// output parameters and you do receive an AffectedRecords counter.
        /// .setItem("list_html", JSON.stringify(data));
        /// </summary>        
        public virtual int ExecuteNonQuery(string sql, params object[] parameters)
        {
            DbCommand command = CreateCommand(sql,parameters);
            if (command == null)
                return -1;

            return ExecuteNonQuery(command);
        }


#if !NET40
        /// <summary>
        /// Executes a non-query command and returns the affected records
        /// </summary>
        /// <param name="Command">Command should be created with GetSqlCommand to have open connection</param>       
        /// <returns>Affected Record count or -1 on error</returns>
        public virtual async Task<int> ExecuteNonQueryAsync(DbCommand Command)
        {
            SetError();

            int RecordCount = 0;

            try
            {
                LastSql = Command.CommandText;

                RecordCount = await Command.ExecuteNonQueryAsync();
                if (RecordCount == -1)
                    RecordCount = 0;
            }
            catch (DbException ex)
            {
                RecordCount = -1;
                SetError(ex); ;
            }
            catch (Exception ex)
            {
                RecordCount = -1;
                SetError(ex);
            }
            finally
            {
                CloseConnection();
            }

            return RecordCount;
        }


        /// <summary>
        /// Executes a command that doesn't return any data. The result
        /// returns the number of records affected or -1 on error.
        /// </summary>
        /// <param name="sql">SQL statement as a string</param>
        /// <param name="parameters">
        /// DbParameters (CreateParameter()) for named parameters
        /// or use @0,@1 parms in SQL and plain values
        /// </param>
        /// <returns></returns>
        /// <summary>
        /// Executes a command that doesn't return a data result. You can return
        /// output parameters and you do receive an AffectedRecords counter.
        /// .setItem("list_html", JSON.stringify(data));
        /// </summary>        
        public virtual async Task<int> ExecuteNonQueryAsync(string sql, params object[] parameters)
        {
            DbCommand command = CreateCommand(sql, parameters);
            if (command == null)
                return -1;

            int result = await ExecuteNonQueryAsync(command);
            return result;
        }
#endif


        /// <summary>
        /// Executes a command and returns a scalar value from it
        /// </summary>
        /// <param name="command">DbCommand containing command to run</param>
        /// <param name="parameters">
        /// DbParameters (CreateParameter()) for named parameters
        ///  or use @0,@1 parms in SQL and plain values
        /// </param>
        /// <returns>value or null on failure</returns>        
        public virtual object ExecuteScalar(DbCommand command, params object[] parameters)
        {
            SetError();

            AddParameters(command, parameters);

            object Result = null;
            try
            {
                LastSql = command.CommandText;
                Result = command.ExecuteScalar();
            }
            catch (Exception ex)
            {
                SetError(ex.GetBaseException());
            }
            finally
            {
                CloseConnection();
            }

            return Result;
        }
        /// <summary>
        /// Executes a Sql command and returns a single value from it.
        /// </summary>
        /// <param name="Sql">Sql string to execute</param>
        /// <param name="parameters">
        /// DbParameters (CreateParameter()) for named parameters
        /// or use @0,@1 parms in SQL and plain values
        /// </param>
        /// <returns>Result value or null. Check ErrorMessage on Null if unexpected</returns>
        public virtual object ExecuteScalar(string sql, params object[] parameters)
        {
            SetError();

            DbCommand command = CreateCommand(sql, parameters);
            if (command == null)
                return null;

            return ExecuteScalar(command, null);
        }

#if !NET40
        /// <summary>
        /// Executes a command and returns a scalar value from it
        /// </summary>
        /// <param name="command">DbCommand containing command to run</param>
        /// <param name="parameters">
        /// DbParameters (CreateParameter()) for named parameters
        ///  or use @0,@1 parms in SQL and plain values
        /// </param>
        /// <returns>value or null on failure</returns>        
        public virtual async Task<object> ExecuteScalarAsync(DbCommand command, params object[] parameters)
        {
            SetError();

            AddParameters(command, parameters);

            object Result = null;
            try
            {
                LastSql = command.CommandText;
                Result = await command.ExecuteScalarAsync();
            }
            catch (Exception ex)
            {
                SetError(ex.GetBaseException());
            }
            finally
            {
                CloseConnection();
            }

            return Result;
        }

        /// <summary>
        /// Executes a Sql command and returns a single value from it.
        /// </summary>
        /// <param name="Sql">Sql string to execute</param>
        /// <param name="parameters">
        /// DbParameters (CreateParameter()) for named parameters
        /// or use @0,@1 parms in SQL and plain values
        /// </param>
        /// <returns>Result value or null. Check ErrorMessage on Null if unexpected</returns>
        public virtual async Task<object> ExecuteScalarAsync(string sql, params object[] parameters)
        {
            SetError();

            DbCommand command = CreateCommand(sql, parameters);
            if (command == null)
                return null;

            return await ExecuteScalarAsync(command, null);
        }
#endif

        /// <summary>
        /// Executes a long SQL script that contains batches (GO commands). This code
        /// breaks the script into individual commands and captures all execution errors.
        /// 
        /// If ContinueOnError is false, operations are run inside of a transaction and
        /// changes are rolled back. If true commands are accepted even if failures occur
        /// and are not rolled back.
        /// </summary>
        /// <param name="script"></param>
        /// <param name="continueOnError"></param>
        /// <param name="scriptIsFile"></param>
        /// <returns></returns>
        public bool RunSqlScript(string script, bool continueOnError = false, bool scriptIsFile = false)
        {
            SetError();

            if (scriptIsFile)
            {
                try
                {
                    script = File.ReadAllText(script);
                }
                catch (Exception ex)
                {
                    SetError(ex.GetBaseException());
                    return false;
                }
            }

            // Normalize line endings to \n
            string scriptNormal = script.Replace("\r\n", "\n").Replace("\r", "\n");
            string[] scriptBlocks = Regex.Split(scriptNormal + "\n", "GO\n");

            string errors = "";

            if (!continueOnError)
                BeginTransaction();

            foreach (string block in scriptBlocks)
            {
                if (string.IsNullOrEmpty(block.TrimEnd()))
                    continue;

                if (ExecuteNonQuery(block) == -1)
                {
                    errors = ErrorMessage +  block;
                    if (!continueOnError)
                    {
                        RollbackTransaction();
                        return false;
                    }
                }
            }

            if (!continueOnError)
                CommitTransaction();

            if (string.IsNullOrEmpty(errors))
                return true;

            ErrorMessage = errors;
            return false;
        }


        /// <summary>
        /// Determines whether a table exists
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        public virtual bool DoesTableExist(string tablename, string schema = null)
        {
            var sql = @"SELECT TABLE_CATALOG FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @0";

            if (!string.IsNullOrEmpty(schema))
                sql += " AND TABLE_SCHEMA = @1";

            var cat = ExecuteScalar(sql, tablename, schema);

            return !(cat is null);
        }

        #endregion 

        #region Sql Execution

        /// <summary>
        /// Executes a SQL Command object and returns a SqlDataReader object
        /// </summary>
        /// <param name="command">Command should be created with GetSqlCommand and open connection</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <returns>A SqlDataReader. Make sure to call Close() to close the underlying connection.</returns>
        //public abstract DbDataReader ExecuteReader(DbCommand Command, params DbParameter[] Parameters)
        public virtual DbDataReader ExecuteReader(DbCommand command, params object[] parameters)
        {
            SetError();

            if (command.Connection == null || command.Connection.State != ConnectionState.Open)
            {
                if (!OpenConnection())
                    return null;

                command.Connection = _Connection;
            }

            AddParameters(command, parameters);

            DbDataReader Reader = null;
            try
            {
                LastSql = command.CommandText;
                Reader = command.ExecuteReader();
            }
            catch (Exception ex)
            {
                SetError(ex.GetBaseException());
                CloseConnection(command);
                return null;
            }

            return Reader;
        }

        /// <summary>
        /// Executes a SQL command against the server and returns a DbDataReader
        /// </summary>
        /// <param name="sql">Sql String</param>
        /// <param name="parameters">
        /// DbParameters (CreateParameter()) for named parameters
        /// or use @0,@1 parms in SQL and plain values
        /// </param>
        /// <returns></returns>
        public virtual DbDataReader ExecuteReader(string sql, params object[] parameters)
        {
            DbCommand command = CreateCommand(sql, parameters);
            if (command == null)
                return null;

            return ExecuteReader(command);
        }


	    /// <summary>
	    /// Executes a Sql statement and returns a dynamic DataReader instance 
	    /// that exposes each field as a property
	    /// </summary>
	    /// <param name="sql">Sql String to executeTable</param>
	    /// <param name="parameters">
	    /// DbParameters (CreateParameter()) for named parameters
	    /// or use @0,@1 parms in SQL and plain values
	    /// </param>
	    /// <returns></returns>
	    public virtual dynamic ExecuteDynamicDataReader(string sql, params object[] parameters)
	    {
		    var reader = ExecuteReader(sql, parameters);
		    return new DynamicDataReader(reader);
	    }

	    /// <summary>
	    /// Return a list of entities that are matched to an object
	    /// </summary>
	    /// <typeparam name="T">Type of object to create from data record</typeparam>
	    /// <param name="sql">Sql string</param>
	    /// <param name="parameters">
	    ///  DbParameters (CreateParameter()) for named parameters
	    ///  or use @0,@1 parms in SQL and plain values
	    /// </param> 
	    /// <returns>An enumerated list of objects or null</returns>
	    [Obsolete("Use the Query method instead with the same syntax")]
	    public virtual IEnumerable<T> ExecuteReader<T>(string sql, params object[] parameters)
		    where T: class, new()
	    {
		    return Query<T>(sql, null, parameters);
	    }

	    /// <summary>
	    /// Allows querying and return a list of entities.
	    /// </summary>	    
	    /// <typeparam name="T"></typeparam>
	    /// <param name="command"></param>
	    /// <param name="parameters">
	    /// DbParameters (CreateParameter()) for named parameters
	    ///  or use @0,@1 parms in SQL and plain values
	    /// </param>
	    /// <returns></returns>
	    [Obsolete("Use the Query method instead with the same syntax")]
	    public virtual IEnumerable<T> ExecuteReader<T>(DbCommand command, params object[] parameters)
		    where T : class, new()
	    {
		    return Query<T>(command, parameters);
	    }

        /// <summary>
        /// Executes a SQL statement and creates an object list using
        /// optimized Reflection.
        /// 
        /// Not very efficient but provides an easy way to retrieve 
        /// an object list from query.
        /// </summary>
        /// <typeparam name="T">Entity type to create from DataReader data</typeparam>
        /// <param name="sql">Sql string to execute</param>        
        /// <param name="parameters">
        /// DbParameters (CreateParameter()) for named parameters
        /// or use @0,@1 parms in SQL and plain values
        /// </param>
        /// <returns>List of objects or null. Null is returned if there are no matches</returns>       
        public virtual IEnumerable<T> Query<T>(string sql, params object[] parameters)            
            where T : class, new()
        {
            var reader = ExecuteReader(sql, parameters);
            
            if (reader == null)
                return null;

            try
            {
                return DataUtils.DataReaderToIEnumerable<T>(reader, null);                
            }
            catch (Exception ex)
            {
                SetError(ex);
                return null;
            }                            
        }

        /// <summary>
        /// Returns list of objects from a query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual List<T> QueryList<T>(string sql, params object[] parameters)
                where T : class, new()
        {
            var reader = ExecuteReader(sql, parameters);

            if (reader == null)
                return null;

            try
            {
                return DataUtils.DataReaderToObjectList<T>(reader,null);
            }
            catch (Exception ex)
            {
                SetError(ex);
                return null;
            }
        }

        /// <summary>
        /// Returns list of objects from a query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">Sql Statement string</param>
        /// <param name="propertiesToSkip">Comma delimited list of property names to skip</param>
        /// <param name="parameters">
        /// DbParameters (CreateParameter()) for named parameters
        /// or use @0,@1 parms in SQL and plain values
        /// </param>
        /// <returns></returns>
        public virtual List<T> QueryListWithExclusions<T>(string sql, string propertiesToSkip, params object[] parameters)
                where T : class, new()
        {
            var reader = ExecuteReader(sql, parameters);

            if (reader == null)
                return null;

            try
            {
                return DataUtils.DataReaderToObjectList<T>(reader, propertiesToSkip);
            }
            catch (Exception ex)
            {
                SetError(ex);
                return null;
            }
        }

        /// <summary>
        /// Executes a SQL command and creates an object list using
        /// Reflection.
        /// 
        /// Not very efficient but provides an easy way to retrieve
        /// object lists from queries.
        /// </summary>
        /// <typeparam name="T">Entity type to create from DataReader data</typeparam>
        /// <param name="command">Command object containing configured SQL command to execute</param>        
        /// <param name="parameters">
        /// DbParameters (CreateParameter()) for named parameters
        ///  or use @0,@1 parms in SQL and plain values
        /// </param>
        /// <returns>List of objects or null. Null is returned if there are no matches</returns>   
        public virtual IEnumerable<T> Query<T>(DbCommand command, params object[] parameters)
            where T : class, new()
        {
            var reader = ExecuteReader(command, parameters);

            if (reader == null)
                return null;

            try
            {
                return DataUtils.DataReaderToIEnumerable<T>(reader, null);
            }
            catch (Exception ex)
            {
                SetError(ex);
                return null;
            }
        }

        /// <summary>
        /// Executes a SQL statement and creates an object list using
        /// Reflection.
        /// 
        /// Not very efficient but provides an easy way to retrieve
        /// object lists from queries
        /// </summary>
        /// <typeparam name="T">Entity type to create from DataReader data</typeparam>
        /// <param name="sql">Sql string to execute</param>        
        /// <param name="propertiesToExclude">Comma delimited list of properties that are not to be updated</param>
        /// <param name="parameters">
        ///  DbParameters (CreateParameter()) for named parameters
        ///  or use @0,@1 parms in SQL and plain values
        /// </param>
        /// <returns>List of objects</returns>        
        public virtual IEnumerable<T> QueryWithExclusions<T>(string sql, string propertiesToExclude, params object[] parameters)            
            where T: class, new()
        {
            IEnumerable<T> result;

            var reader = ExecuteReader(sql, parameters);
            
            if (reader == null)
                return null;

            try
            {
                result = DataUtils.DataReaderToIEnumerable<T>(reader, propertiesToExclude);
            }
            catch (Exception ex)
            {
                SetError(ex);
                return null;
            }
            
            return result;
        }


        /// <summary>
        /// Executes a SQL statement and creates an object list using
        /// Reflection.
        /// 
        /// Not very efficient but provides an easy way to retrieve
        /// </summary>
        /// <typeparam name="T">Entity type to create from DataReader data</typeparam>
        /// <param name="sql">Sql string to execute</param>        
        /// <param name="parameters">DbParameters to fill the SQL statement</param>
        /// <returns>List of objects</returns>
        public virtual IEnumerable<T> QueryWithExclusions<T>(DbCommand sqlCommand, string propertiesToExclude, params object[] parameters)
            where T : class, new()
        {
            var reader = ExecuteReader(sqlCommand, parameters);

            try
            {
                return DataUtils.DataReaderToIEnumerable<T>(reader, propertiesToExclude);
            }
            catch (Exception ex)
            {
                SetError(ex);
                return null;
            }
        }


        /// <summary>
        /// Calls a stored procedure that returns a cursor results
        /// The result is returned as a DataReader
        /// </summary>
        /// <param name="storedProc">Name of the Stored Procedure to call</param>
        /// <param name="parameters">
        /// Parameters to pass. Note that if you need to pass out/inout/return parameters
        /// you need to pass DbParameter instances or use the CreateParameter() method
        /// </param>
        /// <returns>A DataReader or null on failure</returns>
        public virtual DbDataReader ExecuteStoredProcedureReader(string storedProc, params object[] parameters)
        {
            var command = CreateCommand(storedProc, parameters);
            if (command == null)
                return null;
           
            command.CommandType = CommandType.StoredProcedure;

            return ExecuteReader(command);
        }


		/// <summary>
		/// Calls a stored procedure that returns a cursor results
		/// The result is returned as an IEnumerable&lt;T&gt;> list
		/// </summary>
		/// <example>
		///	IEnumerable&lt;Customer%gt; customers = context.Db.ExecuteStoredProcedureReader&lt;Customer&gt;('GetCustomers',         
		///              context.Db.CreateParameter('@cCompany','W%'));
		/// </example>
		/// <param name="storedProc">Name of the Stored Procedure to call</param>
		/// <param name="parameters">
		/// Use CreateParameter() for named, output or return parameters. Plain values for others.
		/// </param>
		/// <returns>A DataReader or null on failure</returns>
		public virtual IEnumerable<T> ExecuteStoredProcedureReader<T>(string storedProc, params object[] parameters)
            where T : class, new()
        {
            var command = CreateCommand(storedProc, parameters);
            if (command == null)
                return null;

            command.CommandType = CommandType.StoredProcedure;

            return Query<T>(command,null);
        }

        /// <summary>
        /// Executes a stored procedure that doesn't return a result set.
        /// </summary>
        /// <param name="storedProc">The Stored Procedure to call</param>
        /// <param name="parameters">
        /// Parameters to pass. Note that if you need to pass out/inout/return parameters
        /// you need to pass DbParameter instances or use the CreateParameter() method
        /// </param>
        /// <returns>> 0 or greater on success, -1 on failure</returns>
        public virtual int ExecuteStoredProcedureNonQuery(string storedProc, params object[] parameters)
        {
            var command = CreateCommand(storedProc, parameters);
            if (command == null)
                return -1;

            command.CommandType = CommandType.StoredProcedure;

            return ExecuteNonQuery(command);
        }


	    /// <summary>
        /// Returns a DataTable from a Sql Command string passed in.
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="command"></param>
        /// <param name="parameters">
        /// DbParameters (CreateParameter()) for named parameters
        ///  or use @0,@1 parms in SQL and plain values
        /// </param>
        /// <returns></returns>
        public virtual DataTable ExecuteTable(string tablename, DbCommand command, params object[] parameters)
        {
	        SetError();

            AddParameters(command, parameters);

            DbDataAdapter Adapter = dbProvider.CreateDataAdapter();
	        if (Adapter == null)
	        {
		        SetError("Failed to create data adapter.");
				return null;
	        }
	        Adapter.SelectCommand = command;
			
			LastSql = command.CommandText;

            DataTable dt = new DataTable(tablename);

            try
            {
                Adapter.Fill(dt);
            }
            catch (Exception ex)
            {
                SetError(ex.GetBaseException());
                return null;
            }
            finally
            {
                CloseConnection(command);
            }

            return dt;
        }

        /// <summary>
        /// Returns a DataTable from a Sql Command string passed in.
        /// </summary>
        /// <param name="Tablename"></param>
        /// <param name="ConnectionString"></param>
        /// <param name="Sql"></param>
        /// <param name="parameters">
        /// DbParameters (CreateParameter()) for named parameters
        ///  or use @0,@1 parms in SQL and plain values
        /// </param>
        /// <returns></returns>
        public virtual DataTable ExecuteTable(string Tablename, string Sql, params object[] Parameters)
        {
            SetError();

            DbCommand Command = CreateCommand(Sql, Parameters);
            if (Command == null)
                return null;

            return ExecuteTable(Tablename, Command);
        }


        /// <summary>
        /// Returns a DataSet/DataTable from a Sql Command string passed in. 
        /// </summary>
        /// <param name="Tablename">The name for the table generated or the base names</param>
        /// <param name="Command"></param>
        /// <param name="parameters">
        /// DbParameters (CreateParameter()) for named parameters
        ///  or use @0,@1 parms in SQL and plain values
        /// </param>
        /// <returns></returns>
        public virtual DataSet ExecuteDataSet(string Tablename, DbCommand Command, params object[] Parameters)
        {
            return ExecuteDataSet(null, Tablename, Command, Parameters);
        }

        /// <summary>
        /// Executes a SQL command against the server and returns a DataSet of the result
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameters">
        /// DbParameters (CreateParameter()) for named parameters
        /// or use @0,@1 parms in SQL and plain values
        /// </param>
        /// <returns></returns>
        public virtual DataSet ExecuteDataSet(string tablename, string sql, params object[] parameters)
        {
            return ExecuteDataSet(tablename, CreateCommand(sql), parameters);
        }


        /// <summary>
        /// Returns a DataSet from a Sql Command string passed in.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>        
        public virtual DataSet ExecuteDataSet(DataSet dataSet, string tableName, DbCommand command, params object[] parameters)
        {
            SetError();

            if (dataSet == null)
                dataSet = new DataSet();

            DbDataAdapter Adapter = dbProvider.CreateDataAdapter();
            Adapter.SelectCommand = command;
            LastSql = command.CommandText;


            if (ExecuteWithSchema)
                Adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

            AddParameters(command, parameters);

            DataTable dt = new DataTable(tableName);

            if (dataSet.Tables.Contains(tableName))
                dataSet.Tables.Remove(tableName);

            try
            {
                Adapter.Fill(dataSet, tableName);
            }
            catch (Exception ex)
            {
                SetError(ex);
                return null;
            }
            finally
            {
                CloseConnection(command);
            }

            return dataSet;
        }

        /// <summary>
        /// Returns a DataTable from a Sql Command string passed in.
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="Command"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual DataSet ExecuteDataSet(DataSet dataSet, string tablename, string sql, params object[] parameters)
        {
            DbCommand Command = CreateCommand(sql, parameters);
            if (Command == null)
                return null;

            return ExecuteDataSet(dataSet, tablename, Command);
        }


        /// <summary>
        /// Sql 2005 specific semi-generic paging routine
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="pageSize"></param>
        /// <param name="page"></param>
        /// <param name="sortOrderFields"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public virtual DbCommand CreatePagingCommand(string sql, int pageSize, int page, string sortOrderFields, params object[] Parameters)
        {
            int pos = sql.IndexOf("select ", 0, StringComparison.OrdinalIgnoreCase);
            if (pos == -1)
            {
                SetError("Invalid Command for paging. Must start with select and followed by field list");
                return null;
            }
            sql = StringUtils.ReplaceStringInstance(sql, "select", string.Empty, 1, true);

            string NewSql = string.Format(
            @"
select * FROM 
   (SELECT ROW_NUMBER() OVER (ORDER BY @OrderByFields) as __No,{0}) __TQuery
where __No > (@Page-1) * @PageSize and __No < (@Page * @PageSize + 1)
", sql);

            return CreateCommand(NewSql,
                            CreateParameter("@PageSize", pageSize),
                            CreateParameter("@Page", page),
                            CreateParameter("@OrderByFields", sortOrderFields));

        }

        #endregion

#region Generic Entity features
        /// <summary>
        /// Generic routine to retrieve an object from a database record
        /// The object properties must match the database fields.
        /// </summary>
        /// <param name="entity">The object to update</param>
        /// <param name="command">Database command object</param>
        /// <param name="propertiesToSkip"></param>
        /// <returns></returns>
        public virtual bool GetEntity(object entity, DbCommand command, string propertiesToSkip = null)
        {
            SetError();

            if (string.IsNullOrEmpty(propertiesToSkip))
                propertiesToSkip = string.Empty;

            DbDataReader reader = ExecuteReader(command);
            if (reader == null)
                return false;

            if (!reader.Read())
            {
                reader.Close();
                CloseConnection(command);
                return false;
            }

            DataUtils.DataReaderToObject(reader, entity, propertiesToSkip);

            reader.Close();
            CloseConnection();

            return true;
        }

        /// <summary>
        /// Retrieves a single record and returns it as an entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="sql"></param>        
        /// <param name="parameters">
        /// DbParameters (CreateParameter()) for named parameters
        /// or use @0,@1 parms in SQL and plain values
        /// </param>
        /// <returns></returns>
        public bool GetEntity(object entity, string sql, object[] parameters)
        {
            return GetEntity(entity, CreateCommand(sql,parameters),null);
        }



        /// <summary>
        /// Generic routine to return an Entity that matches the field names of a 
        /// table exactly.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="table"></param>
        /// <param name="keyField"></param>
        /// <param name="keyValue"></param>
        /// <param name="propertiesToSkip"></param>
        /// <returns></returns>
        public virtual bool GetEntity(object entity, string table, string keyField, object keyValue, string propertiesToSkip = null)
        {
            SetError();

            DbCommand Command = CreateCommand("select * from " + table + " where " + LeftFieldBracket + keyField + RightFieldBracket + "=" + ParameterPrefix + "Key",
                                                    CreateParameter(ParameterPrefix + "Key", keyValue));
            if (Command == null)
                return false;

            return GetEntity(entity, Command, propertiesToSkip);
        }

        /// <summary>
        /// Finds the first matching entity based on a keyfield and key value.
        /// Note the keyfield can be any field that is used in a WHERE clause.
        ///
        /// This method has been renamed from Find() to avoid ambiguous
        /// overload errors.
        /// </summary>
        /// <typeparam name="T">Type of entity to populate</typeparam>
        /// <param name="keyValue">Value to look up in keyfield</param>
        /// <param name="tableName">Name of the table to work on</param>
        /// <param name="keyField">Field that is used for the key lookup</param>
        /// <returns></returns>
        public virtual T FindKey<T>(object keyValue, string tableName,string keyField)
            where T: class,new()
        {
            T obj = new T();
            if (obj == null)
                return null;

            if (!GetEntity(obj, tableName, keyField, keyValue, null))
                return null;

            return obj;
        }

        /// <summary>
        /// Returns the first matching record retrieved from data based on a SQL statement
        /// as an entity or null if no match was found.
        ///
        /// NOTE: Key based method has been renamed to:
        /// `FindKey`
        /// </summary>
        /// <typeparam name="T">Entity type to fill</typeparam>
        /// <param name="sql">SQL string to execute. Use @0,@1,@2 for parameters.
        /// 
        /// Recommend you use `TOP1` in your SQL statements to limit the 
        /// amount of data returned from the underlying query even though
        /// a full list returns the same result.
        /// </param>
        /// <param name="parameters">
        /// DbParameters (CreateParameter()) for named parameters
        /// or use @0,@1 parms in SQL and plain values
        /// </param>
        /// <returns></returns>
        public virtual T Find<T>(string sql, params object[] parameters)
            where T : class,new()
        {
            T obj = new T();
            if (!GetEntity(obj, sql, parameters))
                return null;

            return obj;
        }

        /// <summary>
        /// Returns an entity that is the first match from a sql statement string.
        /// </summary>
        /// <typeparam name="T">Entity type to return</typeparam>
        /// <param name="sql">Sql string to execute. Use @0,@1,@2 for positional parameters</param>
        /// <param name="propertiesToSkip">fields to not update from the resultset</param>
        /// <param name="parameters">
        /// DbParameters (CreateParameter()) for named parameters
        /// or use @0,@1 parms in SQL and plain values
        /// </param>
        /// <returns></returns>
        public virtual T FindEx<T>(string sql, string propertiesToSkip, params object[] parameters)
            where T : class,new()
        {
            T obj = new T();
            if (!GetEntity(obj, CreateCommand( sql, parameters),propertiesToSkip))
                return null;

            return obj;
        }

        
        /// <summary>
        /// Updates an entity object that has matching fields in the database for each
        /// public property. Kind of a poor man's quick entity update mechanism.
        /// 
        /// Note this method will not save if the record doesn't already exist in the db.
        /// </summary>        
        /// <param name="entity">entity to update</param>
        /// <param name="table">the table name to update</param>
        /// <param name="keyField">keyfield used to find entity</param>
        /// <param name="propertiesToSkip"></param>
        /// <returns></returns>
        public virtual bool UpdateEntity(object entity, string table, string keyField, string propertiesToSkip = null)
        {
            SetError();

            var Command = GetUpdateEntityCommand(entity, table, keyField, propertiesToSkip);
            if (Command == null)
                return false;

            bool result;
            using (Command)
            {
                result = ExecuteNonQuery(Command) > -1;
                CloseConnection(Command);
            }

            return result;
        }



        /// <summary>
        /// Updates an entity object that has matching fields in the database for each
        /// public property. Kind of a poor man's quick entity update mechanism.
        /// 
        /// Note this method will not save if the record doesn't already exist in the db.
        /// </summary>        
        /// <param name="entity">entity to update</param>
        /// <param name="table">the table name to update</param>
        /// <param name="keyField">keyfield used to find entity</param>
        /// <param name="propertiesToSkip"></param>
        /// <returns></returns>
        public virtual DbCommand GetUpdateEntityCommand(object entity, string table, string keyField, string propertiesToSkip = null)
        {
            SetError();

            if (string.IsNullOrEmpty(propertiesToSkip))
                propertiesToSkip = string.Empty;
            else
                propertiesToSkip = "," + propertiesToSkip.ToLower() + ",";

            DbCommand Command = CreateCommand(string.Empty);

            Type ObjType = entity.GetType();

            StringBuilder sb = new StringBuilder();
            sb.Append("update " + table + " set ");

            PropertyInfo[] Properties = ObjType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo Property in Properties)
            {
                if (!Property.CanRead)
                    continue;

                string Name = Property.Name;

                if (propertiesToSkip.IndexOf("," + Name.ToLower() + ",") > -1)
                    continue;

                object Value = Property.GetValue(entity, null);

                string parmString = UsePositionalParameters ? ParameterPrefix : ParameterPrefix + Name;
                sb.Append(" " + LeftFieldBracket + Name + RightFieldBracket + "=" + parmString + ",");

                if (Value == null && Property.PropertyType == typeof(byte[]))
                {
                    Command.Parameters.Add(
                        CreateParameter(ParameterPrefix + Name, DBNull.Value, DataUtils.DotNetTypeToDbType(Property.PropertyType))
                    );
                }
                else
                    Command.Parameters.Add(CreateParameter(ParameterPrefix + Name, Value ?? DBNull.Value));
            }

            object pkValue = ReflectionUtils.GetProperty(entity, keyField);

            String CommandText = sb.ToString().TrimEnd(',') + " where " + keyField + "=" + ParameterPrefix + "__PK";

            Command.Parameters.Add(CreateParameter(ParameterPrefix + "__PK", pkValue));
            Command.CommandText = CommandText;

            return Command;
        }


        /// <summary>
        /// This version of UpdateEntity allows you to specify which fields to update and
        /// so is a bit more efficient as it only checks for specific fields in the database
        /// and the underlying table.
        /// </summary>
        /// <seealso cref="SaveEntity"/>
        /// <seealso cref="InsertEntity"/>
        /// <param name="entity">Entity to update</param>
        /// <param name="table">DB Table to udpate</param>
        /// <param name="keyField">The keyfield to query on</param>
        /// <param name="propertiesToSkip">fields to skip in update</param>
        /// <param name="fieldsToUpdate">fields that should be updated</param>
        /// <returns></returns>
        public virtual bool UpdateEntity(object entity, string table, string keyField, string propertiesToSkip, string fieldsToUpdate)
        {
            SetError();

            var Command = GetUpdateEntityCommand(entity, table, keyField, propertiesToSkip, fieldsToUpdate);
            if (Command == null)
                return false;

            bool result;
            using (Command) {
                result = ExecuteNonQuery(Command) > -1;
                CloseConnection(Command);
            }

            return result;
        }



        /// <summary>
        /// Gets a DbCommand that creates an update statement.
        /// that allows you to specify which fields to update and
        /// so is a bit more efficient as it only checks for specific fields in the database
        /// and the underlying table.
        /// </summary>
        /// <seealso cref="SaveEntity"/>
        /// <seealso cref="InsertEntity"/>
        /// <param name="entity">Entity to update</param>
        /// <param name="table">DB Table to udpate</param>
        /// <param name="keyField">The keyfield to query on</param>
        /// <param name="propertiesToSkip">fields to skip in update</param>
        /// <param name="fieldsToUpdate">fields that should be updated</param>
        /// <returns></returns>
        public virtual DbCommand GetUpdateEntityCommand(object entity, string table, 
            string keyField, string propertiesToSkip, string fieldsToUpdate)
        {
            SetError();

            if (propertiesToSkip == null)
                propertiesToSkip = string.Empty;
            else
                propertiesToSkip = "," + propertiesToSkip.ToLower() + ",";


            DbCommand Command = CreateCommand(string.Empty);
            if (Command == null)
            {
                SetError("Unable to create command.");
                return null;
            }

            Type ObjType = entity.GetType();

            StringBuilder sb = new StringBuilder();
            sb.Append("update " + table + " set ");

            string[] Fields = fieldsToUpdate.Split(',');
            foreach (string Name in Fields)
            {
                if (propertiesToSkip.IndexOf("," + Name.ToLower() + ",") > -1)
                    continue;

                PropertyInfo Property = ObjType.GetProperty(Name);
                if (Property == null)
                    continue;

                object Value = Property.GetValue(entity, null);

                string parmString = UsePositionalParameters ? ParameterPrefix : ParameterPrefix + Name;
                sb.Append(" " + LeftFieldBracket + Name + RightFieldBracket + "=" + parmString + ",");

                if (Value == null && Property.PropertyType == typeof(byte[]))
                    Command.Parameters.Add(CreateParameter(ParameterPrefix + Name, DBNull.Value,
                        DataUtils.DotNetTypeToDbType(Property.PropertyType)));
                else
                    Command.Parameters.Add(CreateParameter(ParameterPrefix + Name, Value ?? DBNull.Value));
            }

            object pkValue = ReflectionUtils.GetProperty(entity, keyField);

            // check to see if 
            string commandText = sb.ToString().TrimEnd(',') +
                                 " where " + LeftFieldBracket + keyField + RightFieldBracket + "=" + ParameterPrefix +
                                 (UsePositionalParameters ? "" : "__PK");
            Command.Parameters.Add(CreateParameter(ParameterPrefix + "__PK", pkValue));
            Command.CommandText = commandText;

            return Command;
        }



        /// <summary>
        /// Inserts an object into the database based on its type information.
        /// The properties must match the database structure and you can skip
        /// over fields in the propertiesToSkip list.        
        /// </summary>        
        /// <seealso cref="SaveEntity" />        
        /// <param name="entity"></param>
        /// <param name="table"></param>
        /// <param name="KeyField"></param>
        /// <param name="propertiesToSkip"></param>
        /// <returns>
        /// Scope Identity or Null (when returnIdentityKey is true
        /// Otherwise affected records
        /// </returns>
        public object InsertEntity(object entity, string table, string propertiesToSkip = null, bool returnIdentityKey = true)
        {
            SetError();
            DbCommand Command = GetInsertEntityCommand(entity, table, propertiesToSkip);

            using (Command)
            {
                if (returnIdentityKey)
                {
                    Command.CommandText += ";\r\n" + "select SCOPE_IDENTITY()";
                    return ExecuteScalar(Command);
                }

                int res = ExecuteNonQuery(Command);
                if (res < 0)
                    return null;

                return res;
            }
        }


#if !NET40
        /// <summary>
        /// Inserts an object into the database based on its type information.
        /// The properties must match the database structure and you can skip
        /// over fields in the propertiesToSkip list.        
        /// </summary>        
        /// <seealso cref="SaveEntity" />        
        /// <param name="entity"></param>
        /// <param name="table"></param>
        /// <param name="KeyField"></param>
        /// <param name="propertiesToSkip"></param>
        /// <returns>
        /// Scope Identity or Null (when returnIdentityKey is true
        /// Otherwise affected records
        /// </returns>
        public async Task<object> InsertEntityAsync(object entity, string table, string propertiesToSkip = null, bool returnIdentityKey = true)
        {
            SetError();
            DbCommand Command = GetInsertEntityCommand(entity, table, propertiesToSkip);

            using (Command)
            {
                if (returnIdentityKey)
                {
                    Command.CommandText += ";\r\n" + "select SCOPE_IDENTITY()";
                    return await ExecuteScalarAsync(Command);
                }

                int res = await ExecuteNonQueryAsync(Command);
                if (res < 0)
                    return null;

                return res;
            }
        }
#endif



        /// <summary>
        /// Gets the DbCommand used to insert an object into the database based on its type information.
        /// The properties must match the database structure and you can skip
        /// over fields in the propertiesToSkip list.        
        /// </summary>        
        /// <seealso cref="SaveEntity" />        
        /// <param name="entity"></param>
        /// <param name="table"></param>
        /// <param name="KeyField"></param>
        /// <returns>
        /// Scope Identity or Null (when returnIdentityKey is true
        /// Otherwise affected records
        /// </returns>
        public DbCommand GetInsertEntityCommand(object entity, string table, string propertiesToSkip = null)
        {
            SetError();

            if (string.IsNullOrEmpty(propertiesToSkip))
                propertiesToSkip = string.Empty;
            else
                propertiesToSkip = "," + propertiesToSkip.ToLower() + ",";


            DbCommand Command = CreateCommand(string.Empty);
            if (Command == null)
            {
                SetError("Unable to create DbCommand instance");
                return null;
            }

            Type ObjType = entity.GetType();

            StringBuilder FieldList = new StringBuilder();
            StringBuilder DataList = new StringBuilder();
            FieldList.Append("insert into " + table + " (");
            DataList.Append(" values (");

            PropertyInfo[] Properties = ObjType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo Property in Properties)
            {
                if (!Property.CanRead)
                    continue;

                string Name = Property.Name;

                if (propertiesToSkip.IndexOf("," + Name.ToLower() + ",") > -1)
                    continue;

                object Value = Property.GetValue(entity, null);

                FieldList.Append(" " + LeftFieldBracket + Name + RightFieldBracket + ",");

                string parmString = ParameterPrefix;
                if (!UsePositionalParameters)
                    parmString += Name;

                DataList.Append(parmString + ",");

                if (Value == null && Property.PropertyType == typeof(byte[]))
                    Command.Parameters.Add(CreateParameter(ParameterPrefix + Name, DBNull.Value,
                        DataUtils.DotNetTypeToDbType(Property.PropertyType)));
                else
                    Command.Parameters.Add(CreateParameter(ParameterPrefix + Name, Value ?? DBNull.Value));
            }

            Command.CommandText = FieldList.ToString().TrimEnd(',') + ") " +
                                 DataList.ToString().TrimEnd(',') + ")";

            return Command;
        }


        /// <summary>
        /// Saves an enity into the database using insert or update as required.
        /// Requires a keyfield that exists on both the entity and the database.
        /// </summary>
        /// <param name="entity">entity to save</param>
        /// <param name="table">table to save to</param>
        /// <param name="keyField">keyfield to update</param>
        /// <param name="propertiesToSkip">optional fields to skip when updating (keys related items etc)</param>
        /// <returns></returns>
        public virtual bool SaveEntity(object entity, string table, string keyField, string propertiesToSkip = null)
        {
            object pkValue = ReflectionUtils.GetProperty(entity, keyField);
            object res = null;
            if (pkValue != null)
                res = ExecuteScalar("select " + LeftFieldBracket + keyField + RightFieldBracket + " from " + 
                                    LeftFieldBracket + table + RightFieldBracket + "] " +
                                    "where " + LeftFieldBracket + keyField + RightFieldBracket + "=" + ParameterPrefix + "id",
                                         CreateParameter(ParameterPrefix + "id", pkValue));


            if (res == null)
            {
                InsertEntity(entity, table, propertiesToSkip);
                if (!string.IsNullOrEmpty(ErrorMessage))
                    return false;
            }
            else
                return UpdateEntity(entity, table, keyField, propertiesToSkip);

            return true;
        }
        #endregion

#region Error Handling

        /// <summary>
        /// Sets the error message for the failure operations
        /// </summary>
        /// <param name="Message"></param>
        protected virtual void SetError(string Message, int errorNumber)
        {
            if (string.IsNullOrEmpty(Message))
            {
                ErrorMessage = string.Empty;
                ErrorNumber = 0;
                ErrorException = null;
                return;
            }

            ErrorMessage = Message;
            ErrorNumber = errorNumber;            
        }        

        /// <summary>
        /// Sets the error message and error number.
        /// </summary>
        /// <param name="message"></param>
        protected virtual void SetError(string message)
        {
            SetError(message,0);
        }

        protected virtual void SetError(DbException ex)
        {
            SetError(ex.Message, ex.ErrorCode);
            ErrorException = ex;

            if (ThrowExceptions)
                throw ex;
        }
        protected virtual void SetError(SqlException ex)
        {
            SetError(ex.Message, ex.Number);
            ErrorException = ex;

            if (ThrowExceptions)
                throw ex;
        }

        protected virtual void SetError(Exception ex)
        {
            if (ex is SqlException)
                SetError(ex as SqlException);
            else if (ex is DbException)
                SetError(ex as DbException);
            else
                SetError(ex.Message, 0);

            ErrorException = ex;

            if (ThrowExceptions)
                throw ex;
        }

        /// <summary>
        /// Sets the error message for failure operations.
        /// </summary>
        protected virtual void SetError()
        {
            SetError(null,0);
        }
#endregion

#region IDisposable Members

        public void Dispose()
        {
            if (_Connection != null)
                CloseConnection();
        }


#endregion
    }



}