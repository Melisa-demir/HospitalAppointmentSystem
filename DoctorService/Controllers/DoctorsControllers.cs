using DoctorService.Data;
using DoctorService.Entities;
using DoctorService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoctorService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly DoctorDbContext _context;

        public DoctorsController(DoctorDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var doctor = await _context.Doctors
                .AsNoTracking()
                .ToListAsync();

            if (doctor is null)
                return NotFound();

            return Ok(doctor);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var doctor = await _context.Doctors
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);
            if (doctor is null)
                return NotFound();
            return Ok(doctor);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateDoctorRequest request)
        {
            var doctor = new Doctor
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Department = request.Department,
                Email = request.Email,
                IsAvailable = request.IsAvailable
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAll), new { id = doctor.Id }, doctor);
        }

        [HttpPut("{id:int}/availability")]
        public async Task<IActionResult> UpdateAvailability(int id, [FromBody] bool isAvailable)
        {
            var doctor = await _context.Doctors.FindAsync(id);

            if (doctor is null)
                return NotFound();

            doctor.IsAvailable = isAvailable;
            await _context.SaveChangesAsync();

            return Ok(doctor);
        }
}
}
