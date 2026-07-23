using Shared.Contracts;

namespace AppointmentService.Services
{
    public interface IRabbitMqPublisher
    {
        Task PublishAppointmentCreatedAsync(
        AppointmentCreatedEvent appointmentEvent);
    }
}
