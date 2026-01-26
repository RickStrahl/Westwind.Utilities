using System;
using System.Runtime.InteropServices;
using static System.Environment;

namespace Westwind.Utilities
{
    /// <summary>
    /// Class that returns special Windows file system paths. Extends the folder list
    /// beyond what Environment.GetFolderPath(Environment.SpecialFolder) provides
    /// with additional Windows known folders like Library, Downloads etc.
    /// </summary>
    public static class KnownFolders
    {
        /// <summary>
        /// Gets the current path to the specified known folder as currently configured. This does
        /// not require the folder to be existent.
        /// </summary>
        /// <param name="knownFolder">The known folder which current path will be returned.</param>
        /// <returns>The default path of the known folder.</returns>
        /// <exception cref="System.Runtime.InteropServices.ExternalException">Thrown if the path
        ///     could not be retrieved.</exception>
        public static string GetPath(KnownFolder knownFolder)
        {
            return GetPath(knownFolder, false);
        }


        /// <summary>
        /// Gets the current path to the specified known folder as currently configured. This does
        /// not require the folder to be existent.
        /// </summary>
        /// <param name="knownFolder">The known folder which current path will be returned.</param>
        /// <param name="defaultUser">Specifies if the paths of the default user (user profile
        ///     template) will be used. This requires administrative rights.</param>
        /// <returns>The default path of the known folder.</returns>
        /// <exception cref="System.Runtime.InteropServices.ExternalException">Thrown if the path
        ///     could not be retrieved.</exception>
        public static string GetPath(KnownFolder knownFolder, bool defaultUser)
        {
            return GetPath(knownFolder, KnownFolderFlags.DontVerify, defaultUser);
        }

        /// <summary>
        /// Gets the default path to the specified known folder. This does not require the folder
        /// to be existent.
        /// </summary>
        /// <param name="knownFolder">The known folder which default path will be returned.</param>
        /// <returns>The current (and possibly redirected) path of the known folder.</returns>
        /// <exception cref="System.Runtime.InteropServices.ExternalException">Thrown if the path
        ///     could not be retrieved.</exception>
        public static string GetDefaultPath(KnownFolder knownFolder)
        {
            return GetDefaultPath(knownFolder, false);
        }

        /// <summary>
        /// Gets the default path to the specified known folder. This does not require the folder
        /// to be existent.
        /// </summary>
        /// <param name="knownFolder">The known folder which default path will be returned.</param>
        /// <param name="defaultUser">Specifies if the paths of the default user (user profile
        ///     template) will be used. This requires administrative rights.</param>
        /// <returns>The current (and possibly redirected) path of the known folder.</returns>
        /// <exception cref="System.Runtime.InteropServices.ExternalException">Thrown if the path
        ///     could not be retrieved.</exception>
        public static string GetDefaultPath(KnownFolder knownFolder, bool defaultUser)
        {
            return GetPath(knownFolder, KnownFolderFlags.DefaultPath | KnownFolderFlags.DontVerify,
                defaultUser);
        }

        /// <summary>
        /// Creates and initializes the known folder.
        /// </summary>
        /// <param name="knownFolder">The known folder which will be initialized.</param>
        /// <exception cref="System.Runtime.InteropServices.ExternalException">Thrown if the known
        ///     folder could not be initialized.</exception>
        public static void Initialize(KnownFolder knownFolder)
        {
            Initialize(knownFolder, false);
        }

        /// <summary>
        /// Creates and initializes the known folder.
        /// </summary>
        /// <param name="knownFolder">The known folder which will be initialized.</param>
        /// <param name="defaultUser">Specifies if the paths of the default user (user profile
        ///     template) will be used. This requires administrative rights.</param>
        /// <exception cref="System.Runtime.InteropServices.ExternalException">Thrown if the known
        ///     folder could not be initialized.</exception>
        public static void Initialize(KnownFolder knownFolder, bool defaultUser)
        {
            GetPath(knownFolder, KnownFolderFlags.Create | KnownFolderFlags.Init, defaultUser);
        }

        private static string GetPath(KnownFolder knownFolder, KnownFolderFlags flags,
            bool defaultUser)
        {
            // Handle SpecialFolder-mapped values
            SpecialFolder? specialFolder = knownFolder switch
            {
                KnownFolder.UserProfile => SpecialFolder.UserProfile,
                KnownFolder.ProgramFiles => SpecialFolder.ProgramFiles,
                KnownFolder.ProgramFilesX86 => SpecialFolder.ProgramFilesX86,
                KnownFolder.Programs => SpecialFolder.Programs,
                KnownFolder.ApplicationData => SpecialFolder.ApplicationData,
                KnownFolder.LocalApplicationData => SpecialFolder.LocalApplicationData,
                KnownFolder.AdminTools => SpecialFolder.AdminTools,
                KnownFolder.Startup => SpecialFolder.Startup,
                KnownFolder.Recent => SpecialFolder.Recent,
                KnownFolder.SendTo => SpecialFolder.SendTo,
                KnownFolder.StartMenu => SpecialFolder.StartMenu,
                KnownFolder.DesktopDirectory => SpecialFolder.DesktopDirectory,
                KnownFolder.MyComputer => SpecialFolder.MyComputer,
                KnownFolder.NetworkShortcuts => SpecialFolder.NetworkShortcuts,
                KnownFolder.Fonts => SpecialFolder.Fonts,
                KnownFolder.Templates => SpecialFolder.Templates,
                KnownFolder.CommonStartMenu => SpecialFolder.CommonStartMenu,
                KnownFolder.CommonPrograms => SpecialFolder.CommonPrograms,
                KnownFolder.CommonStartup => SpecialFolder.CommonStartup,
                KnownFolder.CommonDesktopDirectory => SpecialFolder.CommonDesktopDirectory,
                KnownFolder.PrinterShortcuts => SpecialFolder.PrinterShortcuts,
                KnownFolder.InternetCache => SpecialFolder.InternetCache,
                KnownFolder.Cookies => SpecialFolder.Cookies,
                KnownFolder.History => SpecialFolder.History,
                KnownFolder.CommonApplicationData => SpecialFolder.CommonApplicationData,
                KnownFolder.Windows => SpecialFolder.Windows,
                KnownFolder.System => SpecialFolder.System,
                KnownFolder.SystemX86 => SpecialFolder.SystemX86,
                KnownFolder.CommonProgramFiles => SpecialFolder.CommonProgramFiles,
                KnownFolder.CommonProgramFilesX86 => SpecialFolder.CommonProgramFilesX86,
                KnownFolder.CommonTemplates => SpecialFolder.CommonTemplates,
                KnownFolder.CommonDocuments => SpecialFolder.CommonDocuments,
                KnownFolder.CommonAdminTools => SpecialFolder.CommonAdminTools,
                KnownFolder.CommonMusic => SpecialFolder.CommonMusic,
                KnownFolder.CommonPictures => SpecialFolder.CommonPictures,
                KnownFolder.CommonVideos => SpecialFolder.CommonVideos,
                KnownFolder.Resources => SpecialFolder.Resources,
                KnownFolder.LocalizedResources => SpecialFolder.LocalizedResources,
                KnownFolder.CommonOemLinks => SpecialFolder.CommonOemLinks,
                KnownFolder.CDBurning => SpecialFolder.CDBurning,
                _ => null
            };

            if (specialFolder != null)
                return Environment.GetFolderPath(specialFolder.Value);

#if NET60_OR_GREATER
            if (!OperatingSystem.IsWindows())
                return string.Empty;   // exit if not windows
#endif

            // these only work on Windows via PInvoke to Windows API
            IntPtr outPath;
            int result = SHGetKnownFolderPath(new Guid(_knownFolderGuids[(int)knownFolder]),
                (uint)flags, new IntPtr(defaultUser ? -1 : 0), out outPath);
            if (result >= 0)
            {
                return Marshal.PtrToStringUni(outPath);
            }
            else
            {
                throw new ExternalException("Unable to retrieve the known folder path. It may not "
                                            + "be available on this system.", result);
            }
        }

        /// <summary>
        /// Retrieves the full path of a known folder identified by the folder's KnownFolderID.
        /// </summary>
        /// <param name="rfid">A KnownFolderID that identifies the folder.</param>
        /// <param name="dwFlags">Flags that specify special retrieval options. This value can be
        ///     0; otherwise, one or more of the KnownFolderFlag values.</param>
        /// <param name="hToken">An access token that represents a particular user. If this
        ///     parameter is NULL, which is the most common usage, the function requests the known
        ///     folder for the current user. Assigning a value of -1 indicates the Default User.
        ///     The default user profile is duplicated when any new user account is created.
        ///     Note that access to the Default User folders requires administrator privileges.
        ///     </param>
        /// <param name="ppszPath">When this method returns, contains the address of a string that
        ///     specifies the path of the known folder. The returned path does not include a
        ///     trailing backslash.</param>
        /// <returns>Returns S_OK if successful, or an error value otherwise.</returns>
        [DllImport("Shell32.dll")]
        private static extern int SHGetKnownFolderPath(
            [MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken,
            out IntPtr ppszPath);

        [Flags]
        private enum KnownFolderFlags : uint
        {
            SimpleIDList = 0x00000100,
            NotParentRelative = 0x00000200,
            DefaultPath = 0x00000400,
            Init = 0x00000800,
            NoAlias = 0x00001000,
            DontUnexpand = 0x00002000,
            DontVerify = 0x00004000,
            Create = 0x00008000,
            NoAppcontainerRedirection = 0x00010000,
            AliasOnly = 0x80000000
        }

        private static string[] _knownFolderGuids = new string[]
        {            
         //Folder Ids: https://msdn.microsoft.com/en-us/library/windows/desktop/dd378457(v=vs.85).aspx
         "{56784854-C6CB-462B-8169-88E350ACB882}", // Contacts
         "{B4BFCC3A-DB2C-424C-B029-7FE99A87C641}", // Desktop
         "{FDD39AD0-238F-46AF-ADB4-6C85480369C7}", // Documents
         "{374DE290-123F-4565-9164-39C4925E467B}", // Downloads
         "{1777F761-68AD-4D8A-87BD-30B759FA33DD}", // Favorites
         "{BFB9D5E0-C6A9-404C-B2B2-AE6DB6AF4968}", // Links
         "{4BD8D571-6D19-48D3-BE97-422220080E43}", // Music
         "{33E28130-4E1E-4676-835A-98395C3BC3BB}", // Pictures
         "{4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4}", // SavedGames
         "{7D1D3A04-DEBB-4115-95CF-2F29DA2920DA}", // SavedSearches
         "{18989B1D-99B5-455B-841C-AB7C74E4DDFC}", // Videos
         "{7B0DB17D-9CD2-4A93-9733-46CC89022E7C}", // DocumentsLibrary
         "{1B3EA5DC-B587-4786-B4EF-BD1DC332AEAE}"  // Libraries
        };
    }



    /// <summary>
    /// Standard folders registered with the system. These folders are installed with Windows Vista
    /// and later operating systems, and a computer will have only folders appropriate to it
    /// installed.
    /// </summary>
    public enum KnownFolder
    {
        Contacts,
        Desktop,
        Documents,
        Downloads,
        Favorites,
        Links,
        Music,
        Pictures,
        SavedGames,
        SavedSearches,
        Videos,
        DocumentsLibrary,
        Libraries,

        /* Separately handled */

        UserProfile,
        ProgramFiles,
        ProgramFilesX86,
        Programs,
        ApplicationData,
        LocalApplicationData,
        AdminTools,

        // Additional SpecialFolder values
        Startup,
        Recent,
        SendTo,
        StartMenu,
        DesktopDirectory,
        MyComputer,
        NetworkShortcuts,
        Fonts,
        Templates,
        CommonStartMenu,
        CommonPrograms,
        CommonStartup,
        CommonDesktopDirectory,
        PrinterShortcuts,
        InternetCache,
        Cookies,
        History,
        CommonApplicationData,
        Windows,
        System,
        SystemX86,
        CommonProgramFiles,
        CommonProgramFilesX86,
        CommonTemplates,
        CommonDocuments,
        CommonAdminTools,
        CommonMusic,
        CommonPictures,
        CommonVideos,
        Resources,
        LocalizedResources,
        CommonOemLinks,
        CDBurning

    }

}
