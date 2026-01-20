using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;

namespace Westwind.Utilities
{
    /// <summary>
    /// A generic scheduling service that runs on a background
    /// thread and fires events in a given check frequency.
    /// 
    /// 
    /// </summary>
    public class Scheduler : IDisposable
    {
        /// <summary>
        /// Determines the status  the Scheduler
        /// </summary>        
        public bool Cancelled
        {
            get { return _Cancelled; }
            private set { _Cancelled = value; }
        }
        private bool _Cancelled = true;

        /// <summary>
        /// The frequency in how often the main method is executed.
        /// Given in milli-seconds.
        /// </summary>
        public int CheckFrequency
        {
            get { return _CheckFrequency; }
            set { _CheckFrequency = value; }
        }
        private int _CheckFrequency = 60000;

        /// <summary>
        /// Optional URL that is pinged occasionally to
        /// ensure the server stays alive.
        /// 
        /// If empty hits root web page (~/yourapp/)
        /// </summary>
        public string WebServerPingUrl
        {
            get { return _WebServerPingUrl; }
            set { _WebServerPingUrl = value; }
        }
        private string _WebServerPingUrl = "";


        /// <summary>
        /// Event that is fired when
        /// </summary>
        public event EventHandler ExecuteScheduledEvent;

        AutoResetEvent _WaitHandle = new AutoResetEvent(false);

        /// <summary>
        ///  Internal property used for cross thread locking
        /// </summary>
        object _SyncLock = new Object();

        /// <summary>
        /// Optionally usable local memory based queue that 
        /// contains can be used to add items to a queue
        /// ordered retrieval.
        /// 
        /// If message persistence is important your scheduling store
        /// should be a database. You can use the QueueMessageManager
        /// object for example.
        /// </summary>    
        /// <remarks>
        /// Note memory based! This means if app crashses
        /// or is shut down messages might get lost.
        /// </remarks>
        public virtual Queue<object> Items
        {
            get { return _Items; }
            set { _Items = value; }
        }
        private Queue<object> _Items = new Queue<object>();


        /// <summary>
        /// Starts the background thread processing       
        /// </summary>
        /// <param name="CheckFrequency">Frequency that checks are performed in seconds</param>
        public void Start(int checkFrequency)
        {
            // Ensure that any waiting instances are shut down
            //this.WaitHandle.Set();

            CheckFrequency = checkFrequency;
            Cancelled = false;

            Thread t = new Thread(Run);
            t.Start();
        }
        /// <summary>
        /// Starts the background Thread processing
        /// </summary>
        public void Start()
        {
            Cancelled = false;
            IsRunning = false;
            Start(CheckFrequency);
        }

        /// <summary>
        /// Causes the processing to stop. If the operation is still
        /// active it will stop after the current message processing
        /// completes
        /// </summary>
        public void Stop()
        {
            lock (_SyncLock)
            {
                if (!IsRunning || Cancelled)
                    return;

                IsRunning = false;                
                _WaitHandle.Set();
            }
        }

        /// <summary>
        /// Causes the processing to stop. If the operation is still
        /// active it will stop after the current message processing
        /// completes
        /// </summary>
        public void Cancel()
        {
            lock (_SyncLock)
            {
                if (Cancelled)
                    return;

                IsRunning = false;
                Cancelled = true;
                _WaitHandle.Set();
            }
        }

        public bool IsRunning { get; set; }
        
        /// <summary>
        /// Runs the actual processing loop by checking the mail box
        /// </summary>
        private void Run()
        {
            // Start out waiting for the timeout period defined 
            // on the scheduler
            _WaitHandle.WaitOne(CheckFrequency, true);


            IsRunning = true;
            while (!Cancelled && IsRunning)
            {
                try
                {
                    // Call whatever logic is attached to the scheduler
                    OnExecuteScheduledEvent();
                    ExecuteScheduledEventAction?.Invoke(this);
                }
                // always eat the exception and notify listener
                catch (Exception ex)
                {
                    OnError(ex);
                    ErrorAction?.Invoke(this, ex);
                }

                // If execution caused cancelling we want to exit now
                if (Cancelled || !IsRunning)
                    break;

                // if a keep alive ping is required fire it
                if (!string.IsNullOrEmpty(WebServerPingUrl))
                    PingServer();

                // Wait for the specified time out
                _WaitHandle.WaitOne(CheckFrequency, true);
            }

            IsRunning = false;
        }

        /// <summary>
        /// Handles a scheduled operation. Checks to see if an event handler
        /// is set up and if so calls it. 
        /// 
        /// This method can also be overrriden in a subclass to implemnent
        /// custom functionality.
        /// </summary>
        protected virtual void OnExecuteScheduledEvent()
        {
            if (ExecuteScheduledEvent != null)
                ExecuteScheduledEvent(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event handler you can hook up to handle scheduled events
        /// </summary>
        public virtual Action<Scheduler>ExecuteScheduledEventAction { get; set; }

        /// <summary>
        /// This method is called if an error occurs during processing
        /// of the OnExecuteScheduledEvent request
        /// 
        /// Override this method in your own implementation to provide
        /// for error logging or other handling of an error that occurred
        /// in processing.
        /// 
        /// Ideally this shouldn't be necessary - your OnexecuteScheduledEvent
        /// code should handle any errors internally and provide for its own 
        /// logging mechanism but this is here as an additional point of
        /// control.
        /// </summary>
        /// <param name="ex">Exception occurred during item execution</param>
        protected virtual void OnError(Exception ex)
        {

        }

        /// <summary>
        /// Event handler Action you can hook up to respond to errors.
        /// Receives the Scheduler Exception that occurred during processing
        /// as a parameter in addition to the scheduler instance.
        /// </summary>
        public virtual Action<Scheduler, Exception> ErrorAction { get; set; } 

        /// <summary>
        /// Adds an item to the queue. 
        /// </summary>
        /// <param name="item">Any data you want to add to the local queue</param>
        public void AddItem(object item)
        {
            lock (_SyncLock)
            {
                Items.Enqueue(item);
            }
        }

        /// <summary>
        /// Adds an item to the queue. 
        /// </summary>
        /// <param name="item">A specific Scheduler Item to add to the local queue</param>
        public void AddItem(SchedulerItem item)
        {
            AddItem(item);
        }

        /// <summary>
        /// Adds a text item to the queue with a specific identification type
        /// </summary>
        /// <param name="textData"></param>
        /// <param name="type"></param>
        public void AddItem(string textData, string type = null)
        {
            SchedulerItem item = new SchedulerItem();
            item.TextData = textData;
            item.Type = type;
            AddItem(item as object);
        }

        /// <summary>
        /// Adds a binary item to the queue with a specific identification type
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type"></param>
        public void AddItem(byte[] data, string type = null)
        {
            SchedulerItem item = new SchedulerItem
            {
                Data = data,
                Type = type
            };
            AddItem(item as object);
        }

        /// <summary>
        /// Returns the next queued item or null on failure.
        /// </summary>
        /// <returns></returns>
        public object GetNextItem()
        {
            lock (_SyncLock)
            {
                if (Items.Count > 0)
                    return Items.Dequeue();
            }

            return null;
        }

        /// <summary>
        /// Optional routine that pings a Url on the server
        /// to keep the server alive. 
        /// 
        /// Use this to avoid IIS shutting down your AppPools
        /// </summary>
        public void PingServer(string url = null)
        {
            if (string.IsNullOrEmpty(url))
                url = WebServerPingUrl;

            //if (Url.StartsWith("~") && HttpContext.Current != null)
            //    Url = wwUtils.ResolveUrl(Url);

            try
            {
#pragma warning disable SYSLIB0014
                WebClient http = new WebClient();
#pragma warning restore SYSLIB0014
                string Result = http.DownloadString(url);
            }
            catch {}
        }


        #region IDisposable Members

        public void Dispose()
        {
            Stop();
        }

        #endregion
    }

    /// <summary>
    /// A simple item wrapper that allows separating items
    /// by type.
    /// </summary>
    public class SchedulerItem
    {
        /// <summary>
        /// Allows identifying items by type
        /// </summary>
        public string Type
        {
            get { return _Type; }
            set { _Type = value; }
        }
        private string _Type = "";

        /// <summary>
        /// Any text data you want to submit
        /// </summary>
        public string TextData
        {
            get { return _TextData; }
            set { _TextData = value; }
        }
        private string _TextData = "";

        /// <summary>
        /// Any binary data you want to submit
        /// </summary>
        public byte[] Data
        {
            get { return _Data; }
            set { _Data = value; }
        }
        private byte[] _Data = null;


        /// <summary>
        /// The initial date when the item was
        /// created or submitted.
        /// </summary>
        public DateTime Entered
        {
            get { return _Entered; }
            set { _Entered = value; }
        }
        private DateTime _Entered = DateTime.UtcNow;
    }

}