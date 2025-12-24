using Microsoft.AspNetCore.DataProtection;

namespace Logbook.Data
{
    using Microsoft.AspNetCore.DataProtection;

    /// <summary>
    /// Class used to encrypt sensitive string values
    /// </summary>
    public static class EncryptionHelper
    {
        private static IDataProtector? _protector;

        /// <summary>
        /// Initializes the encryption helper
        /// </summary>
        /// <param name="provider">The data protection provider to use</param>
        public static void Init(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("SensitiveDataProtector");
        }

        /// <summary>
        /// Encrypts a string value
        /// </summary>
        /// <param name="value">The value to encrypt</param>
        /// <returns>The encrypted value</returns>
        /// <exception cref="Exception">Throws an exception when the encryptor is not initialized</exception>
        public static string? Encrypt(string value)
        {
            if (_protector == null) throw new Exception("Encryptor not initialized");
            if (value == null) return null;

            return _protector.Protect(value);
        }

        /// <summary>
        /// Decrypts a string value
        /// </summary>
        /// <param name="value">The value to decrypt</param>
        /// <returns>The decrypted value</returns>
        /// <exception cref="Exception">Throws an exception when the encryptor is not initialized</exception>
        public static string? Decrypt(string value)
        {
            if (_protector == null) throw new Exception("Encryptor not initialized");
            if (value == null) return null;

            return _protector.Unprotect(value);
        }
    }

}