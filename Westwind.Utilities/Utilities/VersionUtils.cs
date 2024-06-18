using System;
using System.Linq;

namespace Westwind.Utilities
{

    public static class VersionExtensions
    {
        /// <summary>
        /// Formats a version by stripping all zero values
        /// up to the trimTokens count provided. By default
        /// displays Major.Minor and then displays any
        /// Build and/or revision if non-zero	
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

            var length = maxTokens;
            for (length = maxTokens; length > minTokens; length--)
            {
                if (items[length - 1] != 0)
                {
                    break;
                }
            }

            var builder = new StringBuilder(length * 11);

            for (int i = 0; i < length; i++)
            {
                builder
                    .Append(items[i])
                    .Append(".");
            }

            return builder.ToString(0, builder.Length - 1);
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
