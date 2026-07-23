using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Connections;
using RabbitMQ.Client;
using Shared.Contracts;

namespace AppointmentService.Services;

public class RabbitMqPublisher : IRabbitMqPublisher
{
    private readonly IConfiguration _configuration;

    public RabbitMqPublisher(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task PublishAppointmentCreatedAsync(
        AppointmentCreatedEvent appointmentEvent)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMq:HostName"],
            UserName = _configuration["RabbitMq:UserName"],
            Password = _configuration["RabbitMq:Password"]
        };

        using var connection = factory.CreateConnection();

        using var channel = connection.CreateModel();

        var queueName =
            _configuration["RabbitMq:QueueName"]!;

        channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var json = JsonSerializer.Serialize(appointmentEvent);

        var body = Encoding.UTF8.GetBytes(json);

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;

        channel.BasicPublish(
            exchange: string.Empty,
            routingKey: queueName,
            mandatory: false,
            basicProperties: properties,
            body: body);

        return Task.CompletedTask;
    }
}