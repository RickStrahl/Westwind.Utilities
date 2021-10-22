#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          � West Wind Technologies, 2009-2013
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

namespace Westwind.Utilities.Configuration
{

    /// <summary>
    /// Reads and Writes configuration settings in .NET config files and 
    /// sections. Allows reading and writing to default or external files 
    /// and specification of the configuration section that settings are
    /// applied to.
    /// </summary>
    public class JsonFileConfigurationProvider<TAppConfiguration> : ConfigurationProviderBase<TAppConfiguration>
        where TAppConfiguration : AppConfiguration, new()
    {

        /// <summary>
        /// Optional - the Configuration file where configuration settings are
        /// stored in. If not specified uses the default Configuration Manager
        /// and its default store.
        /// </summary>
        public string JsonConfigurationFile
        {
            get { return _JsonConfigurationFile; }
            set { _JsonConfigurationFile = value; }
        }
        private string _JsonConfigurationFile = "applicationConfiguration.json";
		

        /// <summary>
        /// Reads configuration into the current instance of the config object passed in.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public override bool Read(AppConfiguration config)
        {
            var newConfig = JsonSerializationUtils.DeserializeFromFile(JsonConfigurationFile, typeof(TAppConfiguration)) as TAppConfiguration;
            if (newConfig == null)
            {
                if (Write(config))
                    return true;
                return false;
            }
            DecryptFields(newConfig);
            DataUtils.CopyObjectData(newConfig, config, "Provider,ErrorMessage");

            return true;
        }

        /// <summary>
        /// Return 
        /// </summary>
        /// <typeparam name="TAppConfig"></typeparam>
        /// <returns></returns>
        public override TAppConfig Read<TAppConfig>()
        {
            var result = JsonSerializationUtils.DeserializeFromFile(JsonConfigurationFile, typeof(TAppConfig)) as TAppConfig;
            if (result != null)
                DecryptFields(result);

            return result;
        }

        /// <summary>
        /// Write configuration to XmlConfigurationFile location
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public override bool Write(AppConfiguration config)
        {
            EncryptFields(config);

            bool result = JsonSerializationUtils.SerializeToFile(config, JsonConfigurationFile, false, true);

            // Have to decrypt again to make sure the properties are readable afterwards
            DecryptFields(config);

            return result;
        }
    }

}
