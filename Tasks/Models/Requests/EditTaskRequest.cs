namespace TasksAPI.Models.Requests
{
    public class EditTaskRequest
    {
        public string? Subject { get; set; }
        public string? Description { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }
        public string? AssignedTo { get; set; }
    }
}
