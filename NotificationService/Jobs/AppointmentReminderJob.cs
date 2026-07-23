using NotificationService.Data;
using NotificationService.Entities;
using NotificationService.Enums;
using NotificationService.Services;

namespace NotificationService.Jobs;

public class AppointmentReminderJob
{
    private readonly NotificationDbContext _context;
    private readonly IEmailService _emailService;

    public AppointmentReminderJob(
        NotificationDbContext context,
        IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task ExecuteAsync(
        int appointmentId,
        string recipientEmail,
        DateTime appointmentDate)
    {
        var message =
            $"Hatırlatma: Randevunuz 1 saat sonra. " +
            $"Randevu tarihi: {appointmentDate:g}";

        await _emailService.SendAsync(
            recipientEmail,
            "Randevu Hatırlatması",
            message);

        var notification = new Notification
        {
            AppointmentId = appointmentId,
            RecipientEmail = recipientEmail,
            Message = message,
            Type = NotificationType.AppointmentReminder,
            IsSent = true,
            CreatedAt = DateTime.Now,
            SentAt = DateTime.Now
        };

        await _context.Notifications.AddAsync(notification);

        await _context.SaveChangesAsync();

        Console.WriteLine(
            $"Randevu hatırlatma e-postası gönderildi. " +
            $"AppointmentId: {appointmentId}, " +
            $"RecipientEmail: {recipientEmail}");
    }
}