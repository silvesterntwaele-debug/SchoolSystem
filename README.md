<img width="1876" height="895" alt="UserManagement" src="https://github.com/user-attachments/assets/ef4849b3-9930-4c87-9d45-0c89400ed78d" />

# 🎓 School Management System

A comprehensive **School Management System** built using **ASP.NET Core MVC**, **C#**, **Entity Framework Core**, **SQL Server**, and **ASP.NET Identity**. The system provides secure role-based access for **Administrators**, **Lecturers**, and **Students** while managing academic records, module registrations, assessments, invoicing, payments, and examination schedules.

---

## 📌 Features

### 👨‍💼 Administrator
- User Management
- Student Management
- Lecturer Management
- Module Management
- Register Students for Modules
- Student Module Mark Sheets
- Exam Timetable Management
- Invoice Management
- Payment Management
- Audit Logs
- Dashboard with system statistics

### 👨‍🏫 Lecturer
- View Assigned Modules
- View Registered Students
- Capture and Update Student Marks
- View Exam Timetables
- Lecturer Dashboard

### 👨‍🎓 Student
- View Registered Modules
- View Exam Timetable
- View Module Mark Sheets
- View Invoices
- View Payments
- Student Dashboard

---

## 🔐 Authentication & Authorization

The project uses **ASP.NET Identity** with **Role-Based Authorization**.

### Roles
- Administrator
- Lecturer
- Student

Each role has access only to features permitted by the system.

---

## 🛠 Technologies Used

- ASP.NET Core MVC
- C#
- Entity Framework Core
- SQL Server
- ASP.NET Identity
- Bootstrap 5
- Razor Views
- LINQ

---

## 🗄 Database

The project uses **SQL Server** together with **Entity Framework Core Code First**.

Main entities include:

- Users
- Students
- Lecturers
- Modules
- Registrations
- Student Module Mark Sheets
- Exam Timetables
- Invoices
- Payments
- Audit Logs

---

## 📷 System Modules

- 👥 User Management
- 👨‍🎓 Student Management
- 👩‍🏫 Lecturer Management
- 📚 Module Management
- 📝 Registration Management
- 📊 Student Module Mark Sheets
- 📅 Exam Timetable
- 💵 Invoice Management
- 💳 Payment Management
- 📜 Audit Logs

---

## 🚀 Installation

### 1. Clone the repository

```bash
git clone https://github.com/YOUR_USERNAME/SchoolManagementSystem.git
```

### 2. Open the solution

Open the project using:

- Visual Studio 2022
- or Visual Studio Code (with C# extensions)

### 3. Update the connection string

Open:

```
appsettings.json
```

Update the SQL Server connection string.

### 4. Apply migrations

```powershell
Update-Database
```

or

```bash
dotnet ef database update
```

### 5. Run the application

Press **F5** or execute:

```bash
dotnet run
```

---

## 📁 Project Structure

```
Controllers/
Data/
Models/
Views/
ViewModels/
wwwroot/
Migrations/
Areas/
```

---

## 🔒 Security Features

- ASP.NET Identity Authentication
- Role-Based Authorization
- Secure Login
- Password Hashing
- Audit Logging
- Authorization Policies

---

## 📈 Future Improvements

- Email Notifications
- Online Student Registration
- Report Generation (PDF)
- Dashboard Charts
- Attendance Tracking
- Results Export to Excel
- SMS Notifications
- Online Fee Payments
- Document Uploads

---

## 👨‍💻 Developer

Developed by **Thato_Silvester**

Built as a university project to demonstrate full-stack web development using **ASP.NET Core MVC** and **SQL Server**.

---

## 📄 License

This project is intended for educational and portfolio purposes.<img width="1883" height="977" alt="StudentDashBoard" src="https://github.com/user-attachments/assets/bceb4dde-ad2e-45bf-aa23-62d6ec8484f9" />
<img width="1867" height="891" alt="Login" src="https://github.com/user-attachments/assets/9bfc6cc0-2740-41b8-99dc-66109930b514" />
<img width="1886" height="922" alt="LecturerDashBoard" src="https://github.com/user-attachments/assets/1d39526d-ad18-4326-9498-2d3a2e84b9be" />
<img width="1880" height="911" alt="AdminDashBoard" src="https://github.com/user-attachments/assets/a0722df8-bd53-4096-b65d-1de567d96476" />
<img width="1890" height="914" alt="HomPage" src="https://github.com/user-attachments/assets/2104411c-c47a-4ccd-a98b-1b00d83e1e04" />
