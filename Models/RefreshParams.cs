using System.ComponentModel.DataAnnotations;

namespace Logbook.Models
{
    /// <summary>
    /// The parameters that are passed to the refresh endpoint
    /// </summary>
    public class RefreshParams
    {
        /// <summary>
        /// The filepath of the source file on the users onedrive
        /// </summary>
        public string source { get; init; } = string.Empty;

        /// <summary>
        /// The name of the group
        /// </summary>
        [Required]
        public string group { get; init; } = default!;
    }
}