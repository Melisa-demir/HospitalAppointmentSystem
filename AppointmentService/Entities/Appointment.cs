using AppointmentService.Enums;

namespace AppointmentService.Entities
{
    public class Appointment
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public AppointmentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
