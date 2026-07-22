using NotificationService.Enums;

namespace NotificationService.Models
{
    public class CreateNotificationRequest
    {
        public int AppointmentId { get; set; }

        public string RecipientEmail { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public NotificationType Type { get; set; }
    }
}
