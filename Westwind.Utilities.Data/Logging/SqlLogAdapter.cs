using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Westwind.Utilities.Data;

namespace Westwind.Utilities.Logging
{

    public class SqlLogAdapter : SqlLogAdapter<LogEntry>
    {
        public SqlLogAdapter() : base()
        {
        }
        public SqlLogAdapter(string connectionString) : base(connectionString)
        {
        }
        public SqlLogAdapter(string connectionString, string tableName) : base(connectionString, tableName)
        {
        }
    }
    
    public class SqlLogAdapter<T> : ILogAdapter<T> where T : LogEntry, new()
    {
        public string ConnectionString { get; set; } = "";

        /// <summary>
        /// The name of the table that data in SQL Server is written to
        /// </summary>
        public string Filename { get; set; } = "ApplicationLog";


        public bool AutoCreateLog { get; set; } 

        /// <summary>
        /// The name of the table that data in SQL Server is written to
        /// </summary>
        public string Tablename { 
            get => Filename;
            set => Filename = value;
        }

        /// <summary>
        /// this version configures itself from the LogManager 
        /// configuration section
        /// </summary>
        public SqlLogAdapter()
        {
            ConnectionString = LogManagerConfiguration.Current.ConnectionString;
            Tablename = LogManagerConfiguration.Current.LogFilename;
        }


        /// <summary>
        /// Must pass in a SQL Server connection string or 
        /// config ConnectionString Id.
        /// </summary>
        /// <param name="connectionString"></param>        
        public SqlLogAdapter(string connectionString) : base()
        {
            ConnectionString = connectionString;
            Tablename = LogManagerConfiguration.Current.LogFilename;
        }
        public SqlLogAdapter(string connectionString, string tableName) : base()
        {
            ConnectionString = connectionString;
            Tablename = tableName;
        }


        /// <summary>
        /// Internally creates and configures an instance of the DAL used for data access
        /// </summary>
        /// <returns></returns>
        private SqlDataAccess CreateDal()
        {
            SqlDataAccess dal = new SqlDataAccess(ConnectionString);
            return dal;
        }

        #region ILogAdapter Members



        /// <summary>
        /// Writes a new Web specific entry into the log file
        /// 
        /// Assumes that your log file is set up to be a Web Log file
        /// </summary>
        /// <param name=entry"></param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Thrown if the insert operation fails</exception>
        public bool WriteEntry(T entry)
        {
            using SqlDataAccess data = CreateDal();            
            var parms = new List<DbParameter>();

            parms.Add(data.CreateParameter("@Id", entry.Id));
            parms.Add(data.CreateParameter("@Entered", entry.Entered, DbType.DateTime));
            parms.Add(data.CreateParameter("@Message", StringUtils.Truncate(entry.Message, 255), 255));
            parms.Add(data.CreateParameter("@ErrorLevel", entry.ErrorLevel));
            parms.Add(data.CreateParameter("@Details", StringUtils.Truncate(entry.Details, 4000), 4000));
            parms.Add(data.CreateParameter("@ErrorType", entry.ErrorType));
            parms.Add(data.CreateParameter("@StackTrace", StringUtils.Truncate(entry.StackTrace, 1500), 1500));
            string fieldList = "Id, Entered,Message,ErrorLevel,Details,ErrorType,StackTrace";
            string parmList = "@Id, @Entered,@Message,@ErrorLevel,@Details,@ErrorType,@StackTrace";


            if (entry.Web != null) { 
                parms.Add(data.CreateParameter("@IpAddress", entry.Web.IpAddress));
                parms.Add(data.CreateParameter("@UserAgent", StringUtils.Truncate(entry.Web.UserAgent, 255)));
                parms.Add(data.CreateParameter("@Url", entry.Web.Url));
                parms.Add(data.CreateParameter("@QueryString", StringUtils.Truncate(entry.Web.QueryString, 255)));
                parms.Add(data.CreateParameter("@Referrer", entry.Web.Referrer));
                parms.Add(data.CreateParameter("@PostData", StringUtils.Truncate(entry.Web.PostData, 2048), 2048));
                parms.Add(data.CreateParameter("@RequestDuration", entry.Web.RequestDuration)); 

                fieldList += ",IpAddress,UserAgent,Url,QueryString,Referrer,PostData,RequestDuration";
                parmList += ",@IpAddress,@UserAgent,@Url,@QueryString,@Referrer,@PostData,@RequestDuration";
            }

            string sql = $"""
                insert into   [{Filename}] ({fieldList}) 
                       values ({parmList})
                """;

            Console.WriteLine(sql);

            int result = data.ExecuteNonQuery(sql, parms.ToArray());                       

            // check for table missing and retry
            if (data.ErrorNumber == 208)
            {
                // if the table could be created try again
                if (CreateLog())
                    return WriteEntry(entry);
            }

            if (result == -1)
                throw new InvalidOperationException("Unable add log entry into table " + Filename + ". " + data.ErrorMessage);
            
            return true;
        }



        /// <summary>
        /// Writes a new Web specific entry into the log file
        /// 
        /// Assumes that your log file is set up to be a Web Log file
        /// </summary>
        /// <param name=entry"></param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Thrown if the insert operation fails</exception>
        public async Task<bool> WriteEntryAsync(T entry)
        {
            using SqlDataAccess data = CreateDal();
            var parms = new List<DbParameter>();

            parms.Add(data.CreateParameter("@Id", entry.Id));
            parms.Add(data.CreateParameter("@Entered", entry.Entered, DbType.DateTime));
            parms.Add(data.CreateParameter("@Message", StringUtils.Truncate(entry.Message, 255), 255));
            parms.Add(data.CreateParameter("@ErrorLevel", entry.ErrorLevel));
            parms.Add(data.CreateParameter("@Details", StringUtils.Truncate(entry.Details, 4000), 4000));
            parms.Add(data.CreateParameter("@ErrorType", entry.ErrorType));
            parms.Add(data.CreateParameter("@StackTrace", StringUtils.Truncate(entry.StackTrace, 1500), 1500));
            string fieldList = "Id, Entered,Message,ErrorLevel,Details,ErrorType,StackTrace";
            string parmList = "@Id, @Entered,@Message,@ErrorLevel,@Details,@ErrorType,@StackTrace";


            if (entry.Web != null)
            {
                parms.Add(data.CreateParameter("@IpAddress", entry.Web.IpAddress));
                parms.Add(data.CreateParameter("@UserAgent", StringUtils.Truncate(entry.Web.UserAgent, 255)));
                parms.Add(data.CreateParameter("@Url", entry.Web.Url));
                parms.Add(data.CreateParameter("@QueryString", StringUtils.Truncate(entry.Web.QueryString, 255)));
                parms.Add(data.CreateParameter("@Referrer", StringUtils.Truncate(entry.Web.Referrer, 255)));
                parms.Add(data.CreateParameter("@PostData", StringUtils.Truncate(entry.Web.PostData, 2048), 2048));
                parms.Add(data.CreateParameter("@RequestDuration", entry.Web.RequestDuration));

                fieldList += ",IpAddress,UserAgent,Url,QueryString,Referrer,PostData,RequestDuration";
                parmList += ",@IpAddress,@UserAgent,@Url,@QueryString,@Referrer,@PostData,@RequestDuration";
            }

            string sql = $"""
                insert into   [{Filename}] ({fieldList}) 
                       values ({parmList})
                """;

            int result = await data.ExecuteNonQueryAsync(sql, parms.ToArray()).ConfigureAwait(false);

            // check for table missing and retry
            if (data.ErrorNumber == 208)
            {
                // if the table could be created try again
                if (CreateLog())
                    return await WriteEntryAsync(entry).ConfigureAwait(false);
            }

            if (result == -1)
                throw new InvalidOperationException("Unable add log entry into table " + Filename + ". " + data.ErrorMessage);

            return true;
        }

        /// <summary>
        /// Returns an individual Web log entry from the log table
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T GetEntry(string id)
        {
            using (SqlDataAccess data = CreateDal())
            {
                T entry = new T();
                if (!data.GetEntity(entry, Filename, "Id", id, null))
                    return null;

                return entry;
            }
        }


        /// <summary>
        /// Returns entries for a given error level, and date range
        /// </summary>
        /// <param name="errorLevel"></param>
        /// <param name="count"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <returns></returns>
        public IEnumerable<T> GetEntries(ErrorLevels errorLevel = ErrorLevels.All,
                                      int count = 200,
                                      DateTime? dateFrom = null,
                                      DateTime? dateTo = null,
                                      string fieldList = null)
        {
            if (dateFrom == null)
                dateFrom = DateTime.Now.Date.AddDays(-2);
            if (dateTo == null)
                dateTo = DateTime.Now.Date.AddDays(1);
            if (fieldList == null)
                fieldList = "*";

            SqlDataAccess data = CreateDal();

            string sql = string.Format("select TOP {1} {2} from [{0}] where " +
                                           (errorLevel != ErrorLevels.All ? "ErrorLevel = @ErrorLevel and " : "") +
                                           "Entered >= @dateFrom and Entered < @dateTo " +
                                           "order by Entered DESC", Filename, count, fieldList);
            var list = data.QueryList<T>(sql,
                data.CreateParameter("@ErrorLevel", (int)errorLevel),
                data.CreateParameter("@dateFrom", dateFrom.Value.Date),
                data.CreateParameter("@dateTo", dateTo.Value.AddDays(1).Date));

            return list;
        }

        ////IEnumerable<WebLogEntry> GetEntryList(ErrorLevels errorLevel, int count, DateTime dateFrom, DateTime dateTo, string FieldList);
        //public IEnumerable<T> GetEntryList(ErrorLevels errorLevel = ErrorLevels.All,
        //                              int count = 200,
        //                              DateTime? dateFrom = null,
        //                              DateTime? dateTo = null,
        //                              string fieldList = null)
        //{
        //    var reader = GetEntries(errorLevel, count, dateFrom, dateTo, fieldList);

        //    if (reader == null)
        //    {
        //        yield break;
        //    }

        //    foreach(var )
        //    Dictionary<string, PropertyInfo> piList = new Dictionary<string, PropertyInfo>();
        //    while (reader.Read())
        //    {
        //        var entry = new T();
        //        DataUtils.DataReaderToObject(reader, entry, null, piList);
        //        yield return entry;
        //    }

        //    reader.Close();
        //}

        /// <summary>
        /// Creates a new log table in the current database. If the table exists already it
        /// is dropped and recreated.
        /// 
        /// Requires database admin access.
        /// </summary>
        /// <param name="logType"></param>
        /// <returns></returns>
        public bool CreateLog()
        {

            using SqlDataAccess data = CreateDal();
         
            string sql = string.Format(STR_ApplicationWebLogCreateStatement, Filename);           
            int result = data.ExecuteNonQuery(sql);
            if (result < 0)
                throw new InvalidOperationException("Failed to create Application Log Table: " + data.ErrorMessage);

            return true;
        }


        /// <summary>
        /// Deletes the Sql Log Table
        /// </summary>
        /// <param name="logType"></param>
        /// <returns></returns>
        public bool DeleteLog()
        {
            using (SqlDataAccess data = CreateDal())
            {
                string sql = "DROP TABLE " + Filename;
                int result = data.ExecuteNonQuery(sql);
                if (result < 0)
                    throw new InvalidOperationException("Failed to create Application Log Table: " + data.ErrorMessage);

            }
            return true;
        }


        /// <summary>
        /// Clears all the records of the log table
        /// </summary>
        /// <returns></returns>
        public bool Clear()
        {
            using (SqlDataAccess data = CreateDal())
            {
                if (data.ExecuteNonQuery("delete [" + Filename + "]") < 0)
                    throw new InvalidOperationException("Failed to delete table" + Filename + ". " + data.ErrorMessage);

            }
            return true;
        }

        /// <summary>
        /// Clears the table and leaves the last number of records specified intact
        /// </summary>
        /// <param name="countToLeave"></param>
        /// <returns></returns>
        public bool Clear(int countToLeave)
        {
            string sql = "delete [{0}] where Id not in (select top {1} Id from [{0}] order by entered desc)";
            sql = string.Format(sql, Filename, countToLeave);

            using (SqlDataAccess data = CreateDal())
            {
                if (data.ExecuteNonQuery(sql) < 0)
                    throw new InvalidOperationException("Failed to remove entries. " + data.ErrorMessage);
            }

            return true;
        }

        public bool Clear(decimal daysToDelete)
        {

            var date = DateTime.UtcNow.Date.AddDays((int)daysToDelete * -1);
            string sql = "delete [{0}] where entered < @date";
            sql = string.Format(sql, Filename);

            using (SqlDataAccess data = CreateDal())
            {
                if (data.ExecuteNonQuery(sql, data.CreateParameter("@date", date)) < 0)
                    throw new InvalidOperationException("Failed to remove entries. " + data.ErrorMessage);
            }

            return true;

        }

        /// <summary>
        /// Returns the number of total log entries
        /// </summary>
        /// <returns></returns>
        public int GetEntryCount(ErrorLevels errorLevel = ErrorLevels.All)
        {
            using (SqlDataAccess data = CreateDal())
            {
                string sql = "select count(id) from " + Filename;
                DbParameter[] parms = null;

                if (!(errorLevel == ErrorLevels.All || errorLevel == ErrorLevels.None))
                {
                    sql = sql + " where errorlevel = @ErrorLevel";
                    parms = new DbParameter[1]
                        { data.CreateParameter("@ErrorLevel",(int) errorLevel) };
                }

                object result = data.ExecuteScalar(sql, parms);
                if (result == null)
                    throw new InvalidOperationException("Failed to count entries. " + data.ErrorMessage);

                return (int)result;
            }

        }

        #endregion



        public const string STR_ApplicationWebLogCreateStatement = @"
CREATE TABLE [dbo].[{0}](
	[Id] [nvarchar](20) NOT NULL,
	[Entered] [datetime] NOT NULL Default(getutcdate()),
	[Message] [nvarchar](255) NULL,
	[ErrorLevel] [int] NOT NULL Default((0)),
	[Details] [nvarchar](4000) NULL,
    [ErrorType] [nvarchar](50) NULL,
    [StackTrace] [nvarchar] (1500) NULL,

	[Url] [nvarchar](255) NULL,
	[QueryString] [nvarchar](255) NULL,
	[IpAddress] [nvarchar](20) NULL,
	[Referrer] [nvarchar](255) NULL,
	[UserAgent] [nvarchar](255) NULL,
	[PostData] [nvarchar](2048) NULL,
	[RequestDuration] [decimal](9, 3) NOT NULL Default((-1))
) ON [PRIMARY]
";

    }

}
