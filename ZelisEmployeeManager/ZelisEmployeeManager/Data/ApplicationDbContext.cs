using ZelisEmployeeManager.Models;
using Microsoft.EntityFrameworkCore;

namespace ZelisEmployeeManager.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Address> Addresses => Set<Address>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.Property(e => e.Email);
                entity.HasIndex(e => e.Email).IsUnique();

                // cascade delete on foreign key
                entity.HasOne(e => e.Address)
                    .WithOne(a => a.Employee)
                    .HasForeignKey<Address>(a => a.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);

            });

            //modelBuilder.Entity<Address>(entity =>
            //{

            //});


            // data for 5 seeded entries
            modelBuilder.Entity<Employee>().HasData(
                new Employee { Id = 1, FirstName = "Justin", LastName = "Foster", Email = "Justin.Foster@test.com", Phone = "(123)-123-4567" },
                new Employee { Id = 2, FirstName = "Julia", LastName = "Gehm", Email = "Julia.Gehm@test.com", Phone = "(123)-234-5678" },
                new Employee { Id = 3, FirstName = "Brian", LastName = "Sox", Email = "Brian.Sox@test.com", Phone = "(123)-345-6789" },
                new Employee { Id = 4, FirstName = "Tanner", LastName = "Fisher", Email = "Tanner.Fisher@test.com", Phone = "(123)-456-7890" },
                new Employee { Id = 5, FirstName = "Jane", LastName = "Doe", Email = "Jane.Doe@test.com", Phone = "(123)-567-8901" });

            modelBuilder.Entity<Address>().HasData(
                new Address { Id = 1, EmployeeId = 1, PrimaryAddress = "100 Main St", SecondaryAddress = "Apt 101", City = "St. Louis", State = "MO", ZipCode = "63123" },
                new Address { Id = 2, EmployeeId = 2, PrimaryAddress = "200 Road Road", City = "Chicago", State = "IL", ZipCode = "60601" },
                new Address { Id = 3, EmployeeId = 3, PrimaryAddress = "300 Avenue Ave", SecondaryAddress = "Ap 123", City = "Austin", State = "TX", ZipCode = "73301" },
                new Address { Id = 4, EmployeeId = 4, PrimaryAddress = "400 Street St", City = "Denver", State = "CO", ZipCode = "80202" },
                new Address { Id = 5, EmployeeId = 5, PrimaryAddress = "500 Drive Dr", City = "Kansas City", State = "MO", ZipCode = "64105" });

        }
    }
}
