using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EncaptchaAPI
{
    public class UserContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<CaptchaTask> Captures { get; set; } = null!;
        public UserContext(DbContextOptions<UserContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>()
                .HasIndex(i => i.Email)
                .IsUnique();

            builder.Entity<User>()
                .HasMany(i => i.CustomeredTasks)
                .WithOne(i => i.Customer);

            builder.Entity<User>()
                .HasMany(i => i.CompletedTasks)
                .WithOne(i => i.Employee);

            base.OnModelCreating(builder);
        }
    }

    public enum Roles
    {
        Employee, Customer, Admin
    }
    public class User : UserView
    {
        public string Password { get; set; }
        public int Cache { get; set; }
        public List<CaptchaTask> CustomeredTasks { get; set; }
        public List<CaptchaTask> CompletedTasks { get; set; }
    }

    public class UserView
    {
        public UserView() { }
        public UserView(UserView target)
        {
            Id = target.Id;
            Email = target.Email;
            Role = target.Role;
        }
        [Key]
        public int Id { get; set; }
        public string Email { get; set; }
        public Roles Role { get; set; }
    }

    public enum TaskMode
    {
        Created, AtWork, Completed  
    }

    public class CaptchaTask
    {
        [Key]
        public int Id { get; set; }
        public TaskMode Mode { get; set; }
        public User Customer { get; set; }
        public User? Employee { get; set; }
        public byte[] Captcha { get; set; }
        public DateTime? Working { get; set; }
        public DateTime? Worked { get; set; }
        public string? Solution { get; set; }
    }
}
