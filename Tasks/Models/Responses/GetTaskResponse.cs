namespace TasksAPI.Models.Responses
{
    public class GetTaskResponse
    {
        public required int Id { get; set; }
        public required string Subject { get; set; }
        public string? Description { get; set; }
        public required string Status { get; set; }
        public required string Priority { get; set; }
        public required string ModifiedBy { get; set; }
        public required string CreatedBy { get; set; }
        public required string AssignedTo { get; set; }
    }
}
