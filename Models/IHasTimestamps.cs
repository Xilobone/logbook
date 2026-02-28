namespace Logbook.Models
{   
    /// <summary>
    /// Interface for models that have a CreatedAt and LastUpdated timestamp
    /// </summary>
    public interface IHasTimestamps
    {   
        /// <summary>
        /// The time at which the object was created
        /// </summary>
        DateTime CreatedAt { get; set; }

        /// <summary>
        /// The time at which the object was last modified
        /// </summary>
        DateTime LastUpdated { get; set; }
    }

}