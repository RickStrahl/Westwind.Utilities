# Westwind.Utilities Changelog

### 5.0.7

* **StringUtils 'Many' Operations**  
String extension methods that operate on 'many' values for various string operations: `EqualsMany()`, `ContainsMany()`, `StartsWithMany()`, `ReplaceMany()`

* **AsyncUtils.DelayExecution()**  
Added a couple of handlers that allow delayed code execution off a timer. Pass in an Action to execute, a timeout and an optional error handler.

* **AsyncUtils.Timeout() and TimeoutWithResult()**  
Task extension method that provide for timing out Task execution. Two overloads one that returns true/false and one that returns a value (similar to `Task.WaitAsync()` new in .NET 8.0 but not available prior).

* **VersionUtils.FormatVersion()**  
A generic and configurable string formatter that makes it easier to display version strings more consistently by specifying the number of tokens to display and which `.0` tokens to trim (and leave).

* **Fix: UrlEncodingParser Null Value Handling**  
Fix null values in the query collection from blowing up the parser and string output. Null values explicitly assigned or missing values now return `string.Empty` - no more null returns.


### 5.0 

**Breaking Changes** 

* **New Westwind.utilities.Windows Package**  
Added a new package that moves the `WindowsUtils` class to the new package. Also a copied version of `HtmlUtils` has moved to this pacakge as it includes `System.Web` references. Several legacy, System.Web specific methods have been removed from `HtmlUtils` in the core library and now only exist in the Windows library:  
  * HtmlUtils.ResolveUrl()
  * HtmlUtils.ImageRef()
  * HtmlUtils.Href()

* **Westwind.Utilities.Data now uses `Microsoft.Data.SqlClient` for NETFX**  
Switched over for both the netStandard and net472 targets to use `Microsoft.Data.SqlClient` for consistency. This will increase the size of distribution but it allows for support for latest SQL features.

* **Removed Westwind Logging**  
The old legacy logging library has been removed as it was severely data and introduced nasty dependencies. Apps that depend on it can stick to 4.x versions or move to some more modern logging abstraction.

### 4.0.21

* **StringUtils.GetLastCharacters()**  
Retrieves up to the last n characters from the end of a string.

* **WindowsUtils.IsUserAnAdmin()**  
Determines if user is an Administrator on Windows.

* **StringUtils.ToBase64String()/FromBase64String()**  
Helper method that simplifies converting strings to and from Base64 without having to go through the intermediary byte conversion explicitly.

* **Fix: HttpClientUtils HasErrors Result**  
Fix `HttpClientRequestSettings.HasErrors` when requests hard fail due to network issues. 

### 4.0

* **Refactor Data Utilities into separate Westwind.Utilities.Data Library**   
Due to the large required SQL Data dependencies on .NET Core, we've removed the data access components from this library and separated them out into a `Westwind.Utilities.Data`. Functionality remains the same, but now requires adding this second library in order to use the data features. This keeps the original library dependencies small even on .NET Standard/Core.

* **StringUtils.Occurs()**  
Counts the number of times a character or substring occurs in a given string.

* **StringUtils.StringToStream() and StringUtils.StreamToString()**  
Converts a string to a `MemoryStream` with Encoding, or reads a string from an input stream.

* **StringUtils.GetMaxCharacters()**  
String extension method that retrieves a string with n characters max optionally from a specified start position.

* **Fix StringSerializer Null Values**  
Fix null values for string properties that were returning the string `"null"` instead of an actual `null`.

* **Fix: AppConfiguration.Write() if Provider is not configured**  
Fix `Write()` method when the provider is not configured yet by loading Provider via `OnCreateDefaultProvider()` rather than doing a full initialize which reloaded the config data. This preserves the data and only loads the provider and was the cause of occasional failures if assigning a new configuration object completely.

* **Remove .NET 4.0 Target**  
We've removed the .NET 4.0 target, leaving `net462` and `netstandard2.0` as the two targets for this library.

* **Update Newtonsoft.Json**  
Update to latest JSON.NET Nuget package.

* **ReflectionUtils.ShallowClone()**  
Helper method to clone an object using shallow value cloning. Means: Single level clone only. Value types are cloned, References are just attached as is. Basically calls private `MemberwiseClone()` method on the source object.

* **FileUtils.GetShortPath()**  
Returns a short path that uses 8.3 character file syntax. This allows shortening long paths that exceed `MAX_PATH` to work with APIs that don't support long file or extended path syntax.

* **TimeUtils.IsBetween**  
Add helper extension method to DateTime and TimeSpan for checking for date between a high and low date.

* **Fix TimeUtils ShortDate Formatting**  
Make short date formatting locale aware. Add optional date separator parameter.

* **AsyncUtils.FireAndForget() / Task.FireAndForget()**  
Added a couple of extension methods that allow for safe execution of async methods by explicitly continuing the task if an exception occurs. Works around hte issue that exceptions can otherwise wait around to be fired until the finalizer runs which can cause unexpected failures.

* **JsonSerializationUtils CamelCase Support for Serialization**  
You can now specify an additional optional parameter `camelCase` on the `Serialize()` and `serializeToFile()` methods that applies camelCasing to JSON output, instead of the exact case output used by default.

* **StreamExtensions.AsString()**  
Returns the contents of a stream as a string with optional encoding. 

* **MemoryStreamExtensions.AsString()**  
Helper extension method that lets you convert a memory stream to a string with optional Encoding.

### 3.0.55
<small>March 12, 2021</small>

* **NetworkUtils.IsLocalIpAddress**  
Checks to see if an IP Address is a local address by checking for localhost/loopback and checking host IP (if valid) against local IP Addresses.

* **Fix: Replace Timeout with TimeoutMs**  
Replace Timeout property with TimeoutMs to fix problems with sub-1000ms request timeouts. Conversions from calling apps often resulted in invalid values being assigned due to the conversion from milliseconds to seconds. Existing Timeout property (in seconds) is marked `[Obsolete]` but still present to use.

* **ValidationErrors.HasErrors Property**   
Added a `HasErrors` property to make the check for errors more explicit than `ValidationErrors.Count > 0`.

### 3.0.51
<small>February 17, 2021</small>

* **Switch Data Access to Microsoft.Data.SqlClient**  
Switch the Sql Client library to the new `Microsoft.Data.SqlClient` for .NET Standard and .NET Standard. The full framework libs continue using `System.Data.SqlClient` native in the framework `System.Data` assembly.

* **Add HttpUtils.DownloadImageToFile\Async()**  
Added method to download images from a URL and save them to file. Fixes up the file extension depending on the mime type of the Web request.

* **ImageUtils.GetExtensionFromMediaType()**  
This method retrieves a file extension based on a media/content type. Commonly needed if you're downloading images from the Web to determine what type of file needs to be created.

* **HttpUtils.DownloadBytes\Async() for Binary data**  
Http helper to download HTTP contents into a `byte[]`.

* **ComObject Wrapper for .NET Core**  
Added `ComObject` class that allows wrapping COM objects in .NET Core so they work with late binding since .NET Core doesn't support `dynamic` to access COM objects. This class implements `DynamicObject` and retrieves missing member data via Reflection simplifying COM access in .NET Core.

* **Fix issues with StringUtils.TextAbstract() & Line Breaks**  
Fix behavior of text abstract with line breaks not being turned into spaces consistently. Also check for null.

* **ShellUtils.OpenUrl() - Platform agnostic Browser Opening**  
This method opens a URL in the system browser on Windows, Mac and Linux.  

* **WindowsUtils.TryGetRegistryKey() Signature Change**  
Change the signature to TryGetRegistryKey() to pass in the base key into the optional `baseKey` parameter. The previously used `bool` value was not effective if something other than HKLM or HKCU was used. 
Note: this is a potentially breaking change.

* **FileUtils.HasInvalidPathCharacters()**  
Added a function that checks paths for invalid characters. Uses default invalid character list, plus you can pass in additional characters to be deemed invalid.

> ### Breaking Changes for 3.0.50
> * **WindowUtilities.TryGetRegistryKey() parameter Change**  
The baseKey parameter replaces a `bool` parameter. Signature for most scenarios will stay the same if the parameter was omitted but the new version breaks binary compatibility which a simple recompile should fix.
>
> * **Renamed `DataAccessBase.Find<T>()` KeyLookup overload**  
> Renamed this method to `FindKey<T>()` to avoid ambiguous reference errors. This will break compilation if the method is used for key lookups. You might also want to do a check for all `.Find()` usage to ensure it's not unintentionally firing `Find<T>()` when `FindKey<T>()` is desired.


### 3.0.40
*May 22nd, 2020*

* **ReflectionUtils.InvokeEvent()**  
Method that allows triggering of events even from outside of the host class using Reflection without requiring a wrapper `OnEvent()` handler method to force operation into the original definition's class.

* **XmlIgnore for XML Configuration**  
You can now specify `[XmlIgnore]` for properties when using XML configure to have properties ignored when serializing, deserializing. This worked for external files before, but not for config file configuration in full framework.

* **FileUtils.ShortFilename**  
Turn a Windows long filename into a short filename. Use to get around 256 MAX_PATH limitations for some operations.

* **SqlDataAccess.DoesTableExist()**  
Added a method that checks to see if a table exist in the current database.

* **DataUtils.RemoveBytes()**  
Removes a sequence of bytes from a byte array.

* **Fix: TextAbstract() remove Line Breaks**  
`TextAbstract()` now removes line breaks when creating the the abstract and replaces them with spaces.

* **Fix: FileUtils.SafeFilename trailing Spaces**  
Fix trailing spaces issue in SafeFilename, when last character is a replacement character.

* **Fix: Missing resources**  
Project conversion appears to have removed resx resources for localization resulting in empty messages for some operations.

* **Fix: Xml Logging Adapter with Empty File**  
Fix error when creating a new XML error log and appending the XML closing tag.

### 3.0.30
*August 28th, 2019*

* **Add some Async Methods to DataAccessBase**  
Add `ExecuteNonQueryAsync`, `ExecuteScalarAsync`, `InsertEntityAsync` methods. More to come in future updates. 

* **StringUtils.Right() Method**  
Add method to retrieve the rightmost characters from a string.

* **Add TextLogAdapter**  
Add a log adapter for plain text output.

* **Update: ShellUtils.ExecuteProcess with Output Capture**  
Add overload for `ShellUtils.ExecuteProcess()` that allows passing in a string that is filled with the output generated from execution of a command line tool or an optional `Action<string>` that can intercept output as it's generated and fire your own capture code.

### 3.0.28
*June 23rd, 2019*

* **DataUtils.IndexOfByteArray()**  
Small helper that finds a array of bytes or a string in an buffer of bytes and returns an index. Similar to `Span.IndexOf` but works in environments where not available, and also supports searching for decoded strings.

* **Fix: HtmlUtils.HtmlEncode()**  
Handle encoding of single quotes `'` as well as double quotes. Also marked this method as obsolete (despite the PR fix :-)) since this is handled by `System.Net.WebUtility` class now (since .NET 4.0).

* **Fix: ReflectionUtils.CallMethodExCom()**  
Fix bug when traversing object hierarchy.

* **Add .NET 4.8 to WindowsUtilities.GetDotnetVersion()**  
Add support for .NET 4.8 and also fix an errant `Debug.Writeline()` in this method.

### 3.0.24
*February 28th, 2019*

* **Fix: HtmlUtils.SanitizeHtml() for multi-line**  
Fix SanitizeHtml() function to work across line breaks for a tag.

* **XmlUtils.XmlString()**  
Create an XML encode string for elements or attributes.

* **FileUtils.DeleteFiles()**  
Added routine that deletes files in a folder based on a path spec, optionally recursively.

* **DebugUtils.GetCodeWithLineNumbers()**  
Add method that creates lines with line numbers appended. Useful for displaying source code.

* **StringUtils.Truncate()**  
Added method that truncates a string if it exceeds a certain number of characters.

### 3.0.20
*September 6th, 2018*

* **HtmlUtils.SanitizeHtml()**  
RegEx based HTML sanitation that handles the most common script injection scenarios for `<script>`,`<iframe>`,`<form>` etc. tags, `javascript:` script embeds and `onXXX` DOM element event handlers.

* **StringUtils.IndexOfNth() and .LastIndexOfNth()**  
Helper that returns an index for the nTh occurrance of a matched character in string.

* **ShellUtils.OpenInExplorer()**  
Allows opening an Explorer Window for for a folder or file in a folder. (full framework only)

* **ShellUtils.ExecuteProcess()**  
Wrapper around Process.Start() that captures exceptions and handles a few common scenarios simply including execution with timeout and presetting the Window. (full framework only)

* **ShellUtils.OpenTerminal()**  
Opens a shell window using Powershell or Command Prompt in a given pre-selected folder. (full framework only)

* **WindowsUtils.GetWindowsVersion() and GetDotnetVerision()**  
Helpers that retrieve a display version string that can be used by an application to display the Windows and .NET version in a meaningful way.

* **HttpClient.HttpVerb Property**  
Added HttpVerb property directly to the HttpClient object. This replaces the previous approach that required `CreateHttpWebRequest()` followed by setting  `client.WebRequest.Method` explicit.

* **LanguageUtils.IgnoreErrors()**   
Helper functions that allows you to execute a block of code explicitly with a wrapped around try/catch block. Two version - one that returns true or false, one that allows the operation to return a result.

* **ImageUtils.AdjustAspectRatio**   
Image helper routine that crops an image according to a new aspect ratio from the center outward. Useful for creating uniform images in uploaded files for previews. Also optionally resizes the adjusted image to a fixed width or height.

### 3.0.10
*January 9th, 2018*

* **FileUtils.AddTrailingSlash()**  
Adds a trailing OS specific slash to the end of a path if there isn't one.

* **FileUtils.ExpandPathEnvironmentVariables()**  
Method that checks for %envVar% embedded in the path and tries to evaluate the value from environment vars.

* **Fix: FileUtils.GetRelativePath()**   
Fix Uri usage with local file paths.

### 3.0.4
*November 12th, 2017*

* **DataUtils.GetSqlProviderFactory()**  
Add helper function to allow retrieving a SQL Provider factory without having to take a dependency on the assembly that contains the provider. This is provided for .NET Standard 2.0 which doesn't have `DbProviderFactories.GetFactory()` support that provides this functionality.

* **FileUtils.NormalizePath() and NormalizeDirectory()**  
Added function to normalize a path for the given platform it runs on - forward backward slashes. Mainly useful for legacy code that explicitly formatted paths to Windows formatting. NormalizeDirectory ensures a trailing path slash on a path.

* **FileUtils.GetCompactPath()**  
Added to return a filename that is trimmed in the middle with elipsis for long file names. You specify a max string length the and the method truncates accordingly.

* **FileUtils.SafeFileName() Updates**  
`SafeFileName()` now has options for the replacement character for invalid characters replaced as well (blank by default) as well as for spaces (which by default are not stripped).


### 3.0.1
*August 5th, 2017*

* **Support for .NET Core 2.0**  
Version 3.0 adds support for .NET Core 2.0. Most features of the toolkit have been carried forward, but some features like configuration using standard .NET Configuration files is not available in .NET Core. There are a few other features that are not available.

* **StringUtils.TokenizeString() and DetokenizeString()**  
Added a function that looks for a string pattern based on start and end characters, and replaces the text with numbered tokens. DetokenizeString() then can reinsert the tokens back into the string. Useful for parsing out parts of string for manipulation and then re-adding the values edited out.

* **StringUtils.GetLines() optional maxLines Parameter**  
Added an optional parameter to `GetLines()` to allow specifying the number of lines returned. Internally still all strings are parsed first, but the result retrieves only the max number of lines.

* **StringUtils.GenerateUniqueId() additional characters**
You can now add additional character to be included in the unique ID in addition to numbers and digits. This makes the string more resilient to avoid dupe values.

* **Add support for HMAC hashing in Encryption.ComputeHash()**  
HMAC provides a standardized way to introduces salted values into hashes that results in fixed length hashes are not vulnerable to length attacks. ComputeHash now exposes HMAC versions of the standard hash algorithms.

* **Add Encryption.EncryptBytes() and Encryption.DecryptBytes()**  
Added additional overloads that allow passing byte buffer for the encryption key to make it easier to work with OS data API.

* **Add SecureString Overloads to Encrypt/Decrypt Functions**   
The various implementations of Encrypt/DecryptString/Bytes now work with SecureString values for the encryption key to minimize holding unencrypted keys in memory as string for all but the immediate encrypt/decrypt operations.

* **DataUtils.DataTableToObjectList<T>**   
You can now convert a data table to a strongly typed list with this new function.

* **XmlUtils.GetXmlEnum()/GetXmlBool() and XmlUtils.GetAttributeXmlBool()**   
Added additional conversion methods to the XML helpers to facilitate retrieving values from XML documents more easily.

* **LinqUtils.FlattenTree**   
Flattens a tree type list into a flat enumarable by letting your provide the property for the children to flatten. `var topics = topicTree.FlattenTree(t=> t.Topics);`.

* **FileUtils.CopyDirectory()**  
Copies an entire directory tree to another directory. If the target exists files and folders are merged.

* **Fix: HMAC Processing in Encryption.ComputeHash()**  
HMAC hash computation was broken as salt was added to the data rather than just passed to the hash generator. Fixed.

> ### Breaking Changes for 3.0
> ##### .NET Core 2.0 Version
> * ConfigurationFileProvider for Configuration is not supported
> * We recommend you switch to JsonConfiguration
> * SqlDataAccess ctor requires that you pass in a **full** connection string or SqlProvider Factory. 
> * SqlDataAccess connection string names and provider names are no longer supported. (this may get fixed as additional .NET Core APIs are made available by Microsoft to support providers)
> * Encrypt.Encrypt()/Decrypt() use 24 bit keys where the old version for full framework uses 16 bit keys  
> The result is that encrypted values can't be duplicated currently with .NET Core implementation. This may get fixed in later updates of .NET Core as 16 bit TripleDES keys are possibly reintroduced.
> * Encryption DataProtection API methods don't work with .NET 2.0 since those are based on Windows APIs


### 2.70
*December 15th, 2016*

* **Fix binary encoding for extended characters in Encryption class**  
Binary encoding now uses UTF encoding to encrypt/decrypt strings in order to support extended characters.

* **Encryption adds support for returning binary string data as BinHex**  
You can now return binary values in BinHex format in addition to the default base64 encoded string values.

* **FileUtils.GetPhsysicalPath()**  
This function returns a given pathname with the proper casing for the file that exists on disk. If the file doesn't exist it

* **Fix Encoding in HttpUtils**  
Fix encoding bug that didn't properly manage UTF-8 encoding in uploaded JSON content.