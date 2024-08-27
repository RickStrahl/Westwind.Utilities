using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Westwind.Utilities
{
    public class HttpClientUtils
    {
        public const string STR_MultipartBoundary = "----FormBoundary3vDSIXiW0WSTB551";

        /// <summary>
        /// Runs an Http request and returns success results as a string or null
        /// on failure or non-200/300 requests.
        /// 
        /// Failed requests return null and set the settings.ErrorMessage property
        /// but you can can access the `settings.Response` or use the 
        /// `settings.GetResponseStringAsync()` method or friends to retrieve 
        /// content despite the error.
        /// </summary>
        /// <remarks>
        /// By default this method does not throw on errors or error status codes,
        /// but returns null and an error message. For error requests you can check
        /// settings.HasResponseContent and then either directly access settings.Response,
        /// or use settings.GetResponseStringAsync() or friends to retrieve error content.
        /// 
        /// If you want the method to throw exceptions on errors an > 400 status codes,
        /// use `settings.ThrowExceptions = true`.
        /// </remarks>
        /// <param name="settings">Pass HTTP request configuration parameters object to set the URL, Verb, Headers, content and more</param>
        /// <returns>string of HTTP response</returns>
        public static async Task<string> DownloadStringAsync(HttpClientRequestSettings settings)
        {
            string content = null;

            using (var client = GetHttpClient(null, settings))
            {                
                try
                {
                    settings.Response = await client.SendAsync(settings.Request);
                }
                catch (Exception ex)
                {
                    settings.HasErrors = true;
                    settings.ErrorException = ex;
                    settings.ErrorMessage = ex.GetBaseException().Message;
                    return null;
                }
              

                // Capture the response content
                try
                {
                    if (settings.Response.IsSuccessStatusCode)
                    {
                        // http 201 no content may return null and be success
                        
                        if (settings.HasResponseContent)                            
                             content = await settings.Response.Content.ReadAsStringAsync();
                        return content;
                    }

                    settings.HasErrors = true;                        
                    settings.ErrorMessage = ((int) settings.Response.StatusCode).ToString()  + " " + settings.Response.StatusCode.ToString();

                    if (settings.ThrowExceptions)
                        throw new HttpRequestException(settings.ErrorMessage);

                    // return null but allow for explicit response reading
                    return null;                    
                }
                catch (Exception ex)
                {
                    settings.ErrorException = ex;
                    settings.ErrorMessage = ex.GetBaseException().Message;

                    if(settings.ThrowExceptions)
#pragma warning disable CA2200
                        throw;
#pragma warning restore CA2200

                    return null;
                }
            }
        }


        /// <summary>
        /// Runs an Http request and returns success results as a string or null
        /// on failure or non-200/300 requests.
        /// 
        /// Failed requests return null and set the settings.ErrorMessage property
        /// but you can can access the `settings.Response` or use the 
        /// `settings.GetResponseStringAsync()` method or friends to retrieve 
        /// content despite the error.
        /// </summary>
        /// <remarks>
        /// By default this method does not throw on errors or error status codes,
        /// but returns null and an error message. For error requests you can check
        /// settings.HasResponseContent and then either directly access settings.Response,
        /// or use settings.GetResponseStringAsync() or friends to retrieve error content.
        /// 
        /// If you want the method to throw exceptions on errors an > 400 status codes,
        /// use `settings.ThrowExceptions = true`.
        /// </remarks>        
        /// <param name="url">The url to access</param>      
        /// <param name="data">The data to send. String data is sent as is all other data is JSON encoded.</param>
        /// <param name="contentType">Optional Content type for the request</param>
        /// <param name="verb">The HTTP Verb to use (GET,POST,PUT,DELETE etc.)</param>
        /// <returns>string of HTTP response</returns>
        public static async Task<string> DownloadStringAsync(string url, object data = null, string contentType = null, string verb = null)
        {
            if (string.IsNullOrEmpty(verb))
            {
                if (data != null)
                    verb = "POST";
                else
                    verb = "GET";
            }

            return await DownloadStringAsync(new HttpClientRequestSettings
            {
                Url = url,
                HttpVerb = verb,
                RequestContent = data,
                RequestContentType = contentType
            });
        }


        /// <summary>
        /// Calls a URL and returns the raw, unretrieved HttpResponse. Also set on settings.Response and you 
        /// can read the response content from settings.Response.Content.ReadAsXXX() methods.
        /// </summary>
        /// <param name="settings">Pass HTTP request configuration parameters object to set the URL, Verb, Headers, content and more</param>
        /// <returns>string of HTTP response</returns>
        public static async Task<HttpResponseMessage> DownloadResponseMessageAsync(HttpClientRequestSettings settings)
        {            
            using (var client = GetHttpClient(null, settings))
            {
                try
                {
                    settings.Response = await client.SendAsync(settings.Request);

                    if (settings.ThrowExceptions && !settings.Response.IsSuccessStatusCode)
                        throw new HttpRequestException(settings.Response.StatusCode.ToString() + " " + settings.Response.ReasonPhrase);
                    
                    return settings.Response;
                }
                catch (Exception ex)
                {
                    settings.HasErrors = true;
                    settings.ErrorException = ex;
                    settings.ErrorMessage = ex.GetBaseException().Message;

                    if (settings.ThrowExceptions)
#pragma warning disable CA2200
                        throw;
#pragma warning restore CA2200
                    
                    return null;
                }
            }            
        }


        /// <summary>
        /// Makes a JSON request that returns a JSON result.
        /// </summary>
        /// <typeparam name="TResult">Result type to deserialize to</typeparam>
        /// <param name="settings">Configuration for this request</param>
        /// <returns></returns>
        public static async Task<TResult> DownloadJsonAsync<TResult>(HttpClientRequestSettings settings)             
        {
            settings.RequestContentType = "application/json";
            settings.Encoding = Encoding.UTF8;
            
            string json = await DownloadStringAsync(settings);

            if (json == null)
            {
                return default;
            }
             
            try
            {
                return JsonConvert.DeserializeObject<TResult>(json);
            }
            catch (Exception ex)
            {
                // original error has priority
                if (settings.HasErrors)
                    return default;

                settings.HasErrors = true;
                settings.ErrorMessage = ex.GetBaseException().Message;
                settings.ErrorException = ex;                
            }

            return default;
        }

        public static async Task<TResult> DownloadJsonAsync<TResult>(string url, string verb = "GET", object data = null)
        {
            return await DownloadJsonAsync<TResult>(new HttpClientRequestSettings
            {
                Url = url,
                HttpVerb = verb,
                RequestContent = data,
                RequestContentType = data != null ? "application/json" : null
            });
        }


        /// <summary>
        /// Creates an instance of the HttpClient and sets the API Key
        /// in the headers.
        /// </summary>
        /// <returns>Configured HttpClient instance</returns>
        public static HttpClient GetHttpClient(HttpClientHandler handler = null,
            HttpClientRequestSettings settings = null)
        {
            if (settings == null)
                settings = new HttpClientRequestSettings();


            handler = handler ?? new HttpClientHandler()
            {
                Proxy = settings.Proxy,
                Credentials = settings.Credentials
            };

            var client = new HttpClient(handler);

            settings.Request = new HttpRequestMessage
            {
                RequestUri = new Uri(settings.Url),
                Method = new HttpMethod(settings.HttpVerb ?? "GET"),
                Version = new Version(settings.HttpVersion)
            };


            foreach (var header in settings.Headers)
            {
                SetHttpHeader(settings.Request, header.Key, header.Value);
            }

            if (settings.RequestContent != null && 
                (settings.HttpVerb.Equals("POST", StringComparison.OrdinalIgnoreCase) || 
                settings.HttpVerb.Equals("PUT", StringComparison.OrdinalIgnoreCase) || 
                settings.HttpVerb.Equals("PATCH", StringComparison.OrdinalIgnoreCase))
               )
            {
                HttpContent content = null;

                if (settings.RequestContent is string) {
                    if (!settings.IsRawData && settings.RequestContentType == "application/json") {
                        var jsonString = JsonSerializationUtils.Serialize(settings.RequestContent);
                        content = new StringContent(jsonString, settings.Encoding, settings.RequestContentType);
                    }
                    else
                        content = new StringContent(settings.RequestContent as string, settings.Encoding, settings.RequestContentType);
                }
                else if (settings.RequestContent is byte[])
                {
                    content = new ByteArrayContent(settings.RequestContent as byte[]);
                    content.Headers.ContentType = new MediaTypeHeaderValue(settings.RequestContentType);
                }
                else
                {
                    if (!settings.IsRawData)
                    {
                        var jsonString = JsonSerializationUtils.Serialize(settings.RequestContent);
                        content = new StringContent(jsonString, settings.Encoding, settings.RequestContentType);
                    }
                }


                if (content != null)
                    settings.Request.Content = content;
            }

            return client;
        }


        private static void SetHttpHeader(HttpRequestMessage req, string header, string value)
        {
            if (string.IsNullOrEmpty(header) || string.IsNullOrEmpty(value))
                return;

            var lheader = header.ToLower();


            if (lheader == "content-length")
                return; // auto-generated

            if (lheader == "content-type")
            {
                var contentType = value;
                if (value == "multipart/form" && !value.Contains("boundary"))
                {
                    contentType = "multipart/form-data; boundary=" + STR_MultipartBoundary;
                }

                req.Content?.Headers.Add("Content-Type", contentType);
                return;
            }

            // content-type, content-encoding etc.
            if (lheader.StartsWith("content-"))
            {
                req.Content?.Headers.Add(header, value);
                return;
            }

            //if (lheader == "authorization" && !string.IsNullOrWhiteSpace(Options.ReplaceAuthorization))
            //{
            //    req.Headers.Add("Authorization", Options.ReplaceAuthorization);
            //    return;
            //}

            // set above view property
            // not handled at the moment
            if (lheader == "proxy-connection")
                return;

            req.Headers.Add(header, value);
        }
    }

    /// <summary>
    /// Configuration object for Http Requests used by the HttpUtils
    /// methods. Allows you to set the URL, verb, headers proxy and
    /// credentials that are then passed to the HTTP client.
    /// </summary>
    public class HttpClientRequestSettings
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
        public object RequestContent { get; set; }

        /// <summary>
        /// Content Encoding for the data sent to to server
        /// </summary>
        public Encoding Encoding { get; set; }

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
        public string RequestContentType { get; set; } = "application/json";

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
        public IWebProxy Proxy { get; set; }

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

        public bool HasResponseContent
        {
            get
            {
                if (Response?.Content?.Headers== null)
                    return false;

                return Response.Content.Headers.ContentLength > 0;
           }
        }

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
        public bool ThrowExceptions { get; set; }

        /// <summary>
        /// Http Protocol Version 1.1
        /// </summary>
        public string HttpVersion { get; set; } = "1.1";

        public HttpRequestMessage Request { get; set; }

        public HttpResponseMessage Response { get; set; }

        /// <summary>
        /// Determines whether the request has errors or
        /// didn't return a 200/300 result code
        /// </summary>
        public bool HasErrors { get; set; }

        /// <summary>
        /// Error message if one was set
        /// </summary>
        public string ErrorMessage { get; set; }


        /// <summary>
        /// The full Execption object if an error occurred
        /// </summary>
        public Exception ErrorException { get; set; }

        public HttpClientRequestSettings()
        {
            HttpVerb = "GET";
            Headers = new Dictionary<string, string>();
            Encoding = Encoding.UTF8;
            UserAgent = "West Wind .NET Http Client";
        }

        /// <summary>
        /// Retrieves the response as a 
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetResponseStringAsync()
        {
            if (Response == null)
                return null;

            try
            {
                return await Response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                HasErrors = true;
                ErrorMessage = ex.GetBaseException().Message;
                ErrorException = ex;
                return null;
            }
        }

        /// <summary>
        /// Returns deserialized JSON from the Response
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public async Task<TResult> GetResponseJson<TResult>()
        {
            var json =await GetResponseStringAsync();

            if (json == null)
                return default;

            return JsonSerializationUtils.Deserialize<TResult>(json);
        }

        /// <summary>
        /// Returns an error message from a JSON error object that
        /// contains a message or Message property.
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetResponseErrorMessage()
        {
            var obj = await GetResponseJson<JObject>();
            if (obj == null)
                return null;

            if(obj.ContainsKey("message"))
                return obj["message"].ToString();
            if (obj.ContainsKey("Message"))
                return obj["Message"].ToString();

            return null;
        }

        /// <summary>
        /// Returns byte data from the response
        /// </summary>
        /// <returns></returns>
        public async Task<byte[]> GetResponseDataAsync()
        {
            if (Response == null)
                return null;

            try
            {
                return await Response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                HasErrors = true;
                ErrorMessage = ex.GetBaseException().Message;
                ErrorException = ex;
                return null;
            }
        }   

    }
}