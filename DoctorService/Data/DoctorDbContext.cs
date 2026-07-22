using Microsoft.EntityFrameworkCore;

namespace DoctorService.Data
{
    public class DoctorDbContext : DbContext
    {
        public DoctorDbContext(DbContextOptions<DoctorDbContext> options) : base(options)
        {
        }
        public DbSet<Entities.Doctor> Doctors { get; set; }
    }
}
