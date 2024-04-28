using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading;

namespace Westwind.Utilities
{
    /// <summary>
    /// An object factory that can create instances of types
    /// for Http Web Request and Thread Scoped object objects
    /// and value types.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectFactory<T>
        where T: class, new()
    {
        /// <summary>
        /// Internal locking for collection storage
        /// </summary>
        static object _syncLock = new object();


        /// <summary>
        /// Returns a standard instance of an object.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static T CreateObject(params object[] args)
        {
            return (T) Activator.CreateInstance(typeof(T), args);            
        }

		
        /// <summary>
        /// Create an instance scoped to a current thread.
        /// </summary>
        /// <param name="key">Optional reusable key of the TLS item created</param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static T CreateThreadScopedObject(params object[] args)
        {
            string key = GetUniqueObjectKey(args) + "_" +
                                            Thread.CurrentThread.ManagedThreadId.ToString();              

            LocalDataStoreSlot threadData = Thread.GetNamedDataSlot(key);

            T item;

            if (threadData != null)
                item = (T)Thread.GetData(threadData);
            else
                item = null;

            // no item - create and store
            if ( item == null )
            {
                lock (_syncLock)
                {
                    // check again inside of lock
                    threadData = Thread.GetNamedDataSlot(key);
                    if (threadData == null)
                        threadData = Thread.GetNamedDataSlot(key);

                    if (item == null)
                        item = CreateObject(args);

                    Thread.SetData(threadData, item);                        
                }                
            }

            return item;
        }
		

        /// <summary>
        /// Returns a unique ID for a given type and parameter signature
        /// </summary>        
        /// <param name="args"></param>
        /// <returns></returns>
        public static string GetUniqueObjectKey(params object[] args)
        {
            Type type = typeof(T);

            StringBuilder sb = new StringBuilder("_" + type.GetHashCode().ToString("x"));
            sb.Append( "_" + type.Name);            

            foreach (var arg in args)
            {
                if (arg == null)
                    sb.Append("_null");
                else
                    sb.Append("_" + arg.GetHashCode().ToString("x"));
            }

            return sb.ToString();
        }
  
        
    }
}
