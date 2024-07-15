using System.ComponentModel.DataAnnotations;

namespace TasksBlazor.Models
{
    public class SignupModel
    {
        [Required]
        public string? Username { get; set; }
        [Required]
        [MinLength(6, ErrorMessage = "Passwords should be at least 6 characters long")]
        public string? Password { get; set; }
        [Required]
        [Display(Name = "Admin")]
        public bool IsAdmin { get; set; }
    }
}
