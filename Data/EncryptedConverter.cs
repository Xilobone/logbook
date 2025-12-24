using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Logbook.Data
{   
    /// <summary>
    /// Class used to convert encrypted values
    /// </summary>
    public class EncryptedConverter : ValueConverter<string,string>
    {   
        /// <summary>
        /// Creates a new encrypted converter
        /// </summary>
        public EncryptedConverter() : base(v => EncryptionHelper.Encrypt(v)!, v => EncryptionHelper.Decrypt(v)!) {}
    }
}