using System.Data;
using ExcelDataReader;

namespace Logbook.Calendar
{   
    /// <summary>
    /// Represents an event in a calendar
    /// </summary>
    public class CalendarEvent
    {   
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

        public static string ERROR_MESSAGE = "The provided stream does not contain a valid document";

        /// <summary>
        /// Creates a list of events based on the contents of a stream, stream must be a valid xlsx file
        /// </summary>
        /// <param name="stream">The memorystream to read from</param>
        /// <returns>A list of calendar events</returns>
        public static List<CalendarEvent> CreateFromStream(MemoryStream stream)
        {
            List<CalendarEvent> events = new List<CalendarEvent>();

            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var result = reader.AsDataSet();
                var table = result.Tables[0];

                foreach (DataRow row in table.Rows)
                {

                    if (row[0] is DateTime dateTime)
                    {
                        DateTime startTime = dateTime.AddHours(10);
                        DateTime endTime = dateTime.AddHours(10);

                        string description = row[1].ToString() ?? "No title";

                        events.Add(new CalendarEvent()
                        {
                            StartTime = startTime,
                            EndTime = endTime,
                            Title = description
                        });
                    }
                    else continue;
                }
            }

            return events;
        }
    }
}