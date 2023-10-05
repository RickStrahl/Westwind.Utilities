﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Utilities;

namespace Westwind.Utilities.Tests
{
    /// <summary>
    /// Summary description for StringUtilsTests
    /// </summary>
    [TestClass]
    public class StringUtilsTests
    {
        public StringUtilsTests()
        {
        }

        private TestContext testContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        #region Additional test attributes

        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //

        #endregion

        [TestMethod]
        public void ToCamelCaseTest()
        {
            string original = "This is a test";
            string expected = "ThisIsATest";
            string actual = StringUtils.ToCamelCase(original);
            Assert.AreEqual(expected, actual, "Failed Simple Test");

            original = null;
            expected = "";
            actual = StringUtils.ToCamelCase(original);
            Assert.AreEqual(expected, actual, "Failed Null Test");

            original = "Pronto 123";
            expected = "Pronto123";
            actual = StringUtils.ToCamelCase(original);
            Assert.AreEqual(expected, actual, "Failed Embedded Numbers Test");

            original = "None";
            expected = "None";
            actual = StringUtils.ToCamelCase(original);
            Assert.AreEqual(expected, actual, "Failed Null Test");
        }

        [TestMethod]
        public void FromCamelCaseTest()
        {
            string original = "NoProblem";
            string expected = "No Problem";
            string actual = StringUtils.FromCamelCase(original);
            Assert.AreEqual(expected, actual, "Failed Simple Test");

            expected = null;
            original = null;
            actual = StringUtils.FromCamelCase(original);
            Assert.AreEqual(expected, actual, "Failed null test - exception should have been thrown.");

            expected = "Pronto 123";
            original = "Pronto123";
            actual = StringUtils.FromCamelCase(original);
            Assert.AreEqual(expected, actual, "Failed Embedded Numbers Test");


            expected = "The Mountains Are Beautiful";
            original = "TheMountainsAreBeautiful";
            actual = StringUtils.FromCamelCase(original);
            Assert.AreEqual(expected, actual, "Failed Long String Of Text");


            expected = "None";
            original = "None";
            actual = StringUtils.FromCamelCase(original);
            Assert.AreEqual(expected, actual, "Failed No CamelCase Test");

            expected = "OK"; // multiple upper case letters don't split
            original = "OK";
            actual = StringUtils.FromCamelCase(original);
            Assert.AreEqual(expected, actual, "Failed No CamelCase Test");

            expected = "IISAdmin";   // multiple upper case letters don't split
            original = "IISAdmin";
            actual = StringUtils.FromCamelCase(original);
            Assert.AreEqual(expected, actual, "Failed No CamelCase Test");


        }

        [TestMethod]
        public void ToCamelCaseUpperCasingTest()
        {

            var original = "ABCCompany";
            var expected = "ABCCompany";
            var actual = StringUtils.FromCamelCase(original);
            Console.WriteLine(actual);
            Assert.AreEqual(expected, actual, "Failed UpperCase Letters CamelCase Test");

            original = "ThisIsATest";
            expected = "This Is ATest";
            actual = StringUtils.FromCamelCase(original);
            Console.WriteLine(actual);
            Assert.AreEqual(expected, actual, "Failed UpperCase Letters CamelCase Test");

            original = "ABCdef";
            expected = "ABCdef";
            actual = StringUtils.FromCamelCase(original);
            Console.WriteLine(actual);
            Assert.AreEqual(expected, actual, "Failed UpperCase Letters CamelCase Test");

        }

        [TestMethod]
        public void NormalizeIndentationTest()
        {
            string code = @"
            try
            {
                // null should throw ArgumentException
                actual = StringUtils.FromCamelCase(original);
            }";

            string result = StringUtils.NormalizeIndentation(code).Trim();

            Assert.IsTrue(result.Substring(0, 3) == "try", "Not indented");

            code = @"
		************************************************************************
		FUNCTION IsWinnt
		*****************
		***      Pass: llReturnVersionNumber
		***    Return: .t. or .f.   or Version Number or -1 if not NT
		*************************************************************************
		LPARAMETER llReturnVersionNumber

		loAPI=CREATE(""wwAPI"")
		lcVersion = loAPI.ReadRegistryString(HKEY_LOCAL_MACHINE,;
		           ""SOFTWARE\Microsoft\Windows NT\CurrentVersion"",;
		           ""CurrentVersion"")
		                          
		IF !llReturnVersionNumber
		  IF ISNULL(lcVersion)
		     RETURN .F.
		  ELSE
		     RETURN .T.
		  ENDIF
		ENDIF";

            result = StringUtils.NormalizeIndentation(code).Trim();

            Assert.IsTrue(result.Substring(0, 3) == "***", "Not indented");
        }

        [TestMethod]
        public void ReplicateStringTest()
        {
            Assert.IsTrue(StringUtils.Replicate("123", 3) == "123123123");
            Assert.IsTrue(StringUtils.Replicate("1", 2) == "11");
        }

        [TestMethod]
        public void ReplicateCharTest()
        {            
            Assert.IsTrue(StringUtils.Replicate('1', 4) == "1111");
        }

        [TestMethod]
        public void Base36EncodeTest()
        {
            // positive number
            long inputNumber = 512344131333132;
            long result = 0;
            string base36 = "";

            base36 = StringUtils.Base36Encode(inputNumber);
            Assert.IsTrue(!string.IsNullOrEmpty(base36), "Base36 number resulted in empty or null");
            result = StringUtils.Base36Decode(base36);
            Assert.AreEqual(inputNumber, result, "Base36 conversion failed.");

            // negative number
            inputNumber = -512344131333132;

            base36 = StringUtils.Base36Encode(inputNumber);
            Assert.IsTrue(!string.IsNullOrEmpty(base36), "Base36 number resulted in empty or null");
            result = StringUtils.Base36Decode(base36);
            Assert.AreEqual(inputNumber, result, "Base36 conversion failed.");

            inputNumber = 0;

            base36 = StringUtils.Base36Encode(inputNumber);
            Assert.IsTrue(!string.IsNullOrEmpty(base36), "Base36 number resulted in empty or null");
            result = StringUtils.Base36Decode(base36);
            Assert.AreEqual(inputNumber, result, "Base36 conversion failed.");
        }

        [TestMethod]
        public void GenerateUniqueId()
        {
            List<string> list = new List<string>();
            for (int i = 0; i < 100; i++)
            {
                list.Add(DataUtils.GenerateUniqueId());
            }

            int maxLength = list.Max(str => str.Length);
            int minLength = list.Min(str => str.Length);
            int negVals = list.Where(str => str.StartsWith("-")).Count();
            this.TestContext.WriteLine("Min Length: {0}, Max: {1}, Neg: {2}", minLength, maxLength, negVals);

            Assert.IsTrue(list.Distinct().Count() == 100, "Didn't create 100 unique entries");
        }

        [TestMethod]
        public void RandomStringTest()
        {
            for (int i = 0; i < 20; i++)
            {
                string random = StringUtils.RandomString(20);
                foreach (var ch in random)
                    Assert.IsTrue(char.IsLetter(ch));
            }
        }

        [TestMethod]
        public void RandomStringWithNumbersTest()
        {
            for (int i = 0; i < 20; i++)
            {
                string random = StringUtils.RandomString(20, true);
                foreach (var ch in random)
                    Assert.IsTrue(char.IsLetterOrDigit(ch));
            }
        }

        [TestMethod]
        public void ExtractStringTest()
        {
            string source = "Hello: <rant />";
            string extract = StringUtils.ExtractString(source, "<", "/>", false, false, true);



            Console.WriteLine(extract);
            Assert.AreEqual(extract, "<rant />");
        }

        [TestMethod]
        public void ExtractStringWithDelimitersTest()
        {
            string source = @"
# Another Test Blog Post

So this is a new test blog post. I can read this and can do some cool stuff with this.



<!-- Post Configuration -->
---
```xml
<abstract>
This is the abstract ofr this blog post.
</abstract>
<categories>
</categories>
<postid>1420322</postid>
<keywords>
</keywords>
<weblog>
Rick Strahl's Weblog
</weblog>
```
<!-- End Post Configuration -->
";

            string extract = StringUtils.ExtractString(source, "<!-- Post Configuration -->",
                "<!-- End Post Configuration -->", false, true, true);

            Console.WriteLine(extract);
            Assert.IsTrue(extract.Contains("<!-- Post Configuration -->"));
            Assert.IsTrue(extract.Contains("<!-- End Post Configuration -->"));
        }


        [TestMethod]
        public void GetLinesTest()
        {
            string s =
                @"this is test
with
multiple lines";

            var strings = StringUtils.GetLines(s);
            Assert.IsNotNull(strings);
            Assert.IsTrue(strings.Length == 3);


            s = string.Empty;
            strings = StringUtils.GetLines(s);
            Assert.IsNotNull(strings);
            Assert.IsTrue(strings.Length == 1);

            s = null;
            strings = StringUtils.GetLines(s);
            Assert.IsTrue(strings.Length == 0);
        }

        [TestMethod]
        public void CountLinesTest()
        {
            string s =
                "this is test\r\n" +
                "with\n" +
                "multiple lines";

            int count = StringUtils.CountLines(s);
            Assert.IsTrue(count == 3);

            s = string.Empty;
            count = StringUtils.CountLines(s);
            Assert.IsTrue(count == 0);

            s = null;
            count = StringUtils.CountLines(s);
            Assert.IsTrue(count == 0);
        }

        [TestMethod]
        public void IndexOfNthCharTest()
        {
            string version = "1.11.13.2";

            var idx = StringUtils.IndexOfNth(version, '.', 1);
            Assert.IsTrue(idx == 1);


            idx = StringUtils.IndexOfNth(version, '.', 3);
            Assert.IsTrue(version.Substring(idx, 1) == ".");

            idx = StringUtils.IndexOfNth(version, '.', 2);
            Assert.IsTrue(version.Substring(idx, 1) == ".");

            idx = StringUtils.IndexOfNth(version, '.', 1);
            Assert.IsTrue(version.Substring(idx, 1) == ".");

            idx = StringUtils.IndexOfNth(version, '.', 4);
            Assert.IsTrue(idx == -1,"");

            version = string.Empty;
            idx = StringUtils.IndexOfNth(version, '.', 2);
            Assert.IsTrue(idx == -1);

            version = null;
            idx = StringUtils.IndexOfNth(version, '.', 2);
            Assert.IsTrue(idx == -1);
        }

        [TestMethod]
        public void IndexOfNthStringTest()
        {
            string version = "11.11.11.11";

            var idx = StringUtils.IndexOfNth(version, ".11", 1);
            Assert.IsTrue(idx == 2,"position should be 3");

            idx = StringUtils.IndexOfNth(version, ".11", 2);                   
            Assert.IsTrue(idx == 5,"position should be 5");

            idx = StringUtils.IndexOfNth(version, ".11", 3);            
            Assert.IsTrue(idx == 8, "position should be 8");
            
            idx = StringUtils.IndexOfNth(version, ".11", 4);
            Assert.IsTrue(idx == -1, "no 4th item");
            
            idx = StringUtils.IndexOfNth(version, "11.", 1);
            Assert.IsTrue(idx == 0, "should be at start of string");

            version = string.Empty;
            idx = StringUtils.IndexOfNth(version, ".11", 2);
            Assert.IsTrue(idx == -1,"not empty");

            version = null;
            idx = StringUtils.IndexOfNth(version, ".11", 2);
            Assert.IsTrue(idx == -1,"not null");
        }


        [TestMethod]
        public void LastIndexOfNthStringTest()
        {
            string version = "1.11.11.11";

            var idx = StringUtils.LastIndexOfNth(version, ".11", 3);
            Assert.IsTrue(idx == 1, "Should be index of 1");
            
            idx = StringUtils.LastIndexOfNth(version, ".11", 1);            
            Assert.IsTrue(idx == 7, "should be . at index 7");
            Assert.IsTrue(version.Substring(idx, 3) == ".11", "should be . at index 7");
            
            idx = StringUtils.LastIndexOfNth(version, ".11", 2);
            Assert.IsTrue(idx == 4, "should be . at index 4");

            idx = StringUtils.LastIndexOfNth(version, '.', 3);           
            Assert.IsTrue(idx == 1,"index should be at 1");

            idx = StringUtils.LastIndexOfNth(version, '.', 4);
            Assert.IsTrue(idx == -1, "should not match 4th .");

            version = string.Empty;
            idx = StringUtils.LastIndexOfNth(version, '.', 2);
            Assert.IsTrue(idx == -1, "");

            version = null;
            idx = StringUtils.LastIndexOfNth(version, '.', 2);
            Assert.IsTrue(idx == -1);
        }

        [TestMethod]
        public void LastIndexOfNthCharTest()
        {
            string version = "1.11.13.2";

            var idx = StringUtils.LastIndexOfNth(version, '.', 3);
            Assert.IsTrue(idx == 1,"Should be index of 1");


            idx = StringUtils.LastIndexOfNth(version, '.', 1);
            Assert.IsTrue(version.Substring(idx, 1) == ".","should be . at index 1");

            idx = StringUtils.LastIndexOfNth(version, '.', 2);
            Assert.IsTrue(version.Substring(idx, 1) == ".","should be . at index 2");

            idx = StringUtils.LastIndexOfNth(version, '.', 3);
            Assert.IsTrue(version.Substring(idx, 1) == ".","should be . at index 3");
            Assert.IsTrue(idx == 1);

            idx = StringUtils.LastIndexOfNth(version, '.', 4);
            Assert.IsTrue(idx == -1,"should not match 4th .");

            version = string.Empty;
            idx = StringUtils.LastIndexOfNth(version, '.', 2);
            Assert.IsTrue(idx == -1,"");

            version = null;
            idx = StringUtils.LastIndexOfNth(version, '.', 2);
            Assert.IsTrue(idx == -1);
        }



        [TestMethod]
        public void StringTokenization()
        {
            string code = "This is a test {{DateTime.Now}}  and another {{System.Environment.CurrentDirectory}}";

            string tokenString = code;

            var tokens = StringUtils.TokenizeString(ref tokenString, "{{", "}}");
            Assert.IsTrue(tokens.Count > 0, "No tokens found");


            Console.WriteLine("Tokenized Code String:");
            Console.WriteLine(tokenString);

            Console.WriteLine("Tokens:");
            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }

            string returnedCode = StringUtils.DetokenizeString(tokenString, tokens);

            Console.WriteLine("--- returned ---");


            Console.WriteLine(returnedCode);
            Console.WriteLine(code);

            Assert.IsTrue(returnedCode == code, "Code doesn't match");
        }

        [TestMethod]
        public void NormalizeLineFeedTest()
        {
            string text = "Hello World!\r\nMy name is Harold.\nWhat do you want?";
            string converted = StringUtils.NormalizeLineFeeds(text, LineFeedTypes.Lf);

            Assert.IsNotNull(converted);
            Assert.IsFalse(converted.Contains("\r"));
            Assert.IsTrue(converted.Count(c => c == '\n') == 2);

            converted = StringUtils.NormalizeLineFeeds(text, LineFeedTypes.CrLf);
            Assert.IsNotNull(converted);
            Assert.IsTrue(converted.Count(c => c == '\n') == 2 && converted.Count(c => c == '\r') == 2);


            text = null;
            converted = StringUtils.NormalizeLineFeeds(text, LineFeedTypes.CrLf);
            Assert.IsNull(converted);

            text = String.Empty;
            converted = StringUtils.NormalizeLineFeeds(text, LineFeedTypes.CrLf);
            Assert.IsTrue(converted == string.Empty);
        }

        [TestMethod]
        public void TextAbstractTest()
        {
            var text = "This is a long block of text that needs to be truncated into a text abstract by ending in an elipses.\r\n" +
            "Multiple lines are converted to spaces to make the text cleaner to work with. This is some pretty long text to truncate\n\nand more lines.";

            string result = StringUtils.TextAbstract(text,200);

            Console.WriteLine($"{result.Length} {result}");

            // breaks on word boundaries so size may not be exact
            Assert.IsTrue(result.Length > 180 && result.Length < 204);
        }

        [TestMethod]
        public void IsStringInListTest()
        {
            var list = "value1, value2, value3, value4, Value5";

            Assert.IsTrue(StringUtils.IsStringInList(list, "value3"),"value3 not matched");
            Assert.IsFalse(StringUtils.IsStringInList(list, "value10"),"value10 shouldn't match");
            Assert.IsTrue(StringUtils.IsStringInList(list, "value5", ignoreCase: true),"value5 not matched");
            Assert.IsFalse(StringUtils.IsStringInList(list, "value5", ignoreCase: false),"value5 shouldn't match");
            Assert.IsFalse(StringUtils.IsStringInList(list, "value4", ignoreCase: true, separator: ';'), "value4 shouldn't match");

        }

        [TestMethod]
        public void OccursCharTest()
        {
            var s = "3211-32123-1233-123331";

            var count = StringUtils.Occurs(s, '-');
            Assert.IsTrue(count == 3);

            s = "3211321231233123331";
            count = StringUtils.Occurs(s, '-');
            Assert.IsTrue(count == 0);

            s = "";
            count = StringUtils.Occurs(s, '-');
            Assert.IsTrue(count == 0);

            s = null;
            count = StringUtils.Occurs(s, '-');
            Assert.IsTrue(count == 0);
        }

        [TestMethod]
        public void OccursStringTest()
        {
            var s = "3211-32123-1233-123331";

            var count = StringUtils.Occurs(s, "-");
            Assert.IsTrue(count == 3);

            s = "-3211-32123-1233-123331-";
            count = StringUtils.Occurs(s, "-");
            Assert.IsTrue(count == 5);


            s = "3211321231233123331";
            count = StringUtils.Occurs(s, "-");
            Assert.IsTrue(count == 0);

            s = "";
            count = StringUtils.Occurs(s, "-");
            Assert.IsTrue(count == 0);

            s = null;
            count = StringUtils.Occurs(s, "-");
            Assert.IsTrue(count == 0);
        }


        [TestMethod]
        public void GetMaxCharactersTest()
        {
            var s="1234567890";

            var result = StringUtils.GetMaxCharacters(s, 5);
            Assert.AreEqual(result.Length, 5);

            result = StringUtils.GetMaxCharacters(s, 15);
            Assert.AreEqual(result.Length, 10, "should be 10 lengths");
            
            result = StringUtils.GetMaxCharacters(s, 10);
            Assert.AreEqual(result.Length,10, "should be 10 lengths");

            s = null;
            result = StringUtils.GetMaxCharacters(s, 10);
            Assert.IsNull(result, "result should be null");

            s = string.Empty;
            result = StringUtils.GetMaxCharacters(s, 4);
            Assert.AreEqual(result.Length, 0,"result should be empty");
        }

        [TestMethod]
        public void GetLastCharactersTest()
        {
            string s = "1234567890";
            
            var res = s.GetLastCharacters(4);
            Assert.AreEqual(res, "7890");

            res = StringUtils.GetLastCharacters(null, 4);
            Assert.AreEqual(res, string.Empty);

            s = string.Empty;
            res = s.GetLastCharacters(4);
            Assert.AreEqual(res, string.Empty);

            s = "123";
            res = s.GetLastCharacters(4);
            Assert.AreEqual(res, "123");
        }

        [TestMethod]
        public void ToAndFromBase64StringTest() 
        { 
            var text = "This is a test string";

            var encoded = StringUtils.ToBase64String(text);
            Console.WriteLine(text);
            Console.WriteLine(encoded);
            var decoded = StringUtils.FromBase64String(encoded);

            Assert.AreEqual(decoded, text, "Text doesn't match");        
        }

    }
}
