using Logbook.Models;

namespace Logbook
{
    /// <summary>
    /// Helper class used to fill macros in text
    /// </summary>
    public class Macros
    {
        static readonly IReadOnlyDictionary<string, Func<Event, string>> macros =
        new Dictionary<string, Func<Event, string>>
        {
            ["EVENT.TITLE"] = e => e.Title,
            ["EVENT.NOTES"] = e => e.Notes,
            ["EVENT.ORGANIZER"] = e => e.Organizer
        };
        /// <summary>
        /// Fills the given template text with the macros
        /// </summary>
        /// <param name="text">The template text to fill</param>
        /// <param name="event">The event data to fill the macros with</param>
        /// <returns>The filled text</returns>
        public static string Fill(string text, Event @event)
        {
            foreach (KeyValuePair<string, Func<Event, string>> macro in macros)
            {
                text = text.Replace($"${{{macro.Key}}}", macro.Value(@event));
            }

            return text;
        }
    }
}