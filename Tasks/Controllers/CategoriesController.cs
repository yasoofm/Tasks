using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TasksAPI.Models;
using TasksAPI.Models.Entities;
using TasksAPI.Models.Requests;

namespace TasksAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly TasksDBContext _dbContext;

        public CategoriesController(TasksDBContext dBContext)
        {
            _dbContext = dBContext;
        }

        // POST: api/Categories/5
        [HttpPost("{taskId}")]
        public async Task<IActionResult> AddCategory(int taskId, AddCategoryRequest request)
        {
            using (var dbContext = _dbContext)
            {
                var task = await dbContext.Tasks.Include(x => x.Categories).FirstOrDefaultAsync(x => x.Id == taskId);
                if (task == null)
                {
                    return NotFound("Task not found");
                }

                if (dbContext.Categories.Any(x => x.Name == request.Name))
                {
                    var existingCategory = await dbContext.Categories.Where(x => x.Name == request.Name).FirstOrDefaultAsync();

                    if (task.Categories.Any(x => x.Name == existingCategory!.Name))
                    {
                        return NoContent();
                    }

                    task.Categories.Add(existingCategory!);
                    await dbContext.SaveChangesAsync();
                    return Ok();
                }

                var category = new Category
                {
                    Name = request.Name,
                    Color = request.Color ?? "#d2d2d2",
                };

                category.Tasks.Add(task);

                await dbContext.Categories.AddAsync(category);
                await dbContext.SaveChangesAsync();

                return Created();
            }
        }

        // DELETE: api/Categories/?taskId=5&categoryId=2
        [HttpDelete]
        public async Task<IActionResult> RemoveCategory(int taskId, int categoryId)
        {
            using(var dbContext = _dbContext)
            {
                var task = await dbContext.Tasks.Include(x => x.Categories).FirstOrDefaultAsync(x => x.Id == taskId);
                if (task == null) 
                {
                    return NotFound("Task not found");
                }

                var category = await dbContext.Categories.FindAsync(categoryId);
                if (category == null)
                {
                    return NotFound("Category not found");
                }

                task.Categories.Remove(category);
                await dbContext.SaveChangesAsync();
                return Ok();
            }
        }
    }
}
