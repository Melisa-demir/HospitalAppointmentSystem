using AppointmentService.Data;
using AppointmentService.Entities;
using AppointmentService.Enums;
using AppointmentService.Models;
using AppointmentService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts;


namespace AppointmentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppointmentDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IRabbitMqPublisher _rabbitMqPublisher;
        public AppointmentsController(AppointmentDbContext context, IHttpClientFactory httpClientFactory, IRabbitMqPublisher rabbitMqPublisher)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _rabbitMqPublisher = rabbitMqPublisher;
        }

        [HttpGet]
        public async Task<IActionResult> GetAppointments()
        {
            var appointments = await _context.Appointments
                .AsNoTracking()
                .ToListAsync();
            return Ok(appointments);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var appointment = await _context.Appointments
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);
            if (appointment == null)
            {
                return NotFound();
            }
            return Ok(appointment);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAppointmentRequest request)
        {
            if (request.AppointmentDate <= DateTime.Now)
            {
                return BadRequest("Randevu tarihini gelecekte olmalıdır");
            }
            var patientClient = _httpClientFactory.CreateClient("PatientService");

            var patientResponse = await patientClient.GetAsync($"/api/patients/{request.PatientId}");

            if (!patientResponse.IsSuccessStatusCode)
            {
                return BadRequest("Hasta bulunamadı");
            }

            var patient =
            await patientResponse.Content
                .ReadFromJsonAsync<PatientResponse>();

            if (patient is null)
            {
                return BadRequest("Hasta bilgisi alınamadı.");
            }


            var doctorClient = _httpClientFactory.CreateClient("DoctorService");
            var doctorResponse = await doctorClient.GetAsync(
                $"/api/Doctors/{request.DoctorId}");

            if (!doctorResponse.IsSuccessStatusCode)
            {
                var errorBody = await doctorResponse.Content.ReadAsStringAsync();

                return BadRequest(new
                {
                    Message = "DoctorService isteği başarısız.",
                    StatusCode = (int)doctorResponse.StatusCode,
                    Response = errorBody
                });
            }


            var doctor = await doctorResponse.Content.ReadFromJsonAsync<DoctorResponse>();


            if (doctor is null)
            {
                return BadRequest("Doktor bilgisi alınamadı");
            }


            if (!doctor.IsAvailable)
            {
                return BadRequest("Doktor müsait değil");
            }

            var appointment = new Appointment
            {
                PatientId = request.PatientId,
                DoctorId = request.DoctorId,
                AppointmentDate = request.AppointmentDate,
                Description = request.Description,
                Status = AppointmentStatus.Scheduled,
                CreatedAt = DateTime.Now
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            var appointmentCreatedEvent = new AppointmentCreatedEvent(
            appointment.Id,
            appointment.PatientId,
            appointment.DoctorId,
            appointment.AppointmentDate,
            patient.Email,
            doctor.Email);

            await _rabbitMqPublisher.PublishAppointmentCreatedAsync(
                appointmentCreatedEvent);

            return CreatedAtAction(
                nameof(GetById),
                new { id = appointment.Id },
                appointment);
        }

        [HttpPut("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }
            if (appointment.Status != AppointmentStatus.Scheduled)
            {
                return BadRequest("Randevu iptal edilemez");
            }
            appointment.Status = AppointmentStatus.Cancelled;
            await _context.SaveChangesAsync();
            return Ok(appointment);
        }
    }
}