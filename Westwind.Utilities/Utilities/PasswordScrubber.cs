using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Westwind.Utilities
{
    /// <summary>
    /// A very basic password scrubber that can scrub passwords from
    /// JSON and Sql Connection strings. 
    /// 
    /// This is a very basic implementation where you can provide the
    /// keys to scrub or use default values.
    /// </summary>
    public class PasswordScrubber
    {
        /// <summary>
        /// A static instance that can be used without instantiating first
        /// </summary>
        public static PasswordScrubber Instance = new PasswordScrubber();

        /// <summary>
        /// If set to a non-zero value displays the frist n characters
        /// of the value that is being obscured.
        /// </summary>
        public int ShowUnobscuredCharacterCount = 2;

        /// <summary>
        /// Value displayed for obscured values. If choosing to display first characters
        /// those are in addition to the obscured values.
        /// </summary>
        public string ObscuredValueBaseDisplay = "****";


        public string ScrubJsonValues(string configString, params string[] jsonKeys)
        {
            if (jsonKeys == null || jsonKeys.Length < 1) jsonKeys = new string[1] { "password" };

            foreach (var token in jsonKeys)
            {
                var matchValue = $@"""{token}"":\s*""(.*?)""";
                var match = Regex.Match(configString, matchValue, RegexOptions.Multiline | RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var group = match.Groups[1];
                    configString = configString.Replace(group.Value, ObscureValue(group.Value));
                }
            }

            return configString;
        }

        public string ScrubSqlConnectionStringValues(string configString, params string[] connKeys)
        {
            if (connKeys == null || connKeys.Length < 1) connKeys = new string[] { "pwd", "password" };

            foreach (var key in connKeys)
            {
                // Sql Connection String pwd
                var extract = StringUtils.ExtractString(configString, $"{key}=", ";", allowMissingEndDelimiter: true, returnDelimiters: true);
                if (!string.IsNullOrEmpty(extract))
                {
                    var only = StringUtils.ExtractString(extract, $"{key}=", ";", allowMissingEndDelimiter: true, returnDelimiters: false);
                    configString = configString.Replace(extract, $"{key}=" + ObscureValue(only) + ";");
                }
            }

            return configString;
        }

        public string ObscureValue(string value, int showUnobscuredCharacterCount = -1)
        {
            if (string.IsNullOrEmpty(value)) return value;

            if (showUnobscuredCharacterCount < 0)
                showUnobscuredCharacterCount = ShowUnobscuredCharacterCount;

            // very short –just display the obscured value without any revealed characters
            if (showUnobscuredCharacterCount > value.Length + 2)
                return ObscuredValueBaseDisplay;

            return value.Substring(0, showUnobscuredCharacterCount) + ObscuredValueBaseDisplay;
        }
    }
}
