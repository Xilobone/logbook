namespace Logbook.Models
{
    public class TokenCache
    {
        public int Id { get; set; }
        public string UserId { get; set; }           // MSAL account identifier
        public byte[] CacheData { get; set; }        // Serialized token cache
        public DateTime LastUpdated { get; set; }
    }
}