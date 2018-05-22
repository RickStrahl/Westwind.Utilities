using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace Westwind.Utilities.Test
{
    [TestClass]
    public class FileUtilsTest
    {
        [TestMethod]
        public void SafeFilenameTest()
        {
            string file = "This: iS this file really invalid?";

            string result = FileUtils.SafeFilename(file);

            Assert.AreEqual(result, "This iS this file really invalid");

            Console.WriteLine(result);
        }

        [TestMethod]
        public void NormalizePathTest()
        {            
            var path = @"c:\temp\test/work/play.txt";
            string normal = FileUtils.NormalizePath(path);
            Console.WriteLine(normal);
            Assert.IsTrue(normal.ToArray().Count(c => c == Path.DirectorySeparatorChar) == 4);

            path = @"\temp\test/work/play.txt";
            normal = FileUtils.NormalizePath(path);
            Console.WriteLine(normal);
            Assert.IsTrue(normal.ToArray().Count(c => c == Path.DirectorySeparatorChar) == 4);

            path = @"temp\test/work/play.txt";
            normal = FileUtils.NormalizePath(path);
            Console.WriteLine(normal);
            Assert.IsTrue(normal.ToArray().Count(c => c == Path.DirectorySeparatorChar) == 3);
        }


        [TestMethod]
        public void NormalizeDirectoryTest()
        {
            var path = @"c:\temp\test\work";
            string normal = FileUtils.NormalizeDirectory(path);
            Console.WriteLine(normal);
            Assert.IsTrue(normal.ToArray().Count(c => c == Path.DirectorySeparatorChar) == 4);

            path = @"c:\temp\test\work\"; 
            normal = FileUtils.NormalizeDirectory(path);
            Console.WriteLine(normal);
            Assert.IsTrue(normal.ToArray().Count(c => c == Path.DirectorySeparatorChar) == 4);

            path = @"\temp\test\work\";
            normal = FileUtils.NormalizeDirectory(path);
            Console.WriteLine(normal);
            Assert.IsTrue(normal.ToArray().Count(c => c == Path.DirectorySeparatorChar) == 4);

            path = @"\temp\test/work/";
            normal = FileUtils.NormalizeDirectory(path);
            Console.WriteLine(normal);
            Assert.IsTrue(normal.ToArray().Count(c => c == Path.DirectorySeparatorChar) == 4);


            path = @"\temp\test/work/play.txt";
            normal = FileUtils.NormalizeDirectory(path);
            Console.WriteLine(normal);
            Assert.IsTrue(normal.ToArray().Count(c => c == Path.DirectorySeparatorChar) == 5);

            path = @"temp\test/work/bogus";
            normal = FileUtils.NormalizeDirectory(path);
            Console.WriteLine(normal);
            Assert.IsTrue(normal.ToArray().Count(c => c == Path.DirectorySeparatorChar) == 4);
        }

        [TestMethod]
        public void CompactPathTest()
        {
            var path = @"c:\temp\test\node_modules\SomeVeryLongComponentNameSpaceAndName\SomeLongComponentName.md";
            Console.WriteLine("Orig: "  + path);
            var result = FileUtils.GetCompactPath(path);
            Console.WriteLine($"{result.Length} {result}");
            Assert.IsTrue(result.Length == 70);


            path = @"c:\temp\SomeLongComponentName.md";
            Console.WriteLine("Orig: " + path);
            result = FileUtils.GetCompactPath(path);
            Console.WriteLine($"{result.Length} {result}");
            Assert.IsTrue(result.Length < 70);

            path = @"\\temp\test\node_modules\SomeVeryLongComponentNameSpaceAndName\SomeLongComponentName.md";
            Console.WriteLine("Orig: " + path);
            result = FileUtils.GetCompactPath(path);
            Console.WriteLine($"{result.Length} {result}");
            Assert.IsTrue(result.Length == 70);

        }



        [TestMethod]
        public void CopyDirectory()
        {
            string target = Path.Combine(Path.GetTempPath(), "_TestFolders2");
            string source = target.TrimEnd('2');

            try
            {
                Directory.Delete(target, true);
            }
            catch { }
            try
            {
                Directory.Delete(source, true);
            }
            catch { }
            
            Directory.CreateDirectory(source);
            Directory.CreateDirectory(Path.Combine(source,"SubFolder1"));
            Directory.CreateDirectory(Path.Combine(source, "SubFolder2"));
            File.WriteAllText(Path.Combine(source,"test.txt"),"Hello cruel world");
            File.WriteAllText(Path.Combine(source, "SubFolder1","test.txt"), "Hello cruel world");
            File.WriteAllText(Path.Combine(source, "SubFolder2", "test.txt"), "Hello cruel world");

            FileUtils.CopyDirectory(source, target);

            Assert.IsTrue(Directory.Exists(target));
            Assert.IsTrue(File.Exists(Path.Combine(source, "test.txt")));
            Assert.IsTrue(File.Exists(Path.Combine(source, "SubFolder1", "test.txt")));
            Assert.IsTrue(File.Exists(Path.Combine(source, "SubFolder2", "test.txt")));

            Directory.Delete(target, true);
            Directory.Delete(source, true);
        }

        [TestMethod]
        public void ExpandPathEnvironmentVariablesTest()
        {
            string path = "%appdata%\\Markdown Monster";
            string result = FileUtils.ExpandPathEnvironmentVariables(path);

            Console.WriteLine(result);
            Assert.IsFalse(result.Contains("%appdata%"));
            Assert.IsTrue(result.ToLower().Contains("appdata"));
        }
    }
}
