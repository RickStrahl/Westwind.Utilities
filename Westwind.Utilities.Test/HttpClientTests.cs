using System;
using System.Threading;
using System.Threading.Tasks;
using Westwind.Utilities.InternetTools;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Westwind.Utilities.InternetTools
{
    [TestClass]
    public class HttpClientTests
    { 

        [TestMethod]
        public void InvalidUrlTest()
        {
            var client = new HttpClient();

            var html = client.DownloadString("http://weblog.west-wind.com/nonexistantpage.htm");

            Assert.IsTrue(client.WebResponse.StatusCode == System.Net.HttpStatusCode.NotFound);            
            Console.WriteLine(client.WebResponse.StatusCode);
        }


      
        [TestMethod]
        public void HttpTimingsTest()
        {
            var client = new HttpClient();

            var html = client.DownloadString("http://weblog.west-wind.com/posts/2015/Jan/06/Using-Cordova-and-Visual-Studio-to-build-iOS-Mobile-Apps");

            Console.WriteLine(client.WebResponse.ContentLength);
            Console.WriteLine(client.HttpTimings.StartedTime);
            Console.WriteLine("First Byte: " + client.HttpTimings.TimeToFirstByteMs);
            Console.WriteLine("Last Byte: " + client.HttpTimings.TimeToLastByteMs);
        }


        [TestMethod]
        public async Task HttpTimingsTestsAsync()
        {
            var client = new HttpClient();

            var html = await client.DownloadStringAsync("http://weblog.west-wind.com/posts/2015/Jan/06/Using-Cordova-and-Visual-Studio-to-build-iOS-Mobile-Apps");

            Console.WriteLine(client.WebResponse.ContentLength);
            Console.WriteLine(client.HttpTimings.StartedTime);
            Console.WriteLine("First Byte: " + client.HttpTimings.TimeToFirstByteMs);
            Console.WriteLine("Last Byte: " + client.HttpTimings.TimeToLastByteMs);
        }

        [TestMethod]
        public void AddPostKeyGetPostBufferTest()
        {
            var client = new HttpClient();
            client.ContentType = "application/x-www-form-urlencoded";

            client.AddPostKey("ctl00_Content_Username", "Rick");
            client.AddPostKey("ctl00_Content_Password", "seekrit");

            string post = client.GetPostBuffer();
            Console.WriteLine(post);

            Assert.IsTrue(post.Contains("&ctl00_Content_Password="));
        }

        [TestMethod]
        public void AddPostKeyRawGetPostBufferTest()
        {
            var client = new HttpClient();
            client.ContentType = "application/x-www-form-urlencoded";

            // raw POST buffer
            client.AddPostKey("name=Rick&company=West%20Wind");
            
            string post = client.GetPostBuffer();
            Console.WriteLine(post);

            Assert.IsTrue(post == "name=Rick&company=West%20Wind");
        }

    }
}
