using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using TasksAPI.Models;
using TasksAPI.Models.Entities;
using TasksAPI.Models.Requests;
using TasksAPI.Models.Responses;

namespace TasksAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly TasksDBContext _dbContext;

        public CategoriesController(TasksDBContext dBContext)
        {
            _dbContext = dBContext;
        }

        // POST: api/Categories/5
        [HttpPost("{taskId}")]
        public async Task<ActionResult<GetCategoryResponse>> AddCategory(int taskId, AddCategoryRequest request)
        {
            using (var dbContext = _dbContext)
            {
                GetCategoryResponse result;

                var task = await dbContext.Tasks.Include(x => x.Categories).FirstOrDefaultAsync(x => x.Id == taskId);
                if (task == null)
                {
                    return NotFound("Task not found");
                }

                if (dbContext.Categories.Any(x => x.Name == request.Name))
                {
                    var existingCategory = await dbContext.Categories.Where(x => x.Name == request.Name).Include(x => x.Tasks).FirstOrDefaultAsync();
                    result = new GetCategoryResponse { Id = existingCategory!.Id, Color = existingCategory.Color, Name = existingCategory.Name };

                    if (task.Categories.Any(x => x.Name == existingCategory!.Name))
                    {
                        return Ok(result);
                    }

                    existingCategory.Tasks.Add(task);
                    await dbContext.SaveChangesAsync();
                    return Ok(result);
                }

                var random = new Random();
                var color = String.Format("#{0:X6}", random.Next(0x1000000));

                var category = new Category
                {
                    Name = request.Name,
                    Color = color,
                };

                category.Tasks.Add(task);

                var addedCategory = await dbContext.Categories.AddAsync(category);
                await dbContext.SaveChangesAsync();
                var values = await addedCategory.GetDatabaseValuesAsync();
                values!.TryGetValue("Id", out int categoryId);

                result = new GetCategoryResponse { Color = category.Color, Name = category.Name, Id = categoryId };

                return Ok(result);
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
