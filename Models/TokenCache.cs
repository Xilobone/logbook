namespace Logbook.Models
{   
    /// <summary>
    /// Represents an entry of a stored token cache
    /// </summary>
    public class TokenCache
    {
        /// <summary>
        /// The Id of the cache
        /// </summary>
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// The Id of the user the cache belongs to
        /// </summary>
        public Guid UserId { get; set; } = Guid.Empty;

        /// <summary>
        /// The data of the cache
        /// </summary>
        public byte[] CacheData { get; set; } = [];

        /// <summary>
        /// The time at which the cache was last updated
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }
}