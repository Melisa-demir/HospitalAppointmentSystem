using AppointmentService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString =
    builder.Configuration.GetConnectionString("AppointmentDb");

builder.Services.AddDbContext<AppointmentDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppointmentDbContext>();

builder.Services.AddHttpClient("PatientService", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ServiceUrls:PatientService"]!);
});

builder.Services.AddHttpClient("DoctorService", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ServiceUrls:DoctorService"]!);
});

var app = builder.Build();

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