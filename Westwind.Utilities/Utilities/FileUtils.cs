#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          � West Wind Technologies, 2009
 *          http://www.west-wind.com/
 * 
 * Created: 09/12/2009
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 **************************************************************  
*/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
//using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Westwind.Utilities
{
	/// <summary>
	/// wwUtils class which contains a set of common utility classes for 
	/// Formatting strings
	/// Reflection Helpers
	/// Object Serialization
    /// Stream Manipulation
	/// </summary>
	public static class FileUtils
	{
        #region Path Segments and Path Names


        /// <summary>
        /// This function returns the actual filename of a file
        /// that exists on disk. If you provide a path/file name
        /// that is not proper cased as input, this function fixes
        /// it up and returns the file using the path and file names
        /// as they exist on disk.
        /// 
        /// If the file doesn't exist the original filename is 
        /// returned.
        /// </summary>
        /// <param name="filename">A filename to check</param>
        /// <returns>On disk file name and path with the disk casing</returns>
        public static string GetPhysicalPath(string filename)
	    {
	        try
	        {
	            return new FileInfo(filename).FullName;                
	        }
            catch { }

            return filename;
        }


        /// <summary>
        /// Returns a relative path string from a full path based on a base path
        /// provided.
        /// </summary>
        /// <param name="fullPath">The path to convert. Can be either a file or a directory</param>
        /// <param name="basePath">The base path on which relative processing is based. Should be a directory.</param>
        /// <returns>
        /// String of the relative path.
        /// 
        /// Examples of returned values:
        ///  test.txt, ..\test.txt, ..\..\..\test.txt, ., .., subdir\test.txt
        /// </returns>
        public static string GetRelativePath(string fullPath, string basePath ) 
		{
            try
            {
                var pathChar = Path.DirectorySeparatorChar.ToString();

                // ForceBasePath to a path
                if (!basePath.EndsWith(value: pathChar))
                    basePath += pathChar;

                Uri baseUri = new Uri(uriString: basePath);
                Uri fullUri = new Uri(uriString: fullPath);

                Uri relativeUri = baseUri.MakeRelativeUri(uri: fullUri);
                

                // Uri's use forward slashes so convert back to backward slahes
                var path = relativeUri.ToString().Replace(oldValue: "/", newValue: pathChar);

                return Uri.UnescapeDataString(path);
            }
            catch
            {
                return fullPath;
            }
        }


        /// <summary>
        /// Returns a short form Windows path (using ~8 char segment lengths)
        /// that can help with long filenames.
        /// </summary>
        /// <remarks>
        /// IMPORTANT: File has to exist when this function is called otherwise
        /// `null` is returned.
        ///
        /// Path has to be fully qualified (no relative paths)
        /// 
        /// Max shortened file size is MAX_PATH (260) characters
        /// </remarks>
        /// <param name="path">Long Path syntax</param>
        /// <returns>Shortened 8.3 syntax or null on failure</returns>
        public static string GetShortPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            // allow for extended path syntax
            bool addExtended = false;
            if (path.Length > 240 && !path.StartsWith(@"\\?\"))
            {
                path = @"\\?\" + path;
                addExtended = true;
            }

            var shortPath = new StringBuilder(1024);
            int res = GetShortPathName(path, shortPath, 1024);
            if (res < 1)
                return null;

            path = shortPath.ToString();

            if (addExtended)
                path = path.Substring(4);  // strip off \\?\

            return path;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int GetShortPathName(
            [MarshalAs(UnmanagedType.LPTStr)]
            string path,
            [MarshalAs(UnmanagedType.LPTStr)]
            StringBuilder shortPath,
            int shortPathLength
        );


        //// To ensure that paths are not limited to MAX_PATH, use this signature within .NET
        //[DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetShortPathNameW", SetLastError = true)]
        //static extern int GetShortPathName_Internal(string pathName, StringBuilder shortName, int cbShortName);

        ///// <summary>
        ///// Returns a Windows short path (8 char path segments)
        ///// for a long path.
        ///// </summary>
        ///// <param name="fullPath"></param>
        ///// <remarks>Throws on non-existant files</remarks>
        ///// <returns></returns>
        //public static string GetShortPath(string fullPath)
        //{
        //    if (string.IsNullOrEmpty(fullPath))
        //        return fullPath;

        //    StringBuilder sb = new StringBuilder();
        //    GetShortPathName_Internal(fullPath, sb, 2048);
        //    return sb.ToString();
        //}


        /// <summary>
        /// Expands Path Environment Variables like %appdata% in paths.
        /// </summary>
        /// <param name="path">Path with potential environment variables.</param>
        /// <returns></returns>
        public static string ExpandPathEnvironmentVariables(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            if (path.StartsWith("~"))
            {
                var userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                path = path.Replace("~", userPath);
            }

            return Environment.ExpandEnvironmentVariables(path);
        }

        /// <summary>
        /// Returns a compact path with elipsis from a long path
        /// </summary>
        /// <param name="path">Original path to potentially trim</param>
        /// <param name="length">Max length of the path string returned</param>
        /// <returns></returns>
        public static string GetCompactPath(string path, int length = 70)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            if (path.Length <= length)
                return path;

            var index = -1;
            for (int i = path.Length-1; i >= 0; i--)
            {
                if (path[i] == '\\' || path[i] == '/')
                {
                    index = i;
                    break;
                }
            }

            if (index == -1) // no slashes
                return path.Substring(0,length); 

            var end = path.Substring(index);
            var start = path.Substring(0, index - 1);

            var maxStartLength = length - end.Length ;

            var startBlock = start.Substring(0, maxStartLength);

            if (start.Length > maxStartLength)
                startBlock = startBlock.Substring(0, maxStartLength - 3) + "...";

            return startBlock + end;
        }

#endregion

#region File and Path Normalization

        /// <summary>
        /// Normalizes a file path to the operating system default
        /// slashes.
        /// </summary>
        /// <param name="path"></param>
        public static string NormalizePath(string path)
	    {
            //return Path.GetFullPath(path); // this always turns into a full OS path

	        if (string.IsNullOrEmpty(path))
	            return path;

	        char slash = Path.DirectorySeparatorChar;
	        path = path.Replace('/', slash).Replace('\\', slash);
	        return path.Replace(slash.ToString() + slash.ToString(), slash.ToString());
        }


        /// <summary>
        /// Normalizes path with slashes and forces a trailing slash 
        /// on the end of the path.
        /// </summary>
        /// <param name="Path">Path to pass in</param>
        /// <returns></returns>
	    public static string NormalizeDirectory(string path)
        {
            path = NormalizePath(path);
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
                path += Path.DirectorySeparatorChar;
            return path;
        }


        /// <summary>
        /// Adds a trailing slash to a path if there isn't one.
        /// </summary>
        /// <param name="path">A file system path</param>
        /// <returns></returns>
	    public static string AddTrailingSlash(string path)
	    {
	        string separator = Path.DirectorySeparatorChar.ToString();	        

	        path = path.TrimEnd();

	        if (path.EndsWith(separator) || path.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
	            return path;

	        return path + separator;
	    }


        /// <summary>
        /// Returns a path as a `file:///` url.
        /// </summary>
        /// <param name="path">
        /// A fully rooted path
        /// or: a relative path that can be resolved to a
        /// fully rooted path via GetFullPath().
        /// </param>
        /// <returns></returns>
        public static string FilePathAsUrl(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            // try to resolve the path to a full path
            path = Path.GetFullPath(path);
            var url = new Uri(path);
            return url.ToString();
        }

        #endregion

        #region File Encoding and Checksums

        /// <summary>
        /// Detects the byte order mark of a file and returns
        /// an appropriate encoding for the file.
        /// </summary>
        /// <param name="srcFile"></param>
        /// <returns></returns>
        public static Encoding GetFileEncoding(string srcFile)
        {
            // Use Default of Encoding.Default (Ansi CodePage)
            Encoding enc = Encoding.Default;

            // Detect byte order mark if any - otherwise assume default

            byte[] buffer = new byte[5];
            FileStream file = new FileStream(srcFile, FileMode.Open);
            file.Read(buffer, 0, 5);
            file.Close();

            if (buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
                enc = Encoding.UTF8;
            else if (buffer[0] == 0xfe && buffer[1] == 0xff)
                enc = Encoding.Unicode;
            else if (buffer[0] == 0 && buffer[1] == 0 && buffer[2] == 0xfe && buffer[3] == 0xff)
                enc = Encoding.UTF32;

            else if (buffer[0] == 0x2b && buffer[1] == 0x2f && buffer[2] == 0x76)
                enc = Encoding.UTF7;

            return enc;
        }


        /// <summary>
	    /// Creates an MD5 checksum of a file
	    /// </summary>
	    /// <param name="file"></param>        
	    /// <param name="hashAlgorithm">SHA256, SHA512, SHA1, MD5</param>
	    /// <returns>BinHex file hash</returns>
	    public static string GetChecksumFromFile(string file, string hashAlgorithm = "MD5")
	    {           
	        if (!File.Exists(file))
	            return null;

	        try
	        {
	            byte[] checkSum;
	            using (FileStream stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read))
	            {
                    HashAlgorithm md = null;

                    if(hashAlgorithm == "MD5")
	                    md = new MD5CryptoServiceProvider();
                    else if (hashAlgorithm == "SHA256")
	                    md = new SHA256Managed();
                    else if(hashAlgorithm == "SHA512")
	                    md = new SHA512Managed();
	                else if (hashAlgorithm == "SHA1")
	                    md = new SHA1Managed();
                    else	                   
	                    md = new MD5CryptoServiceProvider();

                    checkSum = md.ComputeHash(stream);
	            }

	            return StringUtils.BinaryToBinHex(checkSum);
	        }
	        catch
	        {
	            return null;
	        }
	    }

#endregion

#region Searching 

        /// <summary>
        /// Searches for a file name based on a current file location up or down the
        /// directory hierarchy including the current folder. First file match is returned.
        /// </summary>
        /// <param name="currentPath">Current path or filename to determine start folder to search up from</param>
        /// <param name="searchFile">File name to search for. Should be a filename but can contain wildcards</param>
        /// <param name="direction">Search up or down the directory hierarchy including base path</param>
         /// <returns></returns>
        public static string FindFileInHierarchy( string currentPath, string searchFile,
                                                  FindFileInHierarchyDirection direction = FindFileInHierarchyDirection.Up)
        {
            string path = null;

            var fi = new FileInfo(currentPath);
            if (!fi.Exists)
            {
                var di = new DirectoryInfo(currentPath);
                if (!di.Exists)
                    return null;

                path = di.FullName;
            }
            else
            {
                path = fi.DirectoryName;
            }

            return FindFileInHierarchyInternal(path, searchFile, direction);
        }

        /// <summary>
        /// Recursive method to walk the hierarchy and find the file requested.
        /// </summary>
        /// <param name="path">Base path</param>
        /// <param name="searchFile">Filename to search for</param>
        /// <param name="direction">Search up or down the tree including base path</param>
        /// <returns></returns>
        private static string FindFileInHierarchyInternal(string path, string searchFile,
                                                          FindFileInHierarchyDirection direction = FindFileInHierarchyDirection.Up)
        {
            if (path == null)
                return null;

            var dir = new DirectoryInfo(path);

            var so = SearchOption.TopDirectoryOnly;

            if (direction == FindFileInHierarchyDirection.Down)
                so = SearchOption.AllDirectories;

            var files = dir.GetFiles(searchFile, so);
            if (files.Length > 0)
                return files[0].FullName;  // closest match

            if (direction == FindFileInHierarchyDirection.Down)
                return null;

            if (dir.Parent == null)
                return null;

            return FindFileInHierarchyInternal(dir.Parent.FullName, searchFile, FindFileInHierarchyDirection.Up);
        }

        /// <summary>
        /// Returns a list of file matches searching up and down a file hierarchy
        /// </summary>
        /// <param name="startPath">Path to start from</param>
        /// <param name="searchFile">Filename or Wildcard</param>
        /// <param name="direction">up or down the hiearchy</param>
        /// <returns></returns>
        public static string[] FindFilesInHierarchy(string startPath, string searchFile,
            FindFileInHierarchyDirection direction = FindFileInHierarchyDirection.Down)
        {
            var list= new string[]{ };

            var fi = new FileInfo(startPath);
            if (!fi.Exists)
            {
                var di = new DirectoryInfo(startPath);
                if (!di.Exists)
                    return list;

                startPath = di.FullName;
            }
            else
            {
                startPath = fi.DirectoryName;
            }

            return FindFilesInHierarchyInternal(startPath, searchFile, direction);
        }

        /// <summary>
        /// Recursive method to walk the hierarchy and find the file requested.
        /// </summary>
        /// <param name="path">Base path</param>
        /// <param name="searchFile">Filename to search for</param>
        /// <param name="direction">Search up or down the tree including base path</param>
        /// <returns></returns>
        private static string[] FindFilesInHierarchyInternal(string path, string searchFile,
            FindFileInHierarchyDirection direction = FindFileInHierarchyDirection.Up)
        {
            var list = new string[] { };

            if (path == null)
                return list;

            var dir = new DirectoryInfo(path);

            var so = SearchOption.TopDirectoryOnly;

            if (direction == FindFileInHierarchyDirection.Down)
                so = SearchOption.AllDirectories;

            var files = dir.GetFiles(searchFile, so);
            if (files.Length > 0)
                return files.Select(fi=> fi.FullName).ToArray();

            if (direction == FindFileInHierarchyDirection.Down)
                return list;

            if (dir.Parent == null)
                return list;

            return FindFilesInHierarchyInternal(dir.Parent.FullName, searchFile, FindFileInHierarchyDirection.Up);
        }

        public enum FindFileInHierarchyDirection
        {
            Up,
            Down
        }

#endregion

#region File and Path Naming

        
        /// <summary>
        /// Returns a safe filename from a string by stripping out
        /// illegal characters
        /// </summary>
        /// <param name="fileName">Filename to fix up</param>
        /// <param name="replacementString">String value to replace illegal chars with. Defaults empty string</param>
        /// <param name="spaceReplacement">Optional - replace spaces with a specified string like a - or _. Optional, if not set leaves spaces which are legal for filenames</param>
        /// <returns>Fixed up string</returns>
        public static string SafeFilename(string fileName, string replacementString = "", string spaceReplacement = null)
        {
            if (string.IsNullOrEmpty(fileName))
                return fileName;

            string file = Path.GetInvalidFileNameChars()
                .Aggregate(fileName.Trim(),
                    (current, c) => current.Replace(c.ToString(), replacementString));

            file = file.Replace("#", "")
                       .Replace("+", "");

            if (!string.IsNullOrEmpty(spaceReplacement))
                file = file.Replace(" ", spaceReplacement);

            return file.Trim();
        }

        /// <summary>
        /// Returns a safe filename in CamelCase
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string CamelCaseSafeFilename(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return filename;

            string fname = Path.GetFileNameWithoutExtension(filename);
            string ext = Path.GetExtension(filename);

            return StringUtils.ToCamelCase(SafeFilename(fname)) + ext;
        }


        /// <summary>
        /// Checks to see if a file has invalid path characters. Use this
        /// to check before using or manipulating paths with `Path` operations
        /// that will fail if files or paths contain invalid characters.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="additionalChars"></param>
        /// <returns></returns>
        public static bool HasInvalidPathCharacters(string path, params char[] additionalChars)
        {
            if(string.IsNullOrEmpty(path)) return true; // no invalids

            var invalids = Path.GetInvalidPathChars();
            if (additionalChars != null)
                invalids.Concat(additionalChars);
	
            return (!string.IsNullOrEmpty(path) && path.IndexOfAny(invalids) >= 0);
        }
#endregion

#region StreamFunctions
        /// <summary>
        /// Copies the content of the one stream to another.
        /// Streams must be open and stay open.
        /// </summary>
        public static void CopyStream(Stream source, Stream dest, int bufferSize)
        {
            byte[] buffer = new byte[bufferSize];
            int read;
            while ( (read = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                dest.Write(buffer, 0, read);
            }
        }

        /// <summary>
        /// Copies the content of one stream to another by appending to the target stream
        /// Streams must be open when passed in.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <param name="bufferSize"></param>
        /// <param name="append"></param>
        public static void CopyStream(Stream source, Stream dest, int bufferSize, bool append)
        {
            if (append)
                dest.Seek(0, SeekOrigin.End);

            CopyStream(source, dest, bufferSize);
            return;
        }

        /// <summary>
        /// Opens a stream reader with the appropriate text encoding applied.
        /// </summary>
        /// <param name="srcFile"></param>
        public static StreamReader OpenStreamReaderWithEncoding(string srcFile)
        {
            Encoding enc = GetFileEncoding(srcFile);
            return new StreamReader(srcFile, enc);
        }

        #endregion

        #region Async File Access for NetFX
        
        /// <summary>
        /// Asynchronously reads files. Use only with NetFx
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static async Task<string> ReadAllTextAsync(string filename, Encoding encoding)
        {
            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
            using (var reader = new StreamReader(stream, encoding))
            {
                return await reader.ReadToEndAsync();
            }
        }

        /// <summary>
        /// Writes out text file content asynchronously.Use only with NetFx.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static async Task WriteAllTextAsync(string filename, string text, Encoding encoding)
        {
            using (var stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                var bytes = encoding.GetBytes(text ?? string.Empty);
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }
        }

        #endregion

        #region Folder Copying and Deleting

        /// <summary>
        /// Copies directories using either top level only or deep merge copy.
        /// 
        /// Copies a directory by copying files from source folder to target folder.
        /// If folder(s) don't exist they are created.
        /// </summary>
        /// <param name="sourceDirectory">Source folder</param>
        /// <param name="targetDirectory">Target folder </param>
        /// <param name="deleteFirst">if set deletes the folder before copying</param>
        /// <param name="recursive">if set copies files recursively</param>
        public static void CopyDirectory(string sourceDirectory, string targetDirectory, bool deleteFirst = false, bool recursive = true )
        {
            var diSource = new DirectoryInfo(sourceDirectory);
            if (!diSource.Exists)
                return;

            var diTarget = new DirectoryInfo(targetDirectory);
            CopyDirectory(diSource, diTarget, deleteFirst, recursive);
        }

        /// <summary>
        /// Copies directories using either top level only or deep merge copy.
        /// 
        /// Copies a directory by copying files from source folder to target folder.
        /// If folder(s) don't exist they are created.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="deleteFirst"></param>
        /// <param name="recursive"></param>
        public static void CopyDirectory(DirectoryInfo source, DirectoryInfo target, bool deleteFirst = false, bool recursive = true )
        {
            if (!source.Exists)
                return;

            if (deleteFirst && target.Exists)
                target.Delete(true);
            
            Directory.CreateDirectory(target.FullName);  // create if it doesn't exist
 
            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            if (recursive)
            {
                // Copy each subdirectory using recursion.
                foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
                {
                    DirectoryInfo nextTargetSubDir =
                        target.CreateSubdirectory(diSourceSubDir.Name);
                    CopyDirectory(diSourceSubDir, nextTargetSubDir);
                }
            }
        }
        /// <summary>
        /// Deletes files in a folder based on a file spec recursively
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="filespec"></param>
        /// <param name="recursive"></param>
        /// <returns>0 when no errors, otherwise number of files that have failed to delete (usually locked)</returns>
        public static int DeleteFiles(string path, string filespec, bool recursive)
        {
            int failed = 0;
            path = Path.GetFullPath(path);
            string spec = Path.GetFileName(filespec);
            string[] files = Directory.GetFiles(path, spec);

            foreach (string file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    failed++;
                } // ignore locked files
            }

            if (recursive)
            {
                var dirs = Directory.GetDirectories(path);
                foreach (string dir in dirs)
                {
                    failed = failed + DeleteFiles(dir, filespec, recursive);
                }
            }


            return failed;
        }

        /// <summary>
        /// Deletes files based on a file spec and a given timeout.
        /// This routine is useful for cleaning up temp files in 
        /// Web applications.
        /// </summary>
        /// <param name="filespec">A filespec that includes path and/or wildcards to select files</param>
        /// <param name="seconds">The timeout - if files are older than this timeout they are deleted</param>
        public static void DeleteTimedoutFiles(string filespec, int seconds)
        {
            string path = Path.GetDirectoryName(filespec);
            string spec = Path.GetFileName(filespec);
            string[] files = Directory.GetFiles(path, spec);

            foreach (string file in files)
            {
                try
                {
                    if (File.GetLastWriteTimeUtc(file) < DateTime.UtcNow.AddSeconds(seconds * -1))
                        File.Delete(file);
                }
                catch { }  // ignore locked files
            }
        }

#endregion
    }

}