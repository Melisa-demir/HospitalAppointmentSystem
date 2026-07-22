using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Entities;
using NotificationService.Models;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationDbContext _context;
        public NotificationsController(NotificationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var notifications = await _context.Notifications
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
            return Ok(notifications);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var notification = await _context.Notifications
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            if (notification == null)
            {
                return NotFound();
            }
            return Ok(notification);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateNotificationRequest request)
        {
            var notification = new Notification
            {
                AppointmentId = request.AppointmentId,
                RecipientEmail = request.RecipientEmail,
                Message = request.Message,
                Type = request.Type,
                IsSent = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = notification.Id }, notification);
        }

        [HttpPut("{id:int}/mark-as-sent")]
        public async Task<IActionResult> MarkAsSent(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound();
            }

            if(notification.IsSent)
            {
                return BadRequest("Bildirim zaten gönderilmiş.");
            }
            notification.IsSent = true;
            notification.SentAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
