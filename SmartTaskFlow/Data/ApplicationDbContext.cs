using Microsoft.EntityFrameworkCore;
using SmartTaskFlow.Models;

namespace SmartTaskFlow.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets (tables)
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<UserActivityLog> UserActivityLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration des relations

            // User -> Tasks (1 à plusieurs)
            modelBuilder.Entity<User>()
                .HasMany(u => u.Tasks)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Categories (1 à plusieurs)
            // User -> Categories (1 à plusieurs)
            modelBuilder.Entity<User>()
                .HasMany(u => u.Categories)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict); 


            // Category -> Tasks (1 à plusieurs)
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Tasks)
                .WithOne(t => t.Category)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // User -> ActivityLogs (1 à plusieurs)
            modelBuilder.Entity<User>()
                .HasMany(u => u.ActivityLogs)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Task -> ActivityLogs (1 à plusieurs)
            modelBuilder.Entity<TaskItem>()
                .HasMany(t => t.ActivityLogs)
                .WithOne(a => a.Task)
                .HasForeignKey(a => a.TaskId)
                .OnDelete(DeleteBehavior.NoAction);

            // Index pour améliorer les performances
            modelBuilder.Entity<TaskItem>()
                .HasIndex(t => t.UserId);

            modelBuilder.Entity<TaskItem>()
                .HasIndex(t => t.Status);

            modelBuilder.Entity<TaskItem>()
                .HasIndex(t => t.Deadline);

            modelBuilder.Entity<UserActivityLog>()
                .HasIndex(a => a.UserId);

            // Valeurs par défaut
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasDefaultValue("User");

            modelBuilder.Entity<User>()
                .Property(u => u.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<TaskItem>()
                .Property(t => t.Status)
                .HasDefaultValue("ToDo");
        }
    }
}