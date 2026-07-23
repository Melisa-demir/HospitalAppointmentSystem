namespace NotificationService.Services
{
    public interface IEmailService
    {
        Task SendAsync(string recipientEmail, string subject, string body);
    }
}
