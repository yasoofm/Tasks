using System.ComponentModel.DataAnnotations;
using TasksBlazor.Models.Responses;

namespace TasksBlazor.Models.Requests
{
    public class UpdateTaskRequest
    {
        [Editable(false)]
        public int? Id { get; set; }
        public string? Subject { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        [Editable(false)]
        public string? ModifiedBy { get; set; }
        [Editable(false)]
        public string? CreatedBy { get; set; }
        public string? AssignedTo { get; set; }
        public List<GetCategoryResponse> Categories { get; set; } = new List<GetCategoryResponse>();
    }
}
