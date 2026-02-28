
using Logbook.Models;

namespace Logbook.Util
{
    /// <summary>
    /// Class used to convert from and to Models and DTO
    /// </summary>
    public class ModelConverter
    {
        /// <summary>
        /// Collection of methods of converting Models to DTO
        /// </summary>
        public class ToDTO
        {
            /// <summary>
            /// Converts an actual group to a DTO group
            /// </summary>
            /// <param name="group">The group to convert</param>
            /// <returns>The converted DTO group</returns>
            public static DTO.Group.Get Group(Group group)
            {
                DTO.Group.Get dtoGroup = new DTO.Group.Get()
                {
                    Id = group.Id,
                    DisplayName = group.Name,
                    SourceId = group.SourceId,
                    FilePath = group.FilePath,
                    StartTime = group.StartTime,
                    EndTime = group.EndTime,
                    TimeZone = group.TimeZone,
                    EventTemplateSet = EventTemplateSet(group.EventTemplateSet),
                    CreatedAt = group.CreatedAt,
                    LastUpdated = group.LastUpdated
                };

                return dtoGroup;
            }

            /// <summary>
            /// Converts an actual event template set to a DTO event template set
            /// </summary>
            /// <param name="eventTemplateSet">The template set to convert</param>
            /// <returns>The converted template set</returns>
            public static DTO.EventTemplateSet EventTemplateSet(EventTemplateSet eventTemplateSet)
            {
                DTO.EventTemplateSet dtoEventTemplateSet = new DTO.EventTemplateSet()
                {
                    DifferentiateOnAttendance = eventTemplateSet.DifferentiateOnAttendance,
                    Attending = EventTemplate(eventTemplateSet.Attending),
                    Tentative = EventTemplate(eventTemplateSet.Tentative),
                    Unavailable = EventTemplate(eventTemplateSet.Unavailable),

                };

                return dtoEventTemplateSet;
            }

            /// <summary>
            /// Converts an actual personal event template set to a dto event template set
            /// </summary>
            /// <param name="personalEventTemplateSet">The personal event template set to convert</param>
            /// <returns>The converted set</returns>
            public static DTO.PersonalEventTemplateSet PersonalEventTemplateSet(PersonalEventTemplateSet personalEventTemplateSet)
            {
                DTO.PersonalEventTemplateSet dtoPersonalEventTemplateSet = new DTO.PersonalEventTemplateSet()
                {
                    Enabled = personalEventTemplateSet.Enabled,
                    EventTemplateSet = EventTemplateSet(personalEventTemplateSet.EventTemplateSet),  
                };

                return dtoPersonalEventTemplateSet;
            }

            static DTO.EventTemplate EventTemplate(EventTemplate eventTemplate)
            {
                DTO.EventTemplate dtoEventTemplate = new DTO.EventTemplate()
                {
                    ShowAs = eventTemplate.ShowAs.ToString(),
                    Title = eventTemplate.Title,
                    Body = eventTemplate.Body
                };

                return dtoEventTemplate;
            }
        }

        /// <summary>
        /// Collection of methods of converting DTO to Models
        /// </summary>
        public class ToModel
        {
            /// <summary>
            /// Converts a DTO group to an actual group
            /// </summary>
            /// <param name="dtoGroup">The DTO group to convert</param>
            /// <returns>The converted group</returns>
            public static Group Group(DTO.Group.Create dtoGroup)
            {
                Group group = new Group()
                {
                    Name = dtoGroup.DisplayName,
                    SourceId = Guid.Parse(dtoGroup.SourceId),
                    FilePath = dtoGroup.FilePath,
                    StartTime = dtoGroup.StartTime,
                    EndTime = dtoGroup.EndTime,
                    TimeZone = dtoGroup.TimeZone,
                    EventTemplateSet = EventTemplateSet(dtoGroup.EventTemplateSet)
                };

                return group;
            }

            /// <summary>
            /// Converts a DTO personal event template set to an actual event template set
            /// </summary>
            /// <param name="dtoPersonalEventTemplateSet">The dto personal event template set to convert</param>
            /// <returns>The converted Personal event template set</returns>
            public static PersonalEventTemplateSet PersonalEventTemplateSet(DTO.PersonalEventTemplateSet dtoPersonalEventTemplateSet)
            {
                PersonalEventTemplateSet personalSet = new PersonalEventTemplateSet
                {
                    Enabled = dtoPersonalEventTemplateSet.Enabled,
                    EventTemplateSet = EventTemplateSet(dtoPersonalEventTemplateSet.EventTemplateSet)
                };

                return personalSet;
            }
            static EventTemplateSet EventTemplateSet(DTO.EventTemplateSet dtoEventTemplateSet)
            {
                EventTemplateSet eventTemplateSet = new EventTemplateSet
                {
                    DifferentiateOnAttendance = dtoEventTemplateSet.DifferentiateOnAttendance,
                    Attending = EventTemplate(dtoEventTemplateSet.Attending ?? DTO.EventTemplate.None),
                    Tentative = EventTemplate(dtoEventTemplateSet.Tentative  ?? DTO.EventTemplate.None),
                    Unavailable = EventTemplate(dtoEventTemplateSet.Unavailable  ?? DTO.EventTemplate.None),
                };

                return eventTemplateSet;
            }

            static EventTemplate EventTemplate(DTO.EventTemplate dtoEventTemplate)
            {
                EventTemplate eventTemplate = new EventTemplate()
                {
                    ShowAs = Enum.Parse<EventStatus>(dtoEventTemplate.ShowAs, ignoreCase: true),
                    Title = dtoEventTemplate.Title,
                    Body = dtoEventTemplate.Body
                };

                return eventTemplate;
            }


        }

    }
}