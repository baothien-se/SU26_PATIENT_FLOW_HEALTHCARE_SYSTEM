namespace Shared.Events;

/// <summary>
/// Common integration events for hospital workflow
/// </summary>

/// <summary>
/// Event raised when a patient is registered
/// </summary>
public class PatientRegisteredEvent : IntegrationEvent
{
    public int PatientId { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? BarcodeId { get; set; }
}

/// <summary>
/// Event raised when an appointment is scheduled
/// </summary>
public class AppointmentScheduledEvent : IntegrationEvent
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public int DepartmentId { get; set; }
    public DateTime AppointmentDateTime { get; set; }
    public string? PatientName { get; set; }
    public string? DepartmentName { get; set; }
}

/// <summary>
/// Event raised when a consultation starts
/// </summary>
public class ConsultationStartedEvent : IntegrationEvent
{
    public int ConsultationId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int DepartmentId { get; set; }
    public DateTime StartTime { get; set; }
    public string? PatientName { get; set; }
    public string? DoctorName { get; set; }
}

/// <summary>
/// Event raised when a consultation completes
/// </summary>
public class ConsultationCompletedEvent : IntegrationEvent
{
    public int ConsultationId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int DepartmentId { get; set; }
    public DateTime EndTime { get; set; }
    public string? Diagnosis { get; set; }
    public bool HasLabOrder { get; set; }
    public bool HasImagingOrder { get; set; }
    public bool HasSurgeryOrder { get; set; }
    public bool HasPrescription { get; set; }
}

/// <summary>
/// Event raised when a lab test is ordered
/// </summary>
public class LabTestOrderedEvent : IntegrationEvent
{
    public int OrderId { get; set; }
    public int ConsultationId { get; set; }
    public int PatientId { get; set; }
    public int LabTestId { get; set; }
    public string? TestName { get; set; }
    public string? TestCode { get; set; }
    public DateTime OrderedAt { get; set; }
}

/// <summary>
/// Event raised when lab test result is ready
/// </summary>
public class LabTestResultReadyEvent : IntegrationEvent
{
    public int OrderId { get; set; }
    public int PatientId { get; set; }
    public int ConsultationId { get; set; }
    public string? TestName { get; set; }
    public string? Result { get; set; }
    public DateTime ResultedAt { get; set; }
}

/// <summary>
/// Event raised when an imaging is ordered
/// </summary>
public class ImagingOrderedEvent : IntegrationEvent
{
    public int OrderId { get; set; }
    public int ConsultationId { get; set; }
    public int PatientId { get; set; }
    public int ImagingTypeId { get; set; }
    public string? ImagingType { get; set; }
    public DateTime OrderedAt { get; set; }
}

/// <summary>
/// Event raised when imaging is completed
/// </summary>
public class ImagingCompletedEvent : IntegrationEvent
{
    public int OrderId { get; set; }
    public int PatientId { get; set; }
    public int ConsultationId { get; set; }
    public string? ImagingType { get; set; }
    public DateTime CompletedAt { get; set; }
}

/// <summary>
/// Event raised when a surgery is ordered
/// </summary>
public class SurgeryOrderedEvent : IntegrationEvent
{
    public int OrderId { get; set; }
    public int ConsultationId { get; set; }
    public int PatientId { get; set; }
    public int SurgeryTypeId { get; set; }
    public string? SurgeryType { get; set; }
    public DateTime OrderedAt { get; set; }
}

/// <summary>
/// Event raised when a prescription is issued
/// </summary>
public class PrescriptionIssuedEvent : IntegrationEvent
{
    public int PrescriptionId { get; set; }
    public int ConsultationId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateTime IssuedAt { get; set; }
    public List<PrescriptionItemDto> Items { get; set; } = [];
}

/// <summary>
/// DTO for prescription items
/// </summary>
public class PrescriptionItemDto
{
    public int MedicineId { get; set; }
    public string? MedicineName { get; set; }
    public int Quantity { get; set; }
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
}

/// <summary>
/// Event raised when a payment is processed
/// </summary>
public class PaymentProcessedEvent : IntegrationEvent
{
    public int PaymentId { get; set; }
    public int InvoiceId { get; set; }
    public int PatientId { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTime PaymentDate { get; set; }
}
