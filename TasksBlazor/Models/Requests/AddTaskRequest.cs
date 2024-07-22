using System.ComponentModel.DataAnnotations;

namespace TasksBlazor.Models.Requests
{
    public class AddTaskRequest
    {
        [Required]
        public string? Subject { get; set; }
        public string? Description { get; set; }
        [Required]
        public string? Status { get; set; }
        [Required]
        public string? Priority { get; set; }
        public string? AssignedTo { get; set; }
    }
}
