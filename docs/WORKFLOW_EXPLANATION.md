# Hospital Management System - Detailed Workflow & Component Explanation

**Mục đích**: Giải thích chi tiết ý nghĩa của từng component được tạo, cách service giao tiếp, và toàn bộ workflow bệnh nhân từ đăng ký đến xuất viện.

---

## 📚 Table of Contents

1. [Component Architecture](#component-architecture)
2. [Service Communication](#service-communication)
3. [Real Workflow Examples](#real-workflow-examples)
4. [Data Flow Diagrams](#data-flow-diagrams)
5. [Event-Driven Architecture](#event-driven-architecture)

---

## 1. Component Architecture

### 1.1 Shared.Domain Layer

**Mục đích**: Cung cấp các base class cho toàn bộ domain entities trên tất cả services.

#### `Entity.cs` - Base Entity Class

```csharp
public abstract class Entity
{
    public int Id { get; protected set; }                    // Unique ID
    public DateTime CreatedAt { get; protected set; }        // When created
    public DateTime? UpdatedAt { get; protected set; }       // When updated
    public int? CreatedBy { get; protected set; }            // Who created
    public int? UpdatedBy { get; protected set; }            // Who updated
    public bool IsDeleted { get; protected set; }            // Soft delete flag
    public DateTime? DeletedAt { get; protected set; }       // When deleted
}
```

**Tại sao cần?**

- Tất cả domain entities (Patient, Consultation, LabTest, etc.) đều kế thừa từ Entity
- Đảm bảo consistency trong cách quản lý audit fields
- Hỗ trợ soft delete (không xóa vĩnh viễn, chỉ mark IsDeleted=true)

**Ví dụ sử dụng:**

```csharp
// PatientRegistrationService/Domain/Entities/Patient.cs
public class Patient : Entity
{
    public string FullName { get; private set; }
    public string BarcodeId { get; private set; }
    // ...
}

// Khi bệnh nhân đăng ký, ta tạo:
var patient = new Patient { FullName = "Nguyen Van A", ... };
// patient.Id được tạo tự động (IDENTITY trong DB)
// patient.CreatedAt = DateTime.UtcNow (hệ thống tự set)
// patient.CreatedBy = currentUserId (từ ICurrentUserService)
```

---

#### `AggregateRoot.cs` - Aggregate Root Class

```csharp
public abstract class AggregateRoot : Entity
{
    private readonly List<DomainEvent> _uncommittedEvents = [];

    // Khi có thay đổi quan trọng, raise event
    protected void RaiseDomainEvent(DomainEvent domainEvent)
    {
        _uncommittedEvents.Add(domainEvent);
    }

    // Sau khi save vào DB, clear events
    public void ClearUncommittedEvents()
    {
        _uncommittedEvents.Clear();
    }
}
```

**Tại sao cần?**

- Aggregate Root là thực thể chính trong DDD (Domain-Driven Design)
- Quản lý internal state changes thông qua events
- Cho phép publish events khi data thay đổi

**Ví dụ sử dụng - Consultation Aggregate:**

```csharp
// ConsultationService/Domain/Aggregates/Consultation.cs
public class Consultation : AggregateRoot
{
    public int PatientId { get; private set; }
    public int DoctorId { get; private set; }
    public string Diagnosis { get; private set; }

    public void CompleteDiagnosis(string diagnosis)
    {
        Diagnosis = diagnosis;

        // Raise event - điều này sẽ được publish sau
        RaiseDomainEvent(new ConsultationCompletedEvent
        {
            ConsultationId = this.Id,
            PatientId = this.PatientId,
            Diagnosis = diagnosis
        });
    }
}
```

**Khi Consultation complete:**

```
1. Bác sĩ điền chẩn đoán → gọi consultation.CompleteDiagnosis()
2. AggregateRoot track event: ConsultationCompletedEvent
3. Command handler lưu vào DB
4. Sau đó publish event → RabbitMQ
5. Lab Service, Pharmacy Service, Billing Service nhận event này
```

---

#### `ValueObject.cs` - Value Objects

**Mục đích**: Represent các giá trị không thay đổi (immutable)

```csharp
public class PhoneNumber : ValueObject
{
    public string Number { get; }

    public PhoneNumber(string number)
    {
        if (!IsValid(number)) throw new ArgumentException();
        Number = number;
    }

    // Value Objects so sánh bằng giá trị, không phải bằng ID
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Number;
    }
}
```

**Sử dụng:**

```csharp
var phone1 = new PhoneNumber("0912345678");
var phone2 = new PhoneNumber("0912345678");
phone1 == phone2  // true (value equality, không cần same object ID)
```

---

#### `DomainEvent.cs` - Domain Events

```csharp
public abstract class DomainEvent
{
    public Guid EventId { get; }              // Unique event ID
    public DateTime OccurredAt { get; }       // When event happened
    public Guid CorrelationId { get; set; }   // Track related events
    public Dictionary<string, object> Metadata { get; set; }
}
```

**Mục đích**: Represent thay đổi quan trọng trong domain

**Ví dụ Domain Events:**

- `PatientRegisteredEvent` - Khi bệnh nhân đăng ký
- `ConsultationCompletedEvent` - Khi bác sĩ khám xong
- `LabTestOrderedEvent` - Khi bác sĩ kê xét nghiệm

---

### 1.2 Shared.Application Layer

**Mục đích**: Cung cấp interfaces và base classes cho application logic

#### `IRepository<TEntity>` - Generic Repository

```csharp
public interface IRepository<TEntity> where TEntity : Entity
{
    Task<TEntity?> GetByIdAsync(int id);
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<TEntity> AddAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task DeleteAsync(TEntity entity);  // Soft delete
}
```

**Tại sao cần?**

- Abstraction để không depend vào specific database implementation
- Cho phép mock trong testing
- Centralize data access logic

**Sử dụng trong Console Application:**

```csharp
// PatientRegistrationService/Application/Commands/RegisterPatientCommand.cs
public class RegisterPatientCommandHandler
    : IRequestHandler<RegisterPatientCommand, Result<int>>
{
    private readonly IRepository<Patient> _repository;

    public async Task<Result<int>> Handle(RegisterPatientCommand request, ...)
    {
        // Gọi repository để lưu patient mới
        var patient = Patient.Create(request.FullName, request.PhoneNumber);
        var created = await _repository.AddAsync(patient);

        return Result<int>.Success(created.Id);
    }
}
```

---

#### `ICurrentUserService` - Current User Context

```csharp
public interface ICurrentUserService
{
    int? UserId { get; }              // Bác sĩ/nhân viên hiện tại
    string? UserName { get; }
    string? UserEmail { get; }
    int? DepartmentId { get; }        // Khoa/phòng
    bool IsInRole(string role);       // Kiểm tra quyền
}
```

**Mục đích**: Lấy thông tin người dùng hiện tại (từ JWT Token)

**Ví dụ:**

```csharp
// Khi bác sĩ tạo consultation, auto ghi DoctorId
public class CreateConsultationCommandHandler
    : IRequestHandler<CreateConsultationCommand, Result<int>>
{
    private readonly ICurrentUserService _currentUser;

    public async Task<Result<int>> Handle(...)
    {
        var consultation = new Consultation
        {
            PatientId = request.PatientId,
            DoctorId = _currentUser.UserId.Value,  // Lấy từ token
            DepartmentId = _currentUser.DepartmentId.Value
        };
        // ...
    }
}
```

---

#### `IUnitOfWork` - Transaction Management

```csharp
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task BeginTransactionAsync(CancellationToken cancellationToken);
    Task CommitTransactionAsync(CancellationToken cancellationToken);
    Task RollbackTransactionAsync(CancellationToken cancellationToken);
}
```

**Tại sao cần?**

- Đảm bảo dữ liệu consistency trong một transaction
- Nếu save fail, auto rollback tất cả changes

**Ví dụ - Tạo Consultation và publish events:**

```csharp
public async Task<Result<int>> Handle(CreateConsultationCommand request, ...)
{
    await _unitOfWork.BeginTransactionAsync();

    try {
        // 1. Tạo consultation
        var consultation = Consultation.Create(...);
        await _consultationRepository.AddAsync(consultation);

        // 2. Update patient status
        var patient = await _patientRepository.GetByIdAsync(request.PatientId);
        patient.UpdateStatus("InConsultation");
        await _patientRepository.UpdateAsync(patient);

        // 3. Save tất cả changes
        await _unitOfWork.SaveChangesAsync();

        // 4. Publish events (sau khi save thành công)
        var events = consultation.GetUncommittedEvents();
        await _eventPublisher.PublishAsync(events);

        await _unitOfWork.CommitTransactionAsync();
        return Result<int>.Success(consultation.Id);
    }
    catch {
        await _unitOfWork.RollbackTransactionAsync();
        throw;
    }
}
```

---

#### `IDomainEventPublisher` & `IIntegrationEventPublisher`

**IDomainEventPublisher** - Publish events trong cùng một service
**IIntegrationEventPublisher** - Publish events qua RabbitMQ (tới services khác)

```csharp
// Trong handler
var events = consultation.GetUncommittedEvents();

// 1. Publish domain events trong service
foreach (var e in events)
    await _domainEventPublisher.PublishAsync(e);

// 2. Publish integration events qua RabbitMQ
var integrationEvent = new ConsultationCompletedEvent
{
    ConsultationId = consultation.Id,
    ...
};
await _integrationEventPublisher.PublishAsync(
    integrationEvent,
    "consultation.completed"
);
```

---

### 1.3 Shared.Events Layer

**Mục đích**: Define tất cả integration events (giao tiếp giữa services)

```csharp
public abstract class IntegrationEvent
{
    public Guid EventId { get; }                  // ID duy nhất
    public DateTime OccurredAt { get; }           // Thời gian xảy ra
    public Guid CorrelationId { get; set; }       // Correlate related events
    public string? SourceService { get; set; }    // Service nào publish
}
```

**Events defined:**

```csharp
// 1. Patient workflow
public class PatientRegisteredEvent : IntegrationEvent
{
    public int PatientId { get; set; }
    public string? FullName { get; set; }
    public string? BarcodeId { get; set; }  // Barcode để scan
}

// 2. Appointment workflow
public class AppointmentScheduledEvent : IntegrationEvent
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public int DepartmentId { get; set; }
    public DateTime AppointmentDateTime { get; set; }
}

// 3. Consultation workflow
public class ConsultationCompletedEvent : IntegrationEvent
{
    public int ConsultationId { get; set; }
    public int PatientId { get; set; }
    public string? Diagnosis { get; set; }
    public bool HasLabOrder { get; set; }         // Có kê xét nghiệm?
    public bool HasImagingOrder { get; set; }     // Có chụp chiếu?
    public bool HasPrescription { get; set; }     // Có kê đơn?
}

// 4. Lab workflow
public class LabTestOrderedEvent : IntegrationEvent
{
    public int OrderId { get; set; }
    public int ConsultationId { get; set; }
    public string? TestName { get; set; }
}

public class LabTestResultReadyEvent : IntegrationEvent
{
    public int OrderId { get; set; }
    public string? Result { get; set; }
}

// 5. Pharmacy workflow
public class PrescriptionIssuedEvent : IntegrationEvent
{
    public int PrescriptionId { get; set; }
    public int PatientId { get; set; }
    public List<PrescriptionItemDto> Items { get; set; }
}

// 6. Billing workflow
public class PaymentProcessedEvent : IntegrationEvent
{
    public int PaymentId { get; set; }
    public decimal Amount { get; set; }
}
```

---

## 2. Service Communication

### 2.1 Synchronous Communication (API Calls)

**Khi nào dùng?**: Cần response ngay lập tức

**Ví dụ:**

```
1. Client → API Gateway
2. API Gateway → PatientRegistrationService (GET /api/patients/{id})
   PatientService xử lý ngay và return response
```

**Hạn chế**: Services tightly coupled

---

### 2.2 Asynchronous Communication (Event-Driven)

**Khi nào dùng?**: Không cần response ngay, cho phép services independent

**Flow:**

```
1. ConsultationService publish: ConsultationCompletedEvent
2. Event → RabbitMQ (Message Broker)
3. LabService, ImagingService, PharmacyService subscribe
4. Mỗi service handle event một cách independent
```

**Lợi ích:**

- ✅ Services loosely coupled
- ✅ Asynchronous processing (không block)
- ✅ Scalable (có thể thêm subscribers mà không modify publisher)

---

### 2.3 RabbitMQ Setup

```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "Exchange": "hospital.events",
    "Routes": {
      "consultation.completed": "ConsultationCompletedEvent",
      "lab.order.received": "LabTestOrderedEvent",
      "imaging.order.received": "ImagingOrderedEvent",
      ...
    }
  }
}
```

**Exchange Type**: Direct

- Publisher → Exchange → RoutingKey → Queue → Subscribers

**Ví dụ Routing:**

```
ConsultationService publish:
    Exchange: hospital.events
    RoutingKey: "consultation.completed"
    Event: ConsultationCompletedEvent

Subscribers:
    - LabService (queue: lab.events) ← listen "consultation.completed"
    - PharmacyService (queue: pharmacy.events) ← listen "consultation.completed"
    - BillingService (queue: billing.events) ← listen "consultation.completed"
```

---

## 3. Real Workflow Examples

### Scenario 1: 👨‍⚕️ Bệnh nhân khám Răng

**Bệnh nhân: Nguyễn Văn A**
**Khoa: Nha Khoa (Dental Department)**

#### Step 1️⃣: Đăng Ký Bệnh Nhân

```
CLIENT (Tiếp Tân App)
  ↓
  POST /api/patients/register
  Body: {
    fullName: "Nguyễn Văn A",
    phoneNumber: "0912345678",
    email: "a@gmail.com"
  }
  ↓
API GATEWAY (YARP)
  ↓ Route to PatientRegistrationService
PATIENT REGISTRATION SERVICE
  ↓
  Command: RegisterPatientCommand
  ↓
  Handler: RegisterPatientCommandHandler
    1. Create Patient aggregate
    2. Generate unique BarcodeId (e.g., "PAT-2024-001")
    3. Save to Database
    4. Get uncommitted events
    ↓
    Event: PatientRegisteredEvent {
      PatientId: 1,
      FullName: "Nguyễn Văn A",
      BarcodeId: "PAT-2024-001"
    }
    ↓
    5. Publish to RabbitMQ
  ↓
  Response: { success: true, patientId: 1, barcodeId: "PAT-2024-001" }
  ↓
CLIENT receives ✓
```

**Subscribers nhận PatientRegisteredEvent:**

- ✓ AppointmentService → Prepare để booking appointment
- ✓ NotificationService → Send welcome email
- ✓ BillingService → Create patient account in billing system

---

#### Step 2️⃣: Đặt Lịch Khám (Appointment)

```
RECEPTION STAFF (Appointment Booking)
  ↓
  POST /api/appointments/schedule
  Body: {
    patientId: 1,
    departmentId: 5,  // Nha Khoa
    preferredDate: "2026-06-30"
  }
  ↓
API GATEWAY
  ↓
APPOINTMENT SERVICE
  ↓
  Command: ScheduleAppointmentCommand
  ↓
  Handler: ScheduleAppointmentCommandHandler
    1. Validate patient exists
    2. Check doctor availability in Nha Khoa department
    3. Create AppointmentSlot if needed
    4. Create Appointment record
    5. Add to Queue
    ↓
    Events:
    - AppointmentScheduledEvent
    - PatientQueuedEvent
    ↓
    6. Publish to RabbitMQ
  ↓
  Response: { appointmentId: 100, queuePosition: 5 }
  ↓
RECEPTION STAFF sees ✓
```

**Database Inserts:**

```sql
INSERT INTO Appointments VALUES (100, 1, slotId, 5, 'Scheduled', ...)
INSERT INTO Queues VALUES (queueId, 100, 1, 5, 5, 'Waiting', ...)
INSERT INTO AppointmentSlots VALUES (slotId, 5, doctorId, '2026-06-30', '09:00', 0, ...)
```

**Subscribers nhận AppointmentScheduledEvent:**

- ✓ NotificationService → SMS to patient: "Your appointment scheduled for 2026-06-30 09:00"
- ✓ DoctorDashboardService → Show in doctor's schedule

---

#### Step 3️⃣: Bệnh nhân Tới Tiếp Tân

```
RECEPTIONIST (Patient Check-in)
  ↓
  Scan barcode từ patient ID card
  BarCode: "PAT-2024-001"
  ↓
  POST /api/queue/check-in
  Body: { barcodeId: "PAT-2024-001" }
  ↓
API GATEWAY
  ↓
APPOINTMENT SERVICE (get queue info)
  ↓
  Query: GetQueueByBarcodeQuery
  ↓
  Response: {
    queueId: queueId,
    patientName: "Nguyễn Văn A",
    appointmentId: 100,
    queuePosition: 5,
    estimatedWaitTime: "25 minutes"
  }
  ↓
RECEPTIONIST updates system: CheckedInAt = now
  ↓
  Update Queue: Status = 'Waiting' → queuePosition calculated
  ↓
  Event: PatientCheckedInEvent
  ↓
Subscribers:
  - NotificationService → Display "Gọi bệnh nhân thứ 1" on screen
  - DoctorDashboard → Show queue in real-time (SignalR)
```

---

#### Step 4️⃣: Bác Sĩ Khám Bệnh Nhân

```
DOCTOR (Opens Dashboard)
  ↓
  GET /api/consultations/pending
  ↓
CONSULTATION SERVICE
  ↓
  Query: GetPendingConsultationsQuery
  ↓
  SELECT * FROM Consultations WHERE Status = 'Pending' ORDER BY CreatedAt
  ↓
  Response: {
    consultations: [
      {
        consultationId: 200,
        patientId: 1,
        patientName: "Nguyễn Văn A",
        appointmentId: 100,
        status: "Pending"
      }
    ]
  }
  ↓
DOCTOR clicks on patient → View patient's history
  ↓
  GET /api/patients/{id}/history
  ↓
PATIENT REGISTRATION SERVICE
  ↓
  SELECT * FROM Patients WHERE PatientId = 1
  SELECT * FROM PatientMedicalHistory WHERE PatientId = 1
  SELECT * FROM Allergies WHERE PatientId = 1
  ↓
  Response: Patient full info + history + allergies
```

---

#### Step 5️⃣: Bác Sĩ Khám & Kê Đơn

```
DOCTOR
  ↓
  POST /api/consultations/{id}/complete
  Body: {
    diagnosis: "Răng sâu cần trám",
    notes: "Bệnh nhân cần vệ sinh răng tốt hơn",
    orders: {
      labTests: [],                    // Không cần xét nghiệm
      imaging: [],                     // Không cần chụp
      surgery: [],                     // Không cần mổ
      prescriptions: [
        {
          medicineId: 10,              // Thuốc kháng sinh
          quantity: 10,
          dosage: "1 viên x 3 lần/ngày",
          frequency: "3 lần/ngày",
          duration: "7 ngày"
        }
      ]
    }
  }
  ↓
API GATEWAY
  ↓
CONSULTATION SERVICE
  ↓
  Command: CompleteConsultationCommand
  ↓
  Handler: CompleteConsultationCommandHandler
    1. Load Consultation aggregate
    2. Call: consultation.CompleteDiagnosis(diagnosis)
    3. Create Prescription aggregate
    4. Create PrescriptionItems
    5. Update Consultation status → "Completed"
    ↓
    Events generated:
    - ConsultationCompletedEvent {
        consultationId: 200,
        diagnosis: "Răng sâu cần trám",
        hasLabOrder: false,
        hasImagingOrder: false,
        hasSurgeryOrder: false,
        hasPrescription: true
      }
    - PrescriptionIssuedEvent {
        prescriptionId: 300,
        patientId: 1,
        items: [{ medicineId: 10, quantity: 10, ... }]
      }
    ↓
    6. Save to Database (UnitOfWork)
    7. Publish events to RabbitMQ
  ↓
  Response: { success: true, consultationId: 200 }
  ↓
DOCTOR sees ✓
```

**Database Updates:**

```sql
UPDATE Consultations SET Status='Completed', Diagnosis='Răng sâu cần trám' WHERE ConsultationId=200
INSERT INTO Prescriptions VALUES (300, 200, 1, doctorId, ...)
INSERT INTO PrescriptionItems VALUES (itemId, 300, 10, 10, '1 viên x 3 lần/ngày', ...)
UPDATE Queues SET Status='Completed', CompletedAt=now WHERE QueueId=queueId
```

**Event Publishing to RabbitMQ:**

```
Exchange: hospital.events
RoutingKey: "consultation.completed"
Message: ConsultationCompletedEvent {
  consultationId: 200,
  patientId: 1,
  diagnosis: "Răng sâu cần trám",
  hasPrescription: true
}
```

---

#### Step 6️⃣: Các Services Nhận Events

**🏥 PHARMACY SERVICE** receives PrescriptionIssuedEvent

```
Handler: OnPrescriptionIssuedEventHandler

1. Load prescription details
2. Check medicine inventory
3. Prepare medicine package
4. Notify patient: "Thuốc sẵn sàng, vui lòng tới nhà thuốc"
5. Update status: PrescriptionFulfilled
6. Publish: MedicineDispensedEvent
   → BillingService: Tính phí thuốc
   → NotificationService: Notify patient
```

**💰 BILLING SERVICE** receives:

- ConsultationCompletedEvent → Add consultation fee (200,000 VND)
- MedicineDispensedEvent → Add medicine cost (150,000 VND)

```
INSERT INTO Invoices VALUES (...)
INSERT INTO InvoiceItems VALUES:
  - Item1: Consultation fee 200,000
  - Item2: Antibiotic medicine 150,000
Total: 350,000 VND
```

**📱 NOTIFICATION SERVICE** receives events:

```
- PatientCheckedInEvent → Show: "Bệnh nhân Nguyễn Văn A đã check-in"
- ConsultationCompletedEvent → SMS: "Khám xong! Lấy thuốc tại quầy"
- MedicineDispensedEvent → Notify: "Thuốc sẵn sàng"
- PaymentProcessedEvent → Email receipt
```

---

#### Step 7️⃣: Thanh Toán

```
PATIENT (at counter)
  ↓
  Receptionist creates invoice
  ↓
  POST /api/billing/invoices
  ↓
BILLING SERVICE
  ↓
  Query unpaid items for patient 1
  ↓
  Response: {
    invoiceId: 500,
    patientName: "Nguyễn Văn A",
    items: [
      { description: "Consultation", amount: 200000 },
      { description: "Medicine", amount: 150000 }
    ],
    total: 350000,
    status: "Pending"
  }
  ↓
PATIENT processes payment
  ↓
  POST /api/billing/payments
  Body: {
    invoiceId: 500,
    amount: 350000,
    method: "Cash" or "Card"
  }
  ↓
BILLING SERVICE
  ↓
  Command: ProcessPaymentCommand
  ↓
  Handler:
    1. Verify payment details
    2. Process payment (if card, call payment gateway)
    3. Update invoice: Status = 'Paid'
    4. Record payment transaction
    ↓
    Event: PaymentProcessedEvent {
      paymentId: 1,
      invoiceId: 500,
      patientId: 1,
      amount: 350000
    }
    ↓
    5. Publish event
  ↓
  Response: { success: true, transactionId: "TXN-2024-001" }
```

**Subscribers:**

- NotificationService → Print receipt & Email
- AppointmentService → Mark as completed
- AnalyticsService → Log transaction for reporting

---

#### Step 8️⃣: Complete Flow Diagram

```
┌─────────────────┐
│ Patient Check-in│  (Scan barcode PAT-2024-001)
└────────┬────────┘
         │
         ▼
┌─────────────────────────────────────────────────┐
│ APPOINTMENT SERVICE                             │
│ - Update Queue status: Waiting                  │
│ - Publish: PatientCheckedInEvent                │
└────────────┬────────────────────────────────────┘
             │
    ┌────────┴─────────┐
    │                  │
    ▼                  ▼
┌────────────┐   ┌──────────────────┐
│DOCTOR      │   │NOTIFICATION      │
│DASHBOARD   │   │SERVICE (SignalR) │
│(SignalR)   │   │Display "Gọi BN"  │
└────────────┘   └──────────────────┘
    │
    │ Doctor clicks patient
    ▼
┌──────────────────────────────────────────────┐
│ CONSULTATION SERVICE                         │
│ - Load pending consultations                 │
│ - Show patient history                       │
│ - Doctor enters diagnosis & prescription     │
└────────┬─────────────────────────────────────┘
         │
         ▼
    ┌────────────────────────────────────────┐
    │ Publish Events to RabbitMQ             │
    │ 1. ConsultationCompletedEvent          │
    │ 2. PrescriptionIssuedEvent             │
    └───┬──────────────┬──────────────┬──────┘
        │              │              │
        ▼              ▼              ▼
    ┌──────────┐  ┌────────────┐  ┌────────┐
    │PHARMACY  │  │BILLING     │  │NOTIF   │
    │SERVICE   │  │SERVICE     │  │SERVICE │
    │✓Fulfill  │  │✓Create     │  │✓SMS    │
    │✓Dispense │  │✓Invoice    │  │✓Email  │
    └──────────┘  └────────────┘  └────────┘
        │              │
        └──────────┬───┘
                   ▼
            ┌────────────────┐
            │PATIENT PAYMENT │
            │COUNTER         │
            └────────┬───────┘
                     │
                     ▼
            ┌────────────────┐
            │ Process Payment│
            └────────┬───────┘
                     │
                     ▼
            ┌────────────────┐
            │Receipt Printed │
            │Patient Leave ✓ │
            └────────────────┘
```

---

### Scenario 2: 🩺 Bệnh nhân Khám Bệnh Nặng (Cần Xét Nghiệm)

```
Same as Scenario 1, but at Step 5:

DOCTOR diagnosis:
  - Symptoms: Sốt cao, ho, mệt mỏi
  - Suspected: Cúm hoặc viêm phổi
  - Needs: Blood test, X-Ray chest

Events generated:
  - ConsultationCompletedEvent (hasPrescription: true)
  - LabTestOrderedEvent (CBC, CRP blood tests)
  - ImagingOrderedEvent (Chest X-Ray)

↓ RabbitMQ routing ↓

LAB SERVICE receives LabTestOrderedEvent
  → Notify patient: "Vui lòng đến phòng lấy máu"
  → Record sample details
  → Process test
  → Upload results → LabTestResultReadyEvent

IMAGING SERVICE receives ImagingOrderedEvent
  → Schedule chest X-Ray
  → Notify patient: "Vui lòng đến chụp X-Ray"
  → Process imaging
  → Radiologist report → ImagingCompletedEvent

NOTIFICATION SERVICE
  → SMS: "Vui lòng xét nghiệm tại Lab"
  → SMS: "Vui lòng chụp X-Ray tại Radiology"

BILLING SERVICE
  → Add lab test fee
  → Add imaging fee
  → Calculate total invoice

PATIENT FLOW:
1. See doctor (Consultation)
2. Go to Lab (Blood sample)
3. Go to Imaging (X-Ray)
4. Wait for results
5. Review results with doctor (Follow-up)
6. Pay at counter
7. Get medicine from pharmacy
8. Leave
```

---

## 4. Data Flow Diagrams

### Request-Response Flow (Synchronous)

```
CLIENT
  │
  └─→ HTTP POST /api/patients/register
       │
       └─→ API GATEWAY (YARP)
            │
            ├─ Route: /api/patients/* → PatientRegistrationService
            │
            └─→ PATIENT REGISTRATION SERVICE
                 │
                 ├─ AuthN/AuthZ (check JWT token)
                 │
                 ├─ RegisterPatientCommand (CQRS)
                 │
                 ├─ Validation (FluentValidation)
                 │
                 ├─ Handler processes
                 │  ├─ Entity.Create()
                 │  ├─ IRepository.AddAsync()
                 │  └─ IUnitOfWork.SaveChangesAsync()
                 │
                 └─→ Response ← HTTP 200 OK
```

### Event-Driven Flow (Asynchronous)

```
CONSULTATION SERVICE (Publisher)
│
├─ ConsultationCompletedEvent
│
├─ IIntegrationEventPublisher.PublishAsync()
│
└─→ RabbitMQ (Message Broker)
    │
    Exchange: hospital.events
    RoutingKey: "consultation.completed"
    │
    ├─→ Queue: lab.events ──→ LAB SERVICE handler
    │                         │
    │                         └─ OnConsultationCompletedHandler
    │                            └─ Process lab orders
    │
    ├─→ Queue: pharmacy.events ──→ PHARMACY SERVICE handler
    │                              │
    │                              └─ OnConsultationCompletedHandler
    │                                 └─ Prepare medicines
    │
    ├─→ Queue: billing.events ──→ BILLING SERVICE handler
    │                             │
    │                             └─ OnConsultationCompletedHandler
    │                                └─ Create invoice
    │
    └─→ Queue: notification.events ──→ NOTIFICATION SERVICE handler
                                       │
                                       └─ OnConsultationCompletedHandler
                                          └─ Send SMS/Email/Push
```

---

## 5. Event-Driven Architecture

### 5.1 Event Publishing

```csharp
// ConsultationService/Application/Commands/CompleteConsultationCommandHandler.cs

public class CompleteConsultationCommandHandler
    : IRequestHandler<CompleteConsultationCommand, Result<int>>
{
    private readonly IRepository<Consultation> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIntegrationEventPublisher _eventPublisher;

    public async Task<Result<int>> Handle(
        CompleteConsultationCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Load aggregate
        var consultation = await _repository.GetByIdAsync(request.ConsultationId);

        // 2. Execute business logic (raises domain events internally)
        consultation.CompleteDiagnosis(request.Diagnosis);

        // 3. Persist changes
        await _repository.UpdateAsync(consultation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 4. Get uncommitted events
        var events = consultation.GetUncommittedEvents();

        // 5. Convert to integration events & publish
        foreach (var e in events)
        {
            if (e is ConsultationCompletedDomainEvent domainEvent)
            {
                var integrationEvent = new ConsultationCompletedEvent
                {
                    ConsultationId = domainEvent.ConsultationId,
                    PatientId = domainEvent.PatientId,
                    Diagnosis = domainEvent.Diagnosis
                };

                await _eventPublisher.PublishAsync(
                    integrationEvent,
                    "consultation.completed",
                    cancellationToken);
            }
        }

        // 6. Clear uncommitted events
        consultation.ClearUncommittedEvents();

        return Result<int>.Success(consultation.Id, "Consultation completed");
    }
}
```

### 5.2 Event Subscribing (Handler)

```csharp
// LabService/Application/EventHandlers/OnConsultationCompletedHandler.cs

public class OnConsultationCompletedHandler
    : IIntegrationEventHandler<ConsultationCompletedEvent>
{
    private readonly ILabTestOrderRepository _labOrderRepository;
    private readonly INotificationService _notificationService;

    public async Task HandleAsync(
        ConsultationCompletedEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        // 1. Check if consultation has lab orders
        if (!integrationEvent.HasLabOrder)
            return;

        // 2. Get pending lab orders for this consultation
        var labOrders = await _labOrderRepository
            .GetByConsultationAsync(integrationEvent.ConsultationId, cancellationToken);

        foreach (var order in labOrders)
        {
            // 3. Process each order
            order.MarkAsReceived();  // Change status to "SampleCollected"

            // 4. Notify patient
            await _notificationService.NotifyPatientAsync(
                order.PatientId,
                $"Vui lòng đến Lab để lấy máu cho test {order.LabTest.TestName}",
                cancellationToken);

            // 5. Save changes
            await _labOrderRepository.UpdateAsync(order, cancellationToken);
        }
    }
}
```

### 5.3 RabbitMQ Configuration

```csharp
// Shared.Infrastructure/RabbitMQ/RabbitMQConfiguration.cs

public static void ConfigureRabbitMQ(this IServiceCollection services)
{
    // Define exchange
    var exchange = new ExchangeDefinition
    {
        Name = "hospital.events",
        Type = "direct",
        Durable = true
    };

    // Define queues
    var queues = new Dictionary<string, QueueDefinition>
    {
        ["lab.events"] = new QueueDefinition { Name = "lab.events", Durable = true },
        ["imaging.events"] = new QueueDefinition { Name = "imaging.events", Durable = true },
        ["pharmacy.events"] = new QueueDefinition { Name = "pharmacy.events", Durable = true },
        ["billing.events"] = new QueueDefinition { Name = "billing.events", Durable = true },
        ["notification.events"] = new QueueDefinition { Name = "notification.events", Durable = true }
    };

    // Define bindings
    var bindings = new Dictionary<string, BindingDefinition>
    {
        ["consultation-completed-to-lab"] = new BindingDefinition
        {
            Queue = "lab.events",
            Exchange = "hospital.events",
            RoutingKey = "consultation.completed"
        },
        ["consultation-completed-to-pharmacy"] = new BindingDefinition
        {
            Queue = "pharmacy.events",
            Exchange = "hospital.events",
            RoutingKey = "consultation.completed"
        },
        // ... more bindings
    };

    // Register with DI
    services.AddSingleton(exchange);
    services.AddSingleton(queues);
    services.AddSingleton(bindings);
}
```

---

## Summary

**Phase 1 đã tạo ra:**

1. ✅ **Domain Layer**: Entity, AggregateRoot, ValueObject, DomainEvent
2. ✅ **Application Layer**: Repository, UnitOfWork, Services
3. ✅ **Event Layer**: Integration events định nghĩa rõ ràng
4. ✅ **Communication Pattern**: Sync (HTTP) + Async (RabbitMQ)

**Khi Phase 2 start:**

1. 🚀 Tạo UserService → RBAC
2. 🚀 Tạo PatientRegistrationService → Patient workflow
3. 🚀 Tạo AppointmentService → Queue management
4. 🚀 Tạo ConsultationService → Doctor dashboard
5. 🚀 Tạo LabService, ImagingService, PharmacyService, BillingService
6. 🚀 Integrate SignalR cho real-time updates

**Mỗi service sẽ:**

- Publish integration events khi có thay đổi
- Subscribe events từ services khác
- Operate independently (loose coupling)
- Scale independently

---

**End of Workflow Explanation**
