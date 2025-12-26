using System.ComponentModel.DataAnnotations;

namespace Logbook.DTO.Config
{
    /// <summary>
    /// Represents the personal config the user can request
    /// </summary>
    public class PersonalResponse
    {
        /// <summary>
        /// The id of the user
        /// </summary>
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// The display name of the user
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Whether the user has the logbook service enabled
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// The name of the calendar to use for the logbook service
        /// </summary>
        public string CalendarName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents the personal config data that is able to be set
    /// </summary>
    public class PersonalRequest
    {
        /// <summary>
        /// The display name of the user
        /// </summary>
        [MinLength(1)]
        [MaxLength(50)]
        public string? DisplayName { get; set; }

        /// <summary>
        /// Whether the user has the logbook service enabled
        /// </summary>
        public bool? Enabled { get; set; }

        /// <summary>
        /// The name of the calendar to use for the logbook service
        /// </summary>
        [MinLength(1)]
        [MaxLength(50)]
        public string? CalendarName { get; set; }
    }

    /// <summary>
    /// Represents the group config the user can request
    /// </summary>
    public class GroupResponse
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
        /// The id of the user that is used as the data source for this group
        /// </summary>
        public Guid SourceId {get; set; } = Guid.Empty;

        /// <summary>
        /// The file path for the onedrive file on the source where the schedule can be found
        /// </summary>
        public string FilePath {get; set;} = string.Empty;

        /// <summary>
        /// The default start time for events
        /// </summary>
        public TimeOnly StartTime {get; set;} = TimeOnly.MinValue;

        /// <summary>
        /// The default end time for events
        /// </summary>
        public TimeOnly EndTime {get; set;} = TimeOnly.MinValue;
        
        /// <summary>
        /// The timezone the start and end times are given in
        /// </summary>
        public string TimeZone {get; set;} = string.Empty;
        /// <summary>
        /// The prefix to the event title
        /// </summary>
        public string EventPrefix {get; set;} = string.Empty;
    }

    /// <summary>
    /// Represents the group config the user can request
    /// </summary>
    public class GroupRequest
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
        public Guid SourceId {get; set; } = Guid.Empty;

        /// <summary>
        /// The file path for the onedrive file on the source where the schedule can be found
        /// </summary>
        [MinLength(1)]
        [MaxLength(100)]
        public string FilePath {get; set;} = string.Empty;

        /// <summary>
        /// The default start time for events
        /// </summary>
        [RegularExpression("^/d{2}:/d{2}$")]
        public TimeOnly StartTime {get; set;} = TimeOnly.MinValue;

        /// <summary>
        /// The default end time for events
        /// </summary>
        [RegularExpression("^/d{2}:/d{2}$")]
        public TimeOnly EndTime {get; set;} = TimeOnly.MinValue;
        
        /// <summary>
        /// The timezone the start and end times are given in
        /// </summary>
        [MinLength(1)]
        [MaxLength(50)]
        public string TimeZone {get; set;} = string.Empty;
        /// <summary>
        /// The prefix to the event title
        /// </summary>
        [MaxLength(50)]
        public string EventPrefix {get; set;} = string.Empty;


    }
}