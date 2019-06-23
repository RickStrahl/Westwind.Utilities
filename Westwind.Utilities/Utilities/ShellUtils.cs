#region 
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2011
 *          http://www.west-wind.com/
 * 
 * Created: 6/19/2011
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
using System.Text;
using System.IO;
using System.Net;
using System.Diagnostics;

namespace Westwind.Utilities
{

    /// <summary>
    /// Windows specific shell utility functions 
    /// </summary>
    public static class ShellUtils
    {

        #region Open in or Start Process
        /// <summary>
        /// Opens a File or Folder in Explorer. If the path is a file
        /// Explorer is opened in the parent folder with the file selected
        /// </summary>
        /// <param name="filename"></param>
        public static void OpenFileInExplorer(string filename)
        {
            if (Directory.Exists(filename))
                ShellUtils.GoUrl(filename);
            else
            {
                if (!File.Exists(filename))
                    filename = Path.GetDirectoryName(filename);
    
                Process.Start("explorer.exe", $"/select,\"{filename}\"");
            }
        }

        /// <summary>
        /// Executes a Windows process with given command line parameters
        /// </summary>
        /// <param name="executable">Executable to run</param>
        /// <param name="arguments"></param>
        /// <param name="timeoutMs">Timeout of the process in milliseconds. Pass -1 to wait forever. Pass 0 to not wait.</param>
        /// <param name="windowStyle"></param>
        /// <returns></returns>
        public static int ExecuteProcess(string executable, string arguments = null, int timeoutMs = 0, ProcessWindowStyle windowStyle = ProcessWindowStyle.Hidden)
        {
            Process process;

            try
            {
                using (process = new Process())
                {
                    process.StartInfo.FileName = executable;
                    process.StartInfo.Arguments = arguments;
                    process.StartInfo.WindowStyle = windowStyle;
                    if (windowStyle == ProcessWindowStyle.Hidden)
                        process.StartInfo.CreateNoWindow = true;

                    process.StartInfo.UseShellExecute = false;
                    process.Start();

                    if (timeoutMs < 0)
                        timeoutMs = 99999999; // indefinitely

                    if (timeoutMs > 0)
                    {
                        if (!process.WaitForExit(timeoutMs))
                        {
                            Console.WriteLine("Process timed out.");
                            return 1460;
                        }
                    }
                    else
                        return 0;

                    return process.ExitCode;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error executing process: " + ex.Message);
                return -1; // unhandled error
            }
        }


        /// <summary>
        /// Executes a Windows process with given command line parameters
        /// and captures console output into a string.
        ///
        /// Writes original output into the application Console which you can
        /// optionally redirect to capture output from the command line
        /// operation using `Console.SetOut` or `Console.SetError`.
        /// </summary>
        /// <param name="executable">Executable to run</param>
        /// <param name="arguments"></param>
        /// <param name="timeoutMs">Timeout of the process in milliseconds. Pass -1 to wait forever. Pass 0 to not wait.</param>
        /// <param name="output">Pass in a string reference that will receive StdOut and StdError output</param>
        /// <param name="windowStyle"></param>
        /// <returns></returns>
        public static int ExecuteProcess(string executable,
                                        string arguments,
                                        int timeoutMs,
                                        out string output,
                                        Action<string> writeDelegate = null,
                                        ProcessWindowStyle windowStyle = ProcessWindowStyle.Hidden)
        {
            Process process;

            try
            {
                using (process = new Process())
                {
                    process.StartInfo.FileName = executable;
                    process.StartInfo.Arguments = arguments;
                    process.StartInfo.WindowStyle = windowStyle;
                    if (windowStyle == ProcessWindowStyle.Hidden)
                        process.StartInfo.CreateNoWindow = true;

                    process.StartInfo.UseShellExecute = false;

                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;

                    StringBuilder sb = new StringBuilder();

                    process.OutputDataReceived += (sender, args) =>
                    {
                        sb.AppendLine(args.Data);
                        writeDelegate?.Invoke(args.Data);
                    };
                    process.ErrorDataReceived += (sender, args) =>
                    {
                        sb.AppendLine(args.Data);
                        writeDelegate?.Invoke(args.Data);
                    };
                    process.Start();

                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();

                    if (timeoutMs < 0)
                        timeoutMs = 99999999; // indefinitely

                    if (timeoutMs > 0)
                    {
                        if (!process.WaitForExit(timeoutMs))
                        {
                            Console.WriteLine("Process timed out.");
                            output = null;
                            return 1460;
                        }
                    }
                    else
                    {
                        // no exit code
                        output = sb.ToString();
                        return 0;
                    }

                    output = sb.ToString();
                    return process.ExitCode;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error executing process: " + ex.Message);
                output = null;
                return -1; // unhandled error
            }
        }

        /// <summary>
        /// Opens a Terminal window in the specified folder
        /// </summary>
        /// <param name="folder"></param>
        /// <param  name="mode">Powershell, Command or Bash</param>
        /// <returns>false if process couldn't be started - most likely invalid link</returns>
        public static bool OpenTerminal(string folder, TerminalModes mode = TerminalModes.Powershell)
        {
            try
            {
                string cmd = null, args = null;

                if (mode == TerminalModes.Powershell)
                {
                    cmd = "powershell.exe";
                    args = "-noexit -command \"cd '{0}'\"";
                }
                else if(mode == TerminalModes.Command)
                {
                    cmd = "cmd.exe";
                    args = "/k \"cd {0}\"";
                }
                
                Process.Start(cmd,string.Format(args, folder));
            }
            catch
            {
                return false;
            }
            return true;
        }
#endregion

        #region URL and HTTP Access
        /// <summary>
        /// Uses the Shell Extensions to launch a program based on URL moniker or file name
        /// Basically a wrapper around ShellExecute
        /// </summary>
        /// <param name="url">Any URL Moniker that the Windows Shell understands (URL, Word Docs, PDF, Email links etc.)</param>
        /// <returns></returns>
        public static int GoUrl(string url)
        {
            string TPath = Path.GetTempPath();
           
            ProcessStartInfo info = new ProcessStartInfo();
            info.UseShellExecute = true;
            info.Verb = "Open";
            info.WorkingDirectory = TPath;
            info.FileName = url;

            Process process = new Process(); 
            process.StartInfo = info;
            process.Start();

            return 0;
        }


        /// <summary>
        /// Displays a string in in a browser as HTML. Optionally
        /// provide an alternate extension to display in the appropriate
        /// text viewer (ie. "txt" likely shows in NotePad)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static int ShowString(string text, string extension = null)
        {
            if (extension == null)
                extension = "htm";

            string File = Path.GetTempPath() + "\\__preview." + extension;
            StreamWriter sw = new StreamWriter(File, false, Encoding.Default);
            sw.Write(text);
            sw.Close();

            return GoUrl(File);
        }

        /// <summary>
        /// Shows a string as HTML
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        public static int ShowHtml(string htmlString)
        {
            return ShowString(htmlString, null);
        }

        /// <summary>
        /// Displays a large Text string as a text file in the
        /// systems' default text viewer (ie. NotePad)
        /// </summary>
        /// <param name="TextString"></param>
        /// <returns></returns>
        public static int ShowText(string TextString)
        {
            string File = Path.GetTempPath() + "\\__preview.txt";

            StreamWriter sw = new StreamWriter(File, false);
            sw.Write(TextString);
            sw.Close();

            return GoUrl(File);
        }

        /// <summary>
        /// Simple method to retrieve HTTP content from the Web quickly
        /// </summary>
        /// <param name="url">Url to access</param>        
        /// <returns>Http response text or null</returns>
        public static string HttpGet(string url)
        {
            string errorMessage;
            return HttpGet(url, out errorMessage);
        }

        /// <summary>
        /// Simple method to retrieve HTTP content from the Web quickly
        /// </summary>
        /// <param name="url">Url to access</param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static string HttpGet(string url, out string errorMessage)
        {
            string responseText = string.Empty;
            errorMessage = null;

            using (WebClient Http = new WebClient())
            {
                try
                {
                    responseText = Http.DownloadString(url);                
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    return null;
                }
            }

            return responseText;
        }


        /// <summary>
        /// Retrieves a buffer of binary data from a URL using
        /// a plain HTTP Get.
        /// </summary>
        /// <param name="url">Url to access</param>
        /// <returns>Response bytes or null on error</returns>
        public static byte[] HttpGetBytes(string url)
        {
            string errorMessage;
            return HttpGetBytes(url,out errorMessage);
        }

        /// <summary>
        /// Retrieves a buffer of binary data from a URL using
        /// a plain HTTP Get.
        /// </summary>
        /// <param name="url">Url to access</param>
        /// <param name="errorMessage">ref parm to receive an error message</param>
        /// <returns>response bytes or null on error</returns>
        public static byte[] HttpGetBytes(string url, out string errorMessage)
        {
            byte[] result = null;
            errorMessage = null;

            using (var http = new WebClient())
            {
                try
                {
                    result = http.DownloadData(url);
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    return null;
                }
            }

            return result;
        }
        #endregion
    }

    public enum TerminalModes
    {
        Powershell,
        Command
    }
}
