using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Westwind.Utilities.Logging;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Data;
using System.Web;
using System.Reflection;

namespace Westwind.Utilities.Logging
{
    public class TextLogAdapter : ILogAdapter
    {

        public TextLogAdapter()
        {
            ConnectionString = LogManagerConfiguration.Current.LogFilename;
            if (string.IsNullOrEmpty(ConnectionString))
                ConnectionString = LogManagerConfiguration.Current.ConnectionString;
        }        

        /// <summary>
        /// The Xml Connection string is the filename
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return _ConnectionString;
            }
            set
            {
                _ConnectionString = value;

#if NETFULL
				// fix up filename in web path. If no path assume server relative
				if (HttpContext.Current != null && !_ConnectionString.Contains(":\\") )
                {
                    _ConnectionString = _ConnectionString.Replace("\\", "/");
                
                    if (!_ConnectionString.StartsWith("~"))
                        _ConnectionString = "~/" + _ConnectionString;

                    _ConnectionString = HttpContext.Current.Server.MapPath(_ConnectionString);

                    if (Path.GetExtension(_ConnectionString) == string.Empty)
                        _ConnectionString += ".xml";
                }
#endif
            }
        }
        private string _ConnectionString = "";

        /// <summary>
        /// The name of the file where we're logging to
        /// </summary>
        public string LogFilename
        {
            get { return ConnectionString; }
            set { ConnectionString = value;}
        }

        private static object _writeLock = new object();

        /// <summary>
        /// Writes an entry to the log
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public bool WriteEntry(WebLogEntry entry)
        {
            if (entry.Id == 0)
                entry.Id = entry.GetHashCode();


            string logFilename = ConnectionString;
            bool writeEndDoc = true;

            lock (_writeLock)
            {
                try
                {
                    using (var fileStream = new FileStream(logFilename, FileMode.OpenOrCreate, FileAccess.Write))
                    using (var sw = new StreamWriter(fileStream))
                    {
                        fileStream.Seek(0, SeekOrigin.End);

                        sw.WriteLine($"Time:       {entry.Entered.ToString("yyyy-MM-dd HH:mm:ss")}");
                        sw.WriteLine($"Id:         {entry.Id}");
                        sw.WriteLine($"Message:    {entry.Message}");
                        sw.WriteLine($"ErrorLevel: {entry.ErrorLevel}");
                        

                        if (!string.IsNullOrEmpty(entry.Url))
                            sw.WriteLine($"Url:        {entry.Url}");
                        if (!string.IsNullOrEmpty(entry.QueryString))
                            sw.WriteLine($"Query:      {entry.QueryString}");
                        if (!string.IsNullOrEmpty(entry.Referrer))
                            sw.WriteLine($"Referrer:   {entry.Referrer}");
                        if (!string.IsNullOrEmpty(entry.IpAddress))
                            sw.WriteLine($"IpAddress:  {entry.IpAddress}");
                        if (!string.IsNullOrEmpty(entry.UserAgent))
                            sw.WriteLine($"UserAgent:  {entry.UserAgent}");

                        if (!string.IsNullOrEmpty(entry.PostData))
                            sw.WriteLine($"PostData:   {entry.PostData.Replace("&","\r\n")}");
                        
                        if (!string.IsNullOrEmpty(entry.Details)) 
                            sw.WriteLine($"Details:    {entry.Details}");

                        if(entry.RequestDuration > 0)
                            sw.WriteLine($"Duration:       {entry.RequestDuration}");


                        sw.WriteLine("----------------------------------------");
                    }
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Returns an individual entry entity
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public WebLogEntry GetEntry(int id)
        {
            throw new NotSupportedException();
        }

        public System.Data.IDataReader GetEntries()
        {
            throw new NotSupportedException();
        }      

        /// <summary>
        /// Returns a filtered list of XML entries sorted in descending order.
        /// </summary>
        /// <param name="errorLevel">The specific error level to return</param>
        /// <param name="count">Max number of items to return</param>
        /// <param name="dateFrom">From Date</param>
        /// <param name="dateTo">To Date</param>
        /// <param name="fieldList">"*" or any of the fields you want returned - currently not supported</param>
        /// <returns></returns>
        public System.Data.IDataReader GetEntries(ErrorLevels errorLevel, int count, DateTime? dateFrom, DateTime? dateTo, string fieldList)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Not implemented yet
        /// </summary>
        /// <param name="errorLevel"></param>
        /// <param name="count"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="fieldList"></param>
        /// <returns></returns>
        public IEnumerable<WebLogEntry> GetEntryList(ErrorLevels errorLevel = ErrorLevels.All,
                                      int count = 200,
                                      DateTime? dateFrom = null,
                                      DateTime? dateTo = null,
                                      string fieldList = null)
        {
            throw new NotSupportedException();
        }


        /// <summary>
        /// Does nothing - log is created with first new entry instead
        /// </summary>
        /// <param name="logType"></param>
        /// <returns></returns>
        public bool CreateLog()
        {
            return true;
        }

        /// <summary>
        /// Deletes the XML log file
        /// </summary>
        /// <returns></returns>
        public bool DeleteLog()
        {
            try
            {
                File.Delete(LogFilename);            
                return true;
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Clears out all items from the XML log - in effect deletes the log file.
        /// </summary>
        /// <returns></returns>
        public bool Clear()
        {
            return DeleteLog();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="countToLeave"></param>
        /// <returns></returns>
        public bool Clear(int countToLeave)
        {
            if (File.Exists(LogFilename))
                File.Delete("LogFilename");

            return true;
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="daysToDelete"></param>
        /// <returns></returns>
        public bool Clear(decimal daysToDelete)
        {
            if (File.Exists(LogFilename))
                File.Delete("LogFilename");

            return true;
        }

        public int GetEntryCount(ErrorLevels errorLevel = ErrorLevels.All)
        {
            throw new NotFiniteNumberException();
        }

        /// <summary>
        /// Creates a DataTable on the fly
        /// </summary>
        /// <returns></returns>
        private DataTable CreateEntryDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Entered", typeof(DateTime));
            dt.Columns.Add("Message", typeof(string));
            dt.Columns.Add("ErrorLevel", typeof(int));
            dt.Columns.Add("Details", typeof(string));
            dt.Columns.Add("Url", typeof(string));
            dt.Columns.Add("QueryString", typeof(string));
            dt.Columns.Add("UserAgent", typeof(string));
            dt.Columns.Add("Referrer", typeof(string));
            dt.Columns.Add("PostData", typeof(string));
            dt.Columns.Add("IpAddress", typeof(string));
            dt.Columns.Add("RequestDuration", typeof(decimal));

            return dt;
        }

        /// <summary>
        /// Updates the DataRow with data from node passed in
        /// </summary>
        /// <param name="node"></param>
        /// <param name="row"></param>
        private void UpdateDataRowFromElement(XElement node, DataRow row)
        {
            row["Id"] = (int)node.Element("Id");
            row["Entered"] = (DateTime)node.Element("Entered");
            row["Message"] = (string)node.Element("Message");
            row["ErrorLevel"] = (int)Enum.Parse(typeof(ErrorLevels), (string)node.Element("ErrorLevel"));

            row["Details"] = (string)node.Element("Details");
            row["Url"] = (string)node.Element("Url");
            row["QueryString"] = (string)node.Element("QueryString");
            row["UserAgent"] = (string)node.Element("UserAgent");
            row["Referrer"] = (string)node.Element("Referrer");
            row["PostData"] = (string)node.Element("PostData");
            row["IpAddress"] = (string)node.Element("IpAddress");
        }
    }
}
