#pragma warning disable SYSLIB0014

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
using System.Runtime.InteropServices;

namespace Westwind.Utilities
{

    /// <summary>
    /// Windows specific shell utility functions 
    /// </summary>
    public static class ShellUtils
    {

        #region Open in or Start Process

        /// <summary>
        /// Executes a Windows process with given command line parameters
        /// </summary>
        /// <param name="executable">Executable to run</param>
        /// <param name="arguments">Command Line Parameters passed to executable</param>
        /// <param name="timeoutMs">Timeout of the process in milliseconds. Pass -1 to wait forever. Pass 0 to not wait.</param>
        /// <param name="windowStyle">Hidden, Normal etc.</param>
        /// <returns>process exit code or 0 if run and forget. 1460 for time out. -1 on error</returns>
        public static int ExecuteProcess(string executable, 
                                        string arguments = null, 
                                        int timeoutMs = 0, 
                                        ProcessWindowStyle windowStyle = ProcessWindowStyle.Normal)
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
                    else // run and don't wait - no exit code
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
        /// Writes command output to the output StringBuilder
        /// from StdOut and StdError.
        /// </summary>
        /// <param name="executable">Executable to run</param>
        /// <param name="arguments">Command Line Parameters passed to executable</param>
        /// <param name="timeoutMs">Timeout of the process in milliseconds. Pass -1 to wait forever. Pass 0 to not wait.</param>
        /// <param name="output">Pass in a string reference that will receive StdOut and StdError output</param>
        /// <param name="windowStyle">Hidden, Normal, etc.</param>
        /// <returns>process exit code or 0 if run and forget. 1460 for time out. -1 on error</returns>
        public static int ExecuteProcess(string executable,
            string arguments,
            int timeoutMs,
            out StringBuilder output,
            ProcessWindowStyle windowStyle = ProcessWindowStyle.Hidden,
            Action<bool> completionDelegate = null)
        {
            return ExecuteProcess(executable, arguments, timeoutMs, out output, null, windowStyle, completionDelegate);
        }


        /// <summary>
        /// Executes a Windows process with given command line parameters
        /// and captures console output into a string.
        /// 
        /// Pass in a String Action that receives output from
        /// StdOut and StdError as it is written (one line at a time).
        /// </summary>
        /// <param name="executable">Executable to run</param>
        /// <param name="arguments">Command Line Parameters passed to executable</param>
        /// <param name="timeoutMs">Timeout of the process in milliseconds. Pass -1 to wait forever. Pass 0 to not wait.</param>
        /// <param name="writeDelegate">Delegate to let you capture streaming output of the executable to stdout and stderror.</param>
        /// <param name="windowStyle">Hidden, Normal etc.</param>
        /// <param name="completionDelegate">delegate that is called when execute completes. Passed true if success or false if timeout or failed</param>
        /// <returns>process exit code or 0 if run and forget. 1460 for time out. -1 on error</returns>
        public static int ExecuteProcess(string executable,
            string arguments,
            int timeoutMs,
            Action<string> writeDelegate = null,
            ProcessWindowStyle windowStyle = ProcessWindowStyle.Hidden,
            Action<bool> completionDelegate = null)
        {
            return ExecuteProcess(executable, arguments, timeoutMs, out StringBuilder output, writeDelegate, windowStyle, completionDelegate);
        }

        private static Process process = null;

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
        /// <param name="output">StringBuilder that will receive StdOut and StdError output</param>
        /// <param name="writeDelegate">Action to capture stdout and stderror output for you to handle</param>
        /// <param name="windowStyle"></param>
        /// <param name="completionDelegate">If waiting for completion you can be notified when the exection is complete</param>
        /// <returns>process exit code or 0 if run and forget. 1460 for time out. -1 on error</returns>
        private static int ExecuteProcess(string executable,
                                        string arguments,
                                        int timeoutMs,
                                        out StringBuilder output,
                                        Action<string> writeDelegate = null,
                                        ProcessWindowStyle windowStyle = ProcessWindowStyle.Hidden,
                                        Action<bool> completionDelegate = null)
        {
            

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

                    var sb  = new StringBuilder();

                    process.OutputDataReceived += (sender, args) =>
                    {
                        if (writeDelegate != null)
                            writeDelegate.Invoke(args.Data);
                        else
                            sb.AppendLine(args.Data);
                    };
                    process.ErrorDataReceived += (sender, args) =>
                    {
                        if (writeDelegate != null)
                            writeDelegate.Invoke(args.Data);
                        else
                            sb.AppendLine(args.Data);
                    };

                    if (completionDelegate != null)
                    {
                        process.Exited += (sender, args) =>
                        {
                            var proc = sender as Process;
                            completionDelegate?.Invoke(proc.ExitCode == 0);
                        };
                    }

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
                            completionDelegate?.Invoke(false);
                            return 1460;
                        }
                        completionDelegate?.Invoke(true);
                    }
                    else
                    {
                        // no exit code
                        output = sb;
                        return 0;
                    }

                    output = sb;
                    return process.ExitCode;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing process: {ex.Message}");
                output = null;
                return -1; // unhandled error
            }
        }

        #endregion

        #region Shell Execute Apis, URL Openening

        /// <summary>
        /// Uses the Shell Extensions to launch a program based on URL moniker or file name
        /// Basically a wrapper around ShellExecute
        /// </summary>
        /// <param name="url">Any URL Moniker that the Windows Shell understands (URL, Word Docs, PDF, Email links etc.)</param>
        /// <returns></returns>
        public static bool GoUrl(string url, string workingFolder = null)
        {
            if (string.IsNullOrEmpty(workingFolder))
                return OpenUrl(url);

            string TPath = Path.GetTempPath();

            ProcessStartInfo info = new ProcessStartInfo();
            info.UseShellExecute = true;
            info.Verb = "Open";
            info.WorkingDirectory = TPath;
            info.FileName = url;

            bool result;

            using (Process process = new Process())
            {
                process.StartInfo = info;

                try
                {
                    result = process.Start();
                }
                catch
                {
                    return false;
                }
            }

            return result;
        }

        /// <summary>
        /// Opens a URL in the browser. This version is specific to opening
        /// a URL in a browser and it's cross platform enabled.
        /// </summary>
        /// <param name="url"></param>
        public static bool OpenUrl(string url)
        {
            bool success = true;

#if NETCORE
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.OSDescription.Contains("microsoft-standard");
#else
            bool isWindows = true;
#endif
            Process p = null;
            try
            {
                var psi = new ProcessStartInfo(url);
                psi.UseShellExecute = isWindows;   // must be explicit -defaults changed in NETFX & NETCORE
                p = Process.Start(psi);
            }
            catch
            {
#if NETCORE
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (isWindows)
                {
                    url = url.Replace("&", "^&");
                    try
                    {
                        Process.Start(new ProcessStartInfo("cmd.exe", $"/c start {url}") {CreateNoWindow = true});
                    }
                    catch
                    {
                        success = false;
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    p = Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    p = Process.Start("open", url);
                }
                else
                {
                    success = false;
                }
#else
                success = false;
#endif
            }

            p?.Dispose();

            return success;
        }



        /// <summary>
        /// Wrapper around the Shell Execute API. Windows specific.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="arguments"></param>
        /// <param name="workingFolder"></param>
        /// <param name="verb"></param>
        /// <returns></returns>
        public static int ShellExecute(string url, string arguments = null,
            string workingFolder = null, string verb = "open")
        {
            ProcessStartInfo info = new ProcessStartInfo();
            info.UseShellExecute = true;
            info.Verb = verb;

            info.FileName = url;
            info.Arguments = arguments;
            info.WorkingDirectory = workingFolder;

            using (Process process = new Process())
            {
                process.StartInfo = info;
                process.Start();
            }

            return 0;
        }

        /// <summary>
        /// Opens a File or Folder in Explorer. If the path is a file
        /// Explorer is opened in the parent folder with the file selected
        /// </summary>
        /// <param name="filename"></param>
        public static bool OpenFileInExplorer(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return false;

            // Is it a directory? Just open
            if (Directory.Exists(filename))
                ShellExecute(filename);
            else
            {
                // required as command Explorer command line doesn't allow mixed slashes
                filename = FileUtils.NormalizePath(filename); 

                if (!File.Exists(filename))
                    filename = Path.GetDirectoryName(filename);

                try
                {
                    Process.Start("explorer.exe", $"/select,\"{filename}\"");
                }
                catch
                {   
                    return false;
                }
            }
            return true;
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


        /// <summary>
        /// Executes a Windows Command Line using Shell Execute as a
        /// single command line with parameters. This method handles
        /// parsing out the executable from the parameters.
        /// </summary>
        /// <param name="fullCommandLine">Full command line - Executable plus arguments.
        /// If the executable contains paths with spaces **make sure to add quotes around the executable** otherwise the executable may not be found.
        /// </param>
        /// <param name="workingFolder">Optional - the folder the executable runs in. If not specified uses current folder.</param>
        /// <param name="waitForExitMs">Optional - Number of milliseconds to wait for completion. 0 don't wait.</param>
        /// <param name="verb">Optional - Shell verb to apply. Defaults to "Open"</param>
        /// <param name="windowStyle">Optional - Windows style for the launched application. Default style is normal</param>
        /// <remarks>
        /// If the executable or parameters contain or **may contain spaces** make sure you use quotes (") or (') around the exec or parameters.
        ///
        /// throws if the process fails to start or doesn't complete in time (if timeout is specified).
        /// </remarks>
        public static void ExecuteCommandLine(string fullCommandLine,
            string workingFolder = null,
            int waitForExitMs = 0,
            string verb = "OPEN",
            ProcessWindowStyle windowStyle = ProcessWindowStyle.Normal,
            bool useShellExecute = true)
        {
            string executable = fullCommandLine;
            string args = null;

            if (executable.StartsWith("\""))
            {
                int at = executable.IndexOf("\" ");
                if (at > 0)
                {
                    args = executable.Substring(at + 1).Trim();
                    executable = executable.Substring(0, at);
                }
            }
            else
            {
                int at = executable.IndexOf(" ");
                if (at > 0)
                {

                    if (executable.Length > at + 1)
                        args = executable.Substring(at + 1).Trim();
                    executable = executable.Substring(0, at);
                }
            }

            var pi = new ProcessStartInfo
            {
                Verb = verb,
                WindowStyle = windowStyle,
                FileName = executable,
                WorkingDirectory = workingFolder,
                Arguments = args,
                UseShellExecute = true
            };

            Process p;
            using (p = Process.Start(pi))
            {
                if (waitForExitMs > 0)
                {
                    if (!p.WaitForExit(waitForExitMs))
                        throw new TimeoutException("Process failed to complete in time.");
                }
            }
        }

        /// <summary>
        /// Displays a string in in a browser as HTML. Optionally
        /// provide an alternate extension to display in the appropriate
        /// text viewer (ie. "txt" likely shows in NotePad)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static bool ShowString(string text, string extension = null)
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
        public static bool ShowHtml(string htmlString)
        {
            return ShowString(htmlString, null);
        }

        /// <summary>
        /// Displays a large Text string as a text file in the
        /// systems' default text viewer (ie. NotePad)
        /// </summary>
        /// <param name="TextString"></param>
        /// <returns></returns>
        public static bool ShowText(string TextString)
        {
            string File = Path.GetTempPath() + "\\__preview.txt";

            StreamWriter sw = new StreamWriter(File, false);
            sw.Write(TextString);
            sw.Close();

            return GoUrl(File);
        }
#endregion

#region Simple HTTP Helpers
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
        Command,
        Bash
    }
}
#pragma warning restore SYSLIB0014