#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2009
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
        /// <param name="basePath">The base path on which relative processing is based. Should be a directory!</param>
        /// <returns>
        /// String of the relative path. Path is returned in OS specific path format.
        /// 
        /// Examples of returned values:
        ///  test.txt, ..\test.txt, ..\..\..\test.txt, ., .., subdir\test.txt
        /// </returns>
        public static string GetRelativePath(string fullPath, string basePath ) 
		{
            try
            {
                // ForceBasePath to a path
                var pathChar = Path.DirectorySeparatorChar.ToString();
                if (!basePath.EndsWith(value: pathChar))
                    basePath += pathChar;

                Uri baseUri = new Uri(uriString: basePath);
                Uri fullUri = new Uri(uriString: fullPath);

                string relativeUri = baseUri.MakeRelativeUri(uri: fullUri).ToString();

                if (relativeUri.StartsWith("file://"))
                    return fullPath;  // invalid path that can't be made relative 

                // Uri's use forward slashes - convert back to backward slases if OS uses                
                var path = relativeUri.Replace(oldValue: "/", newValue: pathChar);

                return Uri.UnescapeDataString(path);
            }
            catch
            {
                return fullPath;
            }
        }

        /// <summary>
        /// Compares two files and returns a relative path to the second file.     
        /// </summary>
        /// <param name="filePath">The path that is the current path you're working with</param>
        /// <param name="compareToPath">The path that that you want to reference</param>
        /// <param name="noOsPathFixup">If true won't fix up the path for current OS (ie. returns original path delimiters via Uri comparison)</param>
        /// <returns>Relative path if possible otherwise original path</returns>
        public static string GetRelativeFilePath(string filePath, string compareToPath, bool noOsPathFixup = false)
        {
            Uri fromUri = new Uri("file://" + filePath);
            Uri toUri = new Uri("file://" + compareToPath);

            string path = fromUri.MakeRelativeUri(toUri).ToString();

            if (path.StartsWith("file://"))
                return filePath;  // invalid path that can't be made relative 

            if (!noOsPathFixup) { 
                var pathChar = Path.DirectorySeparatorChar.ToString();
                path = path.Replace(oldValue: "/", newValue: pathChar);
            }
            return path;
        }


        /// <summary>
        /// Resolves an absolute path from a relative file path to a base file or directory
        /// </summary>
        /// <param name="basePath">Base file or folder which relativeFile is relative to.
        /// If you pass a folder, terminate the folder with a path character!</param>
        /// <param name="relativeFile">The path to resolve against the base path</param>
        /// <returns></returns>
        public static string ResolvePath(string basePath, string relativeFile)
        {
            if (string.IsNullOrEmpty(basePath) ||
                string.IsNullOrEmpty(relativeFile))
                return relativeFile;

            var baseUri = new Uri(basePath);
            var relativeUri = new Uri(relativeFile, UriKind.Relative);
            var relUri = new Uri(baseUri, relativeUri);

            return relUri.LocalPath;
        }
        
        /// <summary>
        /// Checks to see if a given local file or directory is a relative path
        /// </summary>
        /// <param name="path">path to check</param>
        /// <returns></returns>
        public static bool IsRelativePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            if (path.StartsWith("."))
                return true;

            if (path.StartsWith("/") || path.StartsWith("\\") || 
                path.Contains(":\\") || path.StartsWith("file:"))
                return false;

            return true;            
        }

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
                path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + path.TrimStart('~');                
            }
            return Environment.ExpandEnvironmentVariables(path);
        }

        /// <summary>
        /// Changes any path that starts with the Windows user path into a ~ path instead
        /// </summary>
        /// <param name="path">Any windows path</param>
        /// <returns>~ replaces c:\users\someuser in path string, otherwise original path is returned</returns>
        public static string TildefyUserPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            string userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            if (path.StartsWith(userPath, StringComparison.InvariantCultureIgnoreCase))
#if NET6_0_OR_GREATER
                return path.Replace(userPath, "~", true, null);
#else
                return StringUtils.ReplaceString(path, userPath, "~", true);
#endif

            return path;
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


        /// <summary>
        /// Creates a temporary file name with a specific extension. Optionall
        /// provide the base path to create it in otherwise the TEMP path is used.
        ///  Filename is generated as _ + 8 random characters/digits
        /// </summary>
        /// <param name="extension">The extension to use (.png)</param>
        /// <param name="tempPath">Optional - path in which the file is created if it needs to override</param>
        /// <param name="charCount">Optional - character count - minus the prefix _ - of the generated temp filename. Between 8-16</param>
        /// <returns></returns>
        public static string GetTempFilenameWithExtension(string extension, string tempPath = null, int charCount = 8)
        {
            if (string.IsNullOrEmpty(extension))
            {
                extension = ".tmp";
            }
            if (!extension.StartsWith("."))
                extension = "." + extension;

            if (tempPath == null)
            {
                tempPath = Path.GetTempPath();
            }

            string filename = DataUtils.GenerateUniqueId(charCount);

            return Path.Combine(tempPath, filename + extension);
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
            string doubleSlash = string.Concat(slash, slash);
            if (path.StartsWith(doubleSlash))
                return string.Concat(doubleSlash, path.TrimStart(slash).Replace(doubleSlash, slash.ToString()));
            else
                return path.Replace(doubleSlash, slash.ToString());
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
        /// 
        /// Uses the Operating System default path character (`/` or `\`)
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
        /// Adds a trailing slash to a path if there isn't one.
        /// 
        /// Allows you to explicitly specify the path separator character
        /// rather than using the default OS path separator.
        /// </summary>
        /// <param name="path">A file system path</param>
        /// <param name="slashchar">Character to use as trailing character</param>
        /// <returns></returns>
        public static string AddTrailingSlash(string path, char slashChar)
	    {
            var separator = slashChar.ToString();
	        path = path.TrimEnd();

	        if (path.EndsWith(separator))
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
            _ = file.Read(buffer, 0, 5);
            file.Close();

            if (buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
                enc = Encoding.UTF8;
            else if (buffer[0] == 0xfe && buffer[1] == 0xff)
                enc = Encoding.Unicode;
            else if (buffer[0] == 0 && buffer[1] == 0 && buffer[2] == 0xfe && buffer[3] == 0xff)
                enc = Encoding.UTF32;

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

                    if (hashAlgorithm == "MD5")
                        md = MD5.Create();
                    else if (hashAlgorithm == "SHA256")
                        md = SHA256.Create();
                    else if (hashAlgorithm == "SHA512")
                        md = SHA512.Create();
                    else if (hashAlgorithm == "SHA1")
                        md = SHA1.Create();
                    else
                        md = MD5.Create();

                    using (md)
                    {
                        checkSum = md.ComputeHash(stream);
                    }
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

            FileInfo[] files;
            try
            {
                files = dir.GetFiles(searchFile, so);
            }
            catch
            {                
                return null;  // permissions error most likely
            }

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
        /// <param name="path">Path to check</param>
        /// <param name="additionalChars">Optionally allows you to add additional invalid characters to the disallowed OS characters</param>
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
        /// Async Read all bytes. Use only with NetFx
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<byte[]> ReadAllBytesAsync(string filename)
        {
            if (filename == null)
                throw new ArgumentNullException(nameof(filename));

            using (var sourceStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
            {
                var buffer = new byte[sourceStream.Length];
                int bytesRead = 0;
                while (bytesRead < buffer.Length)
                {
                    int read = await sourceStream.ReadAsync(buffer, bytesRead, buffer.Length - bytesRead);
                    if (read == 0)
                        break;
                    bytesRead += read;
                }
                return buffer;
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
            if (filename == null)
                throw new ArgumentNullException(nameof(filename));

            using (var stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                var bytes = encoding.GetBytes(text ?? string.Empty);
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }
        }


        /// <summary>
        /// Async Write all bytes. Use only with NetFx
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task WriteAllBytesAsync(string filename, byte[] bytes)
        {
            if (filename == null)
                throw new ArgumentNullException(nameof(filename));
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            using (var destinationStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                await destinationStream.WriteAsync(bytes, 0, bytes.Length);
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
        public static void CopyDirectory(string sourceDirectory, string targetDirectory, bool deleteFirst = false, bool recursive = true, bool ignoreErrors = false )
        {
            var diSource = new DirectoryInfo(sourceDirectory);
            if (!diSource.Exists)
                return;

            var diTarget = new DirectoryInfo(targetDirectory);
            CopyDirectory(diSource, diTarget, deleteFirst, recursive, ignoreErrors);
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
        public static void CopyDirectory(DirectoryInfo source, DirectoryInfo target, bool deleteFirst = false, bool recursive = true, bool ignoreErrors =false )
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
                if (ignoreErrors)
                {
                    try
                    {
                        fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
                    }
                    catch { }
                }
                else
                    fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            if (recursive)
            {
                // Copy each subdirectory using recursion.
                foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
                {
                    DirectoryInfo nextTargetSubDir =
                        target.CreateSubdirectory(diSourceSubDir.Name);
                    CopyDirectory(diSourceSubDir, nextTargetSubDir, recursive: recursive, ignoreErrors: ignoreErrors);
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
        public static int DeleteFiles(string path, string filespec, bool recursive = false)
        {
            if (!Directory.Exists(path))
                return 0;

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
                    failed =+ DeleteFiles(dir, filespec, recursive);
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