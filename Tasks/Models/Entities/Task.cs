using TasksAPI.Models.Enums;

namespace TasksAPI.Models.Entities
{
    public class Task
    {
        public required int Id { get; set; }
        public required string Subject { get; set; }
        public required string description { get; set; }
        public required Status Status { get; set; }
        public required Priority Priority { get; set; }

        public required int CreatorId { get; set; }
        public required User CreatedBy { get; set; }

        public int? ModifierId { get; set; }
        public  User? ModifiedBy { get; set; }

        public required int UserId { get; set; }
        public required User User {  get; set; }

        public List<Category>? Categories { get; set; }
    }
}
