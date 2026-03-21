namespace Logbook.DTO
{
    /// <summary>
    /// Represents a DTO user registration
    /// </summary>
    public class Registration
    {   
        /// <summary>
        /// Whether the registration is enabled
        /// </summary>
        public bool? Enabled {get; set;}

        /// <summary>
        /// The account linked to the registration
        /// </summary>
        public string? LinkedAccount {get; set;}
    }
}