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
            var task = await _dbContext.Tasks.Include(x => x.CreatedBy).Include(x => x.ModifiedBy).Include(x => x.User).Where(x => x.Id == id).FirstOrDefaultAsync();

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

        //// GET: api/Tasks
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Models.Entities.Task>>> GetTasks()
        //{
        //    return await _dbContext.Tasks.ToListAsync();
        //}

        //// PUT: api/Tasks/5
        //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutTask(int id, Models.Entities.Task task)
        //{
        //    if (id != task.Id)
        //    {
        //        return BadRequest();
        //    }

        //    _dbContext.Entry(task).State = EntityState.Modified;

        //    try
        //    {
        //        await _dbContext.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!TaskExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        //// DELETE: api/Tasks/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteTask(int id)
        //{
        //    var task = await _dbContext.Tasks.FindAsync(id);
        //    if (task == null)
        //    {
        //        return NotFound();
        //    }

        //    _dbContext.Tasks.Remove(task);
        //    await _dbContext.SaveChangesAsync();

        //    return NoContent();
        //}

        //private bool TaskExists(int id)
        //{
        //    return _dbContext.Tasks.Any(e => e.Id == id);
        //}
    }
}
