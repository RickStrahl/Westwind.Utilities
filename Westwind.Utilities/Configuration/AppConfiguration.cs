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

using System;

using System.Collections.Specialized;
using System.Configuration;
using System.Reflection;
using System.Xml;
using System.Globalization;

//#if NETCORE
//    using Microsoft.Data.SqlClient;
//#else
//    using System.Data.SqlClient;
//#endif


using System.Xml.Serialization;
using System.Diagnostics;
using Westwind.Utilities.Properties;

namespace Westwind.Utilities.Configuration
{

    /// <summary>
    /// This class provides a base class for code-first, strongly typed configuration 
    /// settings in .NET. It supports storing of configuration data in
    /// .NET .config files, plain XML files, strings and SQL Server databases and
    /// custom providers.
    /// 
    /// Using this class is easy: Create a subclass of AppConfiguration and 
    /// then simply add properties to the class. Then instantiate the class, 
    /// call Initialize(), then simply access the class 
    /// properties to read configuration values.
    /// 
    /// The default implementation uses standard .NET configuration files and a 
    /// custom section within that file to hold configuration values. Other 
    /// providers can be used to store data to different stores and you can create 
    /// your own custom providers to store configuration data in yet other stores.
    /// <seealso>Managing Configuration Settings with AppConfiguration</seealso>
    /// </summary>
    public abstract class AppConfiguration
    {
        /// <summary>
        /// An instance of a IConfigurationProvider that
        /// needs to be passed in via constructor or set
        /// explicitly to read and write from the configuration
        /// store.
        /// </summary>
        [XmlIgnore]
        [NonSerialized]
        public IConfigurationProvider Provider = null;


        /// <summary>
        /// Contains an error message if a method returns false or the object fails to 
        /// load the configuration data.
        /// <seealso>Class AppConfiguration</seealso>
        /// </summary>
        [XmlIgnore]
        [NonSerialized]
        public string ErrorMessage = string.Empty;

        /// <summary>
        /// Internal flag that checks to see if Initialize was called
        /// if not - automatically calls it without parameters
        /// </summary>
        protected bool InitializeCalled = false;
        
        /// <summary>
        /// Default constructor of this class SHOULD ALWAYS be implemented in
        /// every subclass to allow serialization instantiation and setting
		/// of initial property values.
        /// </summary>         
        public AppConfiguration()
        { }

        /// <summary>
        /// This method initializes the configuration object with a provider
        /// and performs an initial read from the config store.    
        /// </summary>
        /// <param name="provider">
        /// Optional - preconfigured ConfigurationProvider instance.
        /// If not passed ConfigurationFileConfigurationProvider is used.
        /// </param>
        /// <param name="sectionName">
        /// Optional - sub-section name used for config files. Not used by all config stores
        /// </param>
        /// <param name="configData">
        /// Optional - additional config data to pass to OnInitialize if implemented
        /// </param>
        public void Initialize(IConfigurationProvider provider = null, 
                               string sectionName = null, 
                               object configData = null)
        {
            provider = provider ?? Provider;

            // Initialization occurs only once
            if (InitializeCalled)
               return;
            InitializeCalled = true;  
            
            if (string.IsNullOrEmpty(sectionName))
                sectionName = GetType().Name;

            OnInitialize(provider,sectionName,configData);
        }

        /// <summary>
        /// Override this method to handle custom initialization tasks.
        /// 
        /// This method should: create a provider and call it's Read()
        /// method to populate the current instance of the configuration
        /// object.
        /// 
        /// If all you need is to create a default provider configuration
        /// use the OnCreateDefaultProvider() method to override instead.
        /// Use this method if you need to perform custom actions beyond
        /// provider instantiation.
        /// </summary>
        /// <param name="provider">Provider value - can be null in which case ConfigurationFileProvider is used</param>
        /// <param name="sectionName">Sub Section name - can be null. Classname is used if null. Can be "appSettings" </param>
        /// <param name="configData">
        /// Any additional configuration data that can be used to
        /// configure the provider.
        /// </param>
        protected virtual void OnInitialize(IConfigurationProvider provider, 
                                           string sectionName,
                                           object configData)
        {
            if (provider == null)
                provider = OnCreateDefaultProvider(sectionName, configData);

            Provider = provider;
            if (!Provider.Read(this) && !string.IsNullOrWhiteSpace(Provider.ErrorMessage))
                Trace.WriteLine(Resources.ConfigurationInitializationError + ": " + Provider.ErrorMessage);
        }

        /// <summary>
        /// Override this method to use a specialized configuration provider for your config class
        /// when no explicit provider is passed to the Initialize() method.
        /// </summary>
        /// <param name="sectionName">Optional section name that was passed to Initialize()</param>
        /// <param name="configData">Optional config data that was passed to Initialize()</param>
        /// <returns>Instance of configuration provider</returns>
        protected virtual IConfigurationProvider OnCreateDefaultProvider(string sectionName, object configData)
        {
			// dynamically construct the generic provider type
#if NETFULL
			var providerType = typeof(ConfigurationFileConfigurationProvider<>);
#else
			var providerType = typeof(JsonFileConfigurationProvider<>);
#endif
			var type = GetType();
            Type typeProvider = providerType.MakeGenericType(type);

            var provider = Activator.CreateInstance(typeProvider) as IConfigurationProvider;

            // if no section name is passed it goes into standard appSettings
            if (!string.IsNullOrEmpty(sectionName))
                provider.ConfigurationSection = sectionName;

            return provider;
        }

        /// <summary>
        /// Writes the current configuration information data to the
        /// provider's configuration store.
        /// </summary>
        /// <returns></returns>
        public virtual bool Write()
        {
            if (Provider == null)
            {
                Provider = OnCreateDefaultProvider(null,null);
                InitializeCalled = true;
            }

            if (!Provider.Write(this))
            {
                ErrorMessage = Provider.ErrorMessage;
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Writes the current configuration information to an
        /// XML string. String is in .NET XML Serialization format.
        /// </summary>
        /// <returns></returns>
        public virtual string WriteAsString()
        {
            Initialize();

            string xml = string.Empty;
            Provider.EncryptFields(this);

            SerializationUtils.SerializeObject(this, out xml);

            if (!string.IsNullOrEmpty(xml))
                Provider.DecryptFields(this);

            return xml;
        }

        /// <summary>
        /// Reads the configuration information from the 
        /// provider's store and returns a new instance
        /// of an configuration object.
        /// </summary>
        /// <typeparam name="T">This configuration class type</typeparam>
        /// <returns></returns>
        public virtual T Read<T>()
                where T : AppConfiguration, new()
        {
            Initialize();

            var inst = Provider.Read<T>();
            if (inst == null)
            {
                ErrorMessage = Provider.ErrorMessage;
                return null;
            }

            return inst;
        }

        /// <summary>
        /// Reads the configuration from the provider's store
        /// into the current object instance.
        /// </summary>
        /// <returns></returns>
        public virtual bool Read()
        {
            Initialize();

            if (!Provider.Read(this))
            {
                ErrorMessage = Provider.ErrorMessage;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Reads configuration data from a string and populates the current
        /// instance with the values.
        /// 
        /// Data should be serialized in XML Searlization format created
        /// with <seealso cref="WriteAsString" />
        /// </summary>
        /// <param name="xml">Xml string in XML Serialization format</param>
        /// <returns>true or false</returns>
        public virtual bool Read(string xml)
        {
            Initialize();

            var newInstance = SerializationUtils.DeSerializeObject(xml, GetType());

            DataUtils.CopyObjectData(newInstance, this, "Provider,Errormessage");

            if (newInstance != null)
            {
                Provider.DecryptFields(this);
            }

            return true;
        }

        /// <summary>
        /// Reads configuration based on a provider configuration
        /// and returns a new instance of the configuration object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider">A configured  <seealso cref="IConfigurationProvider"/>" provider</param>
        /// <returns>instance of configuration or null on failure</returns>
        public static T Read<T>(IConfigurationProvider provider)
            where T : AppConfiguration, new()
        {            
            return provider.Read<T>() as T;            
        }

        /// <summary>
        /// Creates a new instance of the config object and retrieves
        /// configuration information from the provided string. String 
        /// should be in XML Serialization format or created by the 
        /// <seealso cref="WriteAsString"/> method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <param name="provider">Required if encryption decryption is desired</param>
        /// <returns>config instance or null on failure</returns>
        public static T Read<T>(string xml, IConfigurationProvider provider)
            where T : AppConfiguration, new()
        {                        
            T result =  SerializationUtils.DeSerializeObject(xml, typeof(T)) as T;            

            if (result != null && provider != null)
                provider.DecryptFields(result);

            return result;
        }

        /// <summary>
        /// Creates a new instance of the config object and retrieves
        /// configuration information from the provided string. String 
        /// should be in XML Serialization format or created by the 
        /// <seealso cref="WriteAsString"/> method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <returns>config instance or null on failure</returns>
        public static T Read<T>(string xml)
            where T : AppConfiguration, new()
        {
            return Read<T>(xml, null);
        }

    }

    /// <summary>
    /// Sample class for diagram display
    /// </summary>
    class MyAppConfiguration : AppConfiguration
    {
        public MyAppConfiguration()
        {
            MyProperty = "My default property value";
            MaxPageListItems = 15;
            ApplicationTitle = "My great application!";
        }

        public string MyProperty { get; set; }
        public int MaxPageListItems { get; set; }
        public string ApplicationTitle { get; set; }

        protected override IConfigurationProvider OnCreateDefaultProvider(string sectionName, object configData)
        {
            return base.OnCreateDefaultProvider(sectionName, configData);
        }
    }
}


              