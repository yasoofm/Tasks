using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TasksAPI.Models;
using TasksAPI.Models.Entities;
using TasksAPI.Models.Enums;
using TasksAPI.Models.Requests;
using TasksAPI.Models.Responses;

namespace TasksAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "user, admin")]
    public class TasksController : ControllerBase
    {
        private readonly TasksDBContext _dbContext;

        public TasksController(TasksDBContext context)
        {
            _dbContext = context;
        }

        // POST: api/Tasks
        [HttpPost]
        public async Task<ActionResult<GetTaskResponse>> AddTask(AddTaskRequest taskRequest)
        {
            using (var dbContext = _dbContext)
            {
                var convertedPriority = Enum.TryParse(taskRequest.Priority, out Priority priority);
                var convertedStatus = Enum.TryParse(taskRequest.Status, out Status status);

                var creatorClaim = User.FindFirst(Constants.UserIdClaim);
                if (creatorClaim == null)
                {
                    return NotFound("User id claim not found");
                }

                var creatorId = int.Parse(creatorClaim.Value);
                var creator = await dbContext.Users.FindAsync(creatorId);
                Models.Entities.User? user;

                if (creator == null)
                {
                    return NotFound("Task creator not found");
                }

                if (taskRequest.AssignedTo != null)
                {
                    user = await dbContext.Users.SingleOrDefaultAsync(x => x.Username == taskRequest.AssignedTo);

                    if (user == null)
                    {
                        return NotFound("Assigned to user not found");
                    }
                }
                else
                {
                    user = creator;
                }

                var task = new Models.Entities.Task
                {
                    Subject = taskRequest.Subject,
                    Description = taskRequest.Description,
                    Status = convertedStatus ? status : Status.ToDo,
                    Priority = convertedPriority ? priority : Priority.Low,
                    User = user,
                    UserId = user.Id,
                    CreatedBy = creator,
                    CreatorId = creatorId,
                    ModifiedBy = creator,
                    ModifierId = creator.Id,
                };

                var dbResponse = await dbContext.Tasks.AddAsync(task);
                await _dbContext.SaveChangesAsync();

                return Created(string.Empty, new GetTaskResponse
                {
                    AssignedTo = user.Username,
                    CreatedBy = creator.Username,
                    ModifiedBy = creator.Username,
                    Id = dbResponse.Entity.Id,
                    Description = dbResponse.Entity.Description,
                    Priority = dbResponse.Entity.Priority.ToString(),
                    Status = dbResponse.Entity.Status.ToString(),
                    Subject = dbResponse.Entity.Subject
                });
            }
        }

        // GET: api/Tasks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GetTaskResponse>> GetTask(int id)
        {
            using (var dbContext = _dbContext)
            {
                var task = await dbContext.Tasks
                    .Include(x => x.CreatedBy)
                    .Include(x => x.ModifiedBy)
                    .Include(x => x.User)
                    .Include(x => x.Categories)
                    .Where(x => x.Id == id).FirstOrDefaultAsync();

                if (task == null)
                {
                    return NotFound("Task not found");
                }

                return Ok(new GetTaskResponse
                {
                    AssignedTo = task.User.Username,
                    CreatedBy = task.CreatedBy.Username,
                    Id = task.Id,
                    ModifiedBy = task.ModifiedBy.Username,
                    Description = task.Description,
                    Priority = task.Priority.ToString(),
                    Status = task.Status.ToString(),
                    Subject = task.Subject,
                    Categories = task.Categories.Select(x => new GetCategoryResponse
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Color = x.Color,
                    }).ToList()
                });
            }
        }

        // GET: api/Tasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetTaskResponse>>> GetTasks()
        {
            using (var dbContext = _dbContext)
            {
                var userIdClaim = User.FindFirst(Constants.UserIdClaim);
                if (userIdClaim == null)
                {
                    return NotFound("User id not found");
                }

                var userId = int.Parse(userIdClaim.Value);
                var tasks = await dbContext.Tasks
                    .Include(x => x.User)
                    .Include(x => x.ModifiedBy)
                    .Include(x => x.User)
                    .Include(x => x.Categories)
                    .Where(x => x.UserId == userId).Select(x => new GetTaskResponse
                    {
                        AssignedTo = x.User.Username,
                        CreatedBy = x.CreatedBy.Username,
                        Id = x.Id,
                        ModifiedBy = x.ModifiedBy.Username,
                        Priority = x.Priority.ToString(),
                        Status = x.Status.ToString(),
                        Subject = x.Subject,
                        Description = x.Description,
                        Categories = x.Categories.Select(x => new GetCategoryResponse
                        {
                            Color = x.Color,
                            Id = x.Id,
                            Name = x.Name,
                        }).ToList(),
                    }).ToListAsync();

                return Ok(tasks);
            }
        }

        // PATCH: api/Tasks/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchTask(int id, EditTaskRequest request)
        {
            using (var dbContext = _dbContext)
            {
                var task = await dbContext.Tasks.SingleOrDefaultAsync(x => x.Id == id);
                if (task == null)
                {
                    return NotFound("Task not found");
                }

                var userIdClaim = User.FindFirst(Constants.UserIdClaim);
                if (userIdClaim == null)
                {
                    return NotFound("User id claim not found");
                }
                var userId = int.Parse(userIdClaim.Value);

                var roleClaim = User.FindFirst(ClaimTypes.Role);
                if (roleClaim == null)
                {
                    return NotFound("Role claim not found");
                }
                var role = roleClaim.Value;

                var modifier = await dbContext.Users.FindAsync(userId);
                if (modifier == null)
                {
                    return NotFound("Modifier not found");
                }
                task.ModifiedBy = modifier;
                task.ModifierId = modifier.Id;

                if (request.Subject != null)
                {
                    task.Subject = request.Subject;
                }

                if (request.Description != null)
                {
                    task.Description = request.Description;
                }

                if (request.AssignedTo != null)
                {
                    var assignedTo = await dbContext.Users.SingleOrDefaultAsync(x => x.Username == request.AssignedTo);
                    if (assignedTo == null)
                    {
                        return NotFound("Assigned to not found");
                    }

                    task.UserId = assignedTo.Id;
                    task.User = assignedTo;
                }

                if (request.Priority != null)
                {
                    var isConverted = Enum.TryParse(request.Priority, out Priority priority);
                    if (isConverted)
                    {
                        task.Priority = priority;
                    }
                }

                if ((userId == task.UserId || role == "admin") && request.Status != null)
                {
                    var isConverted = Enum.TryParse(request.Status, out Status status);
                    if (isConverted)
                    {
                        task.Status = status;
                    }
                }

                await dbContext.SaveChangesAsync();
                return NoContent();
            }
        }

        // DELETE: api/Tasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            using (var dbContext = _dbContext)
            {
                var task = await _dbContext.Tasks.FindAsync(id);
                if (task == null)
                {
                    return NotFound();
                }

                _dbContext.Tasks.Remove(task);
                await _dbContext.SaveChangesAsync();

                return NoContent();
            }
        }

        // PATCH: api/Tasks/Move/5
        [HttpPatch("[action]/{id}")]
        public async Task<IActionResult> Move(int id, MoveTaskRequest request)
        {
            using (var dbContext = _dbContext)
            {
                var task = await dbContext.Tasks.FindAsync(id);
                if (task == null)
                {
                    return NotFound("Task not found");
                }

                var roleClaim = User.FindFirst(ClaimTypes.Role);
                if (roleClaim == null)
                {
                    return NotFound("Role claim not found");
                }
                var role = roleClaim.Value;

                var userIdClaim = User.FindFirst(Constants.UserIdClaim);
                if (userIdClaim == null)
                {
                    return NotFound("User id claim not found");
                }
                var userId = int.Parse(userIdClaim.Value);

                var user = await dbContext.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                if (user.Id != task.UserId && role != "admin")
                {
                    return BadRequest("only admin or assigned user can move tasks");
                }

                var converted = Enum.TryParse(request.Status, out Status status);

                if (converted)
                {
                    task.Status = status;
                }

                await dbContext.SaveChangesAsync();
                return NoContent();
            }
        }

        // GET: api/Tasks/GetUserTasks/user
        [HttpGet("[action]/{username}")]
        public async Task<ActionResult<List<GetTaskResponse>>> GetUserTasks(string username)
        {
            using (var dbContext = _dbContext)
            {
                if (!dbContext.Users.Any(x => x.Username == username))
                {
                    return NoContent();
                }

                var usernameClaim = User.FindFirst(Constants.UsernameClaim);
                if (usernameClaim == null)
                {
                    return NotFound("Claim not found");
                }
                var creator = usernameClaim.Value;

                var tasks = await dbContext.Tasks
                    .Include(x => x.User)
                    .Include(x => x.ModifiedBy)
                    .Include(x => x.User)
                    .Include(x => x.Categories)
                    .Where(x => x.User.Username == username).Select(x => new GetTaskResponse
                    {
                        AssignedTo = x.User.Username,
                        CreatedBy = x.CreatedBy.Username,
                        Id = x.Id,
                        ModifiedBy = x.ModifiedBy.Username,
                        Priority = x.Priority.ToString(),
                        Status = x.Status.ToString(),
                        Subject = x.Subject,
                        Description = x.Description,
                        Categories = x.Categories.Select(x => new GetCategoryResponse
                        {
                            Color = x.Color,
                            Id = x.Id,
                            Name = x.Name,
                        }).ToList(),
                    }).ToListAsync();

                return Ok(tasks);
            }
        }
    }
}
