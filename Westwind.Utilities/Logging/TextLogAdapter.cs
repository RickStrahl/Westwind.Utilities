using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Westwind.Utilities.Logging
{
    public class TextLogAdapter : TextLogAdapter<LogEntry>
    {

    }

    public class TextLogAdapter<T> : ILogAdapter<T> where T : LogEntry, new()
    {
        /// <summary>
        /// A connection string name for data stores that use
        /// connections.
        /// </summary>
        public string ConnectionString
        {
            get => _connectionString;
            set => _connectionString = value;
        }
        private string _connectionString = "";


        /// <summary>
        /// A filename for logs stored in plain disk files
        /// </summary>
        public string Filename
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }        

        /// <summary>
        /// If true, log entries are written in a compact format without field names and separators. This is useful for very high volume logging where performance is critical.
        /// </summary>
        public bool UseCompactLogging { get; set; } = false;

#if NET10_0_OR_GREATER
        private static Lock _writeLock = new();
#else
    private static object _writeLock = new object();
#endif
        private static readonly SemaphoreSlim _writeSemaphore = new(1, 1);


        /// <summary>
        /// Writes an entry to the log
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public bool WriteEntry(T entry)
        {
            if (UseCompactLogging)
            {
                return WriteCompactEntry(entry);
            }

            string logFilename = ConnectionString;
            var sbEntryText = GetEntryString(entry);

            var entryString = GetEntryString(entry);

            _writeSemaphore.Wait();
            try
            {
                try
                {
                    File.AppendAllText(logFilename, sbEntryText.ToString());                    
                }
                catch
                {
                    return false;
                }

            }
            finally
            {
                _writeSemaphore.Release();
            }

            return true;
        }

#if NET8_0_OR_GREATER
        /// <summary>
        /// Asynchronously writes an entry to the log.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public async Task<bool> WriteAsync(T entry)
        {
            if (UseCompactLogging)
            {
                return await WriteCompactAsync(entry).ConfigureAwait(false);
            }

            string logFilename = ConnectionString;

            var sbEntryString = GetEntryString(entry);

            await _writeSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                await using var fileStream = new FileStream(logFilename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read, 4096, FileOptions.Asynchronous);
                await using var sw = new StreamWriter(fileStream);
                fileStream.Seek(0, SeekOrigin.End);
                await sw.WriteAsync(sbEntryString.ToString());
            }
            catch
            {
                return false;
            }
            finally
            {
                _writeSemaphore.Release();
            }

            return true;
        }

        private async Task<bool> WriteCompactAsync(T entry)
        {
            string logFilename = ConnectionString;


            var sbEntryText = GetCompactEntryString(entry);

            await _writeSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                await using var fileStream = new FileStream(logFilename, FileMode.OpenOrCreate , FileAccess.Write, FileShare.Read, 4096, FileOptions.Asynchronous);
                await using var sw = new StreamWriter(fileStream);
                fileStream.Seek(0, SeekOrigin.End);
                await sw.WriteAsync(sbEntryText.ToString()).ConfigureAwait(false);               
            }
            catch
            {
                return false;
            }
            finally
            {
                _writeSemaphore.Release();
            }

            return true;
        }
#endif

        /// <summary>
        /// This method is provided for async logging, but it's not 
        /// truly async adn simply wraps in a `Task.Run()`. It works
        /// to offload but isn't truly IO async due to the file locking
        /// requirements.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public Task<bool> WriteEntryAsync(T entry)
        {

#if NET8_0_OR_GREATER
            // offload from main processing pipeline
            return WriteAsync(entry);
#else
            return Task.Run(() => WriteEntry(entry));
#endif
        }




        public bool WriteCompactEntry(T entry)
        {
            string logFilename = ConnectionString;
            var sbEntryText = GetCompactEntryString(entry);

            _writeSemaphore.Wait();
            try
            {
                File.AppendAllText(logFilename, sbEntryText.ToString());
            }
            catch
            {
                return false;
            }
            finally
            {
                _writeSemaphore.Release();
            }

            return true;
        }


        /// <summary>
        /// Returns an individual entry entity
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T GetEntry(string id)
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
        public IEnumerable<T> GetEntries(ErrorLevels errorLevel, int count, DateTime? dateFrom, DateTime? dateTo, string fieldList)
        {
            throw new NotSupportedException("Text Logs don't support log retrieval.");
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
                File.Delete(Filename);
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
            if (File.Exists(Filename))
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
            if (File.Exists(Filename))
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



        private StringBuilder GetEntryString(T entry)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Time:       {entry.Entered.ToString("yyyy-MM-dd HH:mm:ss")}");
            sb.AppendLine($"Message:    {entry.Message}");
            sb.AppendLine($"ErrorLevel: {entry.ErrorLevel}");

            if (entry.Web != null)
            {
                if (!string.IsNullOrEmpty(entry.Web.Url))
                    sb.AppendLine($"Url:        {entry.Web.Url}");
                if (!string.IsNullOrEmpty(entry.Web.QueryString))
                    sb.AppendLine($"Query:      {entry.Web.QueryString}");
                if (!string.IsNullOrEmpty(entry.Web.Referrer))
                    sb.AppendLine($"Referrer:   {entry.Web.Referrer}");
                if (!string.IsNullOrEmpty(entry.Web.IpAddress))
                    sb.AppendLine($"IpAddress:  {entry.Web.IpAddress}");
                if (!string.IsNullOrEmpty(entry.Web.UserAgent))
                    sb.AppendLine($"UserAgent:  {entry.Web.UserAgent}");

                if (!string.IsNullOrEmpty(entry.Web.PostData))
                    sb.AppendLine($"PostData:   {entry.Web.PostData.Replace("&", "\r\n")}");

                if (entry.Web.RequestDuration > 0)
                    sb.AppendLine($"Duration:       {entry.Web.RequestDuration}");
            }

            if (!string.IsNullOrEmpty(entry.Details))
                sb.AppendLine($"Details:    {entry.Details}");

            sb.AppendLine("----------------------------------------");

            return sb;
        }

        private StringBuilder GetCompactEntryString(T entry)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{entry.Entered.ToString("yyyy-MM-dd HH:mm:ss")} - {entry.Message} - {entry.ErrorLevel}");

            if (entry.Web != null)
            {
                if (!string.IsNullOrEmpty(entry.Web.Url))
                    sb.Append($"   {entry.Web.Verb} - {entry.Web.Url}");
                if (!string.IsNullOrEmpty(entry.Web.QueryString))
                    sb.Append($"- {entry.Web.QueryString}");
                if (!string.IsNullOrEmpty(entry.Web.Referrer))
                    sb.Append($"- {entry.Web.Referrer}");
                if (!string.IsNullOrEmpty(entry.Web.IpAddress))
                    sb.Append($"- {entry.Web.IpAddress}");
                if (!string.IsNullOrEmpty(entry.Web.UserAgent))
                    sb.Append($"UserAgent:  {entry.Web.UserAgent}");

                if (!string.IsNullOrEmpty(entry.Web.PostData))
                    sb.AppendLine($"PostData:   {entry.Web.PostData.Replace("&", "\r\n")}");

                if (entry.Web.RequestDuration > 0)
                    sb.AppendLine($"Duration:       {entry.Web.RequestDuration}");
            }

            if (!string.IsNullOrEmpty(entry.Details))
                sb.AppendLine($"Details:    {entry.Details}");


            sb.AppendLine("----------------------------------------");


            return sb;
        }
    }
}
