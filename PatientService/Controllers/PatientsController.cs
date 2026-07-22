using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientService.Data;
using PatientService.Entities;

namespace PatientService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly PatientDbContext _context;
        public PatientsController(PatientDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task <IActionResult> GetAll()
        {
            var patients = await _context.Patients
                .AsNoTracking()
                .ToListAsync();
            return Ok(patients);
        }

        [HttpGet("{id:int}")]
        public async Task <IActionResult> GetById(int id)
        {
            var patient = await _context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
            if (patient == null)
            {
                return NotFound();
            }
            return Ok(patient);
        }
        [HttpPost]
        public async Task<IActionResult> Create(Patient patient)
        {
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = patient.Id }, patient);
        }
    }
}