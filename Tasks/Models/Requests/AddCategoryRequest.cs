﻿namespace TasksAPI.Models.Requests
{
    public class AddCategoryRequest
    {
        public required string Name { get; set; }
        public string? Color { get; set; }
    }
}