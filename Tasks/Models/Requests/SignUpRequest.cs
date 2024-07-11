namespace TasksAPI.Models.Requests
{
    public class SignUpRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string Role { get; set; }
    }
}
