using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using Westwind.Utilities.Properties;


namespace Westwind.Utilities
{
	/// <summary>
	/// Class that provides a number of encryption helper utilities to 
    /// make it easier to create hashes and two-way encryption, create
    /// checksums and more.
	/// </summary>
	/// <remarks>
	/// For best compatibility across platforms of the Encrypt/Decrypt methods use 
	/// overloads that use the key as a byte[24] value rather than string or other sized buffers.
	/// </remarks>
	public static class Encryption
    {
        /// <summary>
        /// Replace this value with some unique key of your own
        /// Best set this in your App start up in a Static constructor
        /// </summary>
        public static string EncryptionKey = "41a3f131dd91";

		/// <summary>
		/// Global configuration property that can be overridden to
		/// set the key size used for Encrypt/Decrypt operations.
		/// Choose between 16 bytes (not recommended except for
		/// backwards compatibility) or 24 bytes (works both in
		/// NET Full and NET Core)
		/// </summary>
#if NETFULL
		public static int EncryptionKeySize = 16;  // set for full framework compatibility with previous version
#else
	    public static int EncryptionKeySize = 24;  // this is the default for .NET Core (doesn't support 16)
#endif	

		#region Two-way Encryption

		/// <summary>
		/// Encodes a stream of bytes using DES encryption with a pass key. Lowest level method that 
		/// handles all work.
		/// </summary>
		/// <param name="inputBytes"></param>
		/// <param name="encryptionKey"></param>
		/// <returns></returns>
		public static byte[] EncryptBytes(byte[] inputBytes, string encryptionKey)
        {
            if (encryptionKey == null)
                encryptionKey = Encryption.EncryptionKey;

            return EncryptBytes(inputBytes, Encoding.UTF8.GetBytes(encryptionKey));
        }


        /// <summary>
        /// Encodes a stream of bytes using DES encryption with a pass key. Lowest level method that 
        /// handles all work.
        /// </summary>
        /// <param name="inputBytes"></param>
        /// <param name="encryptionKey"></param>
        /// <returns></returns>
        public static byte[] EncryptBytes(byte[] inputBytes, SecureString encryptionKey)
        {
            if (encryptionKey == null)
                throw new ArgumentException(Resources.MissingEncryptionKey);

            return EncryptBytes(inputBytes, Encoding.UTF8.GetBytes(encryptionKey.GetString()));
        }


        /// <summary>
        /// Encrypts a byte buffer with a byte encryption key.
        /// </summary>
        /// <param name="inputBytes">Bytes to convert</param>
        /// <param name="encryptionKey">The key bytes used to encode the data. Use a 24 byte key for best compatibility</param>
		/// <param name="cipherMode">Optional CipherMode used. Defaults to older ECB for backwards compatibility</param>
        /// <returns></returns>
		public static byte[] EncryptBytes(byte[] inputBytes, byte[] encryptionKey, CipherMode cipherMode = CipherMode.ECB)
		{
            using (var des = TripleDES.Create()) //new TripleDESCryptoServiceProvider();
            {
                des.Mode = cipherMode;

                if (EncryptionKeySize == 16)
                {
                    using (var hash = MD5.Create())
                    {
                        des.Key = hash.ComputeHash(encryptionKey);
                    }
                }
                else
                {
                    using (var hash =   SHA256.Create())
                    {
                        des.Key = hash.ComputeHash(encryptionKey)
                            .Take(EncryptionKeySize)
                            .ToArray();
                    }
                }
		
                var transform = des.CreateEncryptor();
                byte[] buffer = inputBytes;
                return transform.TransformFinalBlock(buffer, 0, buffer.Length);
            }
		}

        
        /// <summary>
        /// Encrypts a string into bytes using DES encryption with a Passkey. 
        /// </summary>
        /// <param name="inputString"></param>
        /// <param name="encryptionKey"></param>
        /// <returns></returns>
        public static byte[] EncryptBytes(string inputString, string encryptionKey)
        {
            return EncryptBytes(Encoding.UTF8.GetBytes(inputString), encryptionKey);
        }

        /// <summary>
        /// Encrypts a string using Triple DES encryption with a two way encryption key.String is returned as Base64 or BinHex
        /// encoded value rather than binary.
        /// </summary>
        /// <param name="inputString"></param>
        /// <param name="encryptionKey"></param>
        /// <param name="useBinHex">if true returns bin hex rather than base64</param>
        /// <returns></returns>
        public static string EncryptString(string inputString, byte[] encryptionKey, bool useBinHex = false)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(inputString);

            if (useBinHex)
                return BinaryToBinHex(EncryptBytes(bytes, encryptionKey));

            return Convert.ToBase64String(EncryptBytes(bytes, encryptionKey));
        }

        /// <summary>
        /// Encrypts a string using Triple DES encryption with a two way encryption key.
		/// String is returned as Base64 or BinHex encoded string.
        /// </summary>
        /// <param name="inputString"></param>
        /// <param name="encryptionKey"></param>
        /// <param name="useBinHex"></param>
        /// <returns></returns>
        public static string EncryptString(string inputString, string encryptionKey, bool useBinHex = false)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(inputString);

            if (useBinHex)
                return BinaryToBinHex(EncryptBytes(bytes, encryptionKey));

            return Convert.ToBase64String(EncryptBytes(bytes, encryptionKey));
        }


        /// <summary>
        /// Encrypts a string using Triple DES encryption with a two way encryption key.
        /// String is returned as Base64 or BinHex encoded string.
        /// </summary>
        /// <param name="inputString"></param>
        /// <param name="encryptionKey"></param>
        /// <param name="useBinHex"></param>
        /// <returns></returns>
        public static string EncryptString(string inputString,SecureString encryptionKey, bool useBinHex = false)
        {
            if (encryptionKey == null)
                throw new ArgumentException(Resources.MissingEncryptionKey);

            byte[] bytes = Encoding.UTF8.GetBytes(inputString);

            if (useBinHex)
                return BinaryToBinHex(EncryptBytes(bytes, encryptionKey.GetString()));

            return Convert.ToBase64String(EncryptBytes(bytes, encryptionKey.GetString()));
        }


        /// <summary>
        /// Decrypts a Byte array from DES with an Encryption Key.
        /// </summary>
        /// <param name="decryptBuffer"></param>
        /// <param name="encryptionKey"></param>
        /// <returns></returns>
        public static byte[] DecryptBytes(byte[] decryptBuffer, string encryptionKey)
        {
            if (decryptBuffer == null || decryptBuffer.Length == 0)
                return null;

            if (encryptionKey == null)
                encryptionKey = Encryption.EncryptionKey;

            return DecryptBytes(decryptBuffer, Encoding.UTF8.GetBytes(encryptionKey));            
        }



        /// <summary>
        /// Decrypts a Byte array from DES with an Encryption Key.
        /// </summary>
        /// <param name="decryptBuffer"></param>
        /// <param name="encryptionKey"></param>
        /// <returns></returns>
        public static byte[] DecryptBytes(byte[] decryptBuffer, SecureString encryptionKey)
        {
            if (decryptBuffer == null || decryptBuffer.Length == 0)
                return null;

            return DecryptBytes(decryptBuffer, Encoding.UTF8.GetBytes(encryptionKey.GetString()));
        }



        /// <summary>
        /// Decrypts a byte buffer with a byte based encryption key
        /// </summary>
        /// <param name="decryptBuffer">Data to encrypt</param>
        /// <param name="encryptionKey">The key bytes used to encode the data. Use a 24 byte key for best compatibility</param>
        /// <param name="cipherMode">Optional CipherMode used. Defaults to older ECB for backwards compatibility</param>
        /// <returns></returns>
        public static byte[] DecryptBytes(byte[] decryptBuffer, byte[] encryptionKey, CipherMode cipherMode = CipherMode.ECB)
        {
            if (decryptBuffer == null || decryptBuffer.Length == 0)
                return null;

            using (var des = TripleDES.Create())
            {
                des.Mode = cipherMode;
			
                if (EncryptionKeySize == 16)
                {
                    using (var hash = MD5.Create())
                    {
                        des.Key = hash.ComputeHash(encryptionKey);
                    }
                }
                else
                {
                    using (var hash = SHA256.Create())
                    {
                        des.Key = hash.ComputeHash(encryptionKey)
                            .Take(EncryptionKeySize)
                            .ToArray();
                    }
                }

                var transform = des.CreateDecryptor();
                return transform.TransformFinalBlock(decryptBuffer, 0, decryptBuffer.Length);
            }

           
        }

        /// <summary>
        /// Decrypts a string using DES encryption and a pass key that was used for 
        /// encryption and returns a byte buffer.    
        /// </summary>
        /// <param name="decryptString"></param>
        /// <param name="encryptionKey"></param>
        /// <param name="useBinHex">Returns data in useBinHex format (12afb1c3f1). Otherwise base64 is returned.</param>
        /// <returns>String</returns>
        public static byte[] DecryptBytes(string decryptString, string encryptionKey, bool useBinHex = false)
        {
            if (useBinHex)
                return DecryptBytes(BinHexToBinary(decryptString), encryptionKey);

            return DecryptBytes(Convert.FromBase64String(decryptString), encryptionKey);
        }


        /// <summary>
        /// Decrypts a string using DES encryption and a pass key that was used for 
        /// encryption and returns a byte buffer.    
        /// </summary>
        /// <param name="decryptString"></param>
        /// <param name="encryptionKey"></param>
        /// <param name="useBinHex">Returns data in useBinHex format (12afb1c3f1). Otherwise base64 is returned.</param>
        /// <returns>String</returns>
        public static byte[] DecryptBytes(string decryptString, SecureString encryptionKey, bool useBinHex = false)
        {
            if (encryptionKey == null)
                throw new ArgumentException(Resources.MissingEncryptionKey);

            if (useBinHex)
                return DecryptBytes(BinHexToBinary(decryptString), encryptionKey.GetString());

            return DecryptBytes(Convert.FromBase64String(decryptString), encryptionKey.GetString());
        }

        /// <summary>
        /// Decrypts a string using DES encryption and a pass key that was used for 
        /// encryption.
        /// <seealso>Class wwEncrypt</seealso>
        /// </summary>
        /// <param name="decryptString"></param>
        /// <param name="encryptionKey"></param>
        /// <param name="useBinHex">Returns data in useBinHex format (12afb1c3f1). Otherwise base64 is returned.</param>
        /// <returns>String</returns>
        public static string DecryptString(string decryptString, string encryptionKey,  bool useBinHex = false)
        {
            try
            {
                var data = useBinHex ? BinHexToBinary(decryptString) : Convert.FromBase64String(decryptString);

                byte[] decrypted = DecryptBytes(data, encryptionKey);
                return Encoding.UTF8.GetString(decrypted);                
            }
            catch
            {
                // undecryptable data
                return string.Empty;
            }
        }

        /// <summary>
        /// Decrypts a string using DES encryption and a pass key that was used for 
        /// encryption.
        /// <seealso>Class wwEncrypt</seealso>
        /// </summary>
        /// <param name="decryptString"></param>
        /// <param name="encryptionKey"></param>
        /// <param name="useBinHex">Returns data in useBinHex format (12afb1c3f1). Otherwise base64 is returned.</param>
        /// <returns>String</returns>
        public static string DecryptString(string decryptString, SecureString encryptionKey, bool useBinHex = false)
        {
            if (encryptionKey == null)
                throw new ArgumentException(Resources.MissingEncryptionKey);

            var data = useBinHex ? BinHexToBinary(decryptString) : Convert.FromBase64String(decryptString);

            try
            {
                byte[] decrypted = DecryptBytes(data, encryptionKey.GetString());
                return Encoding.UTF8.GetString(decrypted);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Decrypts a string using DES encryption and a pass key that was used for 
        /// encryption.
        /// <seealso>Class wwEncrypt</seealso>
        /// </summary>
        /// <param name="decryptString"></param>
        /// <param name="encryptionKey"></param>
        /// <param name="useBinHex">Returns data in useBinHex format (12afb1c3f1). Otherwise base64 is returned</param>
        /// <returns>String</returns>
        public static string DecryptString(string decryptString, byte[] encryptionKey, bool useBinHex = false)
        {
            var data = useBinHex ? BinHexToBinary(decryptString) : Convert.FromBase64String(decryptString);

            try
            {
                byte[] decrypted = DecryptBytes(data, encryptionKey);
                return Encoding.UTF8.GetString(decrypted);
            }
            catch
            {
                return string.Empty;
            }
        }


      
#if NETFULL
		/// <summary>
		/// Encrypt bytes using the Data Protection API on Windows. This API
		/// uses internal keys to encrypt data which is valid for decryption only
		/// on the same machine.        
		/// 
		/// This is an idea storage mechanism for application registraions, 
		/// service passwords and other semi-transient data that is specific
		/// to the software used on the current machine
		/// </summary>
		/// <remarks>
		/// DO NOT USE FOR DATA THAT WILL CROSS MACHINE BOUNDARIES
		/// </remarks>
		/// <param name="encryptBytes"></param>
		/// <param name="key"></param>
		/// <param name="scope"></param>
		/// <returns></returns>
		public static byte[] ProtectBytes(byte[] encryptBytes, byte[] key, DataProtectionScope scope = DataProtectionScope.LocalMachine)
        {
            return ProtectedData.Protect(encryptBytes,key,scope);            
        }

        /// <summary>
        /// Encrypt bytes using the Data Protection API on Windows. This API
        /// uses internal keys to encrypt data which is valid for decryption only
        /// on the same machine.        
        /// 
        /// This is an idea storage mechanism for application registraions, 
        /// service passwords and other semi-transient data that is specific
        /// to the software used on the current machine
        /// </summary>
        /// <remarks>
        /// DO NOT USE FOR DATA THAT WILL CROSS MACHINE BOUNDARIES
        /// </remarks>
        /// <param name="encryptBytes"></param>
        /// <param name="key"></param>     
        /// <param name="scope"></param>   
        /// <returns></returns>
        public static byte[] ProtectBytes(byte[] encryptBytes, string key, DataProtectionScope scope = DataProtectionScope.LocalMachine)
        {
            return ProtectedData.Protect(encryptBytes, Encoding.UTF8.GetBytes(key),scope);
        }

        /// <summary>
        /// Encrypt bytes using the Data Protection API on Windows. This API
        /// uses internal keys to encrypt data which is valid for decryption only
        /// on the same machine.        
        /// 
        /// This is an idea storage mechanism for application registraions, 
        /// service passwords and other semi-transient data that is specific
        /// to the software used on the current machine
        /// </summary>
        /// <remarks>
        /// DO NOT USE FOR DATA THAT WILL CROSS MACHINE BOUNDARIES
        /// </remarks>
        /// <param name="encryptString"></param>
        /// <param name="key"></param>     
        /// <param name="scope"></param>
        /// <param name="useBinHex">returns bin hex data when set (010A0D10AF)</param>
        /// <returns></returns>
        public static string ProtectString(string encryptString, string key, DataProtectionScope scope = DataProtectionScope.LocalMachine, bool useBinHex = false)
        {
            var encryptedBytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(encryptString), Encoding.UTF8.GetBytes(key), scope);

            if (useBinHex)
                return BinaryToBinHex(encryptedBytes);

            return Convert.ToBase64String(encryptedBytes);
        }

        /// <summary>
        /// Encrypt bytes using the Data Protection API on Windows. This API
        /// uses internal keys to encrypt data which is valid for decryption only
        /// on the same machine.        
        /// 
        /// This is an idea storage mechanism for application registraions, 
        /// service passwords and other semi-transient data that is specific
        /// to the software used on the current machine
        /// </summary>
        /// <remarks>
        /// DO NOT USE FOR DATA THAT WILL CROSS MACHINE BOUNDARIES
        /// </remarks>
        /// <param name="encryptString"></param>
        /// <param name="key"></param>     
        /// <param name="scope"></param>
        /// <param name="useBinHex">returns bin hex data when set (010A0D10AF)</param>
        /// <returns></returns>
        public static string ProtectString(string encryptString, byte[] key, DataProtectionScope scope = DataProtectionScope.LocalMachine, bool useBinHex = false)
        {
            var encryptedBytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(encryptString), key, scope);

            if (useBinHex)
                return BinaryToBinHex(encryptedBytes);

            return Convert.ToBase64String(encryptedBytes);
        }

        /// <summary>
        /// Decrypts bytes using the Data Protection API on Windows. This API
        /// uses internal keys to encrypt data which is valid for decryption only
        /// on the same machine.        
        /// 
        /// This is an idea storage mechanism for application registraions, 
        /// service passwords and other semi-transient data that is specific
        /// to the software used on the current machine
        /// </summary>
        /// <remarks>
        /// DO NOT USE FOR DATA THAT WILL CROSS MACHINE BOUNDARIES
        /// </remarks>
        /// <param name="encryptBytes"></param>
        /// <param name="key"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        public static byte[] UnprotectBytes(byte[] encryptBytes, byte[] key, DataProtectionScope scope = DataProtectionScope.LocalMachine)
        {
            return ProtectedData.Unprotect(encryptBytes, key, scope);
        }

        /// <summary>
        /// Encrypt bytes using the Data Protection API on Windows. This API
        /// uses internal keys to encrypt data which is valid for decryption only
        /// on the same machine.        
        /// 
        /// This is an idea storage mechanism for application registraions, 
        /// service passwords and other semi-transient data that is specific
        /// to the software used on the current machine
        /// </summary>
        /// <remarks>
        /// DO NOT USE FOR DATA THAT WILL CROSS MACHINE BOUNDARIES
        /// </remarks>
        /// <param name="encryptBytes"></param>
        /// <param name="key"></param>     
        /// <param name="scope"></param>   
        /// <returns></returns>
        public static byte[] UnprotectBytes(byte[] encryptBytes, string key, DataProtectionScope scope = DataProtectionScope.LocalMachine)
        {
            return ProtectedData.Unprotect(encryptBytes, Encoding.UTF8.GetBytes(key), scope);
        }

        /// <summary>
        /// Encrypt bytes using the Data Protection API on Windows. This API
        /// uses internal keys to encrypt data which is valid for decryption only
        /// on the same machine.        
        /// 
        /// This is an idea storage mechanism for application registraions, 
        /// service passwords and other semi-transient data that is specific
        /// to the software used on the current machine
        /// </summary>
        /// <remarks>
        /// DO NOT USE FOR DATA THAT WILL CROSS MACHINE BOUNDARIES
        /// </remarks>
        /// <param name="encryptString"></param>
        /// <param name="key"></param>     
        /// <param name="scope"></param>
        /// <param name="useBinHex">returns bin hex data when set (010A0D10AF)</param>
        /// <returns></returns>
        public static string UnprotectString(string encryptString, string key, DataProtectionScope scope = DataProtectionScope.LocalMachine, bool useBinHex = false)
        {
            byte[] buffer;
            if (useBinHex)
                buffer = BinHexToBinary(encryptString);
            else
                buffer = Convert.FromBase64String(encryptString);

            buffer = ProtectedData.Unprotect(buffer, Encoding.UTF8.GetBytes(key), scope);

            return Encoding.UTF8.GetString(buffer);
        }

        /// <summary>
        /// Encrypt bytes using the Data Protection API on Windows. This API
        /// uses internal keys to encrypt data which is valid for decryption only
        /// on the same machine.        
        /// 
        /// This is an idea storage mechanism for application registraions, 
        /// service passwords and other semi-transient data that is specific
        /// to the software used on the current machine
        /// </summary>
        /// <remarks>
        /// DO NOT USE FOR DATA THAT WILL CROSS MACHINE BOUNDARIES
        /// </remarks>
        /// <param name="encryptString"></param>
        /// <param name="key"></param>     
        /// <param name="scope"></param>
        /// <param name="useBinHex">returns bin hex data when set (010A0D10AF)</param>
        /// <returns></returns>
        public static string UnprotectString(string encryptString, byte[] key, DataProtectionScope scope = DataProtectionScope.LocalMachine, bool useBinHex = false)
        {
            byte[] buffer;
            if (useBinHex)
                buffer = BinHexToBinary(encryptString);
            else
                buffer = Convert.FromBase64String(encryptString);

            buffer = ProtectedData.Unprotect(buffer, key, scope);

            return Encoding.UTF8.GetString(buffer);        
        }
#endif

        #endregion


        #region Hashes

        /// <summary>
        /// Generates a hash for the given plain text value and returns a
        /// base64-encoded result. Before the hash is computed, a random salt
        /// is generated and appended to the plain text. This salt is stored at
        /// the end of the hash value, so it can be used later for hash
        /// verification.
        /// </summary>
        /// <param name="plainText">
        /// Plaintext value to be hashed. 
        /// </param>
        /// <param name="hashAlgorithm">
        /// Name of the hash algorithm. Allowed values are: "MD5", "SHA1",
        /// "SHA256", "SHA384", "SHA512", "HMACMD5", "HMACSHA1", "HMACSHA256",
        ///  "HMACSHA512" (if any other value is specified  MD5 will be used). 
        /// 
        /// HMAC algorithms uses Hash-based Message Authentication Code.
        /// The HMAC process mixes a secret key with the message data, hashes 
        /// the result with the hash function, mixes that hash value with 
        /// the secret key again, and then applies the hash function
        /// a second time. HMAC hashes are fixed lenght and generally
        /// much longer than non-HMAC hashes of the same type.
        /// 
        /// https://msdn.microsoft.com/en-us/library/system.security.cryptography.hmacsha256(v=vs.110).aspx      
        /// 
        /// This value is case-insensitive.
        /// </param>
        /// <param name="salt">
        /// Optional but recommended salt string to apply to the hash. If not passed the
        /// raw encoding is used. If salt is nullthe raw algorithm is used (useful for 
        /// file hashes etc.) HMAC versions REQUIRE that salt is passed.
        /// </param>
        /// <param name="useBinHex">if true returns the data as BinHex byte pair string. Otherwise Base64 is returned.</param>
        /// <returns>
        /// Hash value formatted as a base64-encoded or BinHex stringstring.
        /// </returns>
        public static string ComputeHash(string plainText,
                                         string hashAlgorithm,
                                         byte[] saltBytes, 
                                         bool useBinHex = false)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            return ComputeHash(Encoding.UTF8.GetBytes(plainText), hashAlgorithm, saltBytes, useBinHex);
        }

        /// <summary>
        /// Generates a hash for the given plain text value and returns a
        /// base64-encoded result. Before the hash is computed, a random salt
        /// is generated and appended to the plain text. This salt is stored at
        /// the end of the hash value, so it can be used later for hash
        /// verification.
        /// </summary>
        /// <param name="plainText">
        /// Plaintext value to be hashed. 
        /// </param>
        /// <param name="hashAlgorithm">
        /// Name of the hash algorithm. Allowed values are: "MD5", "SHA1",
        /// "SHA256", "SHA384", "SHA512", "HMACMD5", "HMACSHA1", "HMACSHA256",
        ///  "HMACSHA512" (if any other value is specified  MD5 will be used). 
        /// 
        /// HMAC algorithms uses Hash-based Message Authentication Code.
        /// The HMAC process mixes a secret key with the message data, hashes 
        /// the result with the hash function, mixes that hash value with 
        /// the secret key again, and then applies the hash function
        /// a second time. HMAC hashes are fixed lenght and generally
        /// much longer than non-HMAC hashes of the same type.
        /// 
        /// https://msdn.microsoft.com/en-us/library/system.security.cryptography.hmacsha256(v=vs.110).aspx      
        /// 
        /// This value is case-insensitive.
        /// </param>
        /// <param name="salt">
        /// Optional but recommended salt string to apply to the hash. If not passed the
        /// raw encoding is used. If salt is nullthe raw algorithm is used (useful for 
        /// file hashes etc.) HMAC versions REQUIRE that salt is passed.
        /// </param>
        /// <param name="useBinHex">if true returns the data as BinHex byte pair string. Otherwise Base64 is returned.</param>
        /// <returns>
        /// Hash value formatted as a base64-encoded or BinHex stringstring.
        /// </returns>
        public static string ComputeHash(string plainText,
                                         string hashAlgorithm,
                                         string salt,
                                         bool useBinHex = false)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            return ComputeHash(Encoding.UTF8.GetBytes(plainText), hashAlgorithm, Encoding.UTF8.GetBytes(salt), useBinHex);
        }

        /// <summary>
        /// Generates a hash for the given plain text value and returns a
        /// base64-encoded result. Before the hash is computed, a random salt
        /// is generated and appended to the plain text. This salt is stored at
        /// the end of the hash value, so it can be used later for hash
        /// verification.
        /// </summary>
        /// <param name="byteData">
        /// Plaintext value to be hashed. 
        /// </param>
        /// <param name="hashAlgorithm">
        /// Name of the hash algorithm. Allowed values are: "MD5", "SHA1",
        /// "SHA256", "SHA384", "SHA512", "HMACMD5", "HMACSHA1", "HMACSHA256",
        ///  "HMACSHA512" (if any other value is specified  MD5 will be used). 
        /// 
        /// HMAC algorithms uses Hash-based Message Authentication Code.
        /// The HMAC process mixes a secret key with the message data, hashes 
        /// the result with the hash function, mixes that hash value with 
        /// the secret key again, and then applies the hash function
        /// a second time. HMAC hashes are fixed lenght and generally
        /// much longer than non-HMAC hashes of the same type.
        /// 
        /// https://msdn.microsoft.com/en-us/library/system.security.cryptography.hmacsha256(v=vs.110).aspx      
        /// 
        /// This value is case-insensitive.
        /// </param>
        /// <param name="saltBytes">
        /// Optional but recommended salt bytes to apply to the hash. If not passed the
        /// raw encoding is used. If salt is nullthe raw algorithm is used (useful for 
        /// file hashes etc.) HMAC versions REQUIRE that salt is passed.
        /// </param>
        /// <param name="useBinHex">if true returns the data as BinHex byte pair string. Otherwise Base64 is returned.</param>
        /// <returns>
        /// Hash value formatted as a base64-encoded or BinHex stringstring.
        /// </returns>
        public static string ComputeHash(byte[] byteData,
                                         string hashAlgorithm,
                                         byte[] saltBytes,
                                         bool useBinHex = false)
        {
            if (byteData == null)
                return null;

            // Convert plain text into a byte array.            
            byte[] plainTextWithSaltBytes;

            if (saltBytes != null && !hashAlgorithm.StartsWith("HMAC"))
            {
                // Allocate array, which will hold plain text and salt.
                plainTextWithSaltBytes =
                    new byte[byteData.Length + saltBytes.Length];

                // Copy plain text bytes into resulting array.
                for (int i = 0; i < byteData.Length; i++)
                    plainTextWithSaltBytes[i] = byteData[i];

                // Append salt bytes to the resulting array.
                for (int i = 0; i < saltBytes.Length; i++)
                    plainTextWithSaltBytes[byteData.Length + i] = saltBytes[i];
            }
            else
                plainTextWithSaltBytes = byteData;

            HashAlgorithm hash;

            // Make sure hashing algorithm name is specified.
            if (hashAlgorithm == null)
                hashAlgorithm = "";

            // Initialize appropriate hashing algorithm class.
            switch (hashAlgorithm.ToUpper())
            {
                case "SHA1":
                    hash = SHA1.Create();
                    break;
                case "SHA256":
                    hash = SHA256.Create();
                    break;
                case "SHA384":
                    hash = SHA384.Create();
                    break;
                case "SHA512":
                    hash = SHA512.Create();
                    break;
                case "HMACMD5":
                    hash = new HMACMD5(saltBytes);
                    break;
                case "HMACSHA1":
                    hash = new HMACSHA1(saltBytes);
                    break;
                case "HMACSHA256":
                    hash = new HMACSHA256(saltBytes);                    
                    break;
                case "HMACSHA512":
                    hash = new HMACSHA512(saltBytes);
                    break;
                default:
                    // default to MD5
                    hash = MD5.Create();
                    break;
            }
            using (hash)
            {
                byte[] hashBytes = hash.ComputeHash(plainTextWithSaltBytes);

                if (useBinHex)
                    return BinaryToBinHex(hashBytes);

                return Convert.ToBase64String(hashBytes);
            }
        }
#endregion

#region Gzip

        /// <summary>
        /// GZip encodes a memory buffer to a compressed memory buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] GZipMemory(byte[] buffer)
        {
            MemoryStream ms = new MemoryStream();

            GZipStream gZip = new GZipStream(ms, CompressionMode.Compress);

            gZip.Write(buffer, 0, buffer.Length);
            gZip.Close();

            byte[] result = ms.ToArray();
            ms.Close();

            return result;
        }

        /// <summary>
        /// Encodes a string to a gzip compressed memory buffer
        /// </summary>
        /// <param name="Input"></param>
        /// <returns></returns>
        public static byte[] GZipMemory(string Input)
        {
            return GZipMemory(Encoding.UTF8.GetBytes(Input));
        }

        /// <summary>
        /// Encodes a file to a gzip memory buffer
        /// </summary>
        /// <param name="Filename"></param>
        /// <param name="IsFile"></param>
        /// <returns></returns>
        public static byte[] GZipMemory(string Filename, bool IsFile)
        {
            string InputFile = Filename;
            byte[] Buffer = File.ReadAllBytes(Filename);
            return GZipMemory(Buffer);
        }

        /// <summary>
        /// Encodes one file to another file that is gzip compressed.
        /// File is overwritten if it exists and not locked.
        /// </summary>
        /// <param name="Filename"></param>
        /// <param name="OutputFile"></param>
        /// <returns></returns>
        public static bool GZipFile(string Filename, string OutputFile)
        {
            string InputFile = Filename;
            byte[] Buffer = File.ReadAllBytes(Filename);
            FileStream fs = new FileStream(OutputFile, FileMode.OpenOrCreate, FileAccess.Write);
            GZipStream GZip = new GZipStream(fs, CompressionMode.Compress);
            GZip.Write(Buffer, 0, Buffer.Length);
            GZip.Close();
            fs.Close();

            return true;
        }

#endregion

#region CheckSum

        /// <summary>
        /// Creates an SHA256 or MD5 checksum of a file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="mode">SHA256,SHA512,MD5</param>
        /// <returns>A binhex string</returns>
        public static string GetChecksumFromFile(string file, string mode = "SHA256")
        {
            if (!File.Exists(file))
                return null;

            try
            {
                using (FileStream stream = File.OpenRead(file))
                {
                    if (mode == "SHA256")
                    {
                        byte[] checksum;
                        using (var sha = SHA256.Create())
                        {
                            checksum = sha.ComputeHash(stream);
                        }

                        return BinaryToBinHex(checksum);
                    }
                    if (mode == "SHA512")
                    {
                        byte[] checksum;
                        using (var sha = SHA512.Create())
                        {
                            checksum = sha.ComputeHash(stream);
                        }

                        return BinaryToBinHex(checksum);
                    }
                    if (mode == "MD5")
                    {
                        byte[] checkSum;
                        using (var md = MD5.Create())
                        {
                            checkSum = md.ComputeHash(stream);
                        }

                        return BinaryToBinHex(checkSum);
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        /// <summary>
        /// Create a SHA256 or MD5 checksum from a bunch of bytes
        /// </summary>
        /// <param name="fileData"></param>
        /// <param name="mode">SHA256,SHA512,MD5</param>
        /// <returns></returns>
        public static string GetChecksumFromBytes(byte[] fileData, string mode = "SHA256")
        {
            using (MemoryStream stream = new MemoryStream(fileData))
            {
                if (mode == "SHA256")
                {
                    byte[] checksum;
                    using (var sha =  SHA256.Create())
                    {
                        checksum = sha.ComputeHash(stream);
                    }

                    return BinaryToBinHex(checksum);
                }
                if (mode == "SHA512")
                {
                    byte[] checksum;
                    using (var sha = SHA512.Create())
                    {
                        checksum = sha.ComputeHash(stream);
                    }

                    return BinaryToBinHex(checksum);
                }
                if (mode == "MD5")
                {
                    byte[] checkSum;
                    using (var md = MD5.Create())
                    {
                        checkSum = md.ComputeHash(stream);
                    }

                    return BinaryToBinHex(checkSum);
                }
            }

            return null;
        }

#endregion

#region BinHex Helpers

        /// <summary>
        /// Converts a byte array into a BinHex string.
        /// Example: 01552233 
        /// where the numbers are packed
        /// byte values.
        /// </summary>
        /// <param name="data">Raw data to send</param>
        /// <returns>string or null if input is null</returns>
        public static string BinaryToBinHex(byte[] data)
        {
            if (data == null)
                return null;

            StringBuilder sb = new StringBuilder(data.Length * 2);
            foreach (byte val in data)
            {
                sb.AppendFormat("{0:x2}", val);
            }
            return sb.ToString().ToUpper();
        }


        /// <summary>
        /// Turns a BinHex string that contains raw byte values
        /// into a byte array
        /// </summary>
        /// <param name="hex">BinHex string (011a031f) just two byte hex digits strung together)</param>
        /// <returns></returns>
        public static byte[] BinHexToBinary(string hex)
        {
            int offset = hex.StartsWith("0x") ? 2 : 0;
            if ((hex.Length % 2) != 0)
                throw new ArgumentException("Invalid String Length");

            byte[] ret = new byte[(hex.Length - offset) / 2];

            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = (byte)((ParseHexChar(hex[offset]) << 4)
                                 | ParseHexChar(hex[offset + 1]));
                offset += 2;
            }
            return ret;
        }

        static int ParseHexChar(char c)
        {
            if (c >= '0' && c <= '9')
                return c - '0';
            if (c >= 'A' && c <= 'F')
                return c - 'A' + 10;
            if (c >= 'a' && c <= 'f')
                return c - 'a' + 10;

            throw new ArgumentException("Invalid character");
        }

#endregion
    }

    internal static class SecureStringExtensions
    {
        public static string GetString(this SecureString source)
        {
            string result = null;
            int length = source.Length;
            IntPtr pointer = IntPtr.Zero;
            char[] chars = new char[length];

            try
            {
                pointer = Marshal.SecureStringToBSTR(source);
                Marshal.Copy(pointer, chars, 0, length);

                result = string.Join("", chars);
            }
            finally
            {
                if (pointer != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(pointer);
                }
            }

            return result;
        }
    }

}
