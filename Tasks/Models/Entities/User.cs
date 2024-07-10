using TasksAPI.Models.Enums;

namespace TasksAPI.Models.Entities
{
    public class User
    {
        public required int Id { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string Role { get; set; }
        public  List<Task>? Tasks { get; set; }
        public List<Task>? CreatedTasks { get; set; }
        public List<Task>? ModifiedTasks { get; set; }
    }
}
