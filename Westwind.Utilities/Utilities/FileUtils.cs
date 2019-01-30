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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
//using System.Runtime.InteropServices;
using System.Text;

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
            // ForceBasePath to a path
            if (!basePath.EndsWith(value: "\\"))
                basePath += "\\";
            
#pragma warning disable CS0618
            Uri baseUri = new Uri(uriString: basePath,dontEscape: true);
            Uri fullUri = new Uri(uriString: fullPath,dontEscape: true);
#pragma warning restore CS0618

            Uri relativeUri = baseUri.MakeRelativeUri(uri: fullUri);

            // Uri's use forward slashes so convert back to backward slahes
            return relativeUri.ToString().Replace(oldValue: "/", newValue: "\\");            
		}



        /// <summary>
        /// Expands Path Environment Variables like %appdata% in paths.
        /// </summary>
        /// <param name="path">Path with potential environment variables.</param>
        /// <returns></returns>
        public static string ExpandPathEnvironmentVariables(string path)
        {   
            while (path.Contains("%"))
            {
                var extract = StringUtils.ExtractString(path, "%", "%");
                if (string.IsNullOrEmpty(extract))
                    return path;

                var env = Environment.GetEnvironmentVariable(extract);
                if (!string.IsNullOrEmpty(env))
                    path = path.Replace("%" + extract + "%", env);
                else
                    return path;
            }

            return path;
        }

        #endregion

        #region File and Path Normalization

        /// <summary>
        /// Returns a safe filename from a string by stripping out
        /// illegal characters
        /// </summary>
        /// <param name="fileName">Filename to fix up</param>
        /// <param name="replacementString">String value to replace illegal chars with. Defaults empty string</param>
        /// <param name="spaceReplacement">Optional - replace spaces with a specified string.</param>
        /// <returns>Fixed up string</returns>
        public static string SafeFilename(string fileName, string replacementString = "", string spaceReplacement = null)
	    {
	        if (string.IsNullOrEmpty(fileName))
	            return fileName;

	        string file = Path.GetInvalidFileNameChars()
	            .Aggregate(fileName.Trim(),
	                (current, c) => current.Replace(c.ToString(), replacementString));

	        file = file.Replace("#", "");

	        if (!string.IsNullOrEmpty(spaceReplacement))
	            file = file.Replace(" ", spaceReplacement);

	        return file;
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
	    #endregion

        #region Miscellaneous functions

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
        /// Copies directories using either top level only or deep merge copy.
        /// 
        /// Copies a directory by copying files from source folder to target folder.
        /// If folder(s) don't exist they are created.
        /// 
        /// deepCopy copies files in sub-folders and merges them into the target
        /// folder. Unless you specify deleteFirst, files are copied and overwrite or add to
        /// existing structure, leaving old files in place. Use deleteFirst if you
        /// want a new structure with only the source files.
        /// </summary>
        /// <param name="sourcePath">Path to copy from</param>
        /// <param name="targetPath">Path to copy to</param>
        /// <param name="deleteFirst">If true deletes target folder before copying. Otherwise files are merged from source into target.</param>
        public static void CopyDirectory(string sourcePath, string targetPath, bool deleteFirst = false, bool deepCopy = true)
        {
            if (deleteFirst && Directory.Exists(targetPath))
                Directory.Delete(targetPath, true);

            var searchOption = SearchOption.TopDirectoryOnly;
            if (deepCopy)
                searchOption = SearchOption.AllDirectories;

            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", searchOption))
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));

            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", searchOption))
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
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
    }

}