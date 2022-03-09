﻿using System.Text;
using System.IO;

namespace System.IO
{
    /// <summary>
    /// MemoryStream Extension Methods that provide conversions to and from strings
    /// </summary>
    public static class MemoryStreamExtensions
    {
        /// <summary>
        /// Returns the content of the stream as a string
        /// </summary>
        /// <param name="ms">Memory stream</param>
        /// <param name="encoding">Encoding to use - defaults to Unicode</param>
        /// <returns></returns>
        public static string AsString(this MemoryStream ms, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.Unicode;

            return encoding.GetString(ms.ToArray());
        }

        /// <summary>
        /// Writes the specified string into the memory stream
        /// </summary>
        /// <param name="ms"></param>
        /// <param name="inputString"></param>
        /// <param name="encoding"></param>
        public static void FromString(this MemoryStream ms, string inputString, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.Unicode;

            byte[] buffer = encoding.GetBytes(inputString);
            ms.Write(buffer, 0, buffer.Length);
            ms.Position = 0;
        }
    }

    /// <summary>
    /// Stream Extensions
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Converts a stream by copying it to a memory stream and returning
        /// as a string with encoding.
        /// </summary>
        /// <param name="s">stream to turn into a string</param>
        /// <param name="encoding">Encoding of the stream. Defaults to Unicode</param>
        /// <returns>string </returns>
        public static string AsString(this Stream s, Encoding encoding = null)
        {
            using (var ms = new MemoryStream())
            {
                s.CopyTo(ms);
                s.Position = 0;
                return ms.AsString(encoding);
            }
        }
    }
}
