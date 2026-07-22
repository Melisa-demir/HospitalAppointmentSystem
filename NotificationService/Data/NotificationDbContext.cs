using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NotificationService.Entities;

namespace NotificationService.Data
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
        {
        }
        public DbSet<Notification> Notifications { get; set; } = null!;
    }
}
