using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

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

    }
}
