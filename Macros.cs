using System.Text.RegularExpressions;
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

        static readonly IReadOnlyDictionary<string, Func<Event, Models.Group, string[], string>> parameterizedMacros =
 new Dictionary<string, Func<Event, Models.Group, string[], string>>
 {
     ["EVENT.DATE"] = (e, g, p) => ConvertDateTime(e, g, DATE_FORMAT, p),
     ["EVENT.DATETIME"] = (e, g, p) => ConvertDateTime(e, g, DATETIME_FORMAT, p),
     ["EVENT.TIME"] = (e, g, p) => ConvertDateTime(e, g, TIME_FORMAT, p),
 };

        static readonly IReadOnlyDictionary<string, Func<Event, Models.Group, string>> macros =
        new Dictionary<string, Func<Event, Models.Group, string>>
        {
            ["EVENT.TITLE"] = (e, g) => e.Title,
            ["EVENT.NOTES"] = (e, g) => e.Notes,
            ["EVENT.ORGANIZER"] = (e, g) => e.Organizer,
            ["GROUP.NAME"] = (e, g) => g.Name,
            ["EVENT.ATTENDEES.ATTENDING"] = (e, g) => GetAttendees(e, EventAttendance.AttendanceStatus.Attending),
            ["EVENT.ATTENDEES.TENTATIVE"] = (e, g) => GetAttendees(e, EventAttendance.AttendanceStatus.Tentative),
            ["EVENT.ATTENDEES.UNAVAILABLE"] = (e, g) => GetAttendees(e, EventAttendance.AttendanceStatus.Unavailable),
        };
        /// <summary>
        /// Fills the given template text with the macros
        /// </summary>
        /// <param name="text">The template text to fill</param>
        /// <param name="event">The event data to fill the macros with</param>
        /// <param name="group">The group data to fill the macros with</param>
        /// <returns>The filled text</returns>
        // public static string Fill(string text, Event @event, Group group)
        // {
        //     foreach (KeyValuePair<string, Func<Event, Group, string>> macro in macros)
        //     {
        //         text = text.Replace($"${{{macro.Key}}}", macro.Value(@event, group));
        //     }

        //     return text;
        // }

        public static string Fill(string text, Event @event, Models.Group group)
        {
            foreach (var parameterizedMacro in parameterizedMacros)
            {
                var pattern = $@"\$\{{{Regex.Escape(parameterizedMacro.Key)}\((.*?)\)\}}";
                var regex = new Regex(pattern);

                text = regex.Replace(text, match =>
                            {
                                // string macroName = match.Groups[1].Value;
                                string paramString = match.Groups[1].Value;

                                string[] parameters = paramString.Split(',').ToArray();

                                return parameterizedMacro.Value(@event, group, parameters);
                            });
            }

            foreach (var macro in macros)
            {
                text = text.Replace($"${{{macro.Key}}}", macro.Value(@event, group));
            }

            return text;
        }

        static string ConvertDateTime(Event @event, Models.Group group, string format, string[] parameters)
        {
            string type = GetOrDefault(parameters, 0, "start");
            string timezone = GetOrDefault(parameters, 1, group.TimeZone);

            DateTime dateTime = type.Equals("end", StringComparison.InvariantCultureIgnoreCase) ?
                @event.EndTime : @event.StartTime;


            DateTime converted = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTime, "UTC", timezone);

            return converted.ToString(format);
        }

        static string GetOrDefault(string[] arr, int index, string fallback)
        {
            return arr.Length > index && !string.IsNullOrWhiteSpace(arr[index])
                ? arr[index]
                : fallback;
        }

        static string GetAttendees(Event @event, EventAttendance.AttendanceStatus status)
        {
            List<string> attendees = [.. @event.EventAttendances
                .Where(a => a.Status == status)
                .Select(a => a.Name)];

            return string.Join(", ", attendees);
        }
    }
}