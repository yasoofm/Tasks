using Microsoft.EntityFrameworkCore;
using TasksAPI.Models.Entities;

namespace TasksAPI.Models
{
    public class TasksDBContext : DbContext
    {
        public DbSet<Entities.Task> Tasks { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }

        public TasksDBContext(DbContextOptions<TasksDBContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entities.Task>()
                .HasOne(e => e.CreatedBy)
                .WithMany(e => e.CreatedTasks)
                .HasForeignKey(e => e.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Entities.Task>()
                .HasOne(e => e.ModifiedBy)
                .WithMany(e => e.ModifiedTasks)
                .HasForeignKey(e => e.ModifierId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
