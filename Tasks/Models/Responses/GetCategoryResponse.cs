namespace TasksAPI.Models.Responses
{
    public class GetCategoryResponse
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required string Color { get; set; }
    }
}
