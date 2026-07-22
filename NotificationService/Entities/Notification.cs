using NotificationService.Enums;

namespace NotificationService.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        public int AppointmentId { get; set; }

        public string RecipientEmail { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public NotificationType Type { get; set; }

        public bool IsSent { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? SentAt { get; set; }
    }
}
