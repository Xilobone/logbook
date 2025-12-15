namespace Logbook.DTO
{   
    /// <summary>
    /// Represents the data that is obtained from an incoming access token
    /// </summary>
    public class TokenCaller
    {   
        /// <summary>
        /// The id of the user that presented the token
        /// </summary>
        public Guid Id {get; set;} = Guid.Empty;

        /// <summary>
        /// The unique name of the user that presented the token
        /// </summary>
        public string UserPrincipalName {get; set;} = string.Empty;

        /// <summary>
        /// The display name of the user
        /// </summary>
        public string DisplayName {get; set; } = string.Empty;
    }
}