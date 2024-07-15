
using System.ComponentModel.DataAnnotations;

namespace TasksBlazor.Models.Requests
{
    public class SignupRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string Role { get; set; }
    }
}
