﻿using TasksAPI.Models.Enums;

namespace TasksAPI.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string Role { get; set; }
        public  List<Task> Tasks { get; set; } = new List<Task>();
        public List<Task> CreatedTasks { get; set; } = new List<Task>();
        public List<Task> ModifiedTasks { get; set; } = new List<Task>();
    }
}
