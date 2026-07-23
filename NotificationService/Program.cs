using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString =
    builder.Configuration.GetConnectionString("NotificationDb");

builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<NotificationDbContext>();

builder.Services
    .AddHostedService<AppointmentCreatedConsumer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();
