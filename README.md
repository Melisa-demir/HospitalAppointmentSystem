# Author Melisa Demir

# 🏥 Hospital Appointment System

A microservices-based hospital appointment system built with **ASP.NET Core 8**.  
The project demonstrates communication between independent services using **HTTP**, **RabbitMQ**, and **Hangfire**.

---

## 🚀 Technologies

- ASP.NET Core 8 Web API
- Entity Framework Core
- SQL Server
- RabbitMQ
- Hangfire
- MailKit
- Health Checks
- Swagger
- Dependency Injection
- HttpClientFactory

---

## 📂 Project Structure

```
HospitalAppointmentSystem
│
├── ApiGateway
├── PatientService
├── DoctorService
├── AppointmentService
├── NotificationService
└── Shared.Contracts
```

---

## 📌 Services

### PatientService

- Patient CRUD operations
- SQL Server
- Health Check

### DoctorService

- Doctor CRUD operations
- Doctor availability management
- Health Check

### AppointmentService

- Create appointments
- Validate patient and doctor
- HTTP communication with PatientService
- HTTP communication with DoctorService
- Publish appointment events to RabbitMQ

### NotificationService

- Consume RabbitMQ messages
- Save notifications
- Schedule reminder jobs with Hangfire
- Send reminder emails using MailKit

---

## 🔄 System Workflow

```
Client
   │
   ▼
AppointmentService
   │
   ├── HTTP → PatientService
   │
   ├── HTTP → DoctorService
   │
   ▼
RabbitMQ
   │
   ▼
NotificationService
   │
   ├── Save Notification
   │
   ├── Schedule Reminder (Hangfire)
   │
   ▼
AppointmentReminderJob
   │
   ▼
MailKit
   │
   ▼
Patient Email
```

---

## 📬 Appointment Reminder Flow

1. User creates an appointment.
2. AppointmentService validates patient and doctor.
3. AppointmentService publishes an `AppointmentCreatedEvent`.
4. NotificationService consumes the event.
5. A notification record is saved.
6. Hangfire schedules a reminder one hour before the appointment.
7. AppointmentReminderJob sends an email reminder using MailKit.
8. Notification status is updated.

---

## 📊 Features

- Patient Management
- Doctor Management
- Appointment Management
- Microservice Communication
- Event-Driven Architecture
- RabbitMQ Messaging
- Background Jobs with Hangfire
- Email Notifications
- Health Checks
- Swagger Documentation

---

## 🔐 Configuration

Sensitive information is **not stored** in the repository.

The project uses **User Secrets** for SMTP credentials.

Example:

```json
{
  "EmailSettings": {
    "SenderEmail": "your-email@gmail.com",
    "Password": "your-app-password"
  }
}
```

---

## ▶️ Running the Project

1. Clone the repository.

```bash
git clone https://github.com/Melisa-demir/HospitalAppointmentSystem
```

2. Start RabbitMQ.

3. Update connection strings.

4. Configure User Secrets.

5. Run the services:

- PatientService
- DoctorService
- AppointmentService
- NotificationService

6. Open Swagger for each service.

---
