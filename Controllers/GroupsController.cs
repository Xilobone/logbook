using Logbook.Data;
using Logbook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Logbook.Controllers
{
    /// <summary>
    /// Api endpoint for managing groups, exposes methods that allows the users
    /// to create groups, add members to a group, remove themselves from a group and obtain and update
    /// the configuration of a group
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/{controller}")]
    public class GroupsController : ControllerBase
    {
        readonly LogbookDBContext _context;
        readonly ApplicationSettings _settings;

        /// <summary>
        /// Creates a new groups controller
        /// </summary>
        /// <param name="context">The database context to use</param>
        /// <param name="settings">The application settings to use</param>
        public GroupsController(LogbookDBContext context, IOptions<ApplicationSettings> settings)
        {
            _context = context;
            _settings = settings.Value;

        }
        /// <summary>
        /// Gets the groups the user is a member of
        /// </summary>
        /// <returns>A list of all groups the user is a member of</returns>
        [HttpGet("{id:guid?}")]
        public async Task<IActionResult> GetGroups(Guid? id)
        {
            Logger.Log("getting groups");
            (bool isValidRequest, User user, IActionResult requestError) = await Util.Auth.ValidateRequest(this, _context);
            if (!isValidRequest) return requestError;

            if (id == null)
            {
                Logger.Log("id is null");

                List<DTO.Group.Get> groups = user.Groups.Select(g => Util.ModelConverter.ToDTO.Group(g)).ToList();

                return Ok(groups);
            }

            Group? group = user.Groups.FirstOrDefault(g => g.Id.Equals(id));
            if (group == null) return NotFound($"No group with id {id} was found, or you are not a member of the group");

            return Ok(Util.ModelConverter.ToDTO.Group(group));

        }

        /// <summary>
        /// Lets the user create a new group, the user wil automatically be added to the group
        /// </summary>
        /// <param name="groupRequest">The parameters required to create the group</param>
        /// <returns>The created group id</returns>
        [HttpPost("create")]
        public async Task<IActionResult> CreateGroup([FromBody] DTO.Group.Create groupRequest)
        {
            (bool isValidRequest, User user, IActionResult requestError) = await Util.Auth.ValidateRequest(this, _context);
            if (!isValidRequest) return requestError;

            (bool isValidId, User source, IActionResult idError) = ValidateUserId(groupRequest.SourceId);
            if (!isValidId) return idError;

            if (!source.CanBeSource) return Conflict(new
            {
                Message = $"User {source.DisplayName} does not have the right registration to act as the source",
                Success = false
            });

            if (user.Groups.Count >= _settings.maxGroups)
            {
                return Conflict(new
                {
                    Message = $"User cannot be a member of more than {_settings.maxGroups} groups (by design)",
                    Success = false
                });
            }

            Group group = Util.ModelConverter.ToModel.Group(groupRequest);

            _context.Groups.Add(group);
            group.Users.Add(user);
            _context.SaveChanges();

            Logger.Log($"User {user.Id} created group {group.Id}");

            return Ok(new
            {
                Message = $"Group {group.Name} was successfully created",
                Success = true,
                group.Id
            });
        }

        /// <summary>
        /// Updates a group with new values, not all values are required to be included
        /// in the request
        /// </summary>
        /// <param name="id">The id of the group to be updated</param>
        /// <param name="updateParam">The new values of the group</param>
        /// <returns>The updated group id</returns>
        [HttpPost("{id:guid}")]
        public async Task<IActionResult> UpdateGroup(Guid id, [FromBody] DTO.Group.Update updateParam)
        {
            (bool isValidRequest, User user, IActionResult requestError) = await Util.Auth.ValidateRequest(this, _context);
            if (!isValidRequest) return requestError;

            Group? group = user.Groups.FirstOrDefault(g => g.Id.Equals(id));
            if (group == null) return NotFound($"No group with id {id} was found, or you are not a member of the group");

            if (!string.IsNullOrEmpty(updateParam.DisplayName)) group.Name = updateParam.DisplayName;
            if (!string.IsNullOrEmpty(updateParam.FilePath)) group.FilePath = updateParam.FilePath;
            if (!string.IsNullOrEmpty(updateParam.TimeZone)) group.TimeZone = updateParam.TimeZone;
            if (updateParam.StartTime != null) group.StartTime = (TimeOnly)updateParam.StartTime;
            if (updateParam.EndTime != null) group.EndTime = (TimeOnly)updateParam.EndTime;

            if (updateParam.EventTemplateSet != null)
            {
                group.EventTemplateSet.DifferentiateOnAttendance = updateParam.EventTemplateSet.DifferentiateOnAttendance;
                if (updateParam.EventTemplateSet.Attending != null)
                {
                    group.EventTemplateSet.Attending.ShowAs = Enum.Parse<EventStatus>(updateParam.EventTemplateSet.Attending.ShowAs);
                    group.EventTemplateSet.Attending.Title = updateParam.EventTemplateSet.Attending.Title;
                    group.EventTemplateSet.Attending.Body = EventTemplateSet.StripInvalidCharacters(updateParam.EventTemplateSet.Attending.Body);
                }

                if (updateParam.EventTemplateSet.Tentative != null)
                {
                    group.EventTemplateSet.Tentative.ShowAs = Enum.Parse<EventStatus>(updateParam.EventTemplateSet.Tentative.ShowAs);
                    group.EventTemplateSet.Tentative.Title = updateParam.EventTemplateSet.Tentative.Title;
                    group.EventTemplateSet.Tentative.Body = EventTemplateSet.StripInvalidCharacters(updateParam.EventTemplateSet.Tentative.Body);
                }

                if (updateParam.EventTemplateSet.Unavailable != null)
                {
                    group.EventTemplateSet.Unavailable.ShowAs = Enum.Parse<EventStatus>(updateParam.EventTemplateSet.Unavailable.ShowAs);
                    group.EventTemplateSet.Unavailable.Title = updateParam.EventTemplateSet.Unavailable.Title;
                    group.EventTemplateSet.Unavailable.Body = EventTemplateSet.StripInvalidCharacters(updateParam.EventTemplateSet.Unavailable.Body);
                }
            }
            if (!string.IsNullOrEmpty(updateParam.SourceId))
            {
                (bool isValid, User source, IActionResult error) = ValidateUserId(updateParam.SourceId);
                if (!isValid) return error;

                if (!source.CanBeSource) return Conflict($"User {source.DisplayName} does not have the right registration to act as the source");

                group.SourceId = source.Id;
            }

            _context.SaveChanges();
            return Ok(new
            {
                Message = "Group was successfully updated",
                Success = true,
                group.Id
            });
        }

        /// <summary>
        /// Gets a list of all members in the specified group, if the user is a member of this group
        /// themselves
        /// </summary>
        /// <param name="id">The id of the group to get the members of</param>
        /// <returns>A list of all members in a group</returns>
        [HttpGet("{id:guid}/members")]
        public async Task<IActionResult> GetMembers(Guid id)
        {
            (bool isValid, User user, IActionResult error) = await Util.Auth.ValidateRequest(this, _context);
            if (!isValid) return error;

            Group? group = user.Groups.FirstOrDefault(g => g.Id.Equals(id));
            if (group == null) return NotFound($"No group with id {id} was found, or you are not a member of the group");

            List<DTO.Group.Member> members = group.Users.Select(u => new DTO.Group.Member()
            {
                Id = u.Id,
                DisplayName = u.DisplayName
            }).ToList();

            return Ok(new
            {
                members,
                Success = true
            });

        }

        /// <summary>
        /// Adds a member to the specified group, if the user making the call is a member of this group
        /// themselves
        /// </summary>
        /// <param name="id">The group id to add the member to</param>
        /// <param name="memberParams">The parameters required to add a member</param>
        /// <returns></returns>
        [HttpPost("{id:guid}/members/add")]
        public async Task<IActionResult> AddMember(Guid id, [FromBody] DTO.Group.ChangeMember memberParams)
        {
            //check if request was valid
            (bool isValidRequest, User user, IActionResult requestError) = await Util.Auth.ValidateRequest(this, _context);
            if (!isValidRequest) return requestError;

            //check if user is a member of the group they are trying to change
            Group? group = user.Groups.FirstOrDefault(g => g.Id.Equals(id));
            if (group == null) return NotFound($"No group with id {id} was found, or you are not a member of the group");

            //check if the user id they have provided is valid and known
            (bool isValidId, User member, IActionResult idError) = ValidateUserId(memberParams.MemberId);
            if (!isValidId) return idError;

            //check if the provided user id is not already a member of the group
            if (group.Users.Any(u => u.Id.Equals(member.Id))) return Conflict("User is already a member of the group");

            //check if the provided member is not at their max number of groups
            if (member.Groups.Count >= _settings.maxGroups)
            {
                return Conflict($"User cannot be a member of more than {_settings.maxGroups} groups (by design)");
            }

            //finally add the user to the group
            Logger.Log($"Adding user {member.Id} to group {id}, requested by user {user.Id}");
            group.Users.Add(member);
            _context.SaveChanges();

            return Ok(new
            {
                Message = "Successfully added member to group",
                MemberId = member.Id,
                GroupId = group.Id
            });
        }


        /// <summary>
        /// Removes a member to the specified group, if the user making the call is a member of this group
        /// themselves
        /// </summary>
        /// <param name="id">The group id to remve the member from</param>
        /// <param name="memberParams">The parameters required to remove a member</param>
        /// <returns></returns>
        [HttpPost("{id:guid}/members/remove")]
        public async Task<IActionResult> RemoveMember(Guid id, [FromBody] DTO.Group.ChangeMember memberParams)
        {
            //check if request was valid
            (bool isValidRequest, User user, IActionResult requestError) = await Util.Auth.ValidateRequest(this, _context);
            if (!isValidRequest) return requestError;

            //check if user is a member of the group they are trying to change
            Group? group = user.Groups.FirstOrDefault(g => g.Id.Equals(id));
            if (group == null) return NotFound($"No group with id {id} was found, or you are not a member of the group");

            //check if the user id they have provided is valid and known
            (bool isValidId, User member, IActionResult idError) = ValidateUserId(memberParams.MemberId);
            if (!isValidId) return idError;

            //check if the provided user id is actually a member of the group
            if (!group.Users.Any(u => u.Id.Equals(member.Id))) return Conflict("User is not a member of the group");

            //finally remove the user from the group
            Logger.Log($"Removing user {member.Id} from group {id}, requested by user {user.Id}");
            group.Users.Remove(member);
            _context.SaveChanges();

            return Ok(new
            {
                Message = "Successfully removed member from group",
                MemberId = member.Id,
                GroupId = group.Id
            });
        }

        /// <summary>
        /// Adds and removes multiple users from the group at once
        /// </summary>
        /// <param name="id">The group id to update</param>
        /// <param name="memberParams">A list of user ids to add or remove from the group</param>
        /// <returns>A list of return statusses</returns>
        [HttpPost("{id:guid}/members/update")]
        public async Task<IActionResult> UpdateMembers(Guid id, [FromBody] DTO.Group.ChangeMembers memberParams)
        {
            //check if request was valid
            (bool isValidRequest, User user, IActionResult requestError) = await Util.Auth.ValidateRequest(this, _context);
            if (!isValidRequest) return requestError;

            //check if user is a member of the group they are trying to change
            Group? group = user.Groups.FirstOrDefault(g => g.Id.Equals(id));
            if (group == null) return NotFound($"No group with id {id} was found, or you are not a member of the group");

            List<UpdateUserResult> results = new List<UpdateUserResult>();

            foreach (string memberId in memberParams.add)
            {
                //check if the user id they have provided is valid and known
                (bool isValidId, User member, IActionResult idError) = ValidateUserId(memberId);
                if (!isValidId)
                {
                    results.Add(new UpdateUserResult()
                    {
                        UserId = memberId,
                        Success = false,
                        Message = idError.ToString() ?? "No content"
                    });
                    continue;
                }

                //check if the provided user id is not already a member of the group
                if (group.Users.Any(u => u.Id.Equals(member.Id)))
                {
                    results.Add(new UpdateUserResult()
                    {
                        UserId = memberId,
                        Success = false,
                        Message = "User is already a member of the group"
                    });
                    continue;
                }
                //check if the provided member is not at their max number of groups
                if (member.Groups.Count >= _settings.maxGroups)
                {
                    results.Add(new UpdateUserResult()
                    {
                        UserId = memberId,
                        Success = false,
                        Message = $"User cannot be a member of more than {_settings.maxGroups} groups (by design)"
                    });
                    continue;
                }

                //finally add the user to the group
                Logger.Log($"Adding user {member.Id} to group {id}, requested by user {user.Id}");
                group.Users.Add(member);

                results.Add(new UpdateUserResult()
                {
                    UserId = memberId,
                    Success = true,
                    Message = $"{memberId} was added to the group"
                });
            }

            foreach (string memberId in memberParams.remove)
            {
                //check if the user id they have provided is valid and known
                (bool isValidId, User member, IActionResult idError) = ValidateUserId(memberId);
                if (!isValidId)
                {
                    results.Add(new UpdateUserResult()
                    {
                        UserId = memberId,
                        Success = false,
                        Message = idError.ToString() ?? "No content"
                    });
                    continue;
                }

                //check if the provided user id is actually a member of the group
                if (!group.Users.Any(u => u.Id.Equals(member.Id)))
                {
                    results.Add(new UpdateUserResult()
                    {
                        UserId = memberId,
                        Success = false,
                        Message = "User is not a member of the group"
                    });
                    continue;
                }

                //finally remove the user from the group
                Logger.Log($"Removing user {member.Id} from group {id}, requested by user {user.Id}");
                group.Users.Remove(member);

                results.Add(new UpdateUserResult()
                {
                    UserId = memberId,
                    Success = true,
                    Message = $"{memberId} was removed to the group"
                });
            }

            _context.SaveChanges();

            return Ok(new
            {
                results,
                GroupId = group.Id,
                Success = true,
                Message = $"Membership of {results.Count} users was updated"
            });
        }

        (bool isValid, User user, IActionResult error) ValidateUserId(string id)
        {
            if (!Guid.TryParse(id, out Guid guid))
            {
                return (false, null!, BadRequest("Source id must be a valid Guid"));
            }

            User? user = _context.Users.Where(u => u.Id.Equals(guid)).FirstOrDefault();
            if (user == null)
            {
                return (false, null!, NotFound($"No user was found with id {guid}"));
            }

            return (true, user, Ok());
        }

        private class UpdateUserResult
        {
            public string UserId { get; set; } = string.Empty;
            public bool Success { get; set; } = false;
            public string Message { get; set; } = string.Empty;

        }
    }
}