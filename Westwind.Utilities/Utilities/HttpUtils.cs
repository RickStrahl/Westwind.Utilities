﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Westwind.Utilities
{
    /// <summary>
    /// Simple HTTP request helper to let you retrieve data from a Web
    /// server and convert it to something useful.
    /// </summary>
    public static class HttpUtils
    {

        /// <summary>
        /// Retrieves and Http request and returns data as a string.
        /// </summary>
        /// <param name="url">A url to call for a GET request without custom headers</param>
        /// <returns>string of HTTP response</returns>
        public static string HttpRequestString(string url)
        {            
            return HttpRequestString(new HttpRequestSettings() { Url = url });
        }

        /// <summary>
        /// Retrieves and Http request and returns data as a string.
        /// </summary>
        /// <param name="settings">Pass HTTP request configuration parameters object to set the URL, Verb, Headers, content and more</param>
        /// <returns>string of HTTP response</returns>
        public static string HttpRequestString(HttpRequestSettings settings)
        {
            var client = new HttpUtilsWebClient(settings);            
          
            if (settings.Content != null)
            {
                if (!string.IsNullOrEmpty(settings.ContentType))
                    client.Headers["Content-type"] = settings.ContentType;

                if (settings.Content is string)
                {
                    settings.CapturedRequestContent = settings.Content as string;
                    settings.CapturedResponseContent = client.UploadString(settings.Url, settings.HttpVerb, settings.CapturedRequestContent);
                }
                else if (settings.Content is byte[])
                {
                    settings.ResponseByteData = client.UploadData(settings.Url, settings.Content as byte[]);
                    settings.CapturedResponseContent = Encoding.UTF8.GetString(settings.ResponseByteData);
                }
                else
                    throw new ArgumentException("Data must be either string or byte[].");
            }
            else 
                settings.CapturedResponseContent = client.DownloadString(settings.Url);

            settings.Response = client.Response;
            

            return settings.CapturedResponseContent;
        }


        /// <summary>
        /// Retrieves bytes from the server without any request customizations
        /// </summary>
        /// <param name="url">Url to access</param>
        /// <returns></returns>
        public static byte[] HttpRequestBytes(string url)
        {
            return HttpRequestBytes(new HttpRequestSettings() { Url = url });
        }


        /// <summary>
        /// Retrieves bytes from the server 
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static byte[] HttpRequestBytes(HttpRequestSettings settings)
        {
            var client = new HttpUtilsWebClient(settings);            
          
            if (settings.Content != null)
            {
                if (!string.IsNullOrEmpty(settings.ContentType))
                    client.Headers["Content-type"] = settings.ContentType;

                if (settings.Content is string)
                {
                    settings.CapturedRequestContent = settings.Content as string;
                    settings.ResponseByteData = client.UploadData(settings.Url, settings.HttpVerb, settings.Encoding.GetBytes(settings.CapturedRequestContent));
                }
                else if (settings.Content is byte[])
                {
                    settings.ResponseByteData = client.UploadData(settings.Url, settings.Content as byte[]);
                }
                else
                    throw new ArgumentException("Data must be either string or byte[].");
            }
            else 
                settings.ResponseByteData = client.DownloadData(settings.Url);

            settings.Response = client.Response;
            
            return settings.ResponseByteData;
        }
        





        /// <summary>
        /// Makes an HTTP with option JSON data serialized from an object
        /// and parses the result from JSON back into an object.
        /// Assumes that the service returns a JSON response
        /// </summary>
        /// <typeparam name="TResultType">The type of the object returned</typeparam>
        /// <param name="settings"><see cref="HttpRequestSettings"/>
        /// Configuration object for the HTTP request made to the server.
        /// </param>
        /// <returns>deserialized value/object from returned JSON data</returns>
        public static TResultType JsonRequest<TResultType>(HttpRequestSettings settings)
        {
            var client = new HttpUtilsWebClient(settings);            
            client.Headers.Add("Accept", "application/json");

            string jsonResult;

            if (settings.Content != null)
            {
                if (!string.IsNullOrEmpty(settings.ContentType))
                    client.Headers["Content-type"] = settings.ContentType;
                else
                    client.Headers["Content-type"] = "application/json;charset=utf-8;";

                if (!settings.IsRawData)
                {
                    settings.CapturedRequestContent = JsonSerializationUtils.Serialize(settings.Content,
                        throwExceptions: true);                    
                }
                else
                    settings.CapturedRequestContent = settings.Content as string;
                
                jsonResult = client.UploadString(settings.Url, settings.HttpVerb, settings.CapturedRequestContent);

                if (jsonResult == null)
                    return default(TResultType);
            }
            else
                jsonResult = client.DownloadString(settings.Url);

            settings.CapturedResponseContent = jsonResult;
            settings.Response = client.Response;

            return (TResultType)JsonSerializationUtils.Deserialize(jsonResult, typeof(TResultType), true);
        }


        /// <summary>
        /// Retrieves and Http request and returns data as a string.
        /// </summary>
        /// <param name="url">The Url to access</param>
        /// <returns>string of HTTP response</returns>
        public static async Task<string> HttpRequestStringAsync(string url)
        {
            return await HttpRequestStringAsync(new HttpRequestSettings() { Url = url });
        }


        /// <summary>
        /// Retrieves and Http request and returns data as a string.
        /// </summary>
        /// <param name="settings">Pass HTTP request configuration parameters object to set the URL, Verb, Headers, content and more</param>
        /// <returns>string of HTTP response</returns>
        public static async Task<string> HttpRequestStringAsync(HttpRequestSettings settings)
        {
            var client = new HttpUtilsWebClient(settings);

            if (settings.Content != null)
            {
                if (!string.IsNullOrEmpty(settings.ContentType))
                    client.Headers["Content-type"] = settings.ContentType;

                if (settings.Content is string)
                {
                    settings.CapturedRequestContent = settings.Content as string;
                    settings.CapturedResponseContent = await client.UploadStringTaskAsync(settings.Url, settings.HttpVerb, settings.CapturedRequestContent);
                }
                else if (settings.Content is byte[])
                {
                    settings.ResponseByteData = await client.UploadDataTaskAsync(settings.Url, settings.Content as byte[]);
                    settings.CapturedResponseContent = Encoding.UTF8.GetString(settings.ResponseByteData);
                }
                else
                    throw new ArgumentException("Data must be either string or byte[].");
            }
            else 
                settings.CapturedResponseContent = await client.DownloadStringTaskAsync(new Uri(settings.Url));

            settings.Response = client.Response;

            return settings.CapturedResponseContent;
        }


        /// <summary>
        /// Retrieves bytes from the server without any request customizations
        /// </summary>
        /// <param name="url">Url to access</param>
        /// <returns></returns>
        public static async Task<byte[]> HttpRequestBytesAsync(string url)
        {
            return await HttpRequestBytesAsync(new HttpRequestSettings() { Url = url });
        }

        /// <summary>
        /// Retrieves bytes from the server 
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static async Task<byte[]> HttpRequestBytesAsync(HttpRequestSettings settings)
        {
            var client = new HttpUtilsWebClient(settings);            
          
            if (settings.Content != null)
            {
                if (!string.IsNullOrEmpty(settings.ContentType))
                    client.Headers["Content-type"] = settings.ContentType;

                if (settings.Content is string)
                {
                    settings.CapturedRequestContent = settings.Content as string;
                    settings.ResponseByteData = await client.UploadDataTaskAsync(settings.Url, settings.HttpVerb, settings.Encoding.GetBytes(settings.CapturedRequestContent));
                }
                else if (settings.Content is byte[])
                {
                    settings.ResponseByteData = await client.UploadDataTaskAsync(settings.Url, settings.Content as byte[]);
                }
                else
                    throw new ArgumentException("Data must be either string or byte[].");
            }
            else 
                settings.ResponseByteData = await client.DownloadDataTaskAsync(settings.Url);

            settings.Response = client.Response;
            
            return settings.ResponseByteData;
        }


		/// <summary>
		/// Makes an HTTP with option JSON data serialized from an object
		/// and parses the result from JSON back into an object.
		/// Assumes that the service returns a JSON response and that
		/// any data sent is json.
		/// </summary>
		/// <typeparam name="TResultType">The type of the object returned</typeparam>
		/// <param name="settings"><see cref="HttpRequestSettings"/>
		/// Configuration object for the HTTP request made to the server.
		/// </param>
		/// <returns>deserialized value/object from returned JSON data</returns>
		public static async Task<TResultType> JsonRequestAsync<TResultType>(HttpRequestSettings settings)
        {
            var client = new HttpUtilsWebClient(settings);       
            client.Headers.Add("Accept", "application/json");
            
            string jsonResult;

            if (settings.HttpVerb == "POST" || settings.HttpVerb == "PUT" || settings.HttpVerb == "PATCH")
            {
                if (!string.IsNullOrEmpty(settings.ContentType))
                    client.Headers["Content-type"] = settings.ContentType;
                else
                    client.Headers["Content-type"] = "application/json";

                if (!settings.IsRawData)
                    settings.CapturedRequestContent = JsonSerializationUtils.Serialize(settings.Content,
                        throwExceptions: true);
                else
                    settings.CapturedRequestContent = settings.Content as string;

                jsonResult = await client.UploadStringTaskAsync(settings.Url, settings.HttpVerb, settings.CapturedRequestContent);

                if (jsonResult == null)
                    return default(TResultType);
            }
            else
                jsonResult = await client.DownloadStringTaskAsync(settings.Url);

            settings.CapturedResponseContent = jsonResult;
            settings.Response = client.Response;

            return (TResultType)JsonSerializationUtils.Deserialize(jsonResult, typeof(TResultType), true);
        }


        /// <summary>
        /// Creates a temporary image file from a download from a URL
        /// 
        /// If you don't pass a file a temporary file is created in Temp Files folder.
        /// You're responsible for cleaning up the file after you are done with it.
        /// 
        /// You should check the filename that is returned regardless of whether you
        /// passed in a filename - if the file is of a different image type the
        /// extension may be changed.
        /// </summary>
        /// <param name="filename">Url of image to download</param>
        /// <param name="imageUrl">Optional output image file. Filename may change extension if the image format doesn't match the filename.
        /// If not passed a temporary files file is created. Caller is responsible for cleaning up this file.
        /// </param>
        /// <param name="settings">Optional Http Settings for the request</param>
        /// <returns>image filename or null on failure. Note that the filename may have a different extension than the request filename parameter.</returns>
        public static string DownloadImageToFile(string imageUrl, string filename = null, HttpRequestSettings settings = null)
        {
            if (string.IsNullOrEmpty(imageUrl) || 
                !imageUrl.StartsWith("http://") && !imageUrl.StartsWith("https://") )
                return null;

            string newFilename;

            if (string.IsNullOrEmpty(filename))
            {
                filename = Path.Combine(Path.GetTempPath(), "~img-" + DataUtils.GenerateUniqueId());
            }
            filename = Path.ChangeExtension(filename, "bin");

            var client = new HttpUtilsWebClient(settings);

            try
            {
                client.DownloadFile(imageUrl, filename);

                var ct = client.Response.ContentType;   // works

                if (string.IsNullOrEmpty(ct) || !ct.StartsWith("image/"))
                    return null;

                var ext = ImageUtils.GetExtensionFromMediaType(ct);
                if (ext == null)
                    return null; // invalid image type

                newFilename = Path.ChangeExtension(filename, ext);

                if (File.Exists(newFilename))
                    File.Delete(newFilename);

                // rename the file
                File.Move(filename, newFilename);
            }
            catch
            {
                if (File.Exists(filename))
                    File.Delete(filename);

                return null;
            }

            return newFilename;
        }


        /// <summary>
        /// Creates a temporary image file from a download from a URL
        /// 
        /// If you don't pass a file a temporary file is created in Temp Files folder.
        /// You're responsible for cleaning up the file after you are done with it.
        /// 
        /// You should check the filename that is returned regardless of whether you
        /// passed in a filename - if the file is of a different image type the
        /// extension may be changed.
        /// </summary>
        /// <param name="filename">Url of image to download</param>
        /// <param name="imageUrl">Optional output image file. Filename may change extension if the image format doesn't match the filename.
        /// If not passed a temporary files file is created. Caller is responsible for cleaning up this file.
        /// </param>
        /// <param name="settings">Optional Http Settings for the request</param>
        public static async Task<string> DownloadImageToFileAsync(string imageUrl, string filename = null, HttpRequestSettings settings = null)
        {
            if (string.IsNullOrEmpty(imageUrl) || 
                !imageUrl.StartsWith("http://") && !imageUrl.StartsWith("https://") )
                return null;

            string newFilename;

            if (string.IsNullOrEmpty(filename))
            {
                filename = Path.Combine(Path.GetTempPath(), "~img-" + DataUtils.GenerateUniqueId());
            }
            filename = Path.ChangeExtension(filename, "bin");

            var client = new HttpUtilsWebClient(settings);

            try
            {
                await client.DownloadFileTaskAsync(imageUrl, filename);

                var ct = client.ResponseHeaders[HttpRequestHeader.ContentType];

                var ext = ImageUtils.GetExtensionFromMediaType(ct);
                if (ext == null)
                    return null; // invalid image type

                newFilename = Path.ChangeExtension(filename, ext);

                
                if (File.Exists(newFilename))
                    File.Delete(newFilename);

                // rename the file
                File.Move(filename, newFilename);
            }
            catch
            {
                if (File.Exists(filename))
                    File.Delete(filename);

                return null;
            }

            return newFilename;
        }


    }


    /// <summary>
    /// Configuration object for Http Requests used by the HttpUtils
    /// methods. Allows you to set the URL, verb, headers proxy and
    /// credentials that are then passed to the HTTP client.
    /// </summary>
    public class HttpRequestSettings
    {
        /// <summary>
        /// The URL to send the request to
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The HTTP verb to use when sending the request
        /// </summary>
        public string HttpVerb { get; set; }

        /// <summary>
        /// The Request content to send to the server.
        /// Data can be either string or byte[] type
        /// </summary>
        public object Content { get; set; }

        /// <summary>
        /// Content Encoding for the data sent to to server
        /// </summary>
        public Encoding Encoding { get; set;  } 

        /// <summary>
        /// When true data is not translated. For example
        /// when using JSON Request if you want to send 
        /// raw POST data rather than a serialized object.
        /// </summary>
        public bool IsRawData { get; set; }

        /// <summary>
        /// The content type of any request data sent to the server
        /// in the Data property.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// The request timeout in milliseconds. 0 for default (20 seconds typically)
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// Any Http request headers you want to set for this request
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Authentication information for this request
        /// </summary>
        public NetworkCredential Credentials { get; set; }

        /// <summary>
        /// Determines whether credentials pre-authenticate
        /// </summary>
        public bool PreAuthenticate { get; set; }


        /// <summary>
        /// An optional proxy to set for this request
        /// </summary>
        public WebProxy Proxy { get; set; }

        /// <summary>
        /// Capture request string data that was actually sent to the server.
        /// </summary>
        public string CapturedRequestContent { get; set; }

        /// <summary>
        /// Captured string Response Data from the server
        /// </summary>
        public string CapturedResponseContent { get; set; }

        /// <summary>
        /// Capture binary Response data from the server when 
        /// using the Data methods rather than string methods.
        /// </summary>
        public byte[] ResponseByteData { get; set; }

        /// <summary>
        /// The HTTP Status code of the HTTP response
        /// </summary>
        public HttpStatusCode ResponseStatusCode
        {
            get
            {
                if (Response != null)
                    return Response.StatusCode;

                return HttpStatusCode.OK;
            }
        }

        /// <summary>
        /// Instance of the full HttpResponse object that gives access
        /// to the full HttpWebResponse object to provide things
        /// like Response headers, status etc.
        /// </summary>
        public HttpWebResponse Response { get; set; }


        /// <summary>
        /// The User Agent string sent to the server
        /// </summary>
        public string UserAgent { get; set; }


        /// <summary>
        /// By default (false) throws a Web Exception on 500 and 400 repsonses.
        /// This is the default WebClient behavior.
        ///
        /// If `true` doesn't throw, but instead returns the HTTP response.
        /// Useful if you need to return error messages on 500 and 400 responses
        /// from API requests.
        /// </summary>
        public bool DontThrowOnErrorStatusCodes { get; set; }


        public HttpRequestSettings()
        {
            HttpVerb = "GET";
            Headers = new Dictionary<string, string>();
            Encoding = Encoding.UTF8;
            UserAgent = "West Wind .NET Http Client";
        }
    }
}
