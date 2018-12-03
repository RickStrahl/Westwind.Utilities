using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;
using System.Xml;
using Westwind.Utilities;

namespace Westwind.Utilities.Tests
{
    /// <summary>
    /// Summary description for StrExtractTest
    /// </summary>
    [TestClass]
    public class XmlUtilsTest
    {
	    public XmlUtilsTest()
	    {
		    
	    }

        
        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
		private TestContext testContextInstance;


	    const string XmlTest = @"<docRoot>
	<name>Bill Bunsen</name>
	<count>32</count>
	<price>122.22</price>
	<discount>1,30</discount>
	<keyValue>value2</keyValue>
	<isActive>true</isActive>
	<attributeNode type=""data"" count=""21""  isLive=""true"" />
</docRoot>";

		[TestMethod]
		public void ElementValuesTest()
		{
			var dom = new XmlDocument();
			dom.LoadXml(XmlTest);

			XmlNode node = dom.DocumentElement;

			string name = XmlUtils.GetXmlString(node,"name");
			Assert.IsNotNull(name);
			Assert.AreEqual("Bill Bunsen",name);

			int count = XmlUtils.GetXmlInt(node, "count");
			Assert.AreEqual(32, count);

			decimal price = XmlUtils.GetXmlDecimal(node, "price");
			Assert.AreEqual(122.22M, price);

			decimal discount = XmlUtils.GetXmlDecimal(node, "discount");
			Assert.AreEqual(130M, discount);

			bool isActive = XmlUtils.GetXmlBool(node, "isActive");
			Assert.IsTrue(isActive);

			var keyValue = XmlUtils.GetXmlEnum<TestKeyValues>(node, "keyValue");
			Assert.IsTrue(TestKeyValues.value2 == keyValue);
		}

		[TestMethod]
		public void AttributeValuesTest()
		{
			var dom = new XmlDocument();
			dom.LoadXml(XmlTest);

			XmlNode node = dom.DocumentElement;
			
			var attributes = XmlUtils.GetXmlNode(node, "attributeNode");

			string type = XmlUtils.GetXmlAttributeString(attributes, "type");
			Assert.IsNotNull(type);
			Assert.AreEqual("data", type);

			int count = XmlUtils.GetXmlAttributeInt(attributes, "count", -1);
			Assert.AreEqual(21, count);

			bool? isLive = XmlUtils.GetXmlAttributeBool(attributes, "isLive");
			Assert.IsNotNull(isLive);
			Assert.IsTrue(isLive.Value);

		}

        [TestMethod]
        public void XmlStringForElement()
        {
            var text = "Characters: <doc> \" ' & \r\n break and a \t tab too.";

            var xmlString = XmlUtils.XmlString(text);

            Assert.IsTrue(xmlString.Contains("&lt;doc&gt;"));
            Assert.IsTrue(xmlString.Contains("&amp;"));
            Assert.IsFalse(xmlString.Contains("&apos;") || xmlString.Contains("&quot;"));
        }

        [TestMethod]
        public void XmlStringForAttributes()
        {
            var text = "Characters: <doc> \" ' & \r\n break and a \t tab too.";

            var xmlString = XmlUtils.XmlString(text,true);

            Assert.IsTrue(xmlString.Contains("&lt;doc&gt;"));
            Assert.IsTrue(xmlString.Contains("&amp;"));
            Assert.IsTrue(xmlString.Contains("&apos;") || xmlString.Contains("&quot;"),"Missing quotes.");

            Console.WriteLine(xmlString);
        }
    }

	public enum TestKeyValues
	{
		value1,
		value2
	}
}














