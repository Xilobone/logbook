using Logbook.Models;

namespace Logbook
{
    /// <summary>
    /// Helper class used to fill macros in text
    /// </summary>
    public class Macros
    {

        /// <summary>
        /// Fills the given template text with the macros
        /// </summary>
        /// <param name="text">The template text to fill</param>
        /// <param name="event">The event data to fill the macros with</param>
        /// <returns>The filled text</returns>
        public static string Fill(string text, Event @event)
        {
            Dictionary<string, string> macros = new Dictionary<string, string>()
            {
                {"EVENT.TITLE", @event.Title},
                {"EVENT.NOTES", @event.Notes},
            };

            foreach(KeyValuePair<string,string> macro in macros)
            {
                text = text.Replace($"${{{macro.Key}}}",macro.Value);
            }

            return text;
        }
    }
}