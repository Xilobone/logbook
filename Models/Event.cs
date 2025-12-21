using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Logbook.Models
{
    /// <summary>
    /// Represents an event in a calendar
    /// </summary>
    public class Event
    {
        /// <summary>
        /// The id of the event
        /// </summary>
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// The start date and time of the event
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.UnixEpoch;

        /// <summary>
        /// The end date and time of the event
        /// </summary>
        public DateTime EndTime { get; set; } = DateTime.UnixEpoch;

        /// <summary>
        /// The Title of the event
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The Group the event belongs to
        /// </summary>
        public virtual Group Group { get; set; } = Group.None;

        /// <summary>
        /// The id of this event in a users calendar, only set when this object was created from graph
        /// </summary>
        [NotMapped]
        public string? CalendarEventId { get; set; } = string.Empty;
        /// <summary>
        /// Creates a string representation of the event
        /// </summary>
        /// <returns>The string representation of the event</returns>
        public override string ToString()
        {
            return $"({StartTime} - {EndTime}) {Title}";
        }

        /// <summary>
        /// Checks whether two events are functionally the same
        /// </summary>
        /// <param name="obj">The object to compare this event to</param>
        /// <returns>True if the events are the same, false otherwise</returns>
        public override bool Equals(object? obj)
        {
            if (obj is not Event) return false;

            Event @event = (Event)obj;

            if (!StartTime.Equals(@event.StartTime)) return false;
            if (!EndTime.Equals(@event.EndTime)) return false;
            if (!Title.Equals(@event.Title)) return false;
            if (!Group.Id.Equals(@event.Group.Id)) return false;

            return true;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return StartTime.GetHashCode() + EndTime.GetHashCode() + Title.GetHashCode() + Group.Id.GetHashCode();
        }
    }
}