-- ============================================
-- HOSPITAL MANAGEMENT SYSTEM - DATABASE SCHEMA
-- Target: SQL Server
-- Version: 1.0
-- ============================================

-- Drop existing tables (for development/testing)
-- Warning: This will delete all data!
-- USE WITH CAUTION IN PRODUCTION

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RolePermissions]') AND type in (N'U'))
    DROP TABLE [dbo].[RolePermissions]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserRoles]') AND type in (N'U'))
    DROP TABLE [dbo].[UserRoles]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Permissions]') AND type in (N'U'))
    DROP TABLE [dbo].[Permissions]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Roles]') AND type in (N'U'))
    DROP TABLE [dbo].[Roles]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
    DROP TABLE [dbo].[Users]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Departments]') AND type in (N'U'))
    DROP TABLE [dbo].[Departments]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Patients]') AND type in (N'U'))
    DROP TABLE [dbo].[Patients]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PatientMedicalHistory]') AND type in (N'U'))
    DROP TABLE [dbo].[PatientMedicalHistory]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Allergies]') AND type in (N'U'))
    DROP TABLE [dbo].[Allergies]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AppointmentSlots]') AND type in (N'U'))
    DROP TABLE [dbo].[AppointmentSlots]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Appointments]') AND type in (N'U'))
    DROP TABLE [dbo].[Appointments]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Queues]') AND type in (N'U'))
    DROP TABLE [dbo].[Queues]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Consultations]') AND type in (N'U'))
    DROP TABLE [dbo].[Consultations]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LabTests]') AND type in (N'U'))
    DROP TABLE [dbo].[LabTests]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LabTestOrders]') AND type in (N'U'))
    DROP TABLE [dbo].[LabTestOrders]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LabTestResults]') AND type in (N'U'))
    DROP TABLE [dbo].[LabTestResults]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ImagingTypes]') AND type in (N'U'))
    DROP TABLE [dbo].[ImagingTypes]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ImagingOrders]') AND type in (N'U'))
    DROP TABLE [dbo].[ImagingOrders]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ImagingImages]') AND type in (N'U'))
    DROP TABLE [dbo].[ImagingImages]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ImagingReports]') AND type in (N'U'))
    DROP TABLE [dbo].[ImagingReports]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SurgeryTypes]') AND type in (N'U'))
    DROP TABLE [dbo].[SurgeryTypes]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OperatingRooms]') AND type in (N'U'))
    DROP TABLE [dbo].[OperatingRooms]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SurgeryOrders]') AND type in (N'U'))
    DROP TABLE [dbo].[SurgeryOrders]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SurgerySchedules]') AND type in (N'U'))
    DROP TABLE [dbo].[SurgerySchedules]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SurgeryNotes]') AND type in (N'U'))
    DROP TABLE [dbo].[SurgeryNotes]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Medicines]') AND type in (N'U'))
    DROP TABLE [dbo].[Medicines]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Prescriptions]') AND type in (N'U'))
    DROP TABLE [dbo].[Prescriptions]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PrescriptionItems]') AND type in (N'U'))
    DROP TABLE [dbo].[PrescriptionItems]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Services]') AND type in (N'U'))
    DROP TABLE [dbo].[Services]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Invoices]') AND type in (N'U'))
    DROP TABLE [dbo].[Invoices]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceItems]') AND type in (N'U'))
    DROP TABLE [dbo].[InvoiceItems]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Payments]') AND type in (N'U'))
    DROP TABLE [dbo].[Payments]

-- ============================================
-- IDENTITY & AUTHORIZATION TABLES
-- ============================================

-- Roles Table
CREATE TABLE [dbo].[Roles] (
    [RoleId] INT PRIMARY KEY IDENTITY(1,1),
    [RoleName] NVARCHAR(100) UNIQUE NOT NULL,
    [Description] NVARCHAR(500),
    [CreatedAt] DATETIME DEFAULT GETUTCDATE()
);

-- Permissions Table
CREATE TABLE [dbo].[Permissions] (
    [PermissionId] INT PRIMARY KEY IDENTITY(1,1),
    [PermissionName] NVARCHAR(100) UNIQUE NOT NULL,
    [Description] NVARCHAR(500),
    [CreatedAt] DATETIME DEFAULT GETUTCDATE()
);

-- Role-Permission Mapping
CREATE TABLE [dbo].[RolePermissions] (
    [RolePermissionId] INT PRIMARY KEY IDENTITY(1,1),
    [RoleId] INT NOT NULL,
    [PermissionId] INT NOT NULL,
    FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles]([RoleId]) ON DELETE CASCADE,
    FOREIGN KEY ([PermissionId]) REFERENCES [dbo].[Permissions]([PermissionId]) ON DELETE CASCADE,
    UNIQUE ([RoleId], [PermissionId])
);

-- Departments Table
CREATE TABLE [dbo].[Departments] (
    [DepartmentId] INT PRIMARY KEY IDENTITY(1,1),
    [DepartmentName] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(500),
    [HeadDoctorId] INT,
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME DEFAULT GETUTCDATE()
);

-- Users Table
CREATE TABLE [dbo].[Users] (
    [UserId] INT PRIMARY KEY IDENTITY(1,1),
    [Username] NVARCHAR(100) UNIQUE NOT NULL,
    [Email] NVARCHAR(100) UNIQUE NOT NULL,
    [PasswordHash] NVARCHAR(MAX) NOT NULL,
    [FirstName] NVARCHAR(100),
    [LastName] NVARCHAR(100),
    [FullName] NVARCHAR(200),
    [DepartmentId] INT,
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME DEFAULT GETUTCDATE(),
    [LastLoginAt] DATETIME,
    FOREIGN KEY ([DepartmentId]) REFERENCES [dbo].[Departments]([DepartmentId]),
    INDEX [IX_Users_Email] NONCLUSTERED ([Email]),
    INDEX [IX_Users_Username] NONCLUSTERED ([Username])
);

-- User-Role Mapping
CREATE TABLE [dbo].[UserRoles] (
    [UserRoleId] INT PRIMARY KEY IDENTITY(1,1),
    [UserId] INT NOT NULL,
    [RoleId] INT NOT NULL,
    [AssignedAt] DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([UserId]) ON DELETE CASCADE,
    FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles]([RoleId]) ON DELETE CASCADE,
    UNIQUE ([UserId], [RoleId])
);

-- Update Departments HeadDoctor reference
ALTER TABLE [dbo].[Departments]
ADD FOREIGN KEY ([HeadDoctorId]) REFERENCES [dbo].[Users]([UserId]);

-- ============================================
-- PATIENT MANAGEMENT TABLES
-- ============================================

-- Patients Table
CREATE TABLE [dbo].[Patients] (
    [PatientId] INT PRIMARY KEY IDENTITY(1,1),
    [FullName] NVARCHAR(200) NOT NULL,
    [DateOfBirth] DATE NOT NULL,
    [Gender] NVARCHAR(10), -- M, F, Other
    [PhoneNumber] NVARCHAR(20),
    [Email] NVARCHAR(100),
    [Address] NVARCHAR(500),
    [City] NVARCHAR(100),
    [Province] NVARCHAR(100),
    [PostalCode] NVARCHAR(20),
    [IdentityNumber] NVARCHAR(50),
    [BarcodeId] NVARCHAR(50) UNIQUE,
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME DEFAULT GETUTCDATE(),
    [IsDeleted] BIT DEFAULT 0,
    [DeletedAt] DATETIME,
    INDEX [IX_Patients_BarcodeId] NONCLUSTERED ([BarcodeId]),
    INDEX [IX_Patients_IdentityNumber] NONCLUSTERED ([IdentityNumber])
);

-- Patient Medical History Table
CREATE TABLE [dbo].[PatientMedicalHistory] (
    [MedicalHistoryId] INT PRIMARY KEY IDENTITY(1,1),
    [PatientId] INT NOT NULL,
    [Condition] NVARCHAR(500),
    [DiagnosisDate] DATE,
    [Status] NVARCHAR(50), -- Active, Resolved, Chronic
    [Notes] NVARCHAR(MAX),
    [CreatedAt] DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY ([PatientId]) REFERENCES [dbo].[Patients]([PatientId]) ON DELETE CASCADE
);

-- Allergies Table
CREATE TABLE [dbo].[Allergies] (
    [AllergyId] INT PRIMARY KEY IDENTITY(1,1),
    [PatientId] INT NOT NULL,
    [AllergyName] NVARCHAR(200) NOT NULL,
    [Severity] NVARCHAR(50), -- Mild, Moderate, Severe
    [Reaction] NVARCHAR(MAX),
    [CreatedAt] DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY ([PatientId]) REFERENCES [dbo].[Patients]([PatientId]) ON DELETE CASCADE
);

-- ============================================
-- APPOINTMENT & QUEUE TABLES
-- ============================================

-- Appointment Slots Table
CREATE TABLE [dbo].[AppointmentSlots] (
    [SlotId] INT PRIMARY KEY IDENTITY(1,1),
    [DepartmentId] INT NOT NULL,
    [DoctorId] INT NOT NULL,
    [SlotDate] DATE NOT NULL,
    [SlotTime] TIME NOT NULL,
    [IsAvailable] BIT DEFAULT 1,
    [CreatedAt] DATETIME DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY ([DepartmentId]) REFERENCES [dbo].[Departments]([DepartmentId]),
    FOREIGN KEY ([DoctorId]) REFERENCES [dbo].[Users]([UserId]),
    INDEX [IX_AppointmentSlots_SlotDate] NONCLUSTERED ([SlotDate]),
    INDEX [IX_AppointmentSlots_DoctorId] NONCLUSTERED ([DoctorId])
);

-- Appointments Table
CREATE TABLE [dbo].[Appointments] (
    [AppointmentId] INT PRIMARY KEY IDENTITY(1,1),
    [PatientId] INT NOT NULL,
    [SlotId] INT NOT NULL,
    [DepartmentId] INT NOT NULL,
    [Status] NVARCHAR(50) DEFAULT 'Scheduled', -- Scheduled, InProgress, Completed, Cancelled
    [Notes] NVARCHAR(MAX),
    [ScheduledAt] DATETIME DEFAULT GETUTCDATE(),
    [CancelledAt] DATETIME,
    [CancellationReason] NVARCHAR(500),
    [IsDeleted] BIT DEFAULT 0,
    [DeletedAt] DATETIME,
    FOREIGN KEY ([PatientId]) REFERENCES [dbo].[Patients]([PatientId]),
    FOREIGN KEY ([SlotId]) REFERENCES [dbo].[AppointmentSlots]([SlotId]),
    FOREIGN KEY ([DepartmentId]) REFERENCES [dbo].[Departments]([DepartmentId]),
    INDEX [IX_Appointments_PatientId] NONCLUSTERED ([PatientId]),
    INDEX [IX_Appointments_Status] NONCLUSTERED ([Status])
);

-- Queue Table
CREATE TABLE [dbo].[Queues] (
    [QueueId] INT PRIMARY KEY IDENTITY(1,1),
    [AppointmentId] INT NOT NULL,
    [PatientId] INT NOT NULL,
    [DepartmentId] INT NOT NULL,
    [QueuePosition] INT,
    [Status] NVARCHAR(50) DEFAULT 'Waiting', -- Waiting, Called, InConsultation, Completed
    [CheckedInAt] DATETIME,
    [CalledAt] DATETIME,
    [CompletedAt] DATETIME,
    [CreatedAt] DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY ([AppointmentId]) REFERENCES [dbo].[Appointments]([AppointmentId]),
    FOREIGN KEY ([PatientId]) REFERENCES [dbo].[Patients]([PatientId]),
    FOREIGN KEY ([DepartmentId]) REFERENCES [dbo].[Departments]([DepartmentId]),
    INDEX [IX_Queues_DepartmentId_Status] NONCLUSTERED ([DepartmentId], [Status]),
    INDEX [IX_Queues_Status] NONCLUSTERED ([Status])
);

-- ============================================
-- CONSULTATION TABLES
-- ============================================

-- Consultations Table
CREATE TABLE [dbo].[Consultations] (
    [ConsultationId] INT PRIMARY KEY IDENTITY(1,1),
    [PatientId] INT NOT NULL,
    [DoctorId] INT NOT NULL,
    [DepartmentId] INT NOT NULL,
    [AppointmentId] INT,
    [StartTime] DATETIME,
    [EndTime] DATETIME,
    [Status] NVARCHAR(50) DEFAULT 'Pending', -- Pending, InProgress, Completed
    [Notes] NVARCHAR(MAX),
    [Diagnosis] NVARCHAR(MAX),
    [CreatedAt] DATETIME DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME DEFAULT GETUTCDATE(),
    [IsDeleted] BIT DEFAULT 0,
    [DeletedAt] DATETIME,
    FOREIGN KEY ([PatientId]) REFERENCES [dbo].[Patients]([PatientId]),
    FOREIGN KEY ([DoctorId]) REFERENCES [dbo].[Users]([UserId]),
    FOREIGN KEY ([DepartmentId]) REFERENCES [dbo].[Departments]([DepartmentId]),
    FOREIGN KEY ([AppointmentId]) REFERENCES [dbo].[Appointments]([AppointmentId]),
    INDEX [IX_Consultations_PatientId] NONCLUSTERED ([PatientId]),
    INDEX [IX_Consultations_DoctorId] NONCLUSTERED ([DoctorId]),
    INDEX [IX_Consultations_Status] NONCLUSTERED ([Status])
);

-- ============================================
-- LAB TABLES
-- ============================================

-- Lab Tests Master Table
CREATE TABLE [dbo].[LabTests] (
    [LabTestId] INT PRIMARY KEY IDENTITY(1,1),
    [TestCode] NVARCHAR(50) UNIQUE NOT NULL,
    [TestName] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(500),
    [SampleType] NVARCHAR(100),
    [Price] DECIMAL(10,2),
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME DEFAULT GETUTCDATE()
);

-- Lab Test Orders
CREATE TABLE [dbo].[LabTestOrders] (
    [OrderId] INT PRIMARY KEY IDENTITY(1,1),
    [ConsultationId] INT NOT NULL,
    [PatientId] INT NOT NULL,
    [LabTestId] INT NOT NULL,
    [Status] NVARCHAR(50) DEFAULT 'Pending', -- Pending, SampleCollected, Processing, Completed, Rejected
    [OrderedAt] DATETIME DEFAULT GETUTCDATE(),
    [SampleCollectedAt] DATETIME,
    [CompletedAt] DATETIME,
    [IsDeleted] BIT DEFAULT 0,
    [DeletedAt] DATETIME,
    FOREIGN KEY ([ConsultationId]) REFERENCES [dbo].[Consultations]([ConsultationId]),
    FOREIGN KEY ([PatientId]) REFERENCES [dbo].[Patients]([PatientId]),
    FOREIGN KEY ([LabTestId]) REFERENCES [dbo].[LabTests]([LabTestId]),
    INDEX [IX_LabTestOrders_PatientId] NONCLUSTERED ([PatientId]),
    INDEX [IX_LabTestOrders_Status] NONCLUSTERED ([Status])
);

-- Lab Test Results
CREATE TABLE [dbo].[LabTestResults] (
    [ResultId] INT PRIMARY KEY IDENTITY(1,1),
    [OrderId] INT NOT NULL,
    [SampleId] NVARCHAR(100),
    [Result] NVARCHAR(MAX),
    [NormalRange] NVARCHAR(200),
    [IsAbnormal] BIT,
    [ReportedAt] DATETIME DEFAULT GETUTCDATE(),
    [IsDeleted] BIT DEFAULT 0,
    [DeletedAt] DATETIME,
    FOREIGN KEY ([OrderId]) REFERENCES [dbo].[LabTestOrders]([OrderId]),
    INDEX [IX_LabTestResults_OrderId] NONCLUSTERED ([OrderId])
);

-- ============================================
-- IMAGING TABLES
-- ============================================

-- Imaging Types Master Table
CREATE TABLE [dbo].[ImagingTypes] (
    [ImagingTypeId] INT PRIMARY KEY IDENTITY(1,1),
    [TypeName] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500),
    [Price] DECIMAL(10,2),
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME DEFAULT GETUTCDATE()
);

-- Imaging Orders
CREATE TABLE [dbo].[ImagingOrders] (
    [OrderId] INT PRIMARY KEY IDENTITY(1,1),
    [ConsultationId] INT NOT NULL,
    [PatientId] INT NOT NULL,
    [ImagingTypeId] INT NOT NULL,
    [Status] NVARCHAR(50) DEFAULT 'Pending', -- Pending, Scheduled, InProgress, Completed, Reported
    [OrderedAt] DATETIME DEFAULT GETUTCDATE(),
    [ScheduledAt] DATETIME,
    [CompletedAt] DATETIME,
    [IsDeleted] BIT DEFAULT 0,
    [DeletedAt] DATETIME,
    FOREIGN KEY ([ConsultationId]) REFERENCES [dbo].[Consultations]([ConsultationId]),
    FOREIGN KEY ([PatientId]) REFERENCES [dbo].[Patients]([PatientId]),
    FOREIGN KEY ([ImagingTypeId]) REFERENCES [dbo].[ImagingTypes]([ImagingTypeId]),
    INDEX [IX_ImagingOrders_PatientId] NONCLUSTERED ([PatientId]),
    INDEX [IX_ImagingOrders_Status] NONCLUSTERED ([Status])
);

-- Imaging Images
CREATE TABLE [dbo].[ImagingImages] (
    [ImageId] INT PRIMARY KEY IDENTITY(1,1),
    [OrderId] INT NOT NULL,
    [FilePath] NVARCHAR(MAX),
    [FileName] NVARCHAR(255),
    [FileSize] BIGINT,
    [UploadedAt] DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY ([OrderId]) REFERENCES [dbo].[ImagingOrders]([OrderId]),
    INDEX [IX_ImagingImages_OrderId] NONCLUSTERED ([OrderId])
);

-- Imaging Reports
CREATE TABLE [dbo].[ImagingReports] (
    [ReportId] INT PRIMARY KEY IDENTITY(1,1),
    [OrderId] INT NOT NULL,
    [RadiologistId] INT,
    [Findings] NVARCHAR(MAX),
    [Impression] NVARCHAR(MAX),
    [ReportedAt] DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY ([OrderId]) REFERENCES [dbo].[ImagingOrders]([OrderId]),
    FOREIGN KEY ([RadiologistId]) REFERENCES [dbo].[Users]([UserId]),
    INDEX [IX_ImagingReports_OrderId] NONCLUSTERED ([OrderId])
);

-- ============================================
-- SURGERY TABLES
-- ============================================

-- Surgery Types Master Table
CREATE TABLE [dbo].[SurgeryTypes] (
    [SurgeryTypeId] INT PRIMARY KEY IDENTITY(1,1),
    [TypeName] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(500),
    [EstimatedDuration] INT, -- in minutes
    [Price] DECIMAL(10,2),
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME DEFAULT GETUTCDATE()
);

-- Operating Rooms
CREATE TABLE [dbo].[OperatingRooms] (
    [RoomId] INT PRIMARY KEY IDENTITY(1,1),
    [RoomName] NVARCHAR(100) NOT NULL,
    [Capacity] INT,
    [Equipment] NVARCHAR(MAX),
    [IsAvailable] BIT DEFAULT 1,
    [CreatedAt] DATETIME DEFAULT GETUTCDATE()
);

-- Surgery Orders
CREATE TABLE [dbo].[SurgeryOrders] (
    [OrderId] INT PRIMARY KEY IDENTITY(1,1),
    [ConsultationId] INT NOT NULL,
    [PatientId] INT NOT NULL,
    [SurgeryTypeId] INT NOT NULL,
    [Status] NVARCHAR(50) DEFAULT 'Pending', -- Pending, Scheduled, InProgress, Completed, Cancelled
    [OrderedAt] DATETIME DEFAULT GETUTCDATE(),
    [ScheduledAt] DATETIME,
    [StartedAt] DATETIME,
    [CompletedAt] DATETIME,
    [CancelledAt] DATETIME,
    [CancellationReason] NVARCHAR(500),
    [IsDeleted] BIT DEFAULT 0,
    [DeletedAt] DATETIME,
    FOREIGN KEY ([ConsultationId]) REFERENCES [dbo].[Consultations]([ConsultationId]),
    FOREIGN KEY ([PatientId]) REFERENCES [dbo].[Patients]([PatientId]),
    FOREIGN KEY ([SurgeryTypeId]) REFERENCES [dbo].[SurgeryTypes]([SurgeryTypeId]),
    INDEX [IX_SurgeryOrders_PatientId] NONCLUSTERED ([PatientId]),
    INDEX [IX_SurgeryOrders_Status] NONCLUSTERED ([Status])
);

-- Surgery Schedules
CREATE TABLE [dbo].[SurgerySchedules] (
    [ScheduleId] INT PRIMARY KEY IDENTITY(1,1),
    [OrderId] INT NOT NULL,
    [RoomId] INT NOT NULL,
    [SurgeonId] INT NOT NULL,
    [StartTime] DATETIME NOT NULL,
    [EndTime] DATETIME,
    [Status] NVARCHAR(50) DEFAULT 'Scheduled', -- Scheduled, InProgress, Completed
    [CreatedAt] DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY ([OrderId]) REFERENCES [dbo].[SurgeryOrders]([OrderId]),
    FOREIGN KEY ([RoomId]) REFERENCES [dbo].[OperatingRooms]([RoomId]),
    FOREIGN KEY ([SurgeonId]) REFERENCES [dbo].[Users]([UserId]),
    INDEX [IX_SurgerySchedules_StartTime] NONCLUSTERED ([StartTime])
);

-- Surgery Notes
CREATE TABLE [dbo].[SurgeryNotes] (
    [NoteId] INT PRIMARY KEY IDENTITY(1,1),
    [ScheduleId] INT NOT NULL,
    [PreOperativeNotes] NVARCHAR(MAX),
    [IntraOperativeNotes] NVARCHAR(MAX),
    [PostOperativeNotes] NVARCHAR(MAX),
    [Complications] NVARCHAR(MAX),
    [RecordedAt] DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY ([ScheduleId]) REFERENCES [dbo].[SurgerySchedules]([ScheduleId]),
    INDEX [IX_SurgeryNotes_ScheduleId] NONCLUSTERED ([ScheduleId])
);

-- ============================================
-- PHARMACY TABLES
-- ============================================

-- Medicines Master Table
CREATE TABLE [dbo].[Medicines] (
    [MedicineId] INT PRIMARY KEY IDENTITY(1,1),
    [MedicineName] NVARCHAR(200) NOT NULL,
    [ActiveIngredient] NVARCHAR(200),
    [Dosage] NVARCHAR(100),
    [Form] NVARCHAR(50), -- Tablet, Capsule, Liquid, Injection
    [Manufacturer] NVARCHAR(200),
    [Price] DECIMAL(10,2),
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME DEFAULT GETUTCDATE(),
    INDEX [IX_Medicines_MedicineName] NONCLUSTERED ([MedicineName])
);

-- Prescriptions
CREATE TABLE [dbo].[Prescriptions] (
    [PrescriptionId] INT PRIMARY KEY IDENTITY(1,1),
    [ConsultationId] INT NOT NULL,
    [PatientId] INT NOT NULL,
    [DoctorId] INT NOT NULL,
    [CreatedAt] DATETIME DEFAULT GETUTCDATE(),
    [ExpiryDate] DATE,
    [Status] NVARCHAR(50) DEFAULT 'Active', -- Active, Fulfilled, Expired, Cancelled
    [IsDeleted] BIT DEFAULT 0,
    [DeletedAt] DATETIME,
    FOREIGN KEY ([ConsultationId]) REFERENCES [dbo].[Consultations]([ConsultationId]),
    FOREIGN KEY ([PatientId]) REFERENCES [dbo].[Patients]([PatientId]),
    FOREIGN KEY ([DoctorId]) REFERENCES [dbo].[Users]([UserId]),
    INDEX [IX_Prescriptions_PatientId] NONCLUSTERED ([PatientId]),
    INDEX [IX_Prescriptions_Status] NONCLUSTERED ([Status])
);

-- Prescription Items
CREATE TABLE [dbo].[PrescriptionItems] (
    [ItemId] INT PRIMARY KEY IDENTITY(1,1),
    [PrescriptionId] INT NOT NULL,
    [MedicineId] INT NOT NULL,
    [Quantity] INT NOT NULL,
    [Dosage] NVARCHAR(100),
    [Frequency] NVARCHAR(100), -- Once daily, Twice daily, etc
    [Duration] NVARCHAR(100), -- 7 days, 2 weeks, etc
    [Instructions] NVARCHAR(MAX),
    [CreatedAt] DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY ([PrescriptionId]) REFERENCES [dbo].[Prescriptions]([PrescriptionId]) ON DELETE CASCADE,
    FOREIGN KEY ([MedicineId]) REFERENCES [dbo].[Medicines]([MedicineId]),
    INDEX [IX_PrescriptionItems_PrescriptionId] NONCLUSTERED ([PrescriptionId])
);

-- ============================================
-- BILLING TABLES
-- ============================================

-- Services Master Table
CREATE TABLE [dbo].[Services] (
    [ServiceId] INT PRIMARY KEY IDENTITY(1,1),
    [ServiceName] NVARCHAR(200) NOT NULL,
    [ServiceCode] NVARCHAR(50) UNIQUE,
    [Price] DECIMAL(10,2) NOT NULL,
    [Category] NVARCHAR(100), -- Consultation, Lab, Imaging, Surgery, Pharmacy
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME DEFAULT GETUTCDATE()
);

-- Invoices
CREATE TABLE [dbo].[Invoices] (
    [InvoiceId] INT PRIMARY KEY IDENTITY(1,1),
    [PatientId] INT NOT NULL,
    [InvoiceDate] DATETIME DEFAULT GETUTCDATE(),
    [DueDate] DATE,
    [TotalAmount] DECIMAL(12,2),
    [PaidAmount] DECIMAL(12,2) DEFAULT 0,
    [Status] NVARCHAR(50) DEFAULT 'Pending', -- Pending, PartiallyPaid, Paid, Overdue, Cancelled
    [Notes] NVARCHAR(500),
    [IsDeleted] BIT DEFAULT 0,
    [DeletedAt] DATETIME,
    FOREIGN KEY ([PatientId]) REFERENCES [dbo].[Patients]([PatientId]),
    INDEX [IX_Invoices_PatientId] NONCLUSTERED ([PatientId]),
    INDEX [IX_Invoices_Status] NONCLUSTERED ([Status])
);

-- Invoice Items
CREATE TABLE [dbo].[InvoiceItems] (
    [ItemId] INT PRIMARY KEY IDENTITY(1,1),
    [InvoiceId] INT NOT NULL,
    [ServiceId] INT,
    [Description] NVARCHAR(500),
    [Quantity] INT,
    [UnitPrice] DECIMAL(10,2),
    [Amount] DECIMAL(12,2),
    [ReferenceId] INT, -- Reference to consultation, lab test order, etc
    [ReferenceType] NVARCHAR(50), -- ConsultationId, LabOrderId, ImagingOrderId, etc
    FOREIGN KEY ([InvoiceId]) REFERENCES [dbo].[Invoices]([InvoiceId]) ON DELETE CASCADE,
    FOREIGN KEY ([ServiceId]) REFERENCES [dbo].[Services]([ServiceId]),
    INDEX [IX_InvoiceItems_InvoiceId] NONCLUSTERED ([InvoiceId])
);

-- Payments
CREATE TABLE [dbo].[Payments] (
    [PaymentId] INT PRIMARY KEY IDENTITY(1,1),
    [InvoiceId] INT NOT NULL,
    [PaymentDate] DATETIME DEFAULT GETUTCDATE(),
    [Amount] DECIMAL(12,2) NOT NULL,
    [PaymentMethod] NVARCHAR(50), -- Cash, Card, Transfer, etc
    [TransactionId] NVARCHAR(100),
    [Status] NVARCHAR(50) DEFAULT 'Success', -- Success, Pending, Failed
    [Notes] NVARCHAR(500),
    [IsDeleted] BIT DEFAULT 0,
    [DeletedAt] DATETIME,
    FOREIGN KEY ([InvoiceId]) REFERENCES [dbo].[Invoices]([InvoiceId]),
    INDEX [IX_Payments_InvoiceId] NONCLUSTERED ([InvoiceId]),
    INDEX [IX_Payments_PaymentDate] NONCLUSTERED ([PaymentDate])
);

-- ============================================
-- INDEXES FOR OPTIMIZATION
-- ============================================

CREATE INDEX [IX_Appointments_ScheduledAt] ON [dbo].[Appointments]([ScheduledAt]);
CREATE INDEX [IX_Consultations_CreatedAt] ON [dbo].[Consultations]([CreatedAt]);
CREATE INDEX [IX_Queues_CreatedAt] ON [dbo].[Queues]([CreatedAt]);
CREATE INDEX [IX_LabTestOrders_OrderedAt] ON [dbo].[LabTestOrders]([OrderedAt]);
CREATE INDEX [IX_ImagingOrders_OrderedAt] ON [dbo].[ImagingOrders]([OrderedAt]);
CREATE INDEX [IX_SurgeryOrders_OrderedAt] ON [dbo].[SurgeryOrders]([OrderedAt]);

-- ============================================
-- INITIAL DATA (SEED DATA)
-- ============================================

-- Insert Roles
INSERT INTO [dbo].[Roles] ([RoleName], [Description]) VALUES
('Administrator', 'System Administrator'),
('Doctor', 'Medical Doctor'),
('Nurse', 'Registered Nurse'),
('Radiologist', 'Imaging Specialist'),
('LabTechnician', 'Laboratory Technician'),
('Pharmacist', 'Pharmacy Staff'),
('Receptionist', 'Front Desk Receptionist'),
('Surgeon', 'Surgical Specialist'),
('DepartmentHead', 'Department Head');

-- Insert Permissions
INSERT INTO [dbo].[Permissions] ([PermissionName], [Description]) VALUES
('View_Patients', 'View patient information'),
('Create_Patient', 'Register new patient'),
('Edit_Patient', 'Edit patient information'),
('Delete_Patient', 'Delete patient records'),
('View_Appointments', 'View appointments'),
('Create_Appointment', 'Create new appointment'),
('View_Consultations', 'View consultation records'),
('Create_Consultation', 'Start consultation'),
('View_LabTests', 'View lab test orders'),
('Create_LabTest', 'Order lab test'),
('View_Imaging', 'View imaging orders'),
('Create_Imaging', 'Order imaging'),
('View_Surgery', 'View surgery orders'),
('Create_Surgery', 'Order surgery'),
('View_Prescriptions', 'View prescriptions'),
('Create_Prescription', 'Issue prescription'),
('View_Billing', 'View invoices'),
('Create_Invoice', 'Create invoice'),
('Process_Payment', 'Process payment'),
('Manage_Users', 'Manage user accounts'),
('Manage_Roles', 'Manage roles'),
('Manage_Departments', 'Manage departments');

-- Assign permissions to roles (Doctor role example)
INSERT INTO [dbo].[RolePermissions] ([RoleId], [PermissionId])
SELECT 
    (SELECT RoleId FROM Roles WHERE RoleName = 'Doctor'),
    PermissionId
FROM Permissions
WHERE PermissionName IN ('View_Patients', 'View_Appointments', 'Create_Consultation', 
                         'Create_LabTest', 'Create_Imaging', 'Create_Surgery', 
                         'Create_Prescription', 'View_Consultations', 'View_LabTests', 
                         'View_Imaging', 'View_Surgery', 'View_Prescriptions');

-- Insert Sample Departments
INSERT INTO [dbo].[Departments] ([DepartmentName], [Description])
VALUES
('General Practice', 'General Medical Services'),
('Laboratory', 'Pathology and Laboratory Services'),
('Radiology', 'Imaging and Radiography Services'),
('Surgery', 'Surgical Operations'),
('Pharmacy', 'Medication Services'),
('Emergency', 'Emergency Department'),
('Pediatrics', 'Children Medical Services'),
('Cardiology', 'Heart and Cardiovascular Services');

-- Insert Sample Lab Tests
INSERT INTO [dbo].[LabTests] ([TestCode], [TestName], [Description], [SampleType], [Price])
VALUES
('CBC', 'Complete Blood Count', 'Measure blood cells count', 'Blood', 50),
('BS', 'Blood Sugar', 'Measure glucose level', 'Blood', 30),
('CRP', 'C-Reactive Protein', 'Inflammation marker', 'Blood', 40),
('TP', 'Total Protein', 'Protein levels in blood', 'Blood', 35),
('LFT', 'Liver Function Tests', 'Liver enzyme levels', 'Blood', 60),
('RFT', 'Renal Function Tests', 'Kidney function tests', 'Blood', 55);

-- Insert Sample Imaging Types
INSERT INTO [dbo].[ImagingTypes] ([TypeName], [Description], [Price])
VALUES
('X-Ray', 'Standard X-Ray imaging', 200),
('CT Scan', 'Computed Tomography scan', 800),
('MRI', 'Magnetic Resonance Imaging', 1200),
('Ultrasound', 'Ultrasound imaging', 300),
('Mammography', 'Breast imaging', 400);

-- Insert Sample Surgery Types
INSERT INTO [dbo].[SurgeryTypes] ([TypeName], [Description], [EstimatedDuration], [Price])
VALUES
('Minor Surgery', 'Minor surgical procedures', 30, 2000),
('General Surgery', 'General surgical procedures', 90, 5000),
('Orthopedic Surgery', 'Bone and joint surgery', 120, 8000),
('Cardiac Surgery', 'Heart surgery', 180, 20000);

-- Insert Sample Operating Rooms
INSERT INTO [dbo].[OperatingRooms] ([RoomName], [Capacity], [Equipment])
VALUES
('OR-1', 8, 'Standard Surgical Equipment'),
('OR-2', 8, 'Advanced Surgical Equipment'),
('OR-3', 6, 'Minor Surgery Equipment');

-- Insert Sample Medicines
INSERT INTO [dbo].[Medicines] ([MedicineName], [ActiveIngredient], [Dosage], [Form], [Manufacturer], [Price])
VALUES
('Paracetamol', 'Paracetamol', '500mg', 'Tablet', 'Pharma Inc', 5),
('Ibuprofen', 'Ibuprofen', '400mg', 'Tablet', 'Pharma Inc', 10),
('Amoxicillin', 'Amoxicillin', '500mg', 'Capsule', 'Antibiotic Ltd', 15),
('Cough Syrup', 'Dextromethorphan', '10mg/5ml', 'Liquid', 'Syrup Co', 20),
('Aspirin', 'Acetylsalicylic Acid', '100mg', 'Tablet', 'Pharma Inc', 8);

-- Insert Sample Services
INSERT INTO [dbo].[Services] ([ServiceName], [ServiceCode], [Price], [Category])
VALUES
('Consultation', 'CONS-001', 200, 'Consultation'),
('Lab Test', 'LAB-001', 50, 'Lab'),
('X-Ray', 'IMG-001', 200, 'Imaging'),
('CT Scan', 'IMG-002', 800, 'Imaging'),
('General Surgery', 'SUR-001', 5000, 'Surgery'),
('Pharmacy Charge', 'PHARM-001', 0, 'Pharmacy');

-- ============================================
-- SCRIPT END
-- ============================================
-- Run this script in SQL Server Management Studio
-- Make sure to create the database first before running this script
-- Update connection string in application configuration
