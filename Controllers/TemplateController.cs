using Logbook.Data;
using Logbook.Models;
using Logbook.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logbook.Controllers
{
    /// <summary>
    /// Api endpoints for getting and updating personal templates
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TemplateController : ControllerBase
    {
        readonly LogbookDBContext _context;

        /// <summary>
        /// Creates a new template controller
        /// </summary>
        /// <param name="context">The database context to use</param>
        public TemplateController(LogbookDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets the users personal event template for a group, if they are a member of said group
        /// </summary>
        /// <param name="groupId">The id of the group to get the template of</param>
        /// <returns></returns>
        [HttpGet("{groupId:guid}")]
        public async Task<IActionResult> GetPersonalTemplate(Guid groupId)
        {
            (bool isValidRequest, Models.User user, IActionResult error) = await Auth.ValidateRequest(this, _context);
            if (!isValidRequest) return error;

            //check if user is a member of the group
            if (user.Groups.FirstOrDefault(g => g.Id.Equals(groupId)) == null)
            {
                return NotFound(new
                {
                    Message = $"No group with id {groupId} was found, or you are not a member of this group",
                    PersonalEventTemplateSet = ModelConverter.ToDTO.PersonalEventTemplateSet(PersonalEventTemplateSet.None),
                    Success = false,
                });
            }

            PersonalEventTemplateSet? personalEventTemplateSet = user.PersonalEventTemplates.FirstOrDefault(p => p.Group.Id.Equals(groupId));

            if (personalEventTemplateSet == null) return Ok(new
            {
                Message = "no event template set found",
                PersonalEventTemplateSet = ModelConverter.ToDTO.PersonalEventTemplateSet(PersonalEventTemplateSet.None),
                Success = true,
            });

            return Ok(new
            {
                Message = "no event template set found",
                PersonalEventTemplateSet = ModelConverter.ToDTO.PersonalEventTemplateSet(personalEventTemplateSet),
                Success = true,
            });

        }

        /// <summary>
        /// Endpoint for updating the users personal template for a group
        /// </summary>
        /// <param name="groupId">The id of the group to update the personal template of</param>
        /// <param name="personalSet">The event template set to update</param>
        /// <returns></returns>
        [HttpPost("{groupId:guid}")]
        public async Task<IActionResult> UpdatePersonalTemplate(Guid groupId, [FromBody] DTO.PersonalEventTemplateSet personalSet)
        {
            (bool isValidRequest, Models.User user, IActionResult error) = await Auth.ValidateRequest(this, _context);
            if (!isValidRequest) return error;

            //check if user is a member of the group
            Group? group = user.Groups.FirstOrDefault(g => g.Id.Equals(groupId));
            if (group == null)
            {
                return NotFound(new
                {
                    Message = $"No group with id {groupId} was found, or you are not a member of this group",
                    Success = false,
                });
            }

            PersonalEventTemplateSet? existingSet = user.PersonalEventTemplates.FirstOrDefault(p => p.Group.Id.Equals(groupId));
            
            if (existingSet == null)
            {
                PersonalEventTemplateSet set = ModelConverter.ToModel.PersonalEventTemplateSet(personalSet);
                set.User = user;
                set.Group = group;

                user.PersonalEventTemplates.Add(set);
                _context.SaveChanges();
                return Ok(new
                {
                    Message = "event template set was updated",
                    Success = true,
                });
            }

            existingSet.Enabled = personalSet.Enabled;
            existingSet.EventTemplateSet.DifferentiateOnAttendance = personalSet.EventTemplateSet.DifferentiateOnAttendance;
            if (personalSet.EventTemplateSet.Attending != null)
            {
                existingSet.EventTemplateSet.Attending.ShowAs = Enum.Parse<EventStatus>(personalSet.EventTemplateSet.Attending.ShowAs);
                existingSet.EventTemplateSet.Attending.Title = personalSet.EventTemplateSet.Attending.Title;
                existingSet.EventTemplateSet.Attending.Body = EventTemplateSet.StripInvalidCharacters(personalSet.EventTemplateSet.Attending.Body);
            }

            if (personalSet.EventTemplateSet.Tentative != null)
            {
                existingSet.EventTemplateSet.Tentative.ShowAs = Enum.Parse<EventStatus>(personalSet.EventTemplateSet.Tentative.ShowAs);
                existingSet.EventTemplateSet.Tentative.Title = personalSet.EventTemplateSet.Tentative.Title;
                existingSet.EventTemplateSet.Tentative.Body = EventTemplateSet.StripInvalidCharacters(personalSet.EventTemplateSet.Tentative.Body);
            }

            if (personalSet.EventTemplateSet.Unavailable != null)
            {
                existingSet.EventTemplateSet.Unavailable.ShowAs = Enum.Parse<EventStatus>(personalSet.EventTemplateSet.Unavailable.ShowAs);
                existingSet.EventTemplateSet.Unavailable.Title = personalSet.EventTemplateSet.Unavailable.Title;
                existingSet.EventTemplateSet.Unavailable.Body = EventTemplateSet.StripInvalidCharacters(personalSet.EventTemplateSet.Unavailable.Body);
            }

            _context.SaveChanges();

            return Ok(new
            {
                Message = "event template set was updated",
                Success = true,
            });
        }
    }
}