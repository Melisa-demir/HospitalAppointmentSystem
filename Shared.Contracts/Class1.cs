namespace Shared.Contracts
{
    public record AppointmentCreatedEvent(
        int AppointmentId,
        int PatientId,
        int DoctorId,
        DateTime AppointmentDate,
        string PatientEmail,
        string DoctorEmail);
}
