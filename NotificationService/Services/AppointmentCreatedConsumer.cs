using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotificationService.Data;
using NotificationService.Entities;
using NotificationService.Enums;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts;
using Hangfire;
using NotificationService.Jobs;

namespace NotificationService.Services;

public class AppointmentCreatedConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IBackgroundJobClient _backgroundJobClient;

    private IConnection? _connection;
    private IModel? _channel;

    public AppointmentCreatedConsumer(
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        IBackgroundJobClient backgroundJobClient)
    {
        _configuration = configuration;
        _scopeFactory = scopeFactory;
        _backgroundJobClient = backgroundJobClient;
    }   

    protected override Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMq:HostName"],
            UserName = _configuration["RabbitMq:UserName"],
            Password = _configuration["RabbitMq:Password"],

            // AsyncEventingBasicConsumer kullanabilmek için gereklidir.
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();

        _channel = _connection.CreateModel();

        var queueName =
            _configuration["RabbitMq:QueueName"]
            ?? throw new InvalidOperationException(
                "RabbitMq:QueueName ayarı bulunamadı.");

        _channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var consumer =
            new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (_, eventArgs) =>
        {
            Console.WriteLine("RabbitMQ mesajı alındı.");

            try
            {
                var json = Encoding.UTF8.GetString(
                    eventArgs.Body.ToArray());

                var appointmentEvent =
                    JsonSerializer.Deserialize<AppointmentCreatedEvent>(
                        json);

                if (appointmentEvent is null)
                {
                    _channel.BasicNack(
                        deliveryTag: eventArgs.DeliveryTag,
                        multiple: false,
                        requeue: false);

                    return;
                }

                using var scope =
                    _scopeFactory.CreateScope();

                var dbContext =
                    scope.ServiceProvider
                        .GetRequiredService<NotificationDbContext>();

                var notification = new Notification
                {
                    AppointmentId =
                        appointmentEvent.AppointmentId,

                    RecipientEmail =
                        appointmentEvent.PatientEmail,

                    Message =
                        $"Randevunuz oluşturuldu. " +
                        $"Tarih: {appointmentEvent.AppointmentDate:g}",

                    Type =
                        NotificationType.AppointmentCreated,

                    IsSent = false,
                    CreatedAt = DateTime.Now,
                    SentAt = null
                };

                dbContext.Notifications.Add(notification);

                await dbContext.SaveChangesAsync(
                    stoppingToken);

                Console.WriteLine("Notification veritabanına kaydedildi.");

                var reminderTime =
                appointmentEvent.AppointmentDate.AddHours(-1);

                Console.WriteLine($"Reminder Time: {reminderTime}");
                Console.WriteLine($"Now: {DateTime.Now}");

                if (reminderTime <= DateTime.Now)
                {
                    _backgroundJobClient.Enqueue<AppointmentReminderJob>(
                        job => job.ExecuteAsync(
                            appointmentEvent.AppointmentId,
                            appointmentEvent.PatientEmail,
                            appointmentEvent.AppointmentDate));
                }
                else
                {
                    Console.WriteLine("Schedule çalıştı.");

                    _backgroundJobClient.Schedule<AppointmentReminderJob>(
                        job => job.ExecuteAsync(
                            appointmentEvent.AppointmentId,
                            appointmentEvent.PatientEmail,
                            appointmentEvent.AppointmentDate),
                        reminderTime);
                }

                _channel.BasicAck(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false);
            }
            catch (Exception exception)
            {
                Console.WriteLine(
                    $"Mesaj işlenirken hata oluştu: " +
                    $"{exception.Message}");

                _channel?.BasicNack(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false,
                    requeue: false);
            }
        };

        _channel.BasicConsume(
            queue: queueName,
            autoAck: false,
            consumer: consumer);

        return Task.CompletedTask;
    }

    public override Task StopAsync(
        CancellationToken cancellationToken)
    {
        _channel?.Close();
        _connection?.Close();

        _channel?.Dispose();
        _connection?.Dispose();

        return base.StopAsync(cancellationToken);
    }
}