using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Models;



namespace SchoolSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


        public DbSet< Student> Students => Set<Student>();
        public DbSet< Lecturer> Lecturers => Set<Lecturer>();
        public DbSet<Module> Modules => Set<Module>();
        public DbSet<Registration> Registrations => Set<Registration>();
        public DbSet<Mark> Marks => Set<Mark>();
        public DbSet<ExamTimetable> ExamTimetables => Set<ExamTimetable>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Audit> Audits => Set<Audit>();
        public DbSet<StudentModuleMarkSheet> StudentModuleMarkSheets { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Student>()
                .HasIndex(s => s.StudentNumber)
                .IsUnique();

            builder.Entity<Lecturer>()
                .HasIndex(l => l.StaffNumber)
                .IsUnique();

            builder.Entity<Module>()
                .HasIndex(m => m.Code)
                .IsUnique();

            builder.Entity<StudentModuleMarkSheet>()
                .HasIndex(m => new { m.StudentId, m.ModuleId })
                .IsUnique();

            builder.Entity<Registration>()
                .HasIndex(r => new {r.StudentId, r.ModuleId, r.Semester})
                .IsUnique();

            builder.Entity<StudentModuleMarkSheet>()
               .HasOne(m => m.Student)
               .WithMany(s => s.StudentModuleMarkSheets)
               .HasForeignKey(m => m.StudentId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentModuleMarkSheet>()
               .HasOne(m => m.Module)
               .WithMany(m => m.StudentModuleMarkSheets)
               .HasForeignKey(m => m.ModuleId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Registration>()
                 .HasOne(r => r.Student)
                 .WithMany(s => s.Registrations)
                 .HasForeignKey(r => r.StudentId)
                 .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Registration>()
                 .HasOne(r => r.Module)
                 .WithMany(m => m.Registrations)
                 .HasForeignKey(r => r.ModuleId)
                 .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Mark>()
                 .HasOne(m => m.Student)
                 .WithMany(s => s.Marks)
                 .HasForeignKey(m => m.StudentId)
                 .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Mark>()
                 .HasOne(m => m.Module)
                 .WithMany(mo => mo.Marks)
                 .HasForeignKey(m => m.ModuleId)
                 .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamTimetable>()
                 .HasOne(e => e.Module)
                 .WithOne(m => m.ExamTimetable)
                 .HasForeignKey<ExamTimetable>(e => e.ModuleId);

            builder.Entity<Invoice>()
                 .HasOne(i => i.Student)
                 .WithMany(s => s.Invoices)
                 .HasForeignKey(i => i.StudentId)
                 .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Payment>()
                 .HasOne(p => p.Invoice)
                 .WithMany(i => i.Payments)
                 .HasForeignKey(p => p.InvoiceId)
                 .OnDelete(DeleteBehavior.Restrict);





        }









    }


}
