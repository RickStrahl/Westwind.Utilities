using System;
using System.Net;
using System.Web;

namespace Westwind.Utilities
{
    /// <summary>
    /// Customized version of WebClient that provides access
    /// to the Response object so we can read result data 
    /// from the Response.
    /// </summary>
    public class HttpUtilsWebClient : WebClient
    {
        /// <summary>
        /// Intializes this instance of WebClient with settings values
        /// </summary>
        /// <param name="settings"></param>
        public HttpUtilsWebClient(HttpRequestSettings settings = null)
        {
            Settings = settings;

            if (settings != null)
            {
                if (settings.Credentials != null)
                    Credentials = settings.Credentials;

                if (settings.Proxy != null)
                    Proxy = settings.Proxy;

                if (settings.Encoding != null)
                    Encoding = settings.Encoding;

                if (settings.Headers != null)
                {
                    foreach (var header in settings.Headers)
                    {
                        Headers[header.Key] = header.Value;
                    }
                }

            }            
        }

        internal HttpRequestSettings Settings { get; set; }
        internal HttpWebResponse Response { get; set; }
        internal HttpWebRequest Request { get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            Request = base.GetWebRequest(address) as HttpWebRequest;
            
            if (Settings != null)
            {
                if (Settings.Timeout > 0)
                {
                    Request.Timeout = Settings.Timeout;
                    Request.ReadWriteTimeout = Settings.Timeout;
                    Request.PreAuthenticate = Settings.PreAuthenticate;                    
                }

                if (!string.IsNullOrEmpty(Settings.UserAgent))
                    Request.UserAgent = Settings.UserAgent;

                if (!string.IsNullOrEmpty(Settings.HttpVerb))
                    Request.Method = Settings.HttpVerb;
            }

            return Request;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            try
            {
                Response = base.GetWebResponse(request) as HttpWebResponse;
            }
            catch (WebException ex)
            {
                if(Settings.DontThrowOnErrorStatusCodes) {
                    Response = ex?.Response as HttpWebResponse;
                    return ex?.Response;
                }

                throw ex;
            }

            return Response;
        }

        protected override WebResponse GetWebResponse(WebRequest request, System.IAsyncResult result)
        {
            try
            {
                Response = base.GetWebResponse(request, result) as HttpWebResponse;
            }
            catch (WebException ex)
            {
                if (Settings.DontThrowOnErrorStatusCodes) {
                    Response = ex?.Response as HttpWebResponse;
                    return ex?.Response;
                }

                throw ex;
            }

            return Response;
        }


    }
}