using System;
using System.Collections.Generic;
using System.Linq;
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
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly TasksDBContext _dbContext;

        public TasksController(TasksDBContext context)
        {
            _dbContext = context;
        }

        // POST: api/Tasks/AddTask
        [HttpPost("[action]")]
        public async Task<ActionResult<GetTaskResponse>> AddTask(AddTaskRequest taskRequest)
        {
            using(var dbContext = _dbContext)
            {
                var converted = Enum.TryParse(taskRequest.Priority, out Priority priority);
                
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
                    Status = Status.ToDo,
                    Priority = converted ? priority : Priority.Low,
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
            using(var dbContext = _dbContext)
            {
                var task = await dbContext.Tasks
                    .Include(x => x.CreatedBy)
                    .Include(x => x.ModifiedBy)
                    .Include(x => x.User)
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
                    Subject = task.Subject
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
                if(userIdClaim == null)
                {
                    return NotFound("User id not found");
                }

                var userId = int.Parse(userIdClaim.Value);
                var tasks = await dbContext.Tasks
                    .Include(x => x.User)
                    .Include(x => x.ModifiedBy)
                    .Include(x => x.User)
                    .Where(x => x.UserId == userId).Select(x => new GetTaskResponse
                    {
                        AssignedTo = x.User.Username,
                        CreatedBy = x.CreatedBy.Username,
                        Id = x.Id,
                        ModifiedBy = x.ModifiedBy.Username,
                        Priority = x.Priority.ToString(),
                        Status = x.Status.ToString(),
                        Subject = x.Subject,
                        Description = x.Description
                    }).ToListAsync();

                return Ok(tasks);
            }
        }

        // PATCH: api/Tasks/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchTask(int id, EditTaskRequest request)
        {
            using(var dbContext = _dbContext)
            {
                var task = await dbContext.Tasks.SingleOrDefaultAsync(x => x.Id == id);
                if(task == null)
                {
                    return NotFound("Task not found");
                }

                var userIdClaim = User.FindFirst(Constants.UserIdClaim);
                if (userIdClaim == null)
                {
                    return NotFound("Claim not found");
                }
                
                var userId = int.Parse(userIdClaim.Value);
                var modifier = await dbContext.Users.FindAsync(userId);
                if(modifier == null)
                {
                    return NotFound("Modifier not found");
                }
                task.ModifiedBy = modifier;
                task.ModifierId = modifier.Id;

                if (request.Subject != null)
                {
                    task.Subject = request.Subject;
                }

                if(request.Description != null)
                {
                    task.Description = request.Description;
                }

                if(request.AssignedTo != null)
                {
                    var assignedTo = await dbContext.Users.SingleOrDefaultAsync(x => x.Username == request.AssignedTo);
                    if(assignedTo == null)
                    {
                        return NotFound("Assigned to not found");
                    }

                    task.UserId = assignedTo.Id;
                    task.User = assignedTo;
                }

                if(request.Priority != null)
                {
                    var isConverted = Enum.TryParse(request.Priority, out Priority priority);
                    if(isConverted)
                    {
                        task.Priority = priority;
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
            using(var dbContext = _dbContext)
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
    }
}
