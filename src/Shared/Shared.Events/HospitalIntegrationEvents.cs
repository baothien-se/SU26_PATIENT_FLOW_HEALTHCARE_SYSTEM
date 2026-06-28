namespace Shared.Events;

// =====================================================
// Hospital Management System - Integration Events
// Cross-service event contracts for RabbitMQ messaging
// =====================================================

#region Patient Registration Events

/// <summary>
/// Published when a new patient is registered in the system.
/// Consumed by: AppointmentService (ready for scheduling), NotificationService (welcome notification)
/// </summary>
public class PatientRegisteredEvent : IntegrationEvent
{
    public Guid PatientId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string BarcodeId { get; set; } = string.Empty;
}

/// <summary>
/// Published when a patient's profile is updated.
/// Consumed by: All services that cache patient info (name sync)
/// </summary>
public class PatientProfileUpdatedEvent : IntegrationEvent
{
    public Guid PatientId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

#endregion

#region Appointment Events

/// <summary>
/// Published when an appointment is scheduled.
/// Consumed by: NotificationService (confirmation), BillingService (consultation fee)
/// </summary>
public class AppointmentScheduledEvent : IntegrationEvent
{
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DepartmentId { get; set; }
    public DateTime AppointmentDateTime { get; set; }
    public string? PatientName { get; set; }
    public string? DepartmentName { get; set; }
}

/// <summary>
/// Published when a patient checks in at the front desk (barcode scan).
/// Consumed by: ConsultationService (doctor dashboard update), NotificationService (SMS to doctor)
/// </summary>
public class PatientCheckedInEvent : IntegrationEvent
{
    public Guid PatientId { get; set; }
    public Guid AppointmentId { get; set; }
    public Guid DepartmentId { get; set; }
    public int QueuePosition { get; set; }
    public string? PatientName { get; set; }
}

/// <summary>
/// Published when a patient is queued in a department.
/// Consumed by: ConsultationService (doctor dashboard), NotificationService
/// </summary>
public class PatientQueuedEvent : IntegrationEvent
{
    public Guid PatientId { get; set; }
    public Guid DepartmentId { get; set; }
    public int QueuePosition { get; set; }
    public string? PatientName { get; set; }
}

/// <summary>
/// Published when an appointment is completed.
/// Consumed by: BillingService (finalize charges)
/// </summary>
public class AppointmentCompletedEvent : IntegrationEvent
{
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
}

/// <summary>
/// Published when an appointment is cancelled.
/// Consumed by: NotificationService (cancellation notice), BillingService (refund)
/// </summary>
public class AppointmentCancelledEvent : IntegrationEvent
{
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    public string? Reason { get; set; }
}

#endregion

#region Consultation Events

/// <summary>
/// Published when a doctor starts a consultation with a patient.
/// Consumed by: NotificationService
/// </summary>
public class ConsultationStartedEvent : IntegrationEvent
{
    public Guid ConsultationId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid DepartmentId { get; set; }
    public DateTime StartTime { get; set; }
    public string? PatientName { get; set; }
    public string? DoctorName { get; set; }
}

/// <summary>
/// Published when a consultation is completed with diagnosis and orders.
/// Consumed by: LabService, ImagingService, PharmacyService, SurgeryService, BillingService
/// </summary>
public class ConsultationCompletedEvent : IntegrationEvent
{
    public Guid ConsultationId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid DepartmentId { get; set; }
    public DateTime EndTime { get; set; }
    public string? Diagnosis { get; set; }
    public bool HasLabOrder { get; set; }
    public bool HasImagingOrder { get; set; }
    public bool HasSurgeryOrder { get; set; }
    public bool HasPrescription { get; set; }
}

#endregion

#region Lab Events

/// <summary>
/// Published when a doctor orders lab tests for a patient.
/// Consumed by: LabService (receive and process order)
/// </summary>
public class LabTestOrderedEvent : IntegrationEvent
{
    public Guid OrderId { get; set; }
    public Guid ConsultationId { get; set; }
    public Guid PatientId { get; set; }
    public Guid LabTestId { get; set; }
    public string? TestName { get; set; }
    public string? TestCode { get; set; }
    public string? Priority { get; set; }
    public DateTime OrderedAt { get; set; }
}

/// <summary>
/// Published when lab test results are ready.
/// Consumed by: ConsultationService (doctor notification), NotificationService
/// </summary>
public class LabTestResultReadyEvent : IntegrationEvent
{
    public Guid OrderId { get; set; }
    public Guid PatientId { get; set; }
    public Guid ConsultationId { get; set; }
    public string? TestName { get; set; }
    public string? Result { get; set; }
    public bool IsAbnormal { get; set; }
    public DateTime ResultedAt { get; set; }
}

#endregion

#region Imaging Events

/// <summary>
/// Published when imaging (X-ray, CT, MRI, etc.) is ordered.
/// Consumed by: ImagingService
/// </summary>
public class ImagingOrderedEvent : IntegrationEvent
{
    public Guid OrderId { get; set; }
    public Guid ConsultationId { get; set; }
    public Guid PatientId { get; set; }
    public Guid ImagingTypeId { get; set; }
    public string? ImagingType { get; set; }
    public string? ClinicalIndication { get; set; }
    public string? Priority { get; set; }
    public DateTime OrderedAt { get; set; }
}

/// <summary>
/// Published when imaging is completed with results.
/// Consumed by: ConsultationService (doctor notification), NotificationService
/// </summary>
public class ImagingCompletedEvent : IntegrationEvent
{
    public Guid OrderId { get; set; }
    public Guid PatientId { get; set; }
    public Guid ConsultationId { get; set; }
    public string? ImagingType { get; set; }
    public string? Findings { get; set; }
    public DateTime CompletedAt { get; set; }
}

#endregion

#region Surgery Events

/// <summary>
/// Published when surgery is ordered for a patient.
/// Consumed by: SurgeryService
/// </summary>
public class SurgeryOrderedEvent : IntegrationEvent
{
    public Guid OrderId { get; set; }
    public Guid ConsultationId { get; set; }
    public Guid PatientId { get; set; }
    public Guid SurgeryTypeId { get; set; }
    public string? SurgeryType { get; set; }
    public string? Priority { get; set; }
    public DateTime OrderedAt { get; set; }
}

/// <summary>
/// Published when surgery is completed.
/// Consumed by: ConsultationService, BillingService, NotificationService
/// </summary>
public class SurgeryCompletedEvent : IntegrationEvent
{
    public Guid OrderId { get; set; }
    public Guid PatientId { get; set; }
    public string? SurgeryType { get; set; }
    public string? Notes { get; set; }
    public DateTime CompletedAt { get; set; }
}

#endregion

#region Pharmacy Events

/// <summary>
/// Published when a prescription is issued by a doctor.
/// Consumed by: PharmacyService (fulfill prescription)
/// </summary>
public class PrescriptionIssuedEvent : IntegrationEvent
{
    public Guid PrescriptionId { get; set; }
    public Guid ConsultationId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public DateTime IssuedAt { get; set; }
    public List<PrescriptionItemDto> Items { get; set; } = [];
}

/// <summary>
/// DTO for prescription items within integration events
/// </summary>
public class PrescriptionItemDto
{
    public Guid MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public string? Duration { get; set; }
    public string? Instructions { get; set; }
}

/// <summary>
/// Published when medicine is dispensed by pharmacy.
/// Consumed by: BillingService (add charge), NotificationService
/// </summary>
public class MedicineDispensedEvent : IntegrationEvent
{
    public Guid PrescriptionId { get; set; }
    public Guid PatientId { get; set; }
    public List<DispensedItemDto> Items { get; set; } = [];
    public DateTime DispensedAt { get; set; }
}

/// <summary>
/// DTO for dispensed items within integration events
/// </summary>
public class DispensedItemDto
{
    public Guid MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int DispensedQuantity { get; set; }
}

#endregion

#region Billing Events

/// <summary>
/// Published when a charge is created for a service.
/// Consumed by: BillingService (add to invoice)
/// </summary>
public class ChargeCreatedEvent : IntegrationEvent
{
    public Guid ChargeId { get; set; }
    public Guid PatientId { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

/// <summary>
/// Published when an invoice is generated.
/// Consumed by: NotificationService (send invoice to patient)
/// </summary>
public class InvoiceGeneratedEvent : IntegrationEvent
{
    public Guid InvoiceId { get; set; }
    public Guid PatientId { get; set; }
    public decimal TotalAmount { get; set; }
}

/// <summary>
/// Published when a payment is processed.
/// Consumed by: NotificationService (receipt), AppointmentService (can discharge)
/// </summary>
public class PaymentProcessedEvent : IntegrationEvent
{
    public Guid PaymentId { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid PatientId { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTime PaymentDate { get; set; }
}

#endregion
