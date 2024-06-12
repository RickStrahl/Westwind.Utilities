﻿using System;
using System.Collections.Specialized;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Utilities;
using System.Data.Entity;

namespace Westwind.Utilities.Tests
{
    /// <summary>
    /// Summary description for StringUtilsTests
    /// </summary>
    [TestClass]
    public class UrlEncodingParserTests
    {
        [TestMethod]
        public void QueryStringTest()
        {
            string str = "http://mysite.com/page1?id=3123&format=json&action=edit&text=It's%20a%20brave%20new%20world!";

            var query = new UrlEncodingParser(str);
            Console.WriteLine(query);

            Assert.IsTrue(query["id"] == "3123");
            Assert.IsTrue(query["format"] == "json", "wrong format " + query["format"]);
            Assert.IsTrue(query["action"] == "edit");

            Console.WriteLine(query["text"]);
            // It's a brave new world!

            query["id"] = "4123";
            query["format"] = "xml";
            query["name"] = "<< It's a brave new world! say what?";

            var url = query.ToString();

            Console.WriteLine(url);            
            Console.Write(query.ToString());
            //http://mysite.com/page1?id=4123&format=xml&action=edit&
            //text=It's%20a%20brave%20new%20world!&name=%3C%3C%20It's%20a%20brave%20new%20world!
        }

        [TestMethod]
        public void QueryStringNullTest()
        {
            var query = new UrlEncodingParser(null);                        
            Assert.IsTrue(query.Count == 0);

            query.Set("id", "3123");
            Assert.IsTrue(query.Count == 1);
            
            Assert.IsTrue(query["id"] == "3123");

            query = new UrlEncodingParser(null);
            var nvCol = query.Parse(null);

            Assert.IsTrue(nvCol != null);
            Assert.IsTrue(nvCol.Count == 0);
        }

        [TestMethod]
        public void QueryValuesNullTest()
        {
            var query = new UrlEncodingParser(null);
            query["test"] = "1234";
            query["id"] = null;
            query["name"] = "Anita";

            var c = query["id"];
            Assert.IsTrue(c == string.Empty);

            var qs= query.ToString();
            Console.WriteLine(qs);

            // id should not be in the query
            Assert.IsTrue(!qs.Contains("id=&"));
        }


        [TestMethod]
        public void QueryStringMultipleTest()
        {
            string str = "http://mysite.com/page1?id=3123&format=json&format=xml";

            var query = new UrlEncodingParser(str);

            Assert.IsTrue(query["id"] == "3123");
            Assert.IsTrue(query["format"] == "json,xml", "wrong format " + query["format"]);

            // multiple format strings
            string[] formats = query.GetValues("format");
            Assert.IsTrue(formats.Length == 2);

            query.SetValues("multiple", new[]
            {
                "1",
                "2",
                "3"
            });

            var url = query.ToString();

            Console.WriteLine(url);

            Assert.IsTrue(url ==
                          "http://mysite.com/page1?id=3123&format=json&format=xml&multiple=1&multiple=2&multiple=3");
        }

        [TestMethod]
        public void QueryStringPlusSigns()
        {
            string str = "http://mysite.com/page1?text=It's+a+depressing+world+out+there";

            var query = new UrlEncodingParser(str, true);
           
            string text = query["text"];
            Console.WriteLine(text);

            Assert.IsFalse(text.Contains("+") );
            Assert.IsTrue(text.Contains(" "));;
        }

        [TestMethod]
        public void WriteUrlTest()
        {
            // URL only
            string url = "http://test.com/page";

            var query = new UrlEncodingParser(url);
            query["id"] = "321312";
            query["name"] = "rick";

            url = query.ToString();
            Console.WriteLine(url);

            Assert.IsTrue(url.Contains("name="));
            Assert.IsTrue(url.Contains("http://"));

            // URL with ? but no query
            url = "http://test.com/page?";

            query = new UrlEncodingParser(url);
            query["id"] = "321312";
            query["name"] = "rick";

            url = query.ToString();
            Console.WriteLine(url);

            Assert.IsTrue(url.Contains("name="));


            // URL with query
            url = "http://test.com/page?q=search";

            query = new UrlEncodingParser(url);
            query["id"] = "321312";
            query["name"] = "rick";

            url = query.ToString();
            Console.WriteLine(url);

            Assert.IsTrue(url.Contains("name="));
            Assert.IsTrue(url.Contains("http://"));


            // Raw query data
            url = "q=search&name=james";

            query = new UrlEncodingParser(url);
            query["id"] = "321312";
            query["name"] = "rick";

            url = query.ToString();
            Console.WriteLine(url);

            Assert.IsTrue(url.Contains("name="));
            Assert.IsTrue(!url.Contains("http://"));


            // No data at all
            url = null;

            query = new UrlEncodingParser();
            query["id"] = "321312";
            query["name"] = "rick";

            url = query.ToString();
            Console.WriteLine(url);

            Assert.IsTrue(url.Contains("name="));
            Assert.IsTrue(!url.Contains("http://"));
        }

#if NETFULL
		[TestMethod]
        public void HttpUtilityTest()
        {
            var nv = HttpUtility.ParseQueryString("");
            nv["id"] = "Rick";
            nv["format"] = "json";
            Console.WriteLine(nv);
        }
#endif
	}
}
