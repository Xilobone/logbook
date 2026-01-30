using System.ComponentModel.DataAnnotations;

namespace Logbook.DTO.Group
{
    /// <summary>
    /// Represents a group passed on to the user
    /// </summary>
    public class Get
    {
        /// <summary>
        /// The id of the group
        /// </summary>
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// The display name of the group
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// The id of the user that is used as the source
        /// </summary>
        public Guid SourceId { get; set; } = Guid.Empty;

        /// <summary>
        /// The file path to get the event file from the sources onedrive
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// The default start time for events from this group
        /// </summary>
        public TimeOnly StartTime { get; set; } = TimeOnly.MinValue;

        /// <summary>
        /// The default end time for events from this group
        /// </summary>
        public TimeOnly EndTime { get; set; } = TimeOnly.MinValue;

        /// <summary>
        /// The timezone the start and end times are given in
        /// </summary>
        public string TimeZone { get; set; } = string.Empty;

        /// <summary>
        /// The prefix to the event title
        /// </summary>
        public string? EventPrefix { get; set; } = string.Empty;

    }

    /// <summary>
    /// Represents a group create requested to the application
    /// </summary>
    public class Create
    {
        /// <summary>
        /// The display name of the group
        /// </summary>
        [MinLength(1)]
        [MaxLength(50)]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// The id of the user that is used as the data source for this group
        /// </summary>
        [RegularExpression("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$")]
        public string SourceId { get; set; } = string.Empty;

        /// <summary>
        /// The file path for the onedrive file on the source where the schedule can be found
        /// </summary>
        [MinLength(1)]
        [MaxLength(100)]
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// The default start time for events
        /// </summary>
        public TimeOnly StartTime { get; set; } = TimeOnly.MinValue;

        /// <summary>
        /// The default end time for events
        /// </summary>
        public TimeOnly EndTime { get; set; } = TimeOnly.MinValue;

        /// <summary>
        /// The timezone the start and end times are given in
        /// </summary>
        [MinLength(1)]
        [MaxLength(50)]
        public string TimeZone { get; set; } = string.Empty;

        /// <summary>
        /// The prefix to the event title
        /// </summary>
        [MaxLength(50)]
        public string? EventPrefix { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents the parameters passed to the group update endpoint
    /// </summary>
    public class Update
    {
        /// <summary>
        /// The display name of the group
        /// </summary>
        [MinLength(1)]
        [MaxLength(50)]
        public string? DisplayName { get; set; }

        /// <summary>
        /// The id of the user that is used as the data source for this group
        /// </summary>
        [RegularExpression("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$")]
        public string? SourceId { get; set; }

        /// <summary>
        /// The file path for the onedrive file on the source where the schedule can be found
        /// </summary>
        [MinLength(1)]
        [MaxLength(100)]
        public string? FilePath { get; set; }

        /// <summary>
        /// The default start time for events
        /// </summary>
        [RegularExpression("^\\d{2}:\\d{2}$")]
        public TimeOnly? StartTime { get; set; }

        /// <summary>
        /// The default end time for events
        /// </summary>
        [RegularExpression("^\\d{2}:\\d{2}$")]
        public TimeOnly? EndTime { get; set; }

        /// <summary>
        /// The timezone the start and end times are given in
        /// </summary>
        [MinLength(1)]
        [MaxLength(50)]
        public string? TimeZone { get; set; }

        /// <summary>
        /// The prefix to the event title
        /// </summary>
        [MaxLength(50)]
        public string? EventPrefix { get; set; }
    }

    /// <summary>
    /// Represents a group member
    /// </summary>
    public class Member
    {
        /// <summary>
        /// The id of the member
        /// </summary>
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// The display name of the member
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;
    }
    /// <summary>
    /// Parameters that are passed to the add or remove member endpoint
    /// </summary>
    public class ChangeMember
    {
        /// <summary>
        /// The id of the user of which the membership is requested to change
        /// </summary>
        [RegularExpression("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$")]
        public string MemberId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Parameters that are passed to the update members endpoint
    /// </summary>
    public class ChangeMembers
    {   
        /// <summary>
        /// An array of the members to add to the group
        /// </summary>
        public string[] add {get; set; }= [];

        /// <summary>
        /// An array of the members to remove from the group
        /// </summary>
        public string[] remove {get; set; }= [];
    }
}