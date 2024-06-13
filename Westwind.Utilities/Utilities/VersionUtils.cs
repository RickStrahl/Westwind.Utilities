using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
