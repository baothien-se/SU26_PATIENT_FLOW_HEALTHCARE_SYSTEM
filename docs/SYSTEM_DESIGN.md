# Hệ Thống Quản Lý Bệnh Viện - Thiết Kế Chi Tiết

## 1. TỔNG QUAN HỆ THỐNG

### 1.1 Mục Đích
Xây dựng một hệ thống workflow hoàn chỉnh quản lý toàn bộ quy trình khám chữa bệnh từ khi bệnh nhân đăng ký cho đến khi khám xong/xuất viện.

### 1.2 Kiến Trúc Chung
```
┌─────────────────────────────────────────────────────────────┐
│                    CLIENT APPLICATIONS                      │
│         (Web, Mobile, Desktop Admin Panel)                  │
└────────────────────┬────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────────┐
│              API GATEWAY (YARP)                             │
│     - Route management                                      │
│     - Request aggregation                                   │
│     - Rate limiting                                         │
└────────────────────┬────────────────────────────────────────┘
                     │
    ┌────────────────┼────────────────┬──────────────┐
    │                │                │              │
    ▼                ▼                ▼              ▼
┌─────────┐  ┌─────────────┐  ┌──────────────┐  ┌──────────┐
│Identity │  │  Patient    │  │ Appointment │  │Department│
│Service  │  │Registration │  │   Service   │  │Service   │
└─────────┘  │   Service   │  └──────────────┘  └──────────┘
             └─────────────┘
                │       │            │
                ▼       ▼            ▼
            ┌──────────────────────────────────┐
            │    RabbitMQ Event Bus             │
            │  - PatientRegistered             │
            │  - AppointmentScheduled          │
            │  - ConsultationStarted           │
            │  - LabResultReady                │
            │  - etc...                        │
            └──────────────────────────────────┘
                │              │              │
    ┌───────────┼──────────────┼──────────────┴──────────┐
    │           │              │                         │
    ▼           ▼              ▼                         ▼
┌──────────┐ ┌────────┐ ┌──────────┐ ┌───────────┐ ┌─────────┐
│Consult   │ │Lab     │ │Imaging   │ │Surgery    │ │Pharmacy │
│Service   │ │Service │ │Service   │ │Service    │ │Service  │
└──────────┘ └────────┘ └──────────┘ └───────────┘ └─────────┘
    │                                                 │
    └─────────────────┬──────────────────────────────┘
                      │
                      ▼
            ┌──────────────────┐
            │ Billing Service  │
            │ Pharmacy Service │
            └──────────────────┘
```

## 2. CHI TIẾT CÁC MICROSERVICE

### 2.1 Identity Service ✅ (Đã có nền tảng)
**Chức năng:**
- Quản lý người dùng (bác sĩ, y tá, nhân viên hành chính)
- Xác thực/phân quyền (JWT)
- Role-based access control (RBAC)

**Entities (Domain):**
```csharp
- User (tài khoản người dùng)
- Role (vai trò: doctor, nurse, admin, etc.)
- Permission (quyền hạn)
- UserRole (gán role cho user)
- Department (phòng ban người dùng thuộc)
```

**Cần bổ sung:**
- Entity Domain đầy đủ
- Seed data cho roles/permissions
- Endpoints API đầy đủ

---

### 2.2 Patient Registration Service (Mới)
**Chức năng:**
- Đăng ký bệnh nhân mới
- Cập nhật thông tin bệnh nhân
- Tạo barcode/ID bệnh nhân
- Quản lý hồ sơ bệnh nhân
- Kiểm tra bảo hiểm

**Entities:**
```csharp
- Patient (thông tin bệnh nhân)
- PatientBarcode (barcode + ID để scan)
- PatientMedicalHistory (tiền sử bệnh lý)
- PatientInsurance (bảo hiểm)
- EmergencyContact (liên hệ khẩn cấp)
- Allergy (dị ứng)
```

**Domain Events:**
- PatientRegisteredEvent
- PatientProfileUpdatedEvent
- BarcodeGeneratedEvent

**Luồng xử lý:**
```
1. Tiếp tân nhập liệu bệnh nhân
2. Hệ thống tạo barcode
3. PublishEvent: PatientRegistered
4. Các service khác subscribe để chuẩn bị
5. Bệnh nhân được add vào hệ thống
```

---

### 2.3 Appointment Service (Mới)
**Chức năng:**
- Lịch khám giữa các phòng
- Quản lý người khám (bác sĩ)
- Quản lý slot khám
- Hàng chờ bệnh nhân
- Thông báo hẹn khám

**Entities:**
```csharp
- Appointment (cuộc hẹn khám)
- AppointmentSlot (slot thời gian khám)
- Queue (hàng chờ bệnh nhân)
- Doctor (thông tin bác sĩ)
- DoctorSchedule (lịch làm việc)
```

**Domain Events:**
- AppointmentScheduledEvent
- AppointmentReminderEvent
- PatientQueuedEvent
- AppointmentCompletedEvent

**Luồng xử lý:**
```
1. Bệnh nhân scan barcode → hệ thống nhận ID
2. Tạo appointment cho phòng khám tiếp theo
3. PublishEvent: AppointmentScheduled
4. SignalR gửi thông báo real-time cho bác sĩ
5. Bệnh nhân add vào queue
```

---

### 2.4 Consultation Service (Mới)
**Chức năng:**
- Dashboard cho bác sĩ xem bệnh nhân cần khám
- Ghi chép hồ sơ bệnh nhân
- Chẩn đoán ban đầu
- Kê đơn xét nghiệm/chụp/phẫu thuật
- Tạo đơn thuốc

**Entities:**
```csharp
- Consultation (cuộc khám)
- ConsultationNotes (ghi chép khám)
- Diagnosis (chẩn đoán)
- LabTestOrder (đơn xét nghiệm)
- ImagingOrder (đơn chụp)
- SurgeryOrder (đơn phẫu thuật)
- Prescription (đơn thuốc)
```

**Domain Events:**
- ConsultationStartedEvent
- ConsultationCompletedEvent
- LabTestOrderedEvent
- ImagingOrderedEvent
- SurgeryOrderedEvent
- PrescriptionIssuedEvent

**Luồng xử lý:**
```
1. Bác sĩ mở dashboard consultation
2. SignalR show bệnh nhân chờ khám (real-time)
3. Bác sĩ click bệnh nhân → mở hồ sơ
4. Ghi chép/chẩn đoán
5. Nếu cần: Kê xét nghiệm → PublishEvent: LabTestOrdered
6. Hoặc kê chụp → PublishEvent: ImagingOrdered
7. Hoặc kê phẫu thuật → PublishEvent: SurgeryOrdered
8. Hoặc kê thuốc → PublishEvent: PrescriptionIssued
9. PublishEvent: ConsultationCompleted
```

---

### 2.5 Lab Service (Mới)
**Chức năng:**
- Nhận đơn xét nghiệm từ bác sĩ
- Quản lý mẫu xét nghiệm
- Xử lý test
- Upload kết quả
- Gửi báo cáo cho bác sĩ

**Entities:**
```csharp
- LabTest (loại test)
- LabTestOrder (đơn test)
- Sample (mẫu xét nghiệm)
- LabTestResult (kết quả test)
- LabTestReport (báo cáo)
```

**Domain Events:**
- LabTestOrderReceivedEvent
- SampleCollectedEvent
- LabTestProcessingEvent
- LabTestResultReadyEvent
- LabReportGeneratedEvent

**Luồng xử lý:**
```
1. Subscribe: LabTestOrderedEvent
2. Nhân viên lab nhận đơn
3. Hướng dẫn bệnh nhân lấy mẫu
4. PublishEvent: SampleCollected
5. Lab xử lý mẫu
6. PublishEvent: LabTestProcessing
7. Nhập kết quả
8. PublishEvent: LabTestResultReady
9. Tạo báo cáo + PublishEvent: LabReportGenerated
10. Bác sĩ notification thông qua SignalR
```

---

### 2.6 Imaging Service (Mới)
**Chức năng:**
- Nhận đơn chụp/siêu âm
- Quản lý thiết bị chụp
- Lịch chụp
- Lưu trữ ảnh
- Nhận xét từ bác sĩ chuyên khoa

**Entities:**
```csharp
- ImagingType (loại chụp: X-ray, CT, MRI, Ultrasound)
- ImagingOrder (đơn chụp)
- ImagingSession (buổi chụp)
- ImagingImage (file ảnh)
- ImagingReport (báo cáo chụp)
- Radiologist (bác sĩ chụp/siêu âm)
```

**Domain Events:**
- ImagingOrderReceivedEvent
- ImagingSessionScheduledEvent
- ImagingCompletedEvent
- ImagingReportGeneratedEvent

---

### 2.7 Surgery Service (Mới)
**Chức năng:**
- Quản lý phòng mổ
- Lịch phẫu thuật
- Danh sách chuẩn bị phẫu thuật
- Ghi chép quá trình phẫu thuật
- Theo dõi hậu phẫu

**Entities:**
```csharp
- SurgeryType (loại phẫu thuật)
- SurgeryOrder (đơn mổ)
- OperatingRoom (phòng mổ)
- SurgerySchedule (lịch phẫu thuật)
- SurgeryTeam (đội phẫu thuật)
- SurgeryNotes (ghi chép phẫu thuật)
- PostOperativeCare (chăm sóc sau mổ)
```

**Domain Events:**
- SurgeryOrderReceivedEvent
- SurgeryScheduledEvent
- SurgeryStartedEvent
- SurgeryCompletedEvent
- PostOperativeCareOrderedEvent

---

### 2.8 Pharmacy Service (Mới)
**Chức năng:**
- Nhận đơn thuốc từ bác sĩ
- Quản lý kho thuốc
- Cấp phát thuốc
- Quản lý hết hạn
- Hướng dẫn dùng thuốc

**Entities:**
```csharp
- Medicine (thông tin thuốc)
- Prescription (đơn thuốc)
- PrescriptionItem (chi tiết từng loại thuốc)
- MedicineInventory (kho hàng)
- MedicineDispense (cấp phát thuốc)
- MedicineWarning (cảnh báo/tương tác)
```

**Domain Events:**
- PrescriptionReceivedEvent
- MedicineDispensedEvent
- MedicineInventoryLowEvent
- PrescriptionFulfilledEvent

---

### 2.9 Billing Service (Mới)
**Chức năng:**
- Tính phí dịch vụ y tế
- Quản lý chi phí nhập viện
- Xử lý thanh toán
- Tạo hóa đơn
- Quản lý bảo hiểm claim

**Entities:**
```csharp
- Service (dịch vụ y tế + giá)
- InvoiceItem (mục thanh toán)
- Invoice (hóa đơn)
- Payment (thanh toán)
- InsuranceClaim (yêu cầu bảo hiểm)
- CostCenter (trung tâm chi phí)
```

**Domain Events:**
- ChargeCreatedEvent
- InvoiceGeneratedEvent
- PaymentProcessedEvent
- InsuranceClaimSubmittedEvent

---

### 2.10 Notification Service (Mới)
**Chức năng:**
- Gửi thông báo đa kênh (Email, SMS, In-app, Real-time)
- Template notification
- Quản lý subscription
- Lịch sử thông báo

**Event Listeners:**
- Lắng nghe tất cả events từ các service khác
- Quyết định gửi thông báo cho ai, khi nào
- SignalR cho notifications real-time

**Domain Events (Publish):**
- NotificationSentEvent
- NotificationFailedEvent

---

## 3. SCHEMA DATABASE

### 3.1 Core Tables (Dùng chung)

```sql
-- Identity
CREATE TABLE [Users] (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(100) UNIQUE NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    DepartmentId INT,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME DEFAULT GETUTCDATE()
);

CREATE TABLE [Roles] (
    RoleId INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(100) UNIQUE NOT NULL,
    Description NVARCHAR(500)
);

CREATE TABLE [Permissions] (
    PermissionId INT PRIMARY KEY IDENTITY(1,1),
    PermissionName NVARCHAR(100) UNIQUE NOT NULL,
    Description NVARCHAR(500)
);

CREATE TABLE [UserRoles] (
    UserRoleId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);

CREATE TABLE [RolePermissions] (
    RolePermissionId INT PRIMARY KEY IDENTITY(1,1),
    RoleId INT NOT NULL,
    PermissionId INT NOT NULL,
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId),
    FOREIGN KEY (PermissionId) REFERENCES Permissions(PermissionId)
);

-- Departments
CREATE TABLE [Departments] (
    DepartmentId INT PRIMARY KEY IDENTITY(1,1),
    DepartmentName NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500),
    HeadDoctorId INT,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY (HeadDoctorId) REFERENCES Users(UserId)
);

-- Patient Management
CREATE TABLE [Patients] (
    PatientId INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(200) NOT NULL,
    DateOfBirth DATE NOT NULL,
    Gender NVARCHAR(10),
    PhoneNumber NVARCHAR(20),
    Email NVARCHAR(100),
    Address NVARCHAR(500),
    City NVARCHAR(100),
    Province NVARCHAR(100),
    PostalCode NVARCHAR(20),
    IdentityNumber NVARCHAR(50),
    BarcodeId NVARCHAR(50) UNIQUE,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME DEFAULT GETUTCDATE()
);

CREATE TABLE [PatientMedicalHistory] (
    MedicalHistoryId INT PRIMARY KEY IDENTITY(1,1),
    PatientId INT NOT NULL,
    Condition NVARCHAR(500),
    DiagnosisDate DATE,
    Status NVARCHAR(50),
    Notes NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY (PatientId) REFERENCES Patients(PatientId)
);

CREATE TABLE [Allergies] (
    AllergyId INT PRIMARY KEY IDENTITY(1,1),
    PatientId INT NOT NULL,
    AllergyName NVARCHAR(200) NOT NULL,
    Severity NVARCHAR(50),
    Reaction NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY (PatientId) REFERENCES Patients(PatientId)
);

-- Appointments & Queue
CREATE TABLE [AppointmentSlots] (
    SlotId INT PRIMARY KEY IDENTITY(1,1),
    DepartmentId INT NOT NULL,
    DoctorId INT NOT NULL,
    SlotDate DATE NOT NULL,
    SlotTime TIME NOT NULL,
    IsAvailable BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY (DepartmentId) REFERENCES Departments(DepartmentId),
    FOREIGN KEY (DoctorId) REFERENCES Users(UserId)
);

CREATE TABLE [Appointments] (
    AppointmentId INT PRIMARY KEY IDENTITY(1,1),
    PatientId INT NOT NULL,
    SlotId INT NOT NULL,
    DepartmentId INT NOT NULL,
    Status NVARCHAR(50) DEFAULT 'Scheduled',
    Notes NVARCHAR(MAX),
    ScheduledAt DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY (PatientId) REFERENCES Patients(PatientId),
    FOREIGN KEY (SlotId) REFERENCES AppointmentSlots(SlotId),
    FOREIGN KEY (DepartmentId) REFERENCES Departments(DepartmentId)
);

CREATE TABLE [Queues] (
    QueueId INT PRIMARY KEY IDENTITY(1,1),
    AppointmentId INT NOT NULL,
    PatientId INT NOT NULL,
    DepartmentId INT NOT NULL,
    QueuePosition INT,
    Status NVARCHAR(50) DEFAULT 'Waiting',
    CheckedInAt DATETIME,
    CalledAt DATETIME,
    CompletedAt DATETIME,
    FOREIGN KEY (AppointmentId) REFERENCES Appointments(AppointmentId),
    FOREIGN KEY (PatientId) REFERENCES Patients(PatientId),
    FOREIGN KEY (DepartmentId) REFERENCES Departments(DepartmentId)
);

-- Consultations
CREATE TABLE [Consultations] (
    ConsultationId INT PRIMARY KEY IDENTITY(1,1),
    PatientId INT NOT NULL,
    DoctorId INT NOT NULL,
    DepartmentId INT NOT NULL,
    AppointmentId INT,
    StartTime DATETIME,
    EndTime DATETIME,
    Status NVARCHAR(50) DEFAULT 'Pending',
    Notes NVARCHAR(MAX),
    Diagnosis NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY (PatientId) REFERENCES Patients(PatientId),
    FOREIGN KEY (DoctorId) REFERENCES Users(UserId),
    FOREIGN KEY (DepartmentId) REFERENCES Departments(DepartmentId),
    FOREIGN KEY (AppointmentId) REFERENCES Appointments(AppointmentId)
);

-- Lab
CREATE TABLE [LabTests] (
    LabTestId INT PRIMARY KEY IDENTITY(1,1),
    TestCode NVARCHAR(50) UNIQUE NOT NULL,
    TestName NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500),
    SampleType NVARCHAR(100),
    Price DECIMAL(10,2),
    IsActive BIT DEFAULT 1
);

CREATE TABLE [LabTestOrders] (
    OrderId INT PRIMARY KEY IDENTITY(1,1),
    ConsultationId INT NOT NULL,
    PatientId INT NOT NULL,
    LabTestId INT NOT NULL,
    Status NVARCHAR(50) DEFAULT 'Pending',
    OrderedAt DATETIME DEFAULT GETUTCDATE(),
    CompletedAt DATETIME,
    FOREIGN KEY (ConsultationId) REFERENCES Consultations(ConsultationId),
    FOREIGN KEY (PatientId) REFERENCES Patients(PatientId),
    FOREIGN KEY (LabTestId) REFERENCES LabTests(LabTestId)
);

CREATE TABLE [LabTestResults] (
    ResultId INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    SampleId NVARCHAR(100),
    Result NVARCHAR(MAX),
    NormalRange NVARCHAR(200),
    IsAbnormal BIT,
    ReportedAt DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY (OrderId) REFERENCES LabTestOrders(OrderId)
);

-- Imaging
CREATE TABLE [ImagingTypes] (
    ImagingTypeId INT PRIMARY KEY IDENTITY(1,1),
    TypeName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    Price DECIMAL(10,2)
);

CREATE TABLE [ImagingOrders] (
    OrderId INT PRIMARY KEY IDENTITY(1,1),
    ConsultationId INT NOT NULL,
    PatientId INT NOT NULL,
    ImagingTypeId INT NOT NULL,
    Status NVARCHAR(50) DEFAULT 'Pending',
    OrderedAt DATETIME DEFAULT GETUTCDATE(),
    CompletedAt DATETIME,
    FOREIGN KEY (ConsultationId) REFERENCES Consultations(ConsultationId),
    FOREIGN KEY (PatientId) REFERENCES Patients(PatientId),
    FOREIGN KEY (ImagingTypeId) REFERENCES ImagingTypes(ImagingTypeId)
);

CREATE TABLE [ImagingImages] (
    ImageId INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    FilePath NVARCHAR(MAX),
    UploadedAt DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY (OrderId) REFERENCES ImagingOrders(OrderId)
);

CREATE TABLE [ImagingReports] (
    ReportId INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    RadiologistId INT,
    Findings NVARCHAR(MAX),
    Impression NVARCHAR(MAX),
    ReportedAt DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY (OrderId) REFERENCES ImagingOrders(OrderId),
    FOREIGN KEY (RadiologistId) REFERENCES Users(UserId)
);

-- Surgery
CREATE TABLE [SurgeryTypes] (
    SurgeryTypeId INT PRIMARY KEY IDENTITY(1,1),
    TypeName NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500),
    EstimatedDuration INT, -- phút
    Price DECIMAL(10,2)
);

CREATE TABLE [OperatingRooms] (
    RoomId INT PRIMARY KEY IDENTITY(1,1),
    RoomName NVARCHAR(100) NOT NULL,
    Capacity INT,
    Equipment NVARCHAR(MAX),
    IsAvailable BIT DEFAULT 1
);

CREATE TABLE [SurgeryOrders] (
    OrderId INT PRIMARY KEY IDENTITY(1,1),
    ConsultationId INT NOT NULL,
    PatientId INT NOT NULL,
    SurgeryTypeId INT NOT NULL,
    Status NVARCHAR(50) DEFAULT 'Pending',
    OrderedAt DATETIME DEFAULT GETUTCDATE(),
    ScheduledAt DATETIME,
    CompletedAt DATETIME,
    FOREIGN KEY (ConsultationId) REFERENCES Consultations(ConsultationId),
    FOREIGN KEY (PatientId) REFERENCES Patients(PatientId),
    FOREIGN KEY (SurgeryTypeId) REFERENCES SurgeryTypes(SurgeryTypeId)
);

CREATE TABLE [SurgerySchedules] (
    ScheduleId INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    RoomId INT NOT NULL,
    SurgeonId INT NOT NULL,
    StartTime DATETIME NOT NULL,
    EndTime DATETIME,
    Status NVARCHAR(50) DEFAULT 'Scheduled',
    FOREIGN KEY (OrderId) REFERENCES SurgeryOrders(OrderId),
    FOREIGN KEY (RoomId) REFERENCES OperatingRooms(RoomId),
    FOREIGN KEY (SurgeonId) REFERENCES Users(UserId)
);

CREATE TABLE [SurgeryNotes] (
    NoteId INT PRIMARY KEY IDENTITY(1,1),
    ScheduleId INT NOT NULL,
    PreOperativeNotes NVARCHAR(MAX),
    IntraOperativeNotes NVARCHAR(MAX),
    PostOperativeNotes NVARCHAR(MAX),
    Complications NVARCHAR(MAX),
    RecordedAt DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY (ScheduleId) REFERENCES SurgerySchedules(ScheduleId)
);

-- Pharmacy
CREATE TABLE [Medicines] (
    MedicineId INT PRIMARY KEY IDENTITY(1,1),
    MedicineName NVARCHAR(200) NOT NULL,
    ActiveIngredient NVARCHAR(200),
    Dosage NVARCHAR(100),
    Form NVARCHAR(50),
    Manufacturer NVARCHAR(200),
    Price DECIMAL(10,2),
    IsActive BIT DEFAULT 1
);

CREATE TABLE [Prescriptions] (
    PrescriptionId INT PRIMARY KEY IDENTITY(1,1),
    ConsultationId INT NOT NULL,
    PatientId INT NOT NULL,
    DoctorId INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETUTCDATE(),
    ExpiryDate DATE,
    Status NVARCHAR(50) DEFAULT 'Active',
    FOREIGN KEY (ConsultationId) REFERENCES Consultations(ConsultationId),
    FOREIGN KEY (PatientId) REFERENCES Patients(PatientId),
    FOREIGN KEY (DoctorId) REFERENCES Users(UserId)
);

CREATE TABLE [PrescriptionItems] (
    ItemId INT PRIMARY KEY IDENTITY(1,1),
    PrescriptionId INT NOT NULL,
    MedicineId INT NOT NULL,
    Quantity INT NOT NULL,
    Dosage NVARCHAR(100),
    Frequency NVARCHAR(100),
    Duration NVARCHAR(100),
    Instructions NVARCHAR(MAX),
    FOREIGN KEY (PrescriptionId) REFERENCES Prescriptions(PrescriptionId),
    FOREIGN KEY (MedicineId) REFERENCES Medicines(MedicineId)
);

-- Billing
CREATE TABLE [Services] (
    ServiceId INT PRIMARY KEY IDENTITY(1,1),
    ServiceName NVARCHAR(200) NOT NULL,
    ServiceCode NVARCHAR(50) UNIQUE,
    Price DECIMAL(10,2) NOT NULL,
    Category NVARCHAR(100),
    IsActive BIT DEFAULT 1
);

CREATE TABLE [Invoices] (
    InvoiceId INT PRIMARY KEY IDENTITY(1,1),
    PatientId INT NOT NULL,
    InvoiceDate DATETIME DEFAULT GETUTCDATE(),
    DueDate DATE,
    TotalAmount DECIMAL(12,2),
    PaidAmount DECIMAL(12,2) DEFAULT 0,
    Status NVARCHAR(50) DEFAULT 'Pending',
    FOREIGN KEY (PatientId) REFERENCES Patients(PatientId)
);

CREATE TABLE [InvoiceItems] (
    ItemId INT PRIMARY KEY IDENTITY(1,1),
    InvoiceId INT NOT NULL,
    ServiceId INT,
    Description NVARCHAR(500),
    Quantity INT,
    UnitPrice DECIMAL(10,2),
    Amount DECIMAL(12,2),
    FOREIGN KEY (InvoiceId) REFERENCES Invoices(InvoiceId),
    FOREIGN KEY (ServiceId) REFERENCES Services(ServiceId)
);

CREATE TABLE [Payments] (
    PaymentId INT PRIMARY KEY IDENTITY(1,1),
    InvoiceId INT NOT NULL,
    PaymentDate DATETIME DEFAULT GETUTCDATE(),
    Amount DECIMAL(12,2) NOT NULL,
    PaymentMethod NVARCHAR(50),
    TransactionId NVARCHAR(100),
    Status NVARCHAR(50) DEFAULT 'Success',
    FOREIGN KEY (InvoiceId) REFERENCES Invoices(InvoiceId)
);
```

## 4. DOMAIN EVENTS & MESSAGE QUEUE

### 4.1 RabbitMQ Exchanges & Routing Keys

```
Exchange: hospital.events (Direct)

Routing Keys:
- patient.registered
- patient.profile.updated
- appointment.scheduled
- appointment.reminder
- consultation.started
- consultation.completed
- consultation.lab-ordered
- consultation.imaging-ordered
- consultation.surgery-ordered
- consultation.prescription-issued
- lab.order.received
- lab.sample.collected
- lab.test.processing
- lab.result.ready
- lab.report.generated
- imaging.order.received
- imaging.session.scheduled
- imaging.completed
- imaging.report.generated
- surgery.order.received
- surgery.scheduled
- surgery.started
- surgery.completed
- surgery.post-op-ordered
- pharmacy.prescription.received
- pharmacy.medicine.dispensed
- pharmacy.inventory.low
- billing.charge.created
- billing.invoice.generated
- payment.processed
- notification.sent
```

### 4.2 Event Message Format

```json
{
  "eventId": "guid",
  "eventType": "string",
  "aggregateId": "integer (PatientId hoặc ConsultationId)",
  "aggregateType": "string",
  "timestamp": "datetime",
  "version": "integer",
  "correlationId": "guid",
  "causationId": "guid",
  "data": {
    // Event-specific data
  },
  "metadata": {
    "userId": "integer",
    "departmentId": "integer",
    "source": "service_name"
  }
}
```

## 5. CQRS PATTERN

### 5.1 Commands (Write Operations)

**PatientRegistration Service:**
- `RegisterPatientCommand`
- `UpdatePatientProfileCommand`
- `GenerateBarcodeCommand`

**Appointment Service:**
- `ScheduleAppointmentCommand`
- `UpdateAppointmentCommand`
- `CancelAppointmentCommand`
- `AddToQueueCommand`

**Consultation Service:**
- `StartConsultationCommand`
- `CompleteConsultationCommand`
- `RecordDiagnosisCommand`
- `IssuePrescriptionCommand`

**Lab Service:**
- `ReceiveLabTestOrderCommand`
- `RecordLabSampleCommand`
- `EnterLabResultCommand`
- `GenerateLabReportCommand`

... (tương tự cho các service khác)

### 5.2 Queries (Read Operations)

**PatientRegistration Service:**
- `GetPatientByIdQuery`
- `GetPatientByBarcodeQuery`
- `GetPatientMedicalHistoryQuery`

**Appointment Service:**
- `GetAppointmentsQuery`
- `GetAvailableSlotsQuery`
- `GetQueueQuery`

**Consultation Service:**
- `GetPendingConsultationsQuery`
- `GetConsultationDetailsQuery`
- `GetPatientHistoryQuery`

... (tương tự cho các service khác)

## 6. SIGNALR HUB CONNECTIONS

```csharp
// Consultation Hub
- consultations/pending
- consultations/queue-updated
- consultations/patient-called

// Notification Hub
- notifications/new
- notifications/alert
- appointments/reminder

// Lab Hub
- lab/result-ready
- lab/sample-status

// Imaging Hub
- imaging/scheduled
- imaging/completed

// Surgery Hub
- surgery/started
- surgery/completed

// Billing Hub
- billing/invoice-ready
- payment/processed
```

## 7. API GATEWAY ROUTES

```yaml
Routes:
  /api/auth/* -> IdentityService
  /api/patients/* -> PatientRegistrationService
  /api/appointments/* -> AppointmentService
  /api/consultations/* -> ConsultationService
  /api/lab/* -> LabService
  /api/imaging/* -> ImagingService
  /api/surgery/* -> SurgeryService
  /api/pharmacy/* -> PharmacyService
  /api/billing/* -> BillingService
```

## 8. IMPLEMENTATION PHASES

### **Phase 1: Foundation (Week 1-2)**
- ✅ ApiGateway
- ✅ IdentityService
- [x] Shared libraries (Events, DTOs, Common)
- [ ] Database setup & migrations

### **Phase 2: Core Patient Flow (Week 3-5)**
- [ ] PatientRegistrationService
- [ ] AppointmentService
- [ ] Notification integration

### **Phase 3: Clinical Services (Week 6-10)**
- [ ] ConsultationService
- [ ] LabService
- [ ] ImagingService
- [ ] SurgeryService

### **Phase 4: Support Services (Week 11-12)**
- [ ] PharmacyService
- [ ] BillingService
- [ ] Notification Service

### **Phase 5: Integration & Testing (Week 13-14)**
- [ ] RabbitMQ integration
- [ ] SignalR setup
- [ ] End-to-end testing
- [ ] Docker & Kubernetes

## 9. BƯỚC KẾ TIẾP

Bắt đầu từ **Phase 1**: Tạo shared libraries và setup database schema.

---

**LƯU Ý:**
- Mỗi service là hoàn toàn độc lập, có database riêng
- Giao tiếp giữa services thông qua Events + RabbitMQ
- SignalR dùng cho real-time notifications
- API Gateway là gateway duy nhất cho clients
