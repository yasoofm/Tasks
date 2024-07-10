namespace TasksAPI.Models.Entities
{
    public class Category
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required string Color { get; set; }
        public List<Task>? Tasks { get; set; }
    }
}
