namespace Logbook.DTO.EventTemplate
{
    public class Get
    {
        /// <summary>
        /// Determines how to show the event in the Outlook calendar, either busy, tentative or free
        /// </summary>
        public string ShowAs {get; set;} = "busy";
        /// <summary>
        /// The title of the events of this group, supports macros
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The body of the events of this group, supports macros
        /// </summary>
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// The default empty template
        /// </summary>
        public static readonly Get None = new();
    }
}