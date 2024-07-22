using TasksAPI.Models.Enums;

namespace TasksAPI.Models.Requests
{
    public class AddTaskRequest
    {
        public required string Subject { get; set; }
        public string? Description { get; set; }
        public required string Priority { get; set; }
        public required string Status { get; set; }
        public string? AssignedTo { get; set; }
    }
}
