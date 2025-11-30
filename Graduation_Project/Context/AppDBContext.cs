using Graduation_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Project.Context
{
    public class AppDBContext:DbContext
    {
        public DbSet<User> users { set; get; }
		public DbSet<Hospital> Hospitals { set; get; }

		public DbSet<Department> Departments { set; get; }
		public DbSet<Doctor> Doctors { set; get; }
		public DbSet<Booking> Booking { set; get; }
        public DbSet<Payment> Payment { set; get; }
        public DbSet<Equipment> Equipments { set; get; }
        public DbSet<Rental> Rental { set; get; }
        public DbSet<Notification> Notification { set; get; }
        public DbSet<ChatHistory> ChatHistory { set; get; }


        public AppDBContext(DbContextOptions<AppDBContext> opts) : base(opts)
        {
            
        }

    }
}
