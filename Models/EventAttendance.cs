namespace Logbook.Models
{
    /// <summary>
    /// Represents the attendance of an event
    /// </summary>
    public class EventAttendance
    {
        /// <summary>
        /// All possible attendance statusses
        /// </summary>
        public enum AttendanceStatus
        {
            /// <summary>
            /// Indicates that the attendee has confirmed their attendance
            /// </summary>
            Attending,

            /// <summary>
            /// Indicates that the attendee is unsure they will attend the event
            /// </summary>
            Tentative,

            /// <summary>
            /// Indicates that the attendee will not be attending
            /// </summary>
            Unavailable,

            /// <summary>
            /// Indicates that the attendance status is not known
            /// </summary>
            Unknown
        }

        /// <summary>
        /// The unique identifier of this event attendance instance
        /// </summary>
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// The name of the attendee
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The status of the attendee
        /// </summary>
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Attending;

        /// <summary>
        /// Checks if two event attendances are equal, i.e they have the same name and status
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if they are equal, false otherwise</returns>
        public override bool Equals(object? obj)
        {
            if (obj is not EventAttendance) return false;

            EventAttendance @eventAttendance = (EventAttendance)obj;

            return Name.Equals(@eventAttendance.Name) && Status == @eventAttendance.Status;
        }

        /// <summary>
        /// Gets the hash code of the event attendance
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            return Name.GetHashCode() + Status.GetHashCode();
        }
    }

}