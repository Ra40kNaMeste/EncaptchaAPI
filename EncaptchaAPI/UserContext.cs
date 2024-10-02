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

            base.OnModelCreating(builder);
        }
    }

    public enum JobTitles
    {
        Employee, Customer, Admin
    }
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public JobTitles JobTitle { get; set; }
        public int Cache { get; set; }
        public List<CaptchaTask> Tasks { get; set; }
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
    }
}
