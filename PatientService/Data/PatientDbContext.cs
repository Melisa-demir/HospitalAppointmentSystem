using Microsoft.EntityFrameworkCore;
using PatientService.Entities;

namespace PatientService.Data
{
    public class PatientDbContext : DbContext
    {
        public PatientDbContext(DbContextOptions<PatientDbContext> options) : base(options)
        {
        }
        public DbSet<Patient> Patients { get; set; } = null!;
    }
}
