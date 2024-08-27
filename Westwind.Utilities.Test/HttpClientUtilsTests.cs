﻿
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Westwind.Utilities;

namespace Westwind.Utilities.Test
{
    [TestClass]
    public class HttpClientUtilsTests
    {
        public HttpClientUtilsTests()
        {
            // force Json.NET to load
            //var f = Newtonsoft.Json.Formatting.Indented;
        }

        [TestMethod]
        public async Task HttpRequestStringWithUrlTest()
        {
            string html = await HttpClientUtils.DownloadStringAsync(new HttpClientRequestSettings
            {
                Url = "http://west-wind.com"
            });
            Assert.IsNotNull(html);
            Console.WriteLine(html);
        }


        [TestMethod]
        public async Task HttpRequestStringWithBadUrlTest()
        {
            var settings = new HttpClientRequestSettings
            {
                Url = "http://west-wind.com/bogus.html"
            };
            string html = await HttpClientUtils.DownloadStringAsync(settings);
            

            // result is a 404 with content but success has no result (null)
            Assert.IsNull(html);

            Assert.IsTrue(settings.HasErrors); 
            Assert.IsTrue(settings.ResponseStatusCode == HttpStatusCode.NotFound);

            var content = await settings.GetResponseStringAsync();

            Assert.IsNotNull(content);
            Console.WriteLine(content);            
        }

        [TestMethod]
        public async Task HttpRequestJsonStringWithUrlTest()
        {
            // returns a 404 so bad request
            var settings = new HttpClientRequestSettings
            {
                Url = "http://websurgeapi.west-wind.com/api/authentication/usertokenvalidation/1234"
            };
            string json = await HttpClientUtils.DownloadStringAsync(settings);
            Assert.IsNull(json);
            Assert.IsTrue(settings.HasErrors);


            json = await settings.GetResponseStringAsync();
            Console.WriteLine(json);
            Console.WriteLine(settings.ErrorMessage);
        }

        [TestMethod]
        public async Task HttpRequestHtmlPostTest()
        {
            string html = await HttpClientUtils.DownloadStringAsync(new HttpClientRequestSettings
            {
                Url = "https://west-wind.com/wconnect/Testpage.wwd",
                HttpVerb = "POST",
                RequestContent = "FirstName=Rick&Company=West+Windx",
                RequestContentType = "application/x-www-form-urlencoded"                
            });

            Assert.IsNotNull(html);
            Console.WriteLine(html);
        }


        [TestMethod]
        public async Task HttpRequestBadJsonPostTest()
        {
            string json = """
{
    "username": "test@test.com",
    "password": "kqm3ube0jnm!QKC9wcx"
}
""";
            var settings = new HttpClientRequestSettings
            {
                Url = "https://store.west-wind.com/api/account/authenticate",
                HttpVerb = "POST",
                RequestContent = json,
                RequestContentType = "application/json"
            };

            json = await HttpClientUtils.DownloadStringAsync(settings);

            Assert.IsNull(json, settings.ErrorMessage);

            json = await settings.GetResponseStringAsync();

            Assert.IsNotNull(json,"Error content not retrieved.");

            Console.WriteLine(json);

            // 
            var jobj  = await settings.GetResponseJson<JObject>();
            Assert.IsNotNull(jobj);
            
            Console.WriteLine(jobj);
            Console.WriteLine(((dynamic)jobj).message);
        }



        [TestMethod]
        public async Task HttpRequestBadJsonPostWithExceptionsTest()
        {
            string json = """
                          {
                              "username": "test@test.com",
                              "password": "kqm3ube0jnm!QKC9wcx"
                          }
                          """;
            var settings = new HttpClientRequestSettings
            {
                Url = "https://store.west-wind.com/api/account/bogus",
                HttpVerb = "POST",
                RequestContent = json,
                RequestContentType = "application/json",
                ThrowExceptions = true
            };
            
            try
            {
                json = await HttpClientUtils.DownloadStringAsync(settings);
                Console.WriteLine(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);  // 404

                var htmlErrorPage = await settings.GetResponseStringAsync();
                Assert.IsNotNull(htmlErrorPage);
                Assert.IsTrue(htmlErrorPage.StartsWith("<!DOCTYPE"));
                //Console.WriteLine(htmlErrorPage);
            }
            
        }
    }
}
