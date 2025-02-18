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
            Console.WriteLine("Orig: " + path);
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
        public void TildefyPathTest()
        {

            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Projects\\Markdown Monster");
            var result = FileUtils.TildefyUserPath(path);

            Assert.IsTrue(result.StartsWith("~"));
            Assert.IsTrue(result.Contains("Projects\\Markdown Monster"));
            Console.WriteLine(result);

            path = null;
            result = FileUtils.TildefyUserPath(path);
            Assert.IsTrue(result == null);

            path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            result = FileUtils.TildefyUserPath(path);
            Assert.IsTrue(result == "~");


            path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\";
            result = FileUtils.TildefyUserPath(path);
            Assert.IsTrue(result == "~\\");
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
            Directory.CreateDirectory(Path.Combine(source, "SubFolder1"));
            Directory.CreateDirectory(Path.Combine(source, "SubFolder2"));
            File.WriteAllText(Path.Combine(source, "test.txt"), "Hello cruel world");
            File.WriteAllText(Path.Combine(source, "SubFolder1", "test.txt"), "Hello cruel world");
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

        [TestMethod]
        public void ExpandPathEnvironmentVariablesUserFolderTest()
        {
            string path = "~\\Projects";
            string result = FileUtils.ExpandPathEnvironmentVariables(path);

            Console.WriteLine(result);
            Assert.AreNotEqual(path, result);
            Assert.IsFalse(result.Contains("~\\"));

        }

        [TestMethod]
        public void DeleteFilesTest()
        {
            string source = Path.Combine(Path.GetTempPath(), "_TestFolders");
            try
            {
                Directory.Delete(source, true);
            }
            catch { }

            Directory.CreateDirectory(source);
            Directory.CreateDirectory(Path.Combine(source, "SubFolder1"));
            Directory.CreateDirectory(Path.Combine(source, "SubFolder2"));
            File.WriteAllText(Path.Combine(source, "test.txt"), "Hello cruel world");
            File.WriteAllText(Path.Combine(source, "SubFolder1", "test.txt"), "Hello cruel world");
            File.WriteAllText(Path.Combine(source, "SubFolder2", "test.txt"), "Hello cruel world");

            Console.WriteLine(source);
            int fails = FileUtils.DeleteFiles(source, "test.txt", recursive: true);

            Directory.Delete(source, true);

            Assert.IsTrue(fails == 0, "Failed to delete all files");
        }

        //[TestMethod]
        //public void ShortPathTest()
        //{
        //    var path = FileUtils.ExpandPathEnvironmentVariables(@"%appdata%\Markdown Monster\MarkdownMonster.json");
        //    var shortPath = FileUtils.GetShortPath(path);

        //    Assert.IsFalse(path == shortPath);
        //    Console.WriteLine(shortPath);

        //    //path = path.Replace(".json", ".json-bogus");
        //    //shortPath = FileUtils.GetShortPath(path);
        //    //Assert.IsFalse(path == shortPath);
        //    //Console.WriteLine(shortPath);

        //}

        [TestMethod]
        public void FindFileHierarchicalUp()
        {

            var basePath = Path.GetFullPath("./SupportFiles/SquareImage.jpg");
            var matchedFile = FileUtils.FindFileInHierarchy(basePath, "Westwind.Utilities.dll", FileUtils.FindFileInHierarchyDirection.Up);

            Assert.IsNotNull(matchedFile);
            Console.WriteLine(matchedFile);
        }


        [TestMethod]
        public void FindFileHierarchicalDown()
        {
            var basePath = Path.GetFullPath("./SupportFiles");
            var matchedFile = FileUtils.FindFileInHierarchy(basePath, "SquareImage.jpg", FileUtils.FindFileInHierarchyDirection.Down);

            Assert.IsNotNull(matchedFile);
            Console.WriteLine(matchedFile);
        }

        [TestMethod]
        public void FindFilesHierarchal()
        {
            var basePath = Path.GetFullPath("./SupportFiles");
            var matches = FileUtils.FindFilesInHierarchy(basePath, "*.jpg", FileUtils.FindFileInHierarchyDirection.Down);

            Assert.IsNotNull(matches);
            Assert.IsTrue(matches.Length > 0);

            foreach (var file in matches)
            {
                Console.WriteLine(file);
            }

        }


        [TestMethod]
        public void GetRelativePathTest()
        {
            string basePath = @"C:\Users\johndoe\Documents";
            string fullPath = @"c:\Users\Johndoe\Documents\Visual Studio #$%*!(# 2022\Code Snippets\Visual C#\My Code Snippets\clw.snippet";

            var relative = FileUtils.GetRelativePath(fullPath, basePath);

            Assert.AreNotEqual(relative, fullPath);
            Assert.IsFalse(relative.StartsWith("\\"));

            Console.WriteLine(relative);
        }


        /// <summary>
        /// Checks to see if a path is a relative path based on 
        /// how it is formatted. 
        /// Note this method DOES NOT check against any other path
        /// it merely checks to see if it's a path that is relative
        /// based on how the path starts.
        /// </summary>
        [TestMethod]
        public void IsRelativePathTest()
        {
            string path = "test.txt";
            Assert.IsTrue(FileUtils.IsRelativePath(path), "Single path should be relative");

            path = "..\\test.txt";
            Assert.IsTrue(FileUtils.IsRelativePath(path), "..\\ path should be relative");

            path = "\\test.txt";
            Assert.IsFalse(FileUtils.IsRelativePath(path), "\\ path should not be relative");


            path = "d:\\temp\\test.txt";
            Assert.IsFalse(FileUtils.IsRelativePath(path), "Absolute path should not be relative");

            path = "\\\\RASMSI\\temp\\test.txt";
            Assert.IsFalse(FileUtils.IsRelativePath(path), "Network path should not be relative");


            path = "file:///d:/temp/test.txt";
            Assert.IsFalse(FileUtils.IsRelativePath(path), "file:// path  should not be relative");

        }

        [TestMethod]
        public void ResolvePathTest()
        {
            var basePath = @"c:\temp\subfolder\test.txt";

            string path = "..\\test.txt";
            var result = FileUtils.ResolvePath(basePath, path);
            Assert.AreEqual(result, @"c:\temp\test.txt");

            path = "test.txt";
            result = FileUtils.ResolvePath(basePath, path);
            Assert.AreEqual(result, @"c:\temp\subfolder\test.txt");

            path = ".\\test.txt";
            result = FileUtils.ResolvePath(basePath, path);
            Assert.AreEqual(result, @"c:\temp\subfolder\test.txt");


            basePath = @"c:\temp\subfolder\";  // folder - !IMPORTANT add backslash to folders!
            path = "..\\test.txt";
            result = FileUtils.ResolvePath(basePath, path);
            Assert.AreEqual(result, @"c:\temp\test.txt");


            path = "\\test.txt";
            result = FileUtils.ResolvePath(basePath, path);
            Console.WriteLine(result);
            Assert.AreEqual(result, @"c:\test.txt");

        }


    }
}
