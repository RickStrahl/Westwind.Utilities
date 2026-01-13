using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Westwind.Utilities
{
    /// <summary>
    /// Http Client wrapper that provides single line access for common Http requests
    /// that return string, Json or binary content. 
    /// 
    /// Most methods have dual versions using simple parameters, or an
    /// `HttpClientRequestSettings` configuration and results object that is
    /// passed through on requests.    
    /// </summary>
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
                        {
                            if (settings.MaxResponseSize > 0)
                            {
                                using (var stream = await settings.Response.Content.ReadAsStreamAsync())
                                {
                                    var buffer = new byte[settings.MaxResponseSize];
                                    _ = await stream.ReadAsync(buffer, 0, settings.MaxResponseSize);
                                    content = settings.Encoding.GetString(buffer);
                                }
                            }
                            else
                            {
                                content = await settings.Response.Content.ReadAsStringAsync();
                            }
                        }

                        return content;
                    }

                    settings.HasErrors = true;
                    settings.ErrorMessage = ((int)settings.Response.StatusCode).ToString() + " " +
                                            settings.Response.StatusCode.ToString();

                    if (settings.ThrowExceptions)
                        throw new HttpRequestException(settings.ErrorMessage);

                    // return null but allow for explicit response reading
                    return null;
                }
                catch (Exception ex)
                {
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


#if NET6_0_OR_GREATER

#endif


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
        public static async Task<string> DownloadStringAsync(string url, object data = null, string contentType = null,
            string verb = null)
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

#if NET6_0_OR_GREATER

        /// <summary>
        /// Synchronous version of `DownloadStringAsync`.
        /// </summary>
        /// <param name="settings">Request/Response settings instance</param>
        /// <returns>string or null on failure</returns>
        public static string DownloadString(HttpClientRequestSettings settings)
        {
            string content = null;

            using (var client = GetHttpClient(null, settings))
            {
                try
                {
                    settings.Response = client.Send(settings.Request);
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
                        {
                            if (settings.MaxResponseSize > 0)
                            {
                                using (var stream = settings.Response.Content.ReadAsStream())
                                {
                                    var buffer = new byte[settings.MaxResponseSize];
                                    _ = stream.ReadAsync(buffer, 0, settings.MaxResponseSize);
                                    content = settings.Encoding.GetString(buffer);
                                }
                            }
                            else
                            {
                                //content = await settings.Response.Content.ReadAsStringAsync();
                                using (var stream = settings.Response.Content.ReadAsStream())
                                {
                                    var sr = new StreamReader(stream, true);
                                    content = sr.ReadToEnd();
                                }
                            }
                        }

                        return content;
                    }

                    settings.HasErrors = true;
                    settings.ErrorMessage = ((int)settings.Response.StatusCode).ToString() + " " +
                                            settings.Response.StatusCode.ToString();

                    if (settings.ThrowExceptions)
                        throw new HttpRequestException(settings.ErrorMessage);

                    // return null but allow for explicit response reading
                    return null;
                }
                catch (Exception ex)
                {
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
        public static string DownloadString(string url, object data = null, string contentType = null,
            string verb = null)
        {
            if (string.IsNullOrEmpty(verb))
            {
                if (data != null)
                    verb = "POST";
                else
                    verb = "GET";
            }

            return DownloadString(new HttpClientRequestSettings
            {
                Url = url,
                HttpVerb = verb,
                RequestContent = data,
                RequestContentType = contentType
            });
        }
#endif

        /// <summary>
        /// Downloads a Url to a file.
        /// </summary>             
        /// <param name="url">Optional Url to download - settings.Url works too</param>
        /// <param name="filename">Option filename to download to - settings.OutputFilename works too</param>
        /// <param name="settings">Http request settings can be used in lieu of other parameters</param>
        /// <remarks></remarks>
        /// <returns>true or fals</returns>
        public static async Task<bool> DownloadFileAsync(HttpClientRequestSettings settings, string filename = null)
        {
            if (settings == null)
                return false;

            if (string.IsNullOrEmpty(settings.OutputFilename))
            {
                settings.HasErrors = true;
                settings.ErrorMessage = "No ouput file provided. Provide `filename` parameter or `settings.OutputFilename`.";
                return false;
            }
                  
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
                    return false;
                }


                // Capture the response content
                try
                {
                    if (settings.Response.IsSuccessStatusCode)
                    {
                        // http 201 no content may return null and be success

                        if (File.Exists(settings.OutputFilename))
                            File.Delete(settings.OutputFilename);

                        if (settings.HasResponseContent)
                        {
                            if (settings.MaxResponseSize > 0)
                            {
                                using (var outputStream = new FileStream(settings.OutputFilename, FileMode.OpenOrCreate,
                                           FileAccess.Write))
                                {
                                    using (var stream = await settings.Response.Content.ReadAsStreamAsync())
                                    {
                                        var buffer = new byte[settings.MaxResponseSize];
                                        _ = await stream.ReadAsync(buffer, 0, settings.MaxResponseSize);
                                        await outputStream.WriteAsync(buffer, 0, buffer.Length);
                                    }
                                }
                            }
                            else
                            {
                                using (var outputStream = new FileStream(settings.OutputFilename, FileMode.OpenOrCreate,
                                           FileAccess.Write))
                                {
                                    using (var stream = await settings.Response.Content.ReadAsStreamAsync())
                                    {
                                        await stream.CopyToAsync(outputStream, 8 * 1024);
                                    }
                                }
                            }
                        }
                        
                        return true;
                    }

                    settings.HasErrors = true;
                    settings.ErrorMessage = ((int)settings.Response.StatusCode).ToString() + " " +
                                            settings.Response.StatusCode.ToString();

                    if (settings.ThrowExceptions)
                        throw new HttpRequestException(settings.ErrorMessage);

                    // return null but allow for explicit response reading
                    return false;
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

                    return false;
                }
            }
        }
        /// <summary>
        /// Downloads a Url to a file.
        /// </summary>             
        /// <param name="url">Optional Url to download - settings.Url works too</param>
        /// <param name="filename">Option filename to download to - settings.OutputFilename works too</param>
        /// <param name="settings">Http request settings can be used in lieu of other parameters</param>
        /// <remarks></remarks>
        /// <returns>true or fals</returns>
        public static Task<bool> DownloadFileAsync(string url, string filename)
        {
            var settings = new HttpClientRequestSettings();
            if (!string.IsNullOrEmpty(url))
                settings.Url = url;
            if (!string.IsNullOrEmpty(filename))
                settings.OutputFilename = filename;

            return DownloadFileAsync(settings);
        }



        /// <summary>
        /// Runs an Http Request and returns a byte array from the response or null on failure
        /// </summary>
        /// <param name="settings">Pass in a settings object</param>
        /// <returns>byte[] or null - if null check settings for errors</returns>
        public static async Task<byte[]> DownloadBytesAsync(HttpClientRequestSettings settings)
        {
            byte[] content = null;

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
                        {
                            if (settings.MaxResponseSize > 0)
                            {
                                using (var stream = await settings.Response.Content.ReadAsStreamAsync())
                                {
                                    var buffer = new byte[settings.MaxResponseSize];
                                    _ = await stream.ReadAsync(buffer, 0, settings.MaxResponseSize);
                                    content = buffer;
                                }
                            }
                            else
                            {
                                content = await settings.Response.Content.ReadAsByteArrayAsync();
                            }
                        }

                        return content;
                    }

                    settings.HasErrors = true;
                    settings.ErrorMessage = ((int)settings.Response.StatusCode).ToString() + " " +
                                            settings.Response.StatusCode.ToString();

                    if (settings.ThrowExceptions)
                        throw new HttpRequestException(settings.ErrorMessage);

                    // return null but allow for explicit response reading
                    return null;
                }
                catch (Exception ex)
                {
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

        public static async Task<byte[]> DownloadBytesAsync(string url, object data = null, string contentType = null,
            string verb = null)
        {
            if (string.IsNullOrEmpty(verb))
            {
                if (data != null)
                    verb = "POST";
                else
                    verb = "GET";
            }

            return await DownloadBytesAsync(new HttpClientRequestSettings
            {
                Url = url,
                HttpVerb = verb,
                RequestContent = data,
                RequestContentType = contentType
            });
        }


#if NET6_0_OR_GREATER

        /// <summary>
        /// Synchronous version of `DownloadBytesAsync`.
        /// </summary>
        /// <param name="settings">Request/Response settings instance</param>
        /// <returns>string or null on failure</returns>
        public static byte[] DownloadBytes(HttpClientRequestSettings settings)
        {
            byte[] content = null;

            using (var client = GetHttpClient(null, settings))
            {
                try
                {
                    settings.Response = client.Send(settings.Request);
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
                        {
                            if (settings.MaxResponseSize > 0)
                            {
                                using (var stream = settings.Response.Content.ReadAsStream())
                                {
                                    var buffer = new byte[settings.MaxResponseSize];
                                    _ = stream.ReadAsync(buffer, 0, settings.MaxResponseSize);
                                    content = buffer;
                                }
                            }
                            else
                            {
                                using (var stream = settings.Response.Content.ReadAsStream())
                                {
                                    using (var ms = new MemoryStream())
                                    {
                                        stream.CopyTo(ms);
                                        content = ms.ToArray();
                                    }
                                }
                            }
                        }

                        return content;
                    }

                    settings.HasErrors = true;
                    settings.ErrorMessage = ((int)settings.Response.StatusCode).ToString() + " " +
                                            settings.Response.StatusCode.ToString();

                    if (settings.ThrowExceptions)
                        throw new HttpRequestException(settings.ErrorMessage);

                    // return null but allow for explicit response reading
                    return null;
                }
                catch (Exception ex)
                {
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
        /// Synchronous version of `DownloadBytesAsync`.
        /// </summary>       
        /// <param name="url">Request URL</param>
        /// <param name="data">Request data</param>        
        /// <param name="contentType">Request content type</param>
        /// <param name="verb">HTTP verb (GET, POST, etc.)</param>
        /// <returns>string or null on failure</returns>
        public static byte[] DownloadBytes(string url, object data = null, string contentType = null,
            string verb = null)
        {
            if (string.IsNullOrEmpty(verb))
            {
                if (data != null)
                    verb = "POST";
                else
                    verb = "GET";
            }

            return DownloadBytes(new HttpClientRequestSettings
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
                        throw new HttpRequestException(settings.ResponseStatusCode + " " +
                                                       settings.Response.ReasonPhrase);

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

#endif


#if NET6_0_OR_GREATER

        /// <summary>
        /// Calls a URL and returns the raw, unretrieved HttpResponse. Also set on settings.Response and you 
        /// can read the response content from settings.Response.Content.ReadAsXXX() methods.
        /// </summary>
        /// <param name="settings">Pass HTTP request configuration parameters object to set the URL, Verb, Headers, content and more</param>
        /// <returns>string of HTTP response</returns>
        public static HttpResponseMessage DownloadResponseMessage(HttpClientRequestSettings settings)
        {
            using (var client = GetHttpClient(null, settings))
            {
                try
                {
                    settings.Response = client.Send(settings.Request);

                    if (settings.ThrowExceptions && !settings.Response.IsSuccessStatusCode)
                        throw new HttpRequestException(settings.ResponseStatusCode + " " +
                                                       settings.Response.ReasonPhrase);

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
#endif

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

        public static async Task<TResult> DownloadJsonAsync<TResult>(string url, string verb = "GET",
            object data = null)
        {
            return await DownloadJsonAsync<TResult>(new HttpClientRequestSettings
            {
                Url = url,
                HttpVerb = verb,
                RequestContent = data,
                RequestContentType = data != null ? "application/json" : null
            });
        }

#if NET6_0_OR_GREATER

        /// <summary>
        /// Makes a JSON request that returns a JSON result.
        /// </summary>
        /// <typeparam name="TResult">Result type to deserialize to</typeparam>
        /// <param name="settings">Configuration for this request</param>
        /// <returns>Result or null - check ErrorMessage in settings on failure</returns>
        public static TResult DownloadJson<TResult>(HttpClientRequestSettings settings)
        {
            settings.RequestContentType = "application/json";
            settings.Encoding = Encoding.UTF8;

            string json = DownloadString(settings);

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


        /// <summary>
        /// Makes a JSON request that returns a JSON result.
        /// </summary>
        /// <param name="url">Request URL</param>
        /// <param name="verb">Http Verb to use. Defaults to GET on no data or POST when data is passed.</param>
        /// <param name="data">Data to be serialized to JSON for sending</param>
        /// <returns>result or null</returns>
        public static TResult DownloadJson<TResult>(string url, string verb = "GET", object data = null)
        {
            return DownloadJson<TResult>(new HttpClientRequestSettings
            {
                Url = url,
                HttpVerb = verb,
                RequestContent = data,
                RequestContentType = data != null ? "application/json" : null
            });
        }
#endif

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
                Credentials = settings.Credentials,
            };

#if NET6_0_OR_GREATER
            if (settings.IgnoreCertificateErrors)
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif

            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(settings.UserAgent);

            ApplySettingsToRequest(settings);

            return client;
        }

        /// <summary>
        /// Creates a new Request on the Settings object and assigns the settings values
        /// to the request object.
        /// </summary>
        /// <param name="settings">Settings instance</param>        
        public static void ApplySettingsToRequest(HttpClientRequestSettings settings)
        {
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

            if (settings.HasPostData)
            {
                settings.RequestContentType = settings.RequestFormPostMode == HttpFormPostMode.MultiPart
                    ? "multipart/form-data; boundary=" + HttpClientUtils.STR_MultipartBoundary
                    : "application/x-www-form-urlencoded";
                settings.RequestContent = settings.GetPostBufferBytes();
            }

            if (settings.RequestContent != null &&
                (settings.HttpVerb.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
                 settings.HttpVerb.Equals("PUT", StringComparison.OrdinalIgnoreCase) ||
                 settings.HttpVerb.Equals("PATCH", StringComparison.OrdinalIgnoreCase))
               )
            {
                HttpContent content = null;


                if (settings.RequestContent is string)
                {
                    if (!settings.IsRawData && settings.RequestContentType == "application/json")
                    {
                        var jsonString = JsonSerializationUtils.Serialize(settings.RequestContent);
                        content = new StringContent(jsonString, settings.Encoding, settings.RequestContentType);
                    }
                    else
                        content = new StringContent(settings.RequestContent as string, settings.Encoding,
                            settings.RequestContentType);
                }
                else if (settings.RequestContent is byte[])
                {
                    content = new ByteArrayContent(settings.RequestContent as byte[]);
                    settings.Request.Content = content;
                    settings.Request.Content?.Headers.Add("Content-Type", settings.RequestContentType);
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

            // set above view property
            // not handled at the moment
            if (lheader == "proxy-connection")
                return;

            req.Headers.Add(header, value);
        }
    }

    /// <summary>
    /// Configuration object for Http Requests used by the HttpClientUtils
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
        /// Output file name for file download operations
        /// </summary>
        public string OutputFilename { get; set; }

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

                return HttpStatusCode.Unused;
            }
        }

        /// <summary>
        /// Content Type of the response - may be null if there is no content or the content type is not set by server
        /// </summary>
        public string ResponseContentType => Response?.Content?.Headers.ContentType?.MediaType;

        /// <summary>
        /// Content Length of the response. -1 if there is no result content
        /// </summary>
        public long ResponseContentLength => Response?.Content?.Headers?.ContentLength ?? -1;

        /// <summary>
        /// Response content headers (content type, size, charset etc.) - 
        /// check for null if request did not succeed or doesn't produce content
        /// </summary>
        public HttpContentHeaders ResponseContentHeaders => Response?.Content?.Headers;

        /// <summary>
        /// Non-Content Response headers - check for null if request did not succeed
        /// </summary>
        public HttpResponseHeaders ResponseHeaders => Response?.Headers;

        public bool HasResponseContent
        {
            get
            {
                if (Response?.Content?.Headers == null)
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

        public int MaxResponseSize { get; set; }

        /// <summary>
        /// if true ignores most certificate errors (expired, not trusted)
        /// </summary>
#if !NET6_0_OR_GREATER
        [Obsolete("This property is not supported in .NET Framework.")]
#endif
        public bool IgnoreCertificateErrors { get; set; }

        public HttpClientRequestSettings()
        {
            HttpVerb = "GET";
            Headers = new Dictionary<string, string>();
            Encoding = Encoding.UTF8;
            UserAgent = "West Wind .NET Http Client";
        }


        #region POST data

        /// <summary>
        /// Determines how data is POSTed when when using AddPostKey() and other methods
        /// of posting data to the server. Support UrlEncoded, Multi-Part, XML and Raw modes.
        /// </summary>
        public HttpFormPostMode RequestFormPostMode { get; set; } = HttpFormPostMode.UrlEncoded;

        // member properties
        //string cPostBuffer = string.Empty;
        internal MemoryStream PostStream;
        internal BinaryWriter PostData;
        internal bool HasPostData;

        /// <summary>
        /// Resets the Post buffer by clearing out all existing content
        /// </summary>
        public void ResetPostData()
        {
            PostStream = new MemoryStream();
            PostData = new BinaryWriter(PostStream);
        }

        public void SetPostStream(Stream postStream)
        {
            MemoryStream ms = new MemoryStream(1024);
            FileUtils.CopyStream(postStream, ms, 1024);
            ms.Flush();
            ms.Position = 0;
            PostStream = ms;
            PostData = new BinaryWriter(ms);
        }

        /// <summary>
        /// Adds POST form variables to the request buffer.
        /// PostMode determines how parms are handled.
        /// </summary>
        /// <param name="key">Key value or raw buffer depending on post type</param>
        /// <param name="value">Value to store. Used only in key/value pair modes</param>
        public void AddPostKey(string key, byte[] value)
        {
            if (value == null)
                return;

            if (key == "RESET")
            {
                ResetPostData();
                return;
            }

            HasPostData = true;
            if (PostData == null)
            {
                PostStream = new MemoryStream();
                PostData = new BinaryWriter(PostStream);
            }

            if (string.IsNullOrEmpty(key))
                PostData.Write(value);
            else if (RequestFormPostMode == HttpFormPostMode.UrlEncoded)
                PostData.Write(
                    Encoding.Default.GetBytes(key + "=" +
                                              StringUtils.UrlEncode(Encoding.Default.GetString(value)) +
                                              "&"));
            else if (RequestFormPostMode == HttpFormPostMode.MultiPart)
            {
                Encoding iso = Encoding.GetEncoding("ISO-8859-1");
                PostData.Write(iso.GetBytes(
                    "--" + HttpClientUtils.STR_MultipartBoundary + "\r\n" +
                    "Content-Disposition: form-data; name=\"" + key + "\"\r\n\r\n"));

                PostData.Write(value);
                PostData.Write(iso.GetBytes("\r\n"));
            }
            else // Raw or Xml, JSON modes
                PostData.Write(value);
        }

        /// <summary>
        /// Adds POST form variables to the request buffer.
        /// PostMode determines how parms are handled.
        /// </summary>
        /// <param name="key">Key value or raw buffer depending on post type</param>
        /// <param name="value">Value to store. Used only in key/value pair modes</param>
        public void AddPostKey(string key, string value)
        {
            if (value == null)
                return;
            AddPostKey(key, Encoding.Default.GetBytes(value));
        }

        /// <summary>
        /// Adds a fully self contained POST buffer to the request.
        /// Works for XML or previously encoded content.
        /// </summary>
        /// <param name="fullPostBuffer">String based full POST buffer</param>
        public void AddPostKey(string fullPostBuffer)
        {
            AddPostKey(null, fullPostBuffer);
        }

        /// <summary>
        /// Adds a fully self contained POST buffer to the request.
        /// Works for XML or previously encoded content.
        /// </summary>	    
        /// <param name="fullPostBuffer">Byte array of a full POST buffer</param>
        public void AddPostKey(byte[] fullPostBuffer)
        {
            AddPostKey(null, fullPostBuffer);
        }

        /// <summary>
        /// Allows posting a file to the Web Server. Make sure that you 
        /// set PostMode
        /// </summary>
        /// <param name="key"></param>
        /// <param name="filename"></param>
        /// <param name="contentType">Content type of the file to upload. Default is application/octet-stream</param>
        /// <param name="contentFilename">Optional filename to use in the Content-Disposition header. If not specified uses the file name of the file being uploaded.</param>
        /// <returns>true or false. Fails if the file is not found or couldn't be encoded</returns>
        public bool AddPostFile(string key, string filename, string contentType = "application/octet-stream",
            string contentFilename = null)
        {
            byte[] lcFile;

            if (RequestFormPostMode != HttpFormPostMode.MultiPart)
            {
                ErrorMessage = "File upload allowed only with Multi-part forms";
                HasErrors = true;
                return false;
            }

            HasPostData = true;
            try
            {
                FileStream loFile = new FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);

                lcFile = new byte[loFile.Length];
                _ = loFile.Read(lcFile, 0, (int)loFile.Length);
                loFile.Close();
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                HasErrors = true;
                return false;
            }

            if (PostData == null)
            {
                PostStream = new MemoryStream();
                PostData = new BinaryWriter(PostStream);
            }

            if (string.IsNullOrEmpty(contentFilename))
                contentFilename = new FileInfo(filename).Name;

            PostData.Write(Encoding.Default.GetBytes(
                "--" + HttpClientUtils.STR_MultipartBoundary + "\r\n" +
                "Content-Disposition: form-data; name=\"" + key + "\"; filename=\"" +
                contentFilename + "\"\r\n" +
                "Content-Type: " + contentType +
                "\r\n\r\n"));

            PostData.Write(lcFile);
            PostData.Write(Encoding.Default.GetBytes("\r\n"));

            return true;
        }

        /// <summary>
        /// Retrieves the accumulated postbuffer as a byte array
        /// when AddPostKey() or AddPostFile() have been called.        
        /// </summary>
        /// <returns>the Post buffer or null if empty or not using
        /// form post mode</returns>
        public string GetPostBuffer()
        {
            var bytes = PostStream?.ToArray();
            if (bytes == null)
                return null;
            var data = Encoding.Default.GetString(bytes);
            if (RequestFormPostMode == HttpFormPostMode.MultiPart)
            {
                if (PostStream == null)
                    return null;
                // add final boundary
                data += "\r\n--" + HttpClientUtils.STR_MultipartBoundary + "--\r\n";
            }

            return data;
        }

        /// <summary>
        /// Retrieves the accumulated postbuffer as a byte array
        /// when AddPostKey() or AddPostFile() have been called.
        /// 
        /// For multi-part forms this buffer can only be returned
        /// once as a footer needs to be appended and we don't want
        /// copy the buffer and double memory usage.
        /// </summary>
        /// <returns>encoded POST buffer</returns>
        public byte[] GetPostBufferBytes()
        {
            if (RequestFormPostMode == HttpFormPostMode.MultiPart)
            {
                if (PostStream == null)
                    return null;
                // add final boundary
                PostData.Write(Encoding.Default.GetBytes("\r\n--" + HttpClientUtils.STR_MultipartBoundary + "--\r\n"));
                PostStream?.Flush();
            }

            return PostStream?.ToArray();
        }

        #endregion


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
            var json = await GetResponseStringAsync();

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

            if (obj.ContainsKey("message"))
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

        public override string ToString()
        {
            return $"{HttpVerb} {Url}   {ErrorMessage}";
        }
    }

    public enum HttpFormPostMode
    {
        UrlEncoded,
        MultiPart
    };
}