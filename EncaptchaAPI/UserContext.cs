using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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

            builder.Entity<CaptchaTask>()
                .HasOne(i => i.Customer)
                .WithMany(i => i.CustomeredTasks)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CaptchaTask>()
                .HasOne(i => i.Employee)
                .WithMany(i => i.CompletedTasks);

            base.OnModelCreating(builder);
        }
    }

    public enum Roles
    {
        Employee, Customer, Admin, SuperAdmin
    }
    public class User : UserView
    {
        public string Password { get; set; }
        public int Cache { get; set; }
        [JsonIgnore]
        public List<CaptchaTask> CustomeredTasks { get; set; }
        [JsonIgnore]
        public List<CaptchaTask> CompletedTasks { get; set; }
    }
    /// <summary>
    /// Базовая информация пользователя. Наследование здесь только для сокращения кода
    /// </summary>

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

    /// <summary>
    /// Информация о капче
    /// </summary>
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
