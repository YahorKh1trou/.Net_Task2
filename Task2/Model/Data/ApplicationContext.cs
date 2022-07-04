using Microsoft.EntityFrameworkCore;

namespace Task2.Model.Data
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Books> Books { get; set; }

        public ApplicationContext()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=LAPTOP-5T54JKJS\SQLEXPRESS;Database=testdb2;Trusted_Connection=True;TrustServerCertificate=True");
        }
    }
}
