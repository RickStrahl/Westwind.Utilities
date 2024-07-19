
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace Westwind.Utilities.Windows
{

    /// <summary>
    /// Windows specific system and information helpers
    /// Helper class that provides Windows and .NET Version numbers.    
    /// </summary>
    #if NET6_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public static class WindowsUtils
    {
        /// <summary>
        /// Returns the Windows major version number for this computer.
        /// based on this: http://stackoverflow.com/questions/21737985/windows-version-in-c-sharp/37716269#37716269
        /// </summary>
        public static uint WinMajorVersion
        {
            get
            {
                dynamic major;
                // The 'CurrentMajorVersionNumber' string value in the CurrentVersion key is new for Windows 10,
                // and will most likely (hopefully) be there for some time before MS decides to change this - again...
                if (TryGetRegistryKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentMajorVersionNumber",
                    out major, Registry.LocalMachine))
                {
                    return (uint)major;
                }

                // When the 'CurrentMajorVersionNumber' value is not present we fallback to reading the previous key used for this: 'CurrentVersion'
                dynamic version;
                if (!TryGetRegistryKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentVersion",
                    out version, Registry.LocalMachine))
                    return 0;

                var versionParts = ((string)version).Split('.');
                if (versionParts.Length != 2) return 0;
                uint majorAsUInt;
                return uint.TryParse(versionParts[0], out majorAsUInt) ? majorAsUInt : 0;
            }
        }

        /// <summary>
        ///     Returns the Windows minor version number for this computer.
        /// </summary>
        public static uint WinMinorVersion
        {
            get
            {
                dynamic minor;
                // The 'CurrentMinorVersionNumber' string value in the CurrentVersion key is new for Windows 10,
                // and will most likely (hopefully) be there for some time before MS decides to change this - again...
                if (TryGetRegistryKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentMinorVersionNumber",
                    out minor, Registry.LocalMachine))
                {
                    return (uint)minor;
                }

                // When the 'CurrentMinorVersionNumber' value is not present we fallback to reading the previous key used for this: 'CurrentVersion'
                dynamic version;
                if (!TryGetRegistryKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentVersion",
                    out version, Registry.LocalMachine))
                    return 0;

                var versionParts = ((string)version).Split('.');
                if (versionParts.Length != 2) return 0;
                uint minorAsUInt;
                return uint.TryParse(versionParts[1], out minorAsUInt) ? minorAsUInt : 0;
            }
        }

        /// <summary>
        ///     Returns the Windows minor version number for this computer.
        /// </summary>
        public static uint WinBuildVersion
        {
            get
            {
                dynamic buildNumber;
                // The 'CurrentMinorVersionNumber' string value in the CurrentVersion key is new for Windows 10,
                // and will most likely (hopefully) be there for some time before MS decides to change this - again...
                if (TryGetRegistryKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentBuildNumber",
                    out buildNumber, Registry.LocalMachine))
                {
                    return Convert.ToUInt32(buildNumber);
                }


                if (!TryGetRegistryKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentBuild",
                    out buildNumber, Registry.LocalMachine))
                    return 0;

                return Convert.ToUInt32(buildNumber);
            }
        }

        /// <summary>
        ///     Returns the Windows minor version number for this computer.
        /// </summary>
        public static string WinBuildLabVersion
        {
            get
            {
                dynamic buildNumber;
                // The 'CurrentMinorVersionNumber' string value in the CurrentVersion key is new for Windows 10,
                // and will most likely (hopefully) be there for some time before MS decides to change this - again...
                if (TryGetRegistryKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "BuildLabEx",
                    out buildNumber, Registry.LocalMachine))
                {
                    return buildNumber;
                }

                return WinBuildVersion.ToString();
            }
        }

        /// <summary>
        /// Returns whether or not the current computer is a server or not.
        /// </summary>
        public static uint IsServer
        {
            get
            {
                dynamic installationType;
                if (TryGetRegistryKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "InstallationType",
                    out installationType))
                {
                    return (uint)(installationType.Equals("Client") ? 0 : 1);
                }

                return 0;
            }
        }

        static string DotnetVersion = null;

        /// <summary> 
        /// Returns the .NET version installed on the machine
        /// </summary>             
        /// <returns>
        /// Full Framework: number (ie `4.8.1`)
        /// .NET Core: FrameworkVersion (ie. `.NET 7.0.7`)
        /// </returns>
        public static string GetDotnetVersion()
        {
#if NETCORE
            if (!string.IsNullOrEmpty(DotnetVersion))
                return DotnetVersion;

            DotnetVersion = RuntimeInformation.FrameworkDescription;
            return DotnetVersion;
#else

            if (!string.IsNullOrEmpty(DotnetVersion))
                return DotnetVersion;

            dynamic value;
            TryGetRegistryKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\", "Release", out value);

            if (value == null)
            {
                DotnetVersion = "4.0";
                return DotnetVersion;
            }

            int releaseKey = value;

            // https://msdn.microsoft.com/en-us/library/hh925568(v=vs.110).aspx
            // RegEdit paste: HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full
            if (releaseKey >= 533320)
                DotnetVersion = "4.8.1";
            else if (releaseKey >= 528040)
                DotnetVersion = "4.8";
            else if (releaseKey >= 461808)
                DotnetVersion = "4.7.2";
            else if (releaseKey >= 461308)
                DotnetVersion = "4.7.1";
            else if (releaseKey >= 460798)
                DotnetVersion = "4.7";
            else if (releaseKey >= 394802)
                DotnetVersion = "4.6.2";
            else if (releaseKey >= 394254)
                DotnetVersion = "4.6.1";
            else if (releaseKey >= 393295)
                DotnetVersion = "4.6";
            else if (releaseKey >= 379893)
                DotnetVersion = "4.5.2";
            else if (releaseKey >= 378675)
                DotnetVersion = "4.5.1";
            else if (releaseKey >= 378389)
                DotnetVersion = "4.5";

            // This line should never execute. A non-null release key should mean 
            // that 4.5 or later is installed. 
            else
                DotnetVersion = "4.0";

            return DotnetVersion;
#endif
        }

        static string _WindowsVersion = null;

        /// <summary>
        /// Returns a Windows Version string including build number
        /// </summary>
        /// <returns></returns>
        public static string GetWindowsVersion()
        {

            if (string.IsNullOrEmpty(_WindowsVersion))
                _WindowsVersion = WinMajorVersion + "." + WinMinorVersion + "." +
                                  WinBuildLabVersion;

            return _WindowsVersion;
        }


        /// <summary>
        /// Retrieves a registry value into a dynamic value based on path and key.
        /// </summary>
        /// <param name="path">base key relative path (starts without slash)</param>
        /// <param name="key">The keyname to retrieve. Use string.Empty for the default key</param>
        /// <param name="value">Out value result as a dynamic value</param>
        /// <param name="baseKey">Base key like HKLM, HKCU, HKCR</param>
        /// <returns>true or false</returns>
        public static bool TryGetRegistryKey(string path, string key, out dynamic value, RegistryKey baseKey = null)
        {
            if (baseKey == null)
                baseKey = Registry.CurrentUser;

            value = null;
            try
            {
                RegistryKey rk = baseKey.OpenSubKey(path);
                if (rk == null) return false;
                value = rk.GetValue(key);
                return value != null;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Determine whether the user is an Administrator
        /// </summary>
        /// <returns></returns>
        [DllImport("shell32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsUserAnAdmin();
    }



}
