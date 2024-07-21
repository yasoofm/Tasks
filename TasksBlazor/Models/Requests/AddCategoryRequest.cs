using System.ComponentModel.DataAnnotations;

namespace TasksBlazor.Models.Requests
{
    public class AddCategoryRequest
    {
        [Required]
        public string? Name { get; set; }
    }
}
