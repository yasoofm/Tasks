using System.ComponentModel.DataAnnotations;

namespace TasksBlazor.Models
{
    public class UpdateTaskModel
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
        public List<UpdateCategoryModel> Categories { get; set; } = new List<UpdateCategoryModel>();
    }

    public class UpdateCategoryModel
    {
        [Editable(false)]
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Color { get; set; }
    }
}
