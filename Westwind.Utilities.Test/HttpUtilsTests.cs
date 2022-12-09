﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Westwind.Utilities.Test
{
    [TestClass]
    public class HttpUtilsTests
    {
        public HttpUtilsTests()
        {
            // force Json.NET to load
            //var f = Newtonsoft.Json.Formatting.Indented;
        }

        [TestMethod]
        public void HttpRequestStringWithUrlTest()
        {
            string html = HttpUtils.HttpRequestString("http://microsoft.com");
            Assert.IsNotNull(html);
        }

        [TestMethod]
        public async Task HttpRequestStringWithUrlAsyncTest()
        {
            string html = await HttpUtils.HttpRequestStringAsync("http://microsoft.com");
            Assert.IsNotNull(html);
        }

        [TestMethod]
        [ExpectedException(typeof(WebException))]
        public void InvalidUrlTest()
        {
            var settings = new HttpRequestSettings()
            {
                Url = "http://west-wind.com/invalidurl.html",
            };

            string html = HttpUtils.HttpRequestString(settings);                       
            Assert.IsTrue(settings.ResponseStatusCode == System.Net.HttpStatusCode.NotFound);

        }

        
        
        [TestMethod]
        public void HttpRequestStringWithSettingsTest()
        {
            var settings = new HttpRequestSettings()
            {
                Url = "http://microsoft.com",                 
            };

            string html = HttpUtils.HttpRequestString(settings);
            Assert.IsNotNull(html);
            Assert.IsTrue(settings.ResponseStatusCode == System.Net.HttpStatusCode.OK);
        }

#if false      // Web Site has gone away

        [TestMethod]
        public void JsonRequestTest()
        {
            var settings = new HttpRequestSettings()
            {
                Url = "http://codepaste.net/recent?format=json",
            };

            var snippets = HttpUtils.JsonRequest<List<CodeSnippet>>(settings);

            Assert.IsNotNull(snippets);
            Assert.IsTrue(settings.ResponseStatusCode == System.Net.HttpStatusCode.OK);
            Assert.IsTrue(snippets.Count > 0);
            Console.WriteLine(snippets.Count);
        }

        [TestMethod]
        public async Task JsonRequestAsyncTest()
        {
            var settings = new HttpRequestSettings()
            {
                Url = "https://albumviewer.west-wind.com/album/37"

            };

            var snippets = await HttpUtils.JsonRequestAsync<List<CodeSnippet>>(settings);

            Assert.IsNotNull(snippets);
            Assert.IsTrue(settings.ResponseStatusCode == System.Net.HttpStatusCode.OK);
            Assert.IsTrue(snippets.Count > 0);
            Console.WriteLine(snippets.Count);
            Console.WriteLine(settings.CapturedResponseContent);
        }

        [TestMethod]
        public void JsonRequestPostTest()
        {
            var postSnippet = new CodeSnippet()
            {
                UserId = "Bogus",
                Code = "string.Format('die Bären sind süss und sauer.');",
                Comment = "World domination imminent"
            };

            var settings = new HttpRequestSettings()
            {
                Url = "http://codepaste.net/recent?format=json",
                Content = postSnippet,
                HttpVerb = "POST"
            };

            var snippets = HttpUtils.JsonRequest<List<CodeSnippet>>(settings);

            Assert.IsNotNull(snippets);
            Assert.IsTrue(settings.ResponseStatusCode == System.Net.HttpStatusCode.OK);
            Assert.IsTrue(snippets.Count > 0);

            Console.WriteLine(snippets.Count);
            Console.WriteLine(settings.CapturedRequestContent);
            Console.WriteLine();
            Console.WriteLine(settings.CapturedResponseContent);

            foreach (var snippet in snippets)
            {
                if (string.IsNullOrEmpty(snippet.Code))
                    continue;
                Console.WriteLine(snippet.Code.Substring(0, Math.Min(snippet.Code.Length, 200)));
                Console.WriteLine("--");
            }
            
            Console.WriteLine("Status Code: " + settings.Response.StatusCode);

            foreach (var header in settings.Response.Headers)
            {
                Console.WriteLine(header + ": " + settings.Response.Headers[header.ToString()]);
            }
        }

        [TestMethod]
        public async Task JsonRequestPostAsyncTest()
        {
            var postSnippet = new CodeSnippet()
            {
                UserId = "Bogus",
                Code = "string.Format('Hello World, I will own you!');",
                Comment = "World domination imminent"
            };

            var settings = new HttpRequestSettings()
            {
                Url = "http://codepaste.net/recent?format=json",
                Content = postSnippet,
                HttpVerb = "POST"
            };

            var snippets = await HttpUtils.JsonRequestAsync<List<CodeSnippet>>(settings);

            Assert.IsNotNull(snippets);
            Assert.IsTrue(settings.ResponseStatusCode == System.Net.HttpStatusCode.OK);
            Assert.IsTrue(snippets.Count > 0);

            Console.WriteLine(snippets.Count);
            Console.WriteLine(settings.CapturedRequestContent);
            Console.WriteLine();
            Console.WriteLine(settings.CapturedResponseContent);

            foreach (var snippet in snippets)
            {
                if (string.IsNullOrEmpty(snippet.Code))
                    continue;
                Console.WriteLine(snippet.Code.Substring(0, Math.Min(snippet.Code.Length, 200)));
                Console.WriteLine("--");
            }

            // This doesn't work for the async version - Response is never set by the base class
            Console.WriteLine("Status Code: " + settings.Response.StatusCode);

            foreach (var header in settings.Response.Headers)
            {
                Console.WriteLine(header + ": " + settings.Response.Headers[header.ToString()]);
            }
        }
#endif

        [TestMethod]
        public void DownloadImageFile()
        {
            string url = "https://markdownmonster.west-wind.com/Images/MarkdownMonster_Icon_32.png";

            string fname = null;
            try
            {
                fname = HttpUtils.DownloadImageToFile(url);

                Console.WriteLine(fname);

                Assert.IsNotNull(fname);
                Assert.IsTrue(File.Exists(fname));

                //ShellUtils.ShellExecute(fname,null);
                //Thread.Sleep(500);
            }
            finally
            {
                if (!string.IsNullOrEmpty(fname))
                   File.Delete(fname);

                Assert.IsFalse(File.Exists(fname));
            }
        }


        [ExpectedException(typeof(WebException))]
        [TestMethod()]
        public void HttpTimeout()
        {
            string result = HttpUtils.HttpRequestString(new HttpRequestSettings()
            {
                Url="http://west-wind.com/files/wconnect.exe",
                Timeout = 100
            });
			
            Assert.IsNotNull(result);
        }
    }

    public class CodeSnippet
    {
        public int CommentCount { get; set; }
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Language { get; set; }
        public int Views { get; set; }
        public string Code { get; set; }
        public string Comment { get; set; }
        public DateTime Entered { get; set; }
    }
}
