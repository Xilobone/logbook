using Logbook.Models;

namespace Logbook
{
    /// <summary>
    /// Helper class used to fill macros in text
    /// </summary>
    public class Macros
    {
        const string TIME_FORMAT = "HH:mm:ss";
        const string DATE_FORMAT = "dd/MM/yyyy";
        const string DATETIME_FORMAT = "dd/MM/yyyy - HH:mm:ss";
        static readonly IReadOnlyDictionary<string, Func<Event, Group, string>> macros =
        new Dictionary<string, Func<Event, Group, string>>
        {
            ["EVENT.TITLE"] = (e, g) => e.Title,
            ["EVENT.NOTES"] = (e, g) => e.Notes,
            ["EVENT.ORGANIZER"] = (e, g) => e.Organizer,
            ["EVENT.DATE.START"] = (e, g) => ConvertDateTime(e.StartTime, g.TimeZone, DATE_FORMAT),
            ["EVENT.DATE.START.UTC"] = (e, g) => ConvertDateTime(e.StartTime, "UTC", DATE_FORMAT),
            ["EVENT.DATE.END"] = (e, g) => ConvertDateTime(e.EndTime, g.TimeZone, DATE_FORMAT),
            ["EVENT.DATE.END.UTC"] = (e, g) => ConvertDateTime(e.EndTime, "UTC", DATE_FORMAT),
            ["EVENT.DATETIME.START"] = (e, g) => ConvertDateTime(e.StartTime, g.TimeZone, DATETIME_FORMAT),
            ["EVENT.DATETIME.START.UTC"] = (e, g) => ConvertDateTime(e.StartTime, "UTC", DATETIME_FORMAT),
            ["EVENT.DATETIME.END"] = (e, g) => ConvertDateTime(e.EndTime, g.TimeZone, DATETIME_FORMAT),
            ["EVENT.DATETIME.END.UTC"] = (e, g) => ConvertDateTime(e.EndTime, "UTC", DATETIME_FORMAT),
            ["EVENT.TIME.START"] = (e, g) => ConvertDateTime(e.StartTime, g.TimeZone, TIME_FORMAT),
            ["EVENT.TIME.START.UTC"] = (e, g) => ConvertDateTime(e.StartTime, "UTC", TIME_FORMAT),
            ["EVENT.TIME.END"] = (e, g) => ConvertDateTime(e.EndTime, g.TimeZone, TIME_FORMAT),
            ["EVENT.TIME.END.UTC"] = (e, g) => ConvertDateTime(e.EndTime, "UTC", TIME_FORMAT),
            ["GROUP.NAME"] = (e, g) => g.Name,
        };
        /// <summary>
        /// Fills the given template text with the macros
        /// </summary>
        /// <param name="text">The template text to fill</param>
        /// <param name="event">The event data to fill the macros with</param>
        /// <param name="group">The group data to fill the macros with</param>
        /// <returns>The filled text</returns>
        public static string Fill(string text, Event @event, Group group)
        {
            foreach (KeyValuePair<string, Func<Event, Group, string>> macro in macros)
            {
                text = text.Replace($"${{{macro.Key}}}", macro.Value(@event, group));
            }

            return text;
        }

        static string ConvertDateTime(DateTime dateTime, string timeZone, string format)
        {
            DateTime converted = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTime, "UTC", timeZone);
            return converted.ToString(format);
        }
    }
}