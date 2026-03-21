namespace Logbook.Models
{
    /// <summary>
    /// Represents a users registration to a Microsoft account
    /// </summary>
    public class Registration
    {

        /// <summary>
        /// The Id of the registration
        /// </summary>
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// Whether the registration is enabled or not
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// The name of the linked account
        /// </summary>
        public string LinkedAccount { get; set; } = string.Empty;

        /// <summary>
        /// The access token beloning to this registration
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// The refresh token beloning to this registration
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;


        /// <summary>
        /// Represents an empty registration
        /// </summary>
        public static Registration None { get; set; } = new();

    }
}