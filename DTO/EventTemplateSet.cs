namespace Logbook.DTO.EventTemplateSet
{
    public class Get
    {
        /// <summary>
        /// Whether to use different templates depending on the attendance, if 
        /// false only Attending will be used
        /// </summary>
        public bool DifferentiateOnAttendance { get; set; } = false;

        /// <summary>
        /// The event template for when the user is attending the event
        /// </summary>
        public virtual EventTemplate.Get Attending { get; set; } = EventTemplate.Get.None;

        /// <summary>
        /// The event template for when the user may be attending the event
        /// </summary>
        public virtual EventTemplate.Get Tentative { get; set; } = EventTemplate.Get.None;

        /// <summary>
        /// The event template when the user is unavailable to attend the event
        /// </summary>
        public virtual EventTemplate.Get Unavailable { get; set; } = EventTemplate.Get.None;

        /// <summary>
        /// The default empty template set
        /// </summary>
        public static readonly Get None = new();
    }
}