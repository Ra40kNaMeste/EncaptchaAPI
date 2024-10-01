using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EncaptchaAPI
{
    public class UserContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<CaptchaTask> Captures { get; set; } = null!;
        public UserContext(DbContextOptions<UserContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Customer>()
                .HasIndex(i => i.Email)
                .IsUnique();

            builder.Entity<Employee>()
                .HasIndex(i => i.Email)
                .IsUnique();

            base.OnModelCreating(builder);
        }
    }
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public List<CaptchaTask> Tasks { get; set; }
    }

    public class Employee
    {
        [Key]
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
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
        public Customer Customer { get; set; }
        public Employee? Employee { get; set; }
    }
}
