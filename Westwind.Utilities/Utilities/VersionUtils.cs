using System;
using System.Linq;
using System.Net;

namespace Westwind.Utilities
{

    public static class VersionExtensions
    {
        /// <summary>
        /// Formats a version by stripping all zero values
        /// up to the trimTokens count provided. By default
        /// displays Major.Minor and then displays any
        /// Build and/or revision if non-zero	
        /// 
        /// More info: https://weblog.west-wind.com/posts/2024/Jun/13/C-Version-Formatting
        /// </summary>
        /// <param name="version">Version to format</param>
        /// <param name="minTokens">Minimum number of component tokens of the version to display</param>
        /// <param name="maxTokens">Maximum number of component tokens of the version to display</param>
        public static string FormatVersion(this Version version, int minTokens = 2, int maxTokens = 2)
        {
            if (minTokens < 1)
                minTokens = 1;
            if (minTokens > 4)
                minTokens = 4;
            if (maxTokens < minTokens)
                maxTokens = minTokens;
            if (maxTokens > 4)
                maxTokens = 4;

            var items = new int[] { version.Major, version.Minor, version.Build, version.Revision };

            //items = items[0..maxTokens];
            items = items.Take(maxTokens).ToArray();

            var baseVersion = string.Empty;
            for (int i = 0; i < minTokens; i++)
            {
                baseVersion += "." + items[i];

                
                
            }

            var extendedVersion = string.Empty;
            for (int i = minTokens; i < maxTokens; i++)
            {
                extendedVersion += "." + items[i];
            }
            return baseVersion.TrimStart('.') + extendedVersion.TrimEnd('.', '0');
        }


        /// <summary>
        /// Formats a version by stripping all zero values
        /// up to the trimTokens count provided. By default
        /// displays Major.Minor and then displays any
        /// Build and/or revision if non-zero	
        /// </summary>
        /// <param name="version">Version to format</param>
        /// <param name="minTokens">Minimum number of component tokens to display</param>
        /// <param name="maxTokens">Maximum number of component tokens to display</param>
        public static string FormatVersion(string version, int minTokens = 2, int maxTokens = 2)
        {
            var ver = new Version(version);
            return ver.FormatVersion(minTokens, maxTokens);
        }

    }
}
