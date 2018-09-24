using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Utilities;

namespace Westwind.Utilities.Tests
{
    [TestClass]
    public class SanitizeHtmlTests
    {

#if NETFULL
        [TestMethod]
        public void HtmlSanitizeScriptTags()
        {
            string html = "<div>User input with <ScRipt>alert('Gotcha');</ScRipt></div>";

            var result = HtmlUtils.SanitizeHtml(html);

            Console.WriteLine(result);
            Assert.IsTrue(!result.Contains("<ScRipt>"));
        }
#endif

        [TestMethod]
        public void HtmlSanitizeJavaScriptTags()
        {
            string html = "<div>User input with <a href=\"javascript: alert('Gotcha')\">Don't hurt me!<a/></div>";

            var result = HtmlUtils.SanitizeHtml(html);

            Console.WriteLine(result);
            Assert.IsTrue(!result.Contains("javascript:"));
        }

        [TestMethod]
        public void HtmlSanitizeJavaScriptTagsSingleQuotes()
        {
            string html = "<div>User input with <a href='javascript: alert(\"Gotcha\");'>Don't hurt me!<a/></div>";

            var result = HtmlUtils.SanitizeHtml(html);

            Console.WriteLine(result);
            Assert.IsTrue(!result.Contains("javascript:"));
        }

        [TestMethod]
        public void HtmlSanitizeJavaScriptTagsWithUnicodeQuotes()
        {
            string html = "<div>User input with <a href='&#106;&#97;&#118;&#97;&#115;&#99;&#114;&#105;&#112;&#116;:alert(\"javascript active\");'>Don't hurt me!<a/></div>";

            var result = HtmlUtils.SanitizeHtml(html);

            Console.WriteLine(result);
            Assert.IsTrue(!result.Contains("&#106;&#97;&#118"));
        }


        [TestMethod]
        public void HtmlSanitizeEventAttributes()
        {
            string html = "<div onmouseover=\"alert('Gotcha!')\">User input with " +
                          "<div onclick='alert(\"Gotcha!\");'>Don't hurt me!<div/>" +
                          "</div>";

            var result = HtmlUtils.SanitizeHtml(html);

            Console.WriteLine(result);
            Assert.IsTrue(!result.Contains("onmouseover:") && !result.Contains("onclick"));
        }

        [TestMethod]
        public void IncorrectOnParsingBugTest()
        {
            string html = "<div><a href=\"https://west-wind.com\">This</a> is on <a href=\"https://markdownmonster.west-wind.com\">time</a> train.";

            var result = HtmlUtils.SanitizeHtml(html);

            Console.WriteLine(result);
            Assert.IsTrue(result.Contains("on <"), "On shouldn't be transformed or removed: " + result);
        }


        [TestMethod]
        public void IncorrectOnEventParsingWithFakeEventInBodyTest()
        {
            string html = "<div><a href=\"https://west-wind.com\">This</a> is onchange=\"this should stay\" <a href=\"https://markdownmonster.west-wind.com\">time</a> train.";

            var result = HtmlUtils.SanitizeHtml(html);

            Console.WriteLine(result);
            Assert.IsTrue(result.Contains("onchange="), "OnChange should not  be removed: " + result);
        }
    }
}
